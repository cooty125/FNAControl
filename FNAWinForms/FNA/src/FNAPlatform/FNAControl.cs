/* 
 * FNAControl
 * =====================================================================
 * FileName: FNAControl.cs
 * ---------------------------------------------------------------------
 * This document is distributed under General Public License.
 * Copyright © David Kutnar 2025 - All rights reserved.
 * =====================================================================
 * Description: 
 * This class serves as an FNA component that enables users
 * to use FNA3D in WinForms applications. Place this file in FNA/src/FNAPlatform
 * and add System.Windows.Forms to references.
 * =====================================================================
 */

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using SDL3;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Microsoft.Xna.Framework
{
	public abstract class FNAControl : Control
	{
		private IntPtr sdl_window;
		private bool designMode;
		private System.Threading.Timer loopTimer;
		private Stopwatch frameStopwatch;
		private long previousFrameTime;
		private readonly object timerLock = new object();
		private CustomInputState inputState;
		private const int FNAC_MSAA = 0;								// Multisample Anti-Aliasing
		private const int FNAC_FPSMAX = 60;                             // Maximum fps

		[Browsable(false)]
		public GraphicsDevice GraphicsDevice { get; private set; }
		[Browsable(false)]
		public GameWindow Window { get; private set; }
		[Browsable(false)]
		public CustomInputState InputState {
			get { return this.inputState; }
		}
		[Browsable(false)]
		public ContentManager Content { get; private set; }
		[DefaultValue(false)]
		public bool IsInitialized { get; private set; }
		[Category("FNA")]
		[Description("Target framerate")]
		[DefaultValue(60)]
		public int FPSMax { get; set; }
		[Browsable(false)]
		public float FPS { get; private set; }
		[Browsable(false)]
		public bool IsRunning { get; private set; }
		
		protected abstract void Initialize();
		protected abstract void Update(float elapsedTime);
		protected abstract void Draw();

		public FNAControl() {
			this.designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

			if (!this.designMode && (this.Width <= 0 || this.Height <= 0)) {
				this.Width = 800;
				this.Height = 600;
			}

			this.FPSMax = FNAC_FPSMAX;
			this.inputState = new CustomInputState( );

			this.SetStyle( ControlStyles.EnableNotifyMessage, true );
			this.SetStyle( ControlStyles.UserMouse, true );
			this.SetStyle( ControlStyles.UserPaint, true );
			this.SetStyle( ControlStyles.AllPaintingInWmPaint, false );
			this.SetStyle( ControlStyles.OptimizedDoubleBuffer, false );
			this.SetStyle( ControlStyles.Selectable, true );

			this.TabStop = true;
			this.DoubleBuffered = false;
			this.Resize += OnResize;
			this.Enabled = true;
		}

        //
        // FNA
        // Initialize
        private void initialize_FNA() {
			if (this.IsInitialized) { return; }
			if (this.designMode) { return; }

			try {
				this.sdl_window = this.create_SDLWindow( );
				SDL.SDL_SetWindowFocusable(this.sdl_window, true);

				IntPtr sdl_wHandle = this.getHandle_SDLWindow( this.sdl_window );
				this.embed_SDLWindow( sdl_wHandle );

				string fna_displayName = ( @"\\.\DISPLAY" + SDL.SDL_GetDisplayForWindow( this.sdl_window ).ToString( ) );
				this.Window = new FNAWindow( this.sdl_window, fna_displayName );

				this.initialize_GraphicsDevice( );

				ServiceContainer services = new ServiceContainer();
				GraphicsDeviceService gdService = new GraphicsDeviceService(this.GraphicsDevice);
				services.AddService(typeof(IGraphicsDeviceService), gdService);

				this.Content = new ContentManager(services);
				this.Content.RootDirectory = @"Content";

				SDL.SDL_SetWindowFocusable( this.sdl_window, true );
				SDL.SDL_ShowWindow( this.sdl_window );
				SDL.SDL_RaiseWindow( this.sdl_window );
				SDL.SDL_ShowCursor();

				this.frameStopwatch = new Stopwatch( );
				this.frameStopwatch.Start( );
				this.previousFrameTime = 0;

				this.IsInitialized = true;
				this.Initialize( );
				this.StartRendering( );

			} catch (Exception ex) {
				throw new InvalidOperationException("FNA Initialization failed: ", ex);
			}
		}

		//
		// FNAControl
		// LoopCallback
		private void LoopCallback(object state) {
			if ( !this.IsRunning || !this.IsInitialized || this.GraphicsDevice == null ) { return; }

			if ( this.InvokeRequired ) {
				this.BeginInvoke( new Action( ( ) => this.LoopCallback( state ) ) );
				return;
			}

			SDL.SDLBool hasEvents = true;
			while ( hasEvents ) {
				SDL.SDL_Event sdlEvent;
				hasEvents = SDL.SDL_PollEvent( out sdlEvent );
				if ( hasEvents ) {
					this.process_SDLEvent( sdlEvent );
				}
			}

			long currentTime = this.frameStopwatch.ElapsedMilliseconds;
			float elapsedTime = ( ( currentTime - this.previousFrameTime ) / 1000.0f );
			this.previousFrameTime = currentTime;

			if ( elapsedTime > 0 ) {
				this.FPS = ( 1.0f / elapsedTime );
			}

			this.Update( elapsedTime );

			if ( this.IsInitialized || this.GraphicsDevice != null ) {
				this.Draw();
				this.GraphicsDevice.Present();
			}
		}

		//
		// SDLEvent
		// Process
		private void process_SDLEvent( SDL.SDL_Event e ) {
			switch ( e.type ) {
				case 768:   // SDL_EVENT_KEY_DOWN
					this.inputState.PressedKeys.Add( this.convert_SDLKey( e.key.key) );
					break;
				case 769:   // SDL_EVENT_KEY_UP
					this.inputState.PressedKeys.Remove( this.convert_SDLKey( e.key.key ) );
					break;
				case 1025:  // SDL_EVENT_MOUSE_BUTTON_DOWN
					if ( e.button.button == 1 ) {
						IntPtr winHWND = this.getHandle_SDLWindow( this.sdl_window );
						USER32.SetFocus( winHWND );

						SDL.SDL_RaiseWindow( this.sdl_window );
						SDL.SDL_SetWindowFocusable( this.sdl_window, true );
					}
					break;
				case 1026:  // SDL_EVENT_MOUSE_BUTTON_UP
				case 1024:  // SDL_EVENT_MOUSE_MOTION
							
					break;
			}
		}
		//
		// SDLKey
		// Convert
		private Keys convert_SDLKey( uint sdlKeyCode ) {
			switch ( sdlKeyCode ) {
				case (uint) SDL.SDL_Keycode.SDLK_W: return Keys.W;
				case (uint) SDL.SDL_Keycode.SDLK_A: return Keys.A;
				case (uint) SDL.SDL_Keycode.SDLK_S: return Keys.S;
				case (uint) SDL.SDL_Keycode.SDLK_D: return Keys.D;
				case (uint) SDL.SDL_Keycode.SDLK_E: return Keys.E;
				case (uint) SDL.SDL_Keycode.SDLK_Q: return Keys.Q;
				case (uint) SDL.SDL_Keycode.SDLK_SPACE: return Keys.Space;

				default:
					return Keys.None;
			}
		}

		#region SDL Window

		//
		// SDL Window
		// Create
		private IntPtr create_SDLWindow( ) {
			if ( !SDL.SDL_Init( SDL.SDL_InitFlags.SDL_INIT_VIDEO ) ) {
				throw new Exception( "SDL_Init failed: " + SDL.SDL_GetError( ) );
			}

			SDL.SDL_WindowFlags initFlags = (
				SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN |
				SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
				SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS) |
				(SDL.SDL_WindowFlags) FNA3D.FNA3D_PrepareWindowAttributes();

			IntPtr sdlHWND = SDL.SDL_CreateWindow(
				"FNA Control",
				Math.Max(1, this.Width),
				Math.Max(1, this.Height),
				initFlags
			);

			if (sdlHWND == IntPtr.Zero) {
				throw new Exception( "SDL_CreateWindow failed: " + SDL.SDL_GetError( ) );
			}

			return sdlHWND;
		}
		//
		// SDL Window
		// GetHandle
		private IntPtr getHandle_SDLWindow( IntPtr sdlWindow ) {
			uint properties = SDL.SDL_GetWindowProperties( sdlWindow );

			IntPtr windowsHWND = SDL.SDL_GetPointerProperty(
				properties,
				SDL.SDL_PROP_WINDOW_WIN32_HWND_POINTER,
				IntPtr.Zero
			);

			if ( windowsHWND == IntPtr.Zero ) {
				throw new Exception( "SDL_GetPointerProperty failed: Failed to get Windows handle from SDL window." );
			}

			return windowsHWND;
		}
		//
		// SDL Window
		// Embed
		private void embed_SDLWindow( IntPtr windowsHandle ) {
			SDL.SDL_HideWindow( this.sdl_window );

			int style = USER32.GetWindowLong( windowsHandle, USER32.GWL_STYLE );
			style = ( ( style & ~( USER32.WS_OVERLAPPEDWINDOW ) ) | USER32.WS_CHILD );

			USER32.SetWindowLong( windowsHandle, USER32.GWL_STYLE, style );
			USER32.SetParent( windowsHandle, this.Handle );
			USER32.SetWindowPos( windowsHandle, IntPtr.Zero, 0, 0, this.Width, this.Height, 0x0040 );

			SDL.SDL_ShowWindow( this.sdl_window );

			this.Invalidate( );
		}

		#endregion

		#region Graphics Device

		//
		// Graphics Device
		// Initialize
		private void initialize_GraphicsDevice( ) {
			PresentationParameters pParams = new PresentationParameters {
				DeviceWindowHandle = this.Window.Handle,
				BackBufferWidth = Math.Max( 1, this.Width ),
				BackBufferHeight = Math.Max( 1, this.Height ),
				IsFullScreen = false,
				PresentationInterval = PresentInterval.Immediate,
				DepthStencilFormat = DepthFormat.Depth24Stencil8,
				MultiSampleCount = FNAC_MSAA
			};

			this.GraphicsDevice = new GraphicsDevice( GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pParams );
		}
		//
		// Graphics Device
		// Resize
		private void resize_GraphicsDevice( ) {
			if ( this.GraphicsDevice != null ) {
				PresentationParameters pParams = this.GraphicsDevice.PresentationParameters;
				pParams.BackBufferWidth = Math.Max( 1, this.Width );
				pParams.BackBufferHeight = Math.Max( 1, this.Height );

				this.GraphicsDevice.Reset(pParams);
			}
		}

		#endregion

		//
		// FNAControl
		// StartRendering
		public void StartRendering( ) {
			if ( !this.IsInitialized || this.IsRunning ) { return; }

			lock ( this.timerLock ) {
				this.IsRunning = true;
				int interval = Math.Max( 1, 1000 / this.FPSMax );

				this.loopTimer = new System.Threading.Timer( this.LoopCallback, null, 0, interval );
			}
		}
		//
		// FNAControl
		// StopRendering
		public void StopRendering( ) {
			lock ( timerLock ) {
				this.IsRunning = false;
				if ( this.loopTimer != null ) {
					this.loopTimer.Dispose( );
					this.loopTimer = null;
				}
			}
		}
		//
		// FNAControl
		// GetCurrentVideoDriverName
		public string GetCurrentVideoDriverName() {
			string dName = "NULL";

			if ( this.IsInitialized ) {
				string videoDriver = SDL.SDL_GetCurrentVideoDriver( );
				switch ( videoDriver != null ? videoDriver.ToLower( ) : "" ) {
					case "windows":
						dName = "Direct3D 11";
						break;
					case "direct3d":
						dName = "Direct3D";
						break;
					case "d3d11":
						dName = "Direct3D 11";
						break;
					case "d3d12":
						dName = "Direct3D 12";
						break;
					case "x11":
						dName = "OpenGL (X11)";
						break;
					case "opengl":
						dName = "OpenGL";
						break;
					case "vulkan":
						dName = "Vulkan";
						break;
					default:
						dName = videoDriver;
						break;
				}
			}

			return dName;
		}

		protected override void OnHandleCreated( EventArgs e ) {
			base.OnHandleCreated( e );

			if ( !this.designMode && !this.IsInitialized ) {
				this.initialize_FNA( );
			}
		}
		private void OnResize( object sender, EventArgs e ) {
			if ( !this.IsInitialized || this.sdl_window == IntPtr.Zero ) {
				return;
			}

			IntPtr windowsHandle = this.getHandle_SDLWindow( this.sdl_window );
			SDL.SDL_SetWindowSize( this.sdl_window, Math.Max( 1, this.Width ), Math.Max( 1, this.Height ) );
			USER32.SetWindowPos( windowsHandle, IntPtr.Zero, 0, 0, this.Width, this.Height, 0x0040 );

			this.resize_GraphicsDevice( );
		}
		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );
		}
        protected override void Dispose( bool disposing ) {
			this.StopRendering();

			if ( disposing ) {
				System.Threading.Thread.Sleep(50);

				if ( this.GraphicsDevice != null ) {
					this.GraphicsDevice.Dispose( );
					this.GraphicsDevice = null;
				}

				if ( this.sdl_window != IntPtr.Zero ) {
					SDL.SDL_HideWindow( this.sdl_window );
					SDL.SDL_DestroyWindow( this.sdl_window );
					SDL.SDL_Quit( );
				}

				if ( this.loopTimer != null )	{
					this.loopTimer.Dispose( );
					this.loopTimer = null;
				}

				if ( this.frameStopwatch != null ) {
					this.frameStopwatch.Stop( );
					this.frameStopwatch = null;
				}
			}

			base.Dispose( disposing );
		}

		//
		// Input
		// CustomInputState
		public class CustomInputState {
			public HashSet<Keys> PressedKeys = new HashSet<Keys>();
			public MouseState MouseState;

			public KeyboardState GetKeyboardState( ) {
				return new KeyboardState( PressedKeys.ToArray() );
			}
		}

		//
		// USER32
		// API Functions
		private static class USER32
		{
			public const int GWL_STYLE = -16;
			public const int WS_CHILD = 0x40000000;
			public const int WS_OVERLAPPEDWINDOW = 0x00CF0000;

			[DllImport("user32.dll")]
			public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
			[DllImport("user32.dll")]
			public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
			[DllImport("user32.dll")]
			public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
			[DllImport("user32.dll")]
			public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
			[DllImport("user32.dll")]
			public static extern IntPtr SetFocus(IntPtr hWnd);
		}

		//
		// GraphicsDevice
		// ServiceProvider
		public class GraphicsDeviceService : IGraphicsDeviceService
		{
			private GraphicsDevice graphicsDevice;

			public GraphicsDeviceService( GraphicsDevice gDevice ) {
				this.graphicsDevice = gDevice;
			}

			public GraphicsDevice GraphicsDevice {
				get { return this.graphicsDevice; }
			}

			public event EventHandler<EventArgs> DeviceCreated;
			public event EventHandler<EventArgs> DeviceDisposing;
			public event EventHandler<EventArgs> DeviceReset;
			public event EventHandler<EventArgs> DeviceResetting;

			protected virtual void OnDeviceCreated( ) { }
			protected virtual void OnDeviceDisposing( ) { }
		}
	}
}
