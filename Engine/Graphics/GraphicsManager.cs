using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoidEngine.Graphics;

/// <summary>
/// Methods, resources and information in regards drawing and rendering.
/// </summary>
public static class GraphicsManager
{
    // Device.
    private static GraphicsDeviceManager _deviceManager;
    private static GraphicsDevice _device;

    // Graphics resources.
    private static SpriteBatch _spriteBatch;
    private static RenderTarget2D _renderTarget;
    private static Texture2D _pixel;

    // Matrix and space.
    private static Viewport _nativeViewport;

    /// <summary>
    /// Gets or sets the current viewport.
    /// </summary>
    /// <remarks>Call <see cref="ResetViewport"/> to restore the original viewport.</remarks>
    public static Viewport Viewport
    {
        get
        {
            return _device.Viewport;
        }
        set
        {
            _device.Viewport = value;
        }
    }

    /// <summary>
    /// Graphics device manager.
    /// </summary>
    public static GraphicsDeviceManager DeviceManager => _deviceManager;
    /// <summary>
    /// Graphics device.
    /// </summary>
    public static GraphicsDevice Device => _device;
    /// <summary>
    /// Active sprite batch.
    /// </summary>
    public static SpriteBatch SpriteBatch => _spriteBatch;
    /// <summary>
    /// Game's main render target.
    /// </summary>
    public static RenderTarget2D RenderTarget => _renderTarget;

    /// <summary>
    /// Initializes the Graphics Manager. Must be executed during start-up.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the device manager has already been created.</exception>
    public static void Initialize(Game game)
    {
        if (_deviceManager != null)
        {
            throw new InvalidOperationException("Graphics device manager has already been created.");
        }

        _deviceManager = new GraphicsDeviceManager(game);

        _deviceManager.DeviceCreated += OnDeviceCreated;
        _deviceManager.DeviceDisposing += OnDeviceDisposing;

        DisplayManager.Initialize(game);

        _nativeViewport = new(0, 0, DisplayManager.NATIVE_RESOLUTION_X, DisplayManager.NATIVE_RESOLUTION_Y);
    }

    /// <summary>
    /// Draws the game. Must be executed every frame.
    /// </summary>
    public static void DrawGame()
    {
        // Draw game into a render target.
        _device.SetRenderTarget(_renderTarget);
        Begin();
        Draw.Line(Vector2.Zero, new Vector2(DisplayManager.NATIVE_RESOLUTION_X, DisplayManager.NATIVE_RESOLUTION_Y), Color.Red, 5f);
        Draw.Line(new Vector2(DisplayManager.NATIVE_RESOLUTION_X, 0f), new Vector2(0f, DisplayManager.NATIVE_RESOLUTION_Y), Color.Red, 5f);
        Draw.Line(Vector2.Zero, new Vector2(DisplayManager.NATIVE_RESOLUTION_X, 0f), Color.Blue, 5f);
        Draw.Line(new Vector2(DisplayManager.NATIVE_RESOLUTION_X, 0f), new Vector2(DisplayManager.NATIVE_RESOLUTION_X, DisplayManager.NATIVE_RESOLUTION_Y), Color.Blue, 5f);
        Draw.Line(new Vector2(DisplayManager.NATIVE_RESOLUTION_X, DisplayManager.NATIVE_RESOLUTION_Y), new Vector2(0f, DisplayManager.NATIVE_RESOLUTION_Y), Color.Blue, 5f);
        Draw.Line(new Vector2(0f, DisplayManager.NATIVE_RESOLUTION_Y), new Vector2(0f, 0f), Color.Blue, 5f);
        End();
        _device.SetRenderTarget(null);

        // Draw the render target to the destination rectangle.
        // This keeps the window size independent from the
        // game's native resolution.
        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _device.Clear(Color.CornflowerBlue);
        _spriteBatch.Draw(_renderTarget, DisplayManager.DestinationRectangle, Color.White);
        _spriteBatch.End();
    }

    /// <summary>
    /// Begins the active sprite batch.
    /// </summary>
    /// <remarks>
    /// Call this before making any draw calls.
    /// </remarks>
    public static void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred,
                             BlendState blendState = null,
                             SamplerState samplerState = null,
                             DepthStencilState depthStencilState = null,
                             RasterizerState rasterizerState = null,
                             Effect effect = null,
                             Matrix? transformationMatrix = null)
    {
        _spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformationMatrix);
    }

    /// <summary>
    /// Begins the active sprite batch.
    /// </summary>
    /// <remarks>
    /// Call this before making any draw calls.
    /// </remarks>
    public static void Begin(Effect effect, Matrix? transformationMatrix = null)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect, transformationMatrix);
    }

    /// <summary>
    /// Ends the active sprite batch, and presents it to the screen.
    /// </summary>
    /// <remarks>Presents all draw calls made since the last call to <c>Begin()</c>.</remarks>
    public static void End()
    {
        _spriteBatch.End();
    }
    
    /// <summary>
    /// Call this after changing the viewport.<br/>
    /// Restores the original viewport.
    /// </summary>
    public static void ResetViewport()
    {
        _device.Viewport = _nativeViewport;
    }

    #region EVENT HANDLERS

    private static void OnDeviceCreated(object sender, EventArgs e)
    {
        _device = _deviceManager.GraphicsDevice;

        // Create graphics resources using the device created.
        _renderTarget = new(_deviceManager.GraphicsDevice, DisplayManager.NATIVE_RESOLUTION_X, DisplayManager.NATIVE_RESOLUTION_Y);
        _spriteBatch = new SpriteBatch(_device);

        // Create a single white pixel texture,
        // used for drawing lines and shapes.
        _pixel = new(_device, 1, 1);
        _pixel.SetData(new byte[] { 255, 255, 255, 255 });
    }

    private static void OnDeviceDisposing(object sender, EventArgs e)
    {
        // Dispose graphics resources created using the previous device.
        _renderTarget?.Dispose();
        _spriteBatch?.Dispose();
        _pixel?.Dispose();
    }
    #endregion

    public static class Draw
    {
        /// <summary>
        /// Determines where the border will be aligned along a shape's boundaries.
        /// </summary>
        public enum BorderAlign
        {
            Inside,
            Center,
            Outside
        }

        /// <summary>
        /// Draws text to the active sprite batch.
        /// </summary>
        /// <param name="font">Font used to draw the text.</param>
        /// <param name="text">String that will be drawn.</param>
        /// <param name="position">Position where the text will be drawn.</param>
        /// <param name="color">Color of the text.</param>
        /// <param name="radians">Rotation at which the text will be drawn, measured in radians.</param>
        /// <param name="origin">Normalized origin of the drawn text.<br/>
        /// {0, 0} represents the top-left corner, and {1, 1} the bottom-right corner.<br/>
        /// Use {0.5, 0.5} to draw the text centered.</param>
        /// <param name="scale">Scale at which the text will be drawn.</param>
        /// <param name="depth">Depth at which the text will be drawn.</param>
        public static void Text(SpriteFont font, string text, Vector2 position, Color color, float radians = 0f, Vector2 origin = default, float scale = 1f, float depth = 0f)
        {
            _spriteBatch.DrawString(font, text, position, color, radians, origin, scale, SpriteEffects.None, depth);
        }

        /// <summary>
        /// Draws a line from a starting position to an ending position.
        /// </summary>
        /// <param name="start">Starting position of the line.</param>
        /// <param name="end">Ending position of the line.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="thickness">Thickness of the line. Must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="thickness"/> is negative.</exception>
        public static void Line(Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            if (thickness < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), "Thickness must be positive.");
            }

            float radians = start.AngleRadiansTo(end);
            float size = Vector2.Distance(start, end);
            Vector2 scale = new(size, thickness);

            _spriteBatch.Draw(_pixel, start, null, color, radians, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a filled rectangle of the specified size at the specified position.
        /// </summary>
        /// <param name="position">Top-left position where the rectangle will be drawn.</param>
        /// <param name="size">Size of the rectangle. Must be positive.</param>
        /// <param name="color">Fill color of the rectangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if size is negative.</exception>
        public static void RectangleFilled(Vector2 position, Vector2 size, Color color)
        {
            if (size.X * size.Y < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");
            }

            _spriteBatch.Draw(_pixel, position, null, color, 0f, Vector2.Zero, size, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a non-filled rectangle of the specified size at the specified position.
        /// </summary>
        /// <param name="position">Top-left position where the rectangle will be drawn.</param>
        /// <param name="size">Size of the rectangle. Must be positive.</param>
        /// <param name="color">Border color of the rectangle.</param>
        /// <param name="thickness">Thickness of the rectangle's border.</param>
        /// <param name="borderAlign">Determines where the border will be aligned along the rectangle.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if size or thickness are negative.</exception>
        public static void Rectangle(Vector2 position, Vector2 size, Color color, float thickness = 1f, BorderAlign borderAlign = BorderAlign.Inside)
        {
            if (size.X * size.Y < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");
            }

            if (thickness < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), "Thickness must be positive.");
            }

            switch (borderAlign)
            {
                default:
                case BorderAlign.Inside:
                    break;
                case BorderAlign.Outside:
                    position.ApplyOffset(-thickness);
                    size.ApplyOffset(thickness * 2f);
                    break;
                case BorderAlign.Center:
                    position.ApplyOffset(-thickness * 0.5f);
                    size.ApplyOffset(thickness);
                    break;
            }

            Vector2 topLeft = position;
            Vector2 topRight = new(position.X + size.X, position.Y);
            Vector2 bottomLeft = new(position.X, position.Y + size.Y);
            Vector2 bottomRight = position + size;

            Line(topLeft.OffsetX(thickness), topRight, color, thickness);
            Line(topRight.OffsetY(thickness), bottomRight, color, thickness);
            Line(bottomRight.OffsetX(-thickness), bottomLeft, color, thickness);
            Line(bottomLeft.OffsetY(-thickness), topLeft, color, thickness);
        }
    }
}
