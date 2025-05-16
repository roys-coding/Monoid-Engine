using Microsoft.Xna.Framework.Graphics;
using System;

namespace MyMonoGameApp;

/// <summary>
/// Available window modes.
/// </summary>
public enum WindowMode
{
    /// <summary>
    /// Normal windowed mode.
    /// </summary>
    Windowed = 0,
    /// <summary>
    /// Dedicated full-screen mode.
    /// </summary>
    Fullscreen = 2,
    /// <summary>
    /// Non-dedicated full-screen mode.
    /// </summary>
    FullscreenBorderless = 3
}

public static class Graphics
{
    const int NATIVE_RESOLUTION_X = 1280;
    const int NATIVE_RESOLUTION_Y = 720;

    const float NATIVE_ASPECT_RATIO = (float)NATIVE_RESOLUTION_Y / NATIVE_RESOLUTION_X;

    // Device.
    private static GraphicsDeviceManager _deviceManager;
    private static GraphicsDevice _device;

    // Graphics resources.
    private static SpriteBatch _spriteBatch;
    private static RenderTarget2D _renderTarget;
    private static Texture2D _pixel;

    // Matrix and space.
    private static Viewport _nativeViewport;
    private static Rectangle _destinationRectangle;
    private static Matrix _destinationMatrix;
    private static Matrix _destinationMatrixInverse;

    // View mode.
    private static Point _windowedResolution = new(NATIVE_RESOLUTION_X, NATIVE_RESOLUTION_Y);
    private static Point _fullScreenResolution;
    private static WindowMode _activeWindowMode = WindowMode.Windowed;

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
    /// Active sprite batch.
    /// </summary>
    public static SpriteBatch SpriteBatch => _spriteBatch;
    /// <summary>
    /// Game's main render target.
    /// </summary>
    public static RenderTarget2D RenderTarget => _renderTarget;
    /// <summary>
    /// Represents the zone between the black bars when the window is resized to different aspect ratios, and where the game is rendered.
    /// </summary>
    public static Rectangle GameBounds => _destinationRectangle;
    /// <summary>
    /// A matrix that maps from <see cref="GameBounds"/> coordinates to window coordinates.
    /// </summary>
    public static Matrix GameBoundsMatrix => _destinationMatrix;
    /// <summary>
    /// A matrix that maps window coordinates to <see cref="GameBounds"/> coordinates.
    /// </summary>
    public static Matrix GameBoundsMatrixInverse => _destinationMatrixInverse;
    /// <summary>
    /// Gets the active window mode.
    /// </summary>
    /// <remarks>Use <see cref="SetWindowMode(WindowMode)"/> to change the window mode.</remarks>
    public static WindowMode ActiveWindowMode => _activeWindowMode;

    public static void Initialize(Game game)
    {
        if (_deviceManager != null)
        {
            throw new InvalidOperationException("Graphics device manager has already been created.");
        }

        _deviceManager = new GraphicsDeviceManager(game);

        _fullScreenResolution = new()
        {
            X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
        };

        game.Window.ClientSizeChanged += OnClientSizeChanged;
        game.Window.OrientationChanged += OnClientSizeChanged;
        game.Window.AllowUserResizing = true;

        _deviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft
                                             | DisplayOrientation.LandscapeRight;

        _deviceManager.PreferredBackBufferWidth = _windowedResolution.X;
        _deviceManager.PreferredBackBufferHeight = _windowedResolution.Y;

        _deviceManager.DeviceCreated += OnDeviceCreated;
        _deviceManager.DeviceDisposing += OnDeviceDisposing;

        _nativeViewport = new(0, 0, NATIVE_RESOLUTION_X, NATIVE_RESOLUTION_Y);
    }

    public static void DrawGame()
    {
        // Draw game into a render target.
        _device.SetRenderTarget(_renderTarget);
        SceneManager.Draw();
        _device.SetRenderTarget(null);

        // Draw the render target to the destination rectangle.
        // This keeps the window size independent from the
        // game's native resolution.
        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _device.Clear(Color.Black);
        _spriteBatch.Draw(_renderTarget, _destinationRectangle, Color.White);
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

    public static void SetWindowMode(WindowMode mode)
    {
        if (_activeWindowMode == mode) return;

        switch (mode)
        {
            case WindowMode.Windowed:
                GoWindowed();
                break;
            case WindowMode.Fullscreen:
                GoFullscreen(false);
                break;
            case WindowMode.FullscreenBorderless:
                GoFullscreen(true);
                break;
            default:
                throw new NotImplementedException($"Window mode '{mode}' not implemented.");
        }

        _activeWindowMode = mode;
    }
    
    /// <summary>
    /// Call this after changing the viewport.<br/>
    /// Restores the original viewport.
    /// </summary>
    public static void ResetViewport()
    {
        _device.Viewport = _nativeViewport;
    }

    /// <summary>
    /// Transforms coordinates from <see cref="GameBounds"/> to the game window.
    /// </summary>
    public static Vector2 TransformVector(Vector2 vector)
    {
        return Vector2.Transform(vector, _destinationMatrix);
    }

    /// <summary>
    /// Transforms coordinates from the window to the <see cref="GameBounds"/>.
    /// </summary>
    public static Vector2 TransformVectorInverse(Vector2 vector)
    {
        return Vector2.Transform(vector, _destinationMatrixInverse);
    }

    /// <summary>
    /// Transforms coordinates from <see cref="GameBounds"/> to the game window.
    /// </summary>
    public static void TransformVector(ref Vector2 vector)
    {
        vector = Vector2.Transform(vector, _destinationMatrix);
    }

    /// <summary>
    /// Transforms coordinates from the window to the <see cref="GameBounds"/>.
    /// </summary>
    public static void TransformVectorInverse(ref Vector2 vector)
    {
        vector = Vector2.Transform(vector, _destinationMatrixInverse);
    }

    private static void GoWindowed()
    {
        _deviceManager.IsFullScreen = false;
        _deviceManager.HardwareModeSwitch = false;
        _deviceManager.PreferredBackBufferWidth = _windowedResolution.X;
        _deviceManager.PreferredBackBufferHeight = _windowedResolution.Y;

        _deviceManager.ApplyChanges();
    }

    private static void GoFullscreen(bool borderless)
    {
        _deviceManager.IsFullScreen = true;
        _deviceManager.HardwareModeSwitch = !borderless;
        _deviceManager.PreferredBackBufferWidth = _fullScreenResolution.X;
        _deviceManager.PreferredBackBufferHeight = _fullScreenResolution.Y;

        _deviceManager.ApplyChanges();
    }

    private static void CalculateDestinationRectangle()
    {
         Point viewportSize = _device.Viewport.Bounds.Size;

        // Skip calculating the destination rectangle if width or height is zero.
        if (viewportSize.X == 0 || viewportSize.Y == 0)
        {
            _destinationRectangle.Location = new Point(0, 0);
            _destinationRectangle.Size = new Point(1, 1);
            return;
        }

        // Resize the destination rectangle to fit inside the game window,
        // maintaining the correct aspect ratio and staying centered.
        // This causes black bars to appear, but this is preferred over stretching.
        _destinationRectangle.Width = viewportSize.X;
        _destinationRectangle.Height = viewportSize.Y;
        _destinationRectangle.X = 0;
        _destinationRectangle.Y = 0;

        float aspect = (float)_destinationRectangle.Height / _destinationRectangle.Width;

        if (aspect > NATIVE_ASPECT_RATIO)
        {
            _destinationRectangle.Height = (int)(_destinationRectangle.Width * NATIVE_ASPECT_RATIO);
            _destinationRectangle.Y = (int)(viewportSize.Y * 0.5f - _destinationRectangle.Height * 0.5f);
        }
        else if (aspect < NATIVE_ASPECT_RATIO)
        {
            _destinationRectangle.Width = (int)(_destinationRectangle.Height / NATIVE_ASPECT_RATIO);
            _destinationRectangle.X = (int)(viewportSize.X * 0.5f - _destinationRectangle.Width * 0.5f);
        }

        float scaleX = (float)_destinationRectangle.Width / NATIVE_RESOLUTION_X;
        float scaleY = (float)_destinationRectangle.Height / NATIVE_RESOLUTION_Y;

        // Recalculate transformation matrices.
        _destinationMatrix = new()
        {
            M11 = scaleX, // Scale in X and rotate
            M22 = scaleY,   // Scale in Y and rotate
            M33 = 1f,
            M41 = _destinationRectangle.X, // Translate
            M42 = _destinationRectangle.Y, // Translate
            M44 = 1f
        };
        _destinationMatrixInverse = Matrix.Invert(_destinationMatrix);
    }

    #region EVENT HANDLERS
    private static void OnClientSizeChanged(object sender, EventArgs e)
    {
        CalculateDestinationRectangle();
    }

    private static void OnDeviceCreated(object sender, EventArgs e)
    {
        _device = _deviceManager.GraphicsDevice;

        // Create graphics resources using the device created.
        _renderTarget = new(_deviceManager.GraphicsDevice, NATIVE_RESOLUTION_X, NATIVE_RESOLUTION_Y);
        _spriteBatch = new SpriteBatch(_device);

        CalculateDestinationRectangle();

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
        /// Draws a sprite to the active sprite batch.
        /// </summary>
        /// <param name="sprite">Sprite that will be rendered.</param>
        public static void Sprite(Sprite sprite)
        {
            sprite.Draw(_spriteBatch);
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
