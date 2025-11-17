/* 
 * FNA
 * =====================================================================
 * FileName: FNA.cs
 * ---------------------------------------------------------------------
 * This document is distributed under General Public License.
 * Copyright © David Kutnar 2025 - All rights reserved.
 * =====================================================================
 * Description: FNA WinForms sample with simple rotating 3D cube
 * =====================================================================
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

class FNA : FNAControl
{
    private BasicEffect effect;
    private VertexBuffer vertexBuffer;
    private IndexBuffer indexBuffer;

    private readonly VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[24];
    private readonly short[] indices = new short[36];
    private float rotation = 0f;

    protected override void Initialize()
    {
        this.Content.RootDirectory = @"Content";
    
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        effect = new BasicEffect(GraphicsDevice);
        effect.VertexColorEnabled = false;
        effect.LightingEnabled = true;
        effect.PreferPerPixelLighting = true;
        effect.Projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100.0f);
        effect.View = Matrix.CreateLookAt( new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
        effect.World = Matrix.Identity;

        effect.DirectionalLight0.Enabled = true;
        effect.DirectionalLight0.Direction = new Vector3(1, -1, -1);
        effect.DirectionalLight0.DiffuseColor = Color.White.ToVector3();
        effect.DirectionalLight0.SpecularColor = Color.White.ToVector3();
        effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
        effect.DiffuseColor = new Vector3(0.8f, 0.5f, 0.3f);
        effect.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
        effect.SpecularPower = 32f;

        this.createCube();

        vertexBuffer = new VertexBuffer(
            GraphicsDevice,
            VertexPositionNormalTexture.VertexDeclaration,
            vertices.Length,
            BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices);

        indexBuffer = new IndexBuffer(
            GraphicsDevice,
            IndexElementSize.SixteenBits,
            indices.Length,
            BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);
    }

    protected override void Update(float elapsedTime)
    {
        MouseState mState = this.Input.GetMouseState( );
        KeyboardState kState = this.Input.GetKeyboardState( );
    
        rotation += elapsedTime * MathHelper.PiOver2;
        effect.World = Matrix.CreateRotationY(rotation) * Matrix.CreateRotationX(rotation * 0.7f);
    }
    protected override void Draw()
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.SetVertexBuffer(vertexBuffer);
        GraphicsDevice.Indices = indexBuffer;

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length,0, indices.Length / 3);
        }
    }

    void createCube()
    {
        float size = 1.0f;
        int vertexIndex = 0;
        int indexIndex = 0;

        // Funkce pro vytvoření strany krychle s normálami
        void CreateFace(Vector3 normal, Vector3[] corners, Color color)
        {
            // Přidej 4 vrcholy pro tuto stranu
            for (int i = 0; i < 4; i++)
            {
                vertices[vertexIndex + i] = new VertexPositionNormalTexture(
                    corners[i],
                    normal,
                    Vector2.Zero);
            }

            // Přidej 2 trojúhelníky (6 indexů)
            indices[indexIndex++] = (short)(vertexIndex + 0);
            indices[indexIndex++] = (short)(vertexIndex + 1);
            indices[indexIndex++] = (short)(vertexIndex + 2);

            indices[indexIndex++] = (short)(vertexIndex + 0);
            indices[indexIndex++] = (short)(vertexIndex + 2);
            indices[indexIndex++] = (short)(vertexIndex + 3);

            vertexIndex += 4;
        }

        // Přední strana (Z+)
        CreateFace(Vector3.UnitZ, new Vector3[] {
            new Vector3(-size, -size, size),
            new Vector3(size, -size, size),
            new Vector3(size, size, size),
            new Vector3(-size, size, size)
        }, Color.Red);

        // Zadní strana (Z-)
        CreateFace(-Vector3.UnitZ, new Vector3[] {
            new Vector3(size, -size, -size),
            new Vector3(-size, -size, -size),
            new Vector3(-size, size, -size),
            new Vector3(size, size, -size)
        }, Color.Green);

        // Horní strana (Y+)
        CreateFace(Vector3.UnitY, new Vector3[] {
            new Vector3(-size, size, -size),
            new Vector3(-size, size, size),
            new Vector3(size, size, size),
            new Vector3(size, size, -size)
        }, Color.Blue);

        // Spodní strana (Y-)
        CreateFace(-Vector3.UnitY, new Vector3[] {
            new Vector3(-size, -size, size),
            new Vector3(-size, -size, -size),
            new Vector3(size, -size, -size),
            new Vector3(size, -size, size)
        }, Color.Yellow);

        // Levá strana (X-)
        CreateFace(-Vector3.UnitX, new Vector3[] {
            new Vector3(-size, -size, -size),
            new Vector3(-size, -size, size),
            new Vector3(-size, size, size),
            new Vector3(-size, size, -size)
        }, Color.Cyan);

        // Pravá strana (X+)
        CreateFace(Vector3.UnitX, new Vector3[] {
            new Vector3(size, -size, size),
            new Vector3(size, -size, -size),
            new Vector3(size, size, -size),
            new Vector3(size, size, size)
        }, Color.Magenta);
    }

}
