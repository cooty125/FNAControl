/* 
 * FNAControl
 * =====================================================================
 * FileName: FNAControl.cs
 * Project: FNA
 * Location: FNA/src/FNAPlatform
 * Version: 1.1.0.0
 * ---------------------------------------------------------------------
 * This document is distributed under General Public License v3.0
 * Copyright © David Kutnar 2026 - All rights reserved.
 * =====================================================================
 * Description: 
 * This class serves as an FNA component that enables users
 * to use FNA3D in WinForms applications. Place this file in FNA/src/FNAPlatform
 * and add System.Windows.Forms to references.
 * =====================================================================
 */

#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

using SDL3;

#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// FNA Control for WinForms applications.
	/// Uses background thread for rendering and VSync for frame synchronization.
	/// </summary>
	public abstract class FNAControl : Control
	{
		private static readonly Dictionary<uint, Keys> FNAC_KEYMAP = new Dictionary<uint, Keys>
		{
			// Letters
			{ (uint)SDL.SDL_Keycode.SDLK_A, Keys.A },
			{ (uint)SDL.SDL_Keycode.SDLK_B, Keys.B },
			{ (uint)SDL.SDL_Keycode.SDLK_C, Keys.C },
			{ (uint)SDL.SDL_Keycode.SDLK_D, Keys.D },
			{ (uint)SDL.SDL_Keycode.SDLK_E, Keys.E },
			{ (uint)SDL.SDL_Keycode.SDLK_F, Keys.F },
			{ (uint)SDL.SDL_Keycode.SDLK_G, Keys.G },
			{ (uint)SDL.SDL_Keycode.SDLK_H, Keys.H },
			{ (uint)SDL.SDL_Keycode.SDLK_I, Keys.I },
			{ (uint)SDL.SDL_Keycode.SDLK_J, Keys.J },
			{ (uint)SDL.SDL_Keycode.SDLK_K, Keys.K },
			{ (uint)SDL.SDL_Keycode.SDLK_L, Keys.L },
			{ (uint)SDL.SDL_Keycode.SDLK_M, Keys.M },
			{ (uint)SDL.SDL_Keycode.SDLK_N, Keys.N },
			{ (uint)SDL.SDL_Keycode.SDLK_O, Keys.O },
			{ (uint)SDL.SDL_Keycode.SDLK_P, Keys.P },
			{ (uint)SDL.SDL_Keycode.SDLK_Q, Keys.Q },
			{ (uint)SDL.SDL_Keycode.SDLK_R, Keys.R },
			{ (uint)SDL.SDL_Keycode.SDLK_S, Keys.S },
			{ (uint)SDL.SDL_Keycode.SDLK_T, Keys.T },
			{ (uint)SDL.SDL_Keycode.SDLK_U, Keys.U },
			{ (uint)SDL.SDL_Keycode.SDLK_V, Keys.V },
			{ (uint)SDL.SDL_Keycode.SDLK_W, Keys.W },
			{ (uint)SDL.SDL_Keycode.SDLK_X, Keys.X },
			{ (uint)SDL.SDL_Keycode.SDLK_Y, Keys.Y },
			{ (uint)SDL.SDL_Keycode.SDLK_Z, Keys.Z },

			// Numbers
			{ (uint)SDL.SDL_Keycode.SDLK_0, Keys.D0 },
			{ (uint)SDL.SDL_Keycode.SDLK_1, Keys.D1 },
			{ (uint)SDL.SDL_Keycode.SDLK_2, Keys.D2 },
			{ (uint)SDL.SDL_Keycode.SDLK_3, Keys.D3 },
			{ (uint)SDL.SDL_Keycode.SDLK_4, Keys.D4 },
			{ (uint)SDL.SDL_Keycode.SDLK_5, Keys.D5 },
			{ (uint)SDL.SDL_Keycode.SDLK_6, Keys.D6 },
			{ (uint)SDL.SDL_Keycode.SDLK_7, Keys.D7 },
			{ (uint)SDL.SDL_Keycode.SDLK_8, Keys.D8 },
			{ (uint)SDL.SDL_Keycode.SDLK_9, Keys.D9 },

			// Numbers NumPad
			{ (uint)SDL.SDL_Keycode.SDLK_KP_0, Keys.NumPad0 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_1, Keys.NumPad1 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_2, Keys.NumPad2 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_3, Keys.NumPad3 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_4, Keys.NumPad4 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_5, Keys.NumPad5 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_6, Keys.NumPad6 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_7, Keys.NumPad7 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_8, Keys.NumPad8 },
			{ (uint)SDL.SDL_Keycode.SDLK_KP_9, Keys.NumPad9 },

			// Special keys
			{ (uint)SDL.SDL_Keycode.SDLK_SPACE, Keys.Space },
			{ (uint)SDL.SDL_Keycode.SDLK_ESCAPE, Keys.Escape },
			{ (uint)SDL.SDL_Keycode.SDLK_RETURN, Keys.Enter },
			{ (uint)SDL.SDL_Keycode.SDLK_TAB, Keys.Tab },
			{ (uint)SDL.SDL_Keycode.SDLK_BACKSPACE, Keys.Back },
			{ (uint)SDL.SDL_Keycode.SDLK_UP, Keys.Up },
			{ (uint)SDL.SDL_Keycode.SDLK_DOWN, Keys.Down },
			{ (uint)SDL.SDL_Keycode.SDLK_LEFT, Keys.Left },
			{ (uint)SDL.SDL_Keycode.SDLK_RIGHT, Keys.Right },
			{ (uint)SDL.SDL_Keycode.SDLK_LALT, Keys.LeftAlt },
			{ (uint)SDL.SDL_Keycode.SDLK_RALT, Keys.RightAlt },
			{ (uint)SDL.SDL_Keycode.SDLK_LCTRL, Keys.LeftControl },
			{ (uint)SDL.SDL_Keycode.SDLK_RCTRL, Keys.RightControl },
			{ (uint)SDL.SDL_Keycode.SDLK_LSHIFT, Keys.LeftShift },
			{ (uint)SDL.SDL_Keycode.SDLK_RSHIFT, Keys.RightShift },
		};
		private static readonly int[ ] FNAC_SUPPORTED_MSAA = new int[ ] { 0, 2, 4, 8, 16 };
		private const int FNAC_MAX_MSAA = 16;                           // Maximum count of anti-aliasing samples
		private const int FNAC_UI_QUEUE_THRESHOLD = 10;                 // UI Queue messages threshold

		private bool _disposed = false;
		private IntPtr sdl_window;										// HWND
		private bool designMode;										// Desingner

		private Thread renderThread;                                    // Rendering thread
		private volatile bool shouldThreadStop = false;
		private Stopwatch renderStopwatch;                              // Frame timing
		private long previousFrameTime = 0;                             // Last frame time
		private volatile bool renderThreadRunning = false;              // Thread life
		private readonly ManualResetEvent pauseEvent = new ManualResetEvent( true );

		private readonly object timerLock = new object();               // Timer LOCK
		private readonly object renderLock = new object( );             // Render LOCK
		private readonly object pauseLock = new object( );              // Pause LOCK
		private readonly object inputLock = new object( );              // Input LOCK

		private CustomInputState inputState;                            // Input
		private GraphicsDeviceService graphicsDeviceService;            // Graphics device service
		private int multiSampleCount = 0;                               // Default MSAA count

		// User - Properties
		[ Browsable(false) ]
		public GraphicsDevice GraphicsDevice { get; private set; }
		[ Browsable(false) ]
		public GameWindow Window { get; private set; }
		[ Browsable(false) ]
		public CustomInputState Input { get { return this.inputState; } }
		[ Browsable(false) ]
		public ContentManager Content { get; private set; }
		[ Browsable( false ) ]
		public bool IsDesignMode { get { return this.designMode; } }
		[ DefaultValue( false ) ]
		public bool IsInitialized { get; private set; }
		[ Browsable(false) ]
		public bool IsRunning { get; private set; }
		[ Browsable( false ) ]
		public bool IsPaused { get; private set; }
		[ Browsable( false ) ]
		public bool IsFocused { get { return this.hasFocus( ); } }
		[Browsable( false )]
		public float FPS { get; private set; }
		[DefaultValue( 0 )]
		[Browsable( true )]
		[Category( "Rendering" )]
		[Description( "Multisample Anti-Aliasing count." )]
		public int MultiSampleCount
		{
			get { return this.multiSampleCount; }
			set
			{
				int clamped = Math.Max( 0, Math.Min( value, FNAC_MAX_MSAA ) );
				int closest = FNAC_SUPPORTED_MSAA[ 0 ];
				int minDiff = int.MaxValue;

				foreach ( int supported in FNAC_SUPPORTED_MSAA )
				{
					int diff = Math.Abs( clamped - supported );
					if ( diff < minDiff ) {
						minDiff = diff;
						closest = supported;
					}
				}

				if ( this.multiSampleCount == closest ) {
					return;
				}

				this.multiSampleCount = closest;

				if ( this.IsInitialized && this.GraphicsDevice != null ) {
					this.apply_GraphicsDevice( delegate ( PresentationParameters p ) {
						p.MultiSampleCount = this.multiSampleCount;
					} );
				}
			}
		}

		// User - Events
		/// <summary>Raised when rendering is paused.</summary>
		public event EventHandler RenderingPaused;
		/// <summary>Raised when rendering is resumed.</summary>
		public event EventHandler RenderingResumed;

		// User - Abstract methods
		/// <summary>Called once after FNA initialization.</summary>
		protected abstract void Initialize( );
		/// <summary>Called every frame. Use Input here.</summary>
		protected abstract void Update( float elapsedTime );
		/// <summary>Called every frame after Update.</summary>
		protected abstract void Draw( );

		// User - Virtual methods
		protected virtual void OnControlResized( EventArgs e ) { }
		protected virtual void OnRenderingPaused( EventArgs e ) {
			if ( this.RenderingPaused != null ) {
				this.RenderingPaused.Invoke( this, e );
			}
		}
		protected virtual void OnRenderingResumed( EventArgs e ) {
			if ( this.RenderingResumed != null ) {
				this.RenderingResumed.Invoke( this, e );
			}
		}


		public FNAControl() {
			this.designMode = ( LicenseManager.UsageMode == LicenseUsageMode.Designtime );

			// Set default size
			if ( !this.designMode && ( this.Width <= 0 || this.Height <= 0 ) ) {
				this.Width = 800;
				this.Height = 600;
			}

			// Initialize input
			this.inputState = new CustomInputState( );

			// Set control style
			this.SetStyle( ControlStyles.EnableNotifyMessage, true );
			this.SetStyle( ControlStyles.UserMouse, true );
			this.SetStyle( ControlStyles.UserPaint, true );
			this.SetStyle( ControlStyles.AllPaintingInWmPaint, false );
			this.SetStyle( ControlStyles.OptimizedDoubleBuffer, false );
			this.SetStyle( ControlStyles.Selectable, false );

			this.TabStop = false;
			this.DoubleBuffered = false;
			this.Enabled = true;
			this.Resize += this.OnResize;
		}

		//
		// Render Thread Proc
		//
		private void renderThreadProc( ) {
			Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
			Thread.CurrentThread.Name = "FNAControlRenderThread";

			// Create frame timer
			this.renderStopwatch = Stopwatch.StartNew( );
			this.previousFrameTime = 0;

			// Main loop
			while ( this.renderThreadRunning && this.IsRunning && !this.shouldThreadStop )
			{
				// Events
				SDL.SDL_Event sdlEvent;
				while ( SDL.SDL_PollEvent( out sdlEvent ) ) {
					this.process_SDLEvent( sdlEvent );
				}

				// Pause
				if ( this.IsPaused ) {
					this.pauseEvent.WaitOne( );
					continue;
				}

				// Focus
				if ( !this.hasFocus() ) {
					Thread.Sleep( 10 );
					continue;
				}

				// Check if UI is fast enought
				if ( this.isUIMessageQueueOverloaded( ) ) {
					Thread.Sleep( 5 );
					// Skip this frame
					continue;
				}

				// Render
				lock ( this.renderLock )
				{
					this.RenderFrame( );
				}

				// Free CPU
				Thread.Sleep( 1 );
			}
		}

		//
		// Is UI Message Queue Overloaded
		//
		private bool isUIMessageQueueOverloaded( ) {
			const uint PM_NOREMOVE = 0x0000;
			MSG msg;
			int messageCount = 0;

			while ( USER32.PeekMessage( out msg, IntPtr.Zero, 0, 0, PM_NOREMOVE ) )
			{
				messageCount++;
				if ( messageCount > FNAC_UI_QUEUE_THRESHOLD ) {
					return true;
				}
			}

			return false;
		}

		#region Public API

		/// <summary>
		/// Renders one frame immediately.
		/// </summary>
		public void RenderFrame()
		{
			// TIME
			long currentTime = this.renderStopwatch.ElapsedMilliseconds;
			float elapsedTime = ( ( currentTime - this.previousFrameTime ) / 1000.0f );
			this.previousFrameTime = currentTime;

			if ( elapsedTime > 0.1f ) {
				elapsedTime = 0.1f;
			}
			if ( elapsedTime > 0f ) {
				this.FPS = ( 1.0f / elapsedTime );
			}

			// UPDATE
			lock ( this.inputLock )
			{
				this.Update( elapsedTime );
			}

			// DRAW
			this.Draw( );

			// PRESENT
			if ( this.IsHandleCreated && !this.IsDisposed && this.GraphicsDevice != null ) {
				this.GraphicsDevice.Present( );
			}
		}

		/// <summary>
		/// Starts the rendering thread.
		/// </summary>
		public void StartRendering( )
		{
			if ( !this.IsInitialized || this.IsRunning ) {
				return;
			}

			lock ( this.timerLock )
			{
				this.IsRunning = true;
				this.renderThreadRunning = true;
				this.shouldThreadStop = false;

				// Create adn start rendering thread
				this.renderThread = new Thread( this.renderThreadProc );
				this.renderThread.IsBackground = true;
				this.renderThread.Priority = ThreadPriority.AboveNormal;
				this.renderThread.Name = "FNAControlRenderThread";
				this.renderThread.Start( );
			}
		}

		/// <summary>
		/// Stops the rendering thread.
		/// </summary>
		public void StopRendering( )
		{
			lock ( timerLock )
			{
				this.IsRunning = false;
				this.renderThreadRunning = false;
				this.shouldThreadStop = true;

				if ( this.renderThread != null && this.renderThread.IsAlive ) {
					// Thread interruption
					if ( !this.renderThread.Join( 500 ) ) {
						this.renderThread.Interrupt( );

						if ( !this.renderThread.Join( 200 ) ) {
							Debug.WriteLine( "[FNAControl] Render thread did not stop in time!" );
						}

						Thread.Sleep( 100 );
					}

					this.renderThread = null;
				}
			}
		}

		/// <summary>
		/// Pauses rendering (saves CPU when window is minimized).
		/// </summary>
		public void PauseRendering( )
		{
			if ( !this.IsInitialized || this.IsPaused ) {
				return;
			}

			lock ( this.pauseLock )
			{
				if ( !this.IsPaused ) {
					this.IsPaused = true;

					// Block the render thread by resetting the event
					this.pauseEvent.Reset( );
					this.OnRenderingPaused( EventArgs.Empty );
				}
			}
		}

		/// <summary>
		/// Resumes rendering after pause.
		/// </summary>
		public void ResumeRendering( )
		{
			if ( !this.IsInitialized || !this.IsPaused ) {
				return;
			}

			lock ( this.pauseLock )
			{
				if ( this.IsPaused ) {
					this.IsPaused = false;

					// Unblock the render thread by setting the event
					this.pauseEvent.Set( );
					this.OnRenderingResumed( EventArgs.Empty );
				}
			}
		}

		/// <summary>
		/// Returns active video driver name.
		/// </summary>
		public string GetCurrentVideoDriverName( )
		{
			string dName = "NULL";

			if ( this.IsInitialized ) {
				string videoDriver = SDL.SDL_GetCurrentVideoDriver( );

				switch ( videoDriver != null ? videoDriver.ToLower( ) : "" )
				{
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

		/// <summary>
		/// Clears all input states (keys, mouse, scroll).
		/// </summary>
		public void ResetInputState( )
		{
			lock ( this.inputLock )
			{
				this.inputState.KeyboardPressedKeys.Clear( );
				this.inputState.MousePressedButtons.Clear( );
				this.inputState.ScrollWheelValue = 0;
			}
		}

		#endregion

		//
		// Events
		//
		// OnHandleCreated
		// OnResize
		// OnPaint
		//
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
			this.RenderFrame( );

			this.OnControlResized( EventArgs.Empty );
		}
		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );
		}

		//
		// Dispose
		//
		protected override void Dispose( bool disposing )
		{
			if ( this._disposed ) {
				return;
			}

			if ( disposing ) {
				this.shouldThreadStop = true;
				this.StopRendering( );

				if ( this.pauseEvent != null ) {
					// Signal the event to unblock any waiting threads
					this.pauseEvent.Set( );
					this.pauseEvent.Dispose( );
				}

				if ( this.graphicsDeviceService != null ) {
					// Destroy graphics device service provider
					//this.graphicsDeviceService.Dispose( );
					//this.graphicsDeviceService = null;
				}

				if ( this.GraphicsDevice != null ) {
					// Destroy graphics device
					this.GraphicsDevice.Dispose( );
					this.GraphicsDevice = null;
				}

				if ( this.sdl_window != IntPtr.Zero ) {
					// Destroy SDL window
					SDL.SDL_HideWindow( this.sdl_window );
					SDL.SDL_DestroyWindow( this.sdl_window );
					//SDL.SDL_Quit( );
					this.sdl_window = IntPtr.Zero;
				}
			}

			this._disposed = true;
			base.Dispose( disposing );
		}


		#region FNA

		//
		// FNA
		//
		// Custom Input State
		// MouseButton
		//
		public class CustomInputState
		{
			/// <summary>Currently pressed keys.</summary>
			public HashSet<Keys> KeyboardPressedKeys = new HashSet<Keys>( );
			/// <summary>Currently pressed mouse buttons.</summary>
			public HashSet<MouseButton> MousePressedButtons = new HashSet<MouseButton>( );
			/// <summary>Mouse X position relative to control.</summary>
			public int MouseX { get; set; }
			/// <summary>Mouse Y position relative to control.</summary>
			public int MouseY { get; set; }
			/// <summary>Accumulated scroll wheel value (120 per click).</summary>
			public int ScrollWheelValue { get; set; }

			/// <summary>FNA-compatible keyboard state.</summary>
			public KeyboardState GetKeyboardState( ) {
				return new KeyboardState( this.KeyboardPressedKeys.ToArray( ) );
			}
			/// <summary>FNA-compatible mouse state.</summary>
			public MouseState GetMouseState( ) {
				return new MouseState(
					this.MouseX, this.MouseY, this.ScrollWheelValue,
					this.getMouseButtonState( MouseButton.Left ),
					this.getMouseButtonState( MouseButton.Middle ),
					this.getMouseButtonState( MouseButton.Right ),
					this.getMouseButtonState( MouseButton.X1 ),
					this.getMouseButtonState( MouseButton.X2 )
				);
			}

			/// <summary>True if key is pressed.</summary>
			public bool IsKeyDown( Keys key ) {
				return this.KeyboardPressedKeys.Contains( key );
			}
			/// <summary>True if key is not pressed.</summary>
			public bool IsKeyUp( Keys key ) {
				return !this.IsKeyDown( key );
			}
			/// <summary>True if mouse button is pressed.</summary>
			public bool IsMouseButtonDown( MouseButton button ) {
				return this.MousePressedButtons.Contains( button );
			}
			/// <summary>True if mouse button is not pressed.</summary>
			public bool IsMouseButtonUp( MouseButton button ) {
				return !this.IsMouseButtonDown( button );
			}

			private Microsoft.Xna.Framework.Input.ButtonState getMouseButtonState( MouseButton button ) {
				return this.MousePressedButtons.Contains( button ) ? Microsoft.Xna.Framework.Input.ButtonState.Pressed : Microsoft.Xna.Framework.Input.ButtonState.Released;
			}
		}
		/// <summary>Mouse button identifiers.</summary>
		public enum MouseButton
		{
			Left = 1,
			Middle = 2,
			Right = 3,
			X1 = 4,
			X2 = 5
		};

		//
		// FNA
		//
		// Initialize
		//
		private void initialize_FNA( )
		{
			if ( this.IsInitialized || this.designMode )
			{ return; }

			try {
				// Handle
				this.sdl_window = this.create_SDLWindow( );
				IntPtr sdl_wHandle = this.getHandle_SDLWindow( this.sdl_window );

				// Window
				this.embed_SDLWindow( sdl_wHandle );
				string fna_displayName = (@"\\.\DISPLAY" + SDL.SDL_GetDisplayForWindow( this.sdl_window ).ToString( ));
				this.Window = new FNAWindow( this.sdl_window, fna_displayName );

				// Graphics device
				this.initialize_GraphicsDevice( );
				ServiceContainer services = new ServiceContainer( );
				this.graphicsDeviceService = new GraphicsDeviceService( this.GraphicsDevice );
				services.AddService( typeof( IGraphicsDeviceService ), this.graphicsDeviceService );

				// Content manager
				this.Content = new ContentManager( services );
				this.Content.RootDirectory = @"Content";

				// SDL
				SDL.SDL_SetWindowFocusable( this.sdl_window, true );
				SDL.SDL_ShowWindow( this.sdl_window );
				SDL.SDL_RaiseWindow( this.sdl_window );
				SDL.SDL_ShowCursor( );

				// Initialization
				this.IsInitialized = true;
				this.Initialize( );

				// Rendering
				this.StartRendering( );

			}
			catch ( Exception ex ) {
				throw new InvalidOperationException( "FNA Initialization failed: ", ex );
			}
		}
		//
		// FNA
		//
		// Has Control Focus
		//
		private bool hasFocus( )
		{
			try {
				if ( this.IsDisposed || !this.IsHandleCreated ) {
					return false;
				}

				Form parentForm = this.FindForm( );
				if ( parentForm == null || parentForm.IsDisposed ) {
					return false;
				}

				// Intentionally returns false when parent form is active to prevent SDL from stealing focus from WinForms controls.
				if ( Form.ActiveForm == parentForm ) {
					return false;
				}

				if ( this.sdl_window != IntPtr.Zero ) {
					IntPtr foreground = USER32.GetForegroundWindow( );

					if ( foreground == this.sdl_window ) {
						return true;
					}
				}

				if ( parentForm.Visible && Form.ActiveForm == null ) {
					return true;
				}

				return false;
			}
			catch {
				return true;
			}
		}

		#endregion

		#region SDL Input

		//
		// SDL Event Types
		//
		internal static class SDLEventTypes
		{
			// Focus
			public const uint SDL_EVENT_WINDOW_FOCUS_GAINED = 512;
			public const uint SDL_EVENT_WINDOW_FOCUS_LOST = 513;
			// Keyboard
			public const uint SDL_EVENT_KEY_DOWN = 768;
			public const uint SDL_EVENT_KEY_UP = 769;
			// Mouse
			public const uint SDL_EVENT_MOUSE_MOTION = 1024;
			public const uint SDL_EVENT_MOUSE_BUTTON_DOWN = 1025;
			public const uint SDL_EVENT_MOUSE_BUTTON_UP = 1026;
			public const uint SDL_EVENT_MOUSE_WHEEL = 1027;
			// Text
			public const uint SDL_EVENT_TEXT_INPUT = 770;
			public const uint SDL_EVENT_TEXT_EDITING = 771;
		}

		//
		// SDL Event
		//
		// Process SDL Event
		//
		private void process_SDLEvent( SDL.SDL_Event e ) {
			lock ( this.inputLock )
			{
				switch ( e.type )
				{
					case SDLEventTypes.SDL_EVENT_WINDOW_FOCUS_GAINED:
						break;

					case SDLEventTypes.SDL_EVENT_WINDOW_FOCUS_LOST:
						break;

					case SDLEventTypes.SDL_EVENT_KEY_DOWN:
						this.inputState.KeyboardPressedKeys.Add( this.convert_SDLKey( e.key.key ) );
						break;

					case SDLEventTypes.SDL_EVENT_KEY_UP:
						this.inputState.KeyboardPressedKeys.Remove( this.convert_SDLKey( e.key.key ) );
						break;

					case SDLEventTypes.SDL_EVENT_MOUSE_BUTTON_DOWN:
						if ( e.button.button == ( uint ) MouseButton.Left ) {
							// Set focus to SDL window
							IntPtr winHWND = this.getHandle_SDLWindow( this.sdl_window );
							USER32.SetFocus( winHWND );

							SDL.SDL_RaiseWindow( this.sdl_window );
							SDL.SDL_SetWindowFocusable( this.sdl_window, true );
						}

						this.inputState.MousePressedButtons.Add( ( MouseButton ) e.button.button );
						break;

					case SDLEventTypes.SDL_EVENT_MOUSE_BUTTON_UP:
						this.inputState.MousePressedButtons.Remove( ( MouseButton ) e.button.button );
						break;

					case SDLEventTypes.SDL_EVENT_MOUSE_MOTION:
						this.inputState.MouseX = ( int ) e.motion.x;
						this.inputState.MouseY = ( int ) e.motion.y;
						break;

					case SDLEventTypes.SDL_EVENT_MOUSE_WHEEL:
						float scrollDelta = (e.wheel.y * 120);
						this.inputState.ScrollWheelValue += ( int ) scrollDelta;
						break;
				}
			}
		}
		//
		// SDL Key
		//
		// Convert
		//
		private Keys convert_SDLKey( uint sdlKeyCode ) {
			Keys key;

			if ( FNAC_KEYMAP.TryGetValue( sdlKeyCode, out key ) ) {
				return key;
			}

			return Keys.None;
		}

		#endregion

		#region SDL Host

		//
		// SDL Window
		//
		// Create
		//
		private IntPtr create_SDLWindow( ) {
			// Initialize
			if ( !SDL.SDL_Init( SDL.SDL_InitFlags.SDL_INIT_VIDEO ) ) {
				throw new Exception( "SDL_Init failed: " + SDL.SDL_GetError( ) );
			}

			// Set flags
			SDL.SDL_WindowFlags initFlags = (
				SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN |
				SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
				SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS) |
				(SDL.SDL_WindowFlags) FNA3D.FNA3D_PrepareWindowAttributes();

			// Create window
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
		//
		// Get Handle
		//
		private IntPtr getHandle_SDLWindow( IntPtr sdlWindow ) {
			// Get window properties
			uint properties = SDL.SDL_GetWindowProperties( sdlWindow );

			// Get translated handle
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
		//
		// Embed
		//
		private void embed_SDLWindow( IntPtr windowsHandle ) {
			SDL.SDL_HideWindow( this.sdl_window );

			// TRUE MAGIC
			int style = USER32.GetWindowLong( windowsHandle, WinMessages.GWL_STYLE );
			style = ( ( style & ~( WinMessages.WS_OVERLAPPEDWINDOW ) ) | WinMessages.WS_CHILD );

			USER32.SetWindowLong( windowsHandle, WinMessages.GWL_STYLE, style );
			USER32.SetParent( windowsHandle, this.Handle );
			USER32.SetWindowPos( windowsHandle, IntPtr.Zero, 0, 0, this.Width, this.Height, 0x0040 );

			SDL.SDL_ShowWindow( this.sdl_window );

			this.Invalidate( );
		}

		#endregion

		#region Graphics Device

		//
		// Graphics Device Service
		//
		internal class GraphicsDeviceService : IGraphicsDeviceService {
			private bool _DISPOSED = false;
			private GraphicsDevice gDevice;

			public GraphicsDevice GraphicsDevice {
				get { return this.gDevice; }
			}

			public event EventHandler<EventArgs> DeviceCreated;
			public event EventHandler<EventArgs> DeviceDisposing;
			public event EventHandler<EventArgs> DeviceReset;
			public event EventHandler<EventArgs> DeviceResetting;


			public GraphicsDeviceService( GraphicsDevice graphicsDevice ) {
				if ( graphicsDevice == null ) {
					throw new ArgumentNullException( "graphicsDevice" );
				}

				this.gDevice = graphicsDevice;
			}

			// Event notifications
			public void NotifyDeviceDisposing( ) {
				this.OnDeviceDisposing( EventArgs.Empty );
			}
			public void NotifyDeviceReset( ) {
				this.OnDeviceReset( EventArgs.Empty );
			}
			public void NotifyDeviceResetting( ) {
				this.OnDeviceResetting( EventArgs.Empty );
			}

			// Events
			protected virtual void OnDeviceCreated( EventArgs e ) {
				if ( this.DeviceCreated != null ) {
					this.DeviceCreated.Invoke( this, e );
				}
			}
			protected virtual void OnDeviceDisposing( EventArgs e ) {
				if ( this.DeviceDisposing != null ) {
					this.DeviceDisposing.Invoke( this, e );
				}
			}
			protected virtual void OnDeviceReset( EventArgs e ) {
				if ( this.DeviceReset != null ) {
					this.DeviceReset.Invoke( this, e );
				}
			}
			protected virtual void OnDeviceResetting( EventArgs e ) {
				if ( this.DeviceResetting != null ) {
					this.DeviceResetting.Invoke( this, e );
				}
			}

			// Dispose
			protected virtual void Dispose( bool disposing ) {
				if ( !this._DISPOSED ) {
					if ( disposing ) {
						if ( this.gDevice != null ) {
							this.NotifyDeviceDisposing( );

							this.gDevice.Dispose( );
							this.gDevice = null;
						}
					}

					this._DISPOSED = true;
				}
			}
			public void Dispose( ) {
				this.Dispose( true );
				GC.SuppressFinalize( this );
			}
		}

		//
		// Graphics Device
		//
		// Initialize
		//
		private void initialize_GraphicsDevice( ) {
			PresentationParameters pParams = new PresentationParameters {
				DeviceWindowHandle = this.Window.Handle,
				BackBufferWidth = Math.Max( 1, this.Width ),
				BackBufferHeight = Math.Max( 1, this.Height ),
				IsFullScreen = false,								// Fullscreen Disabled
				// IMPORTANT: do not turn VSYNC off! UI Thread will be not able to process event pump.
				PresentationInterval = PresentInterval.One,			// VSYNC Enabled
				DepthStencilFormat = DepthFormat.Depth24Stencil8,
				MultiSampleCount = this.multiSampleCount
			};

			this.GraphicsDevice = new GraphicsDevice( GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pParams );
		}
		//
		// Graphics Device
		//
		// Resize
		//
		private void resize_GraphicsDevice( ) {
			if ( this.GraphicsDevice == null ) {
				return;
			}

			lock ( this.renderLock )
			{
				PresentationParameters pParams = this.GraphicsDevice.PresentationParameters;
				pParams.BackBufferWidth = Math.Max( 1, this.Width );
				pParams.BackBufferHeight = Math.Max( 1, this.Height );

				this.GraphicsDevice.Reset( pParams );
			}
		}
		//
		// Graphics Device
		//
		// Apply
		//
		private void apply_GraphicsDevice( Action<PresentationParameters> configure ) {
			if ( this.GraphicsDevice == null || this.graphicsDeviceService == null ) {
				return;
			}

			if ( configure == null ) {
				throw new ArgumentNullException( "configure" );
			}

			lock ( this.renderLock )
			{
				this.graphicsDeviceService.NotifyDeviceResetting( );

				PresentationParameters pParams = this.GraphicsDevice.PresentationParameters;
				configure( pParams );

				this.GraphicsDevice.Reset( pParams );
				this.graphicsDeviceService.NotifyDeviceReset( );
			}
		}

		#endregion

		#region Win32

		//
		// WndProc
		//
		protected override void WndProc( ref Message m )
		{
			if ( m.Msg == WinMessages.WM_MOUSEACTIVATE ) {
				if ( this.sdl_window != IntPtr.Zero ) {
					try {
						// Get sdl handle
						IntPtr hwnd = this.getHandle_SDLWindow( this.sdl_window );

						// Set focus to sdl window
						if ( hwnd != IntPtr.Zero )
						{
							USER32.SetFocus( hwnd );
							SDL.SDL_RaiseWindow( this.sdl_window );
						}
					} catch { }
				}

				m.Result = ( IntPtr ) 1;        // MA_ACIVATE
				return;
			}

			base.WndProc( ref m );
		}

		//
		// USER32 API
		// MSG
		//
		internal static class USER32
		{
			[DllImport( "user32.dll" )]
			public static extern bool PeekMessage( out MSG msg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg );
			[DllImport("user32.dll")]
			public static extern IntPtr SetParent( IntPtr hWndChild, IntPtr hWndNewParent );
			[DllImport("user32.dll")]
			public static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags );
			[DllImport("user32.dll")]
			public static extern int SetWindowLong( IntPtr hWnd, int nIndex, int dwNewLong );
			[DllImport("user32.dll")]
			public static extern int GetWindowLong( IntPtr hWnd, int nIndex );
			[DllImport("user32.dll")]
			public static extern IntPtr SetFocus( IntPtr hWnd );
			[DllImport( "user32.dll" )]
			public static extern IntPtr GetForegroundWindow( );
			[DllImport( "user32.dll" )]
			public static extern IntPtr GetParent( IntPtr hWnd );
			[DllImport( "user32.dll" )]
			public static extern uint GetWindowThreadProcessId( IntPtr hWnd, out uint processId );
		}
		[StructLayout( LayoutKind.Sequential )]
		internal struct MSG
		{
			public IntPtr hwnd;
			public uint message;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public int pt_x;
			public int pt_y;
		}

		//
		// Win Messages
		//
		internal static class WinMessages
		{
			// Focus & Activation
			public const int WM_MOUSEACTIVATE =					0x0021;
			public const int WM_SETFOCUS =						0x0007;
			public const int WM_KILLFOCUS =						0x0008;
			public const int WM_ACTIVATE =						0x0006;
			// Painting
			public const int WM_PAINT =							0x000F;
			public const int WM_ERASEBKGND =					0x0014;
			// Input
			public const int WM_KEYDOWN =						0x0100;
			public const int WM_KEYUP =							0x0101;
			public const int WM_CHAR =							0x0102;
			public const int WM_MOUSEMOVE =						0x0200;
			public const int WM_LBUTTONDOWN =					0x0201;
			public const int WM_LBUTTONUP =						0x0202;
			// Window styles
			public const int GWL_STYLE =						-16;
			public const int WS_CHILD =							0x40000000;
			public const int WS_OVERLAPPEDWINDOW =				0x00CF0000;
		}

		#endregion
	}
}
