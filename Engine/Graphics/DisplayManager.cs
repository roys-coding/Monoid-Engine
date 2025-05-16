using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine.Graphics;

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

public enum ScalingMode
{
    ScaleToFit,
    Stretch,
    Crop,
    Integer
}

/// <summary>
/// Handles resolution, scaling and window modes.
/// </summary>
public static class DisplayManager
{
    public const int NATIVE_RESOLUTION_X = 1280;
    public const int NATIVE_RESOLUTION_Y = 720;
    public const float NATIVE_ASPECT_RATIO = (float)NATIVE_RESOLUTION_Y / NATIVE_RESOLUTION_X;

    /// <summary>
    /// Gets the active window mode.
    /// </summary>
    /// <remarks>Use <see cref="SetWindowMode(WindowMode)"/> to change the window mode.</remarks>
    public static WindowMode ActiveWindowMode => _activeWindowMode;
    public static ScalingMode ActiveScalingMode
    {
        set
        {
            _activeScalingMode = value;
            CalculateViewportSize();
        }
        get => _activeScalingMode;
    }

    // View mode.
    private static Point _windowedResolution = new(NATIVE_RESOLUTION_X, NATIVE_RESOLUTION_Y);
    private static Point _fullScreenResolution;
    private static WindowMode _activeWindowMode = WindowMode.Windowed;
    private static ScalingMode _activeScalingMode = ScalingMode.ScaleToFit;

    // Destination boundaries.
    private static Rectangle _destinationRectangle;
    private static Matrix _destinationMatrix;
    private static Matrix _destinationMatrixInverse;

    /// <summary>
    /// Represents the zone between the black bars when the window is resized to different aspect ratios, and where the game is rendered.
    /// </summary>
    public static Rectangle DestinationRectangle => _destinationRectangle;
    /// <summary>
    /// A matrix that maps from <see cref="DestinationRectangle"/> coordinates to window coordinates.
    /// </summary>
    public static Matrix DestinationRectangleMatrix => _destinationMatrix;
    /// <summary>
    /// A matrix that maps window coordinates to <see cref="DestinationRectangle"/> coordinates.
    /// </summary>
    public static Matrix DestinationRectangleMatrixInverse => _destinationMatrixInverse;
    /// <summary>
    /// The resolution of the game in windowed mode.
    /// </summary>
    public static Point WindowedResolution => _windowedResolution;
    /// <summary>
    /// The resolution of the game in full screen mode.
    /// </summary>
    public static Point FullscreenResolution => _fullScreenResolution;

    /// <summary>
    /// Initializes the Display Manager. Must be called during start-up.
    /// </summary>
    /// <param name="game"></param>
    public static void Initialize(Game game)
    { 
        _fullScreenResolution = new()
        {
            X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
        };

        game.Window.ClientSizeChanged += OnClientSizeChanged;
        game.Window.OrientationChanged += OnClientSizeChanged;
        game.Window.AllowUserResizing = true;

        GraphicsManager.DeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft
                                             | DisplayOrientation.LandscapeRight;

        GraphicsManager.DeviceManager.PreferredBackBufferWidth = WindowedResolution.X;
        GraphicsManager.DeviceManager.PreferredBackBufferHeight = WindowedResolution.Y;

        GraphicsManager.DeviceManager.DeviceCreated += OnDeviceCreated;
        GraphicsManager.DeviceManager.DeviceReset += OnDeviceReset;
    }

    /// <summary>
    /// Changes the current window mode.
    /// </summary>
    /// <param name="mode">Mode to change to.</param>
    /// <exception cref="NotImplementedException">Thrown if the specified window mode does not exist.</exception>
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

        CalculateViewportSize();
        _activeWindowMode = mode;
    }

    /// <summary>
    /// Transforms coordinates from <see cref="DestinationRectangle"/> to the game window.
    /// </summary>
    public static Vector2 TransformVector(Vector2 vector)
    {
        return Vector2.Transform(vector, DestinationRectangleMatrix);
    }

    /// <summary>
    /// Transforms coordinates from the window to the <see cref="DestinationRectangle"/>.
    /// </summary>
    public static Vector2 TransformVectorInverse(Vector2 vector)
    {
        return Vector2.Transform(vector, DestinationRectangleMatrixInverse);
    }

    /// <summary>
    /// Transforms coordinates from <see cref="DestinationRectangle"/> to the game window.
    /// </summary>
    public static void TransformVector(ref Vector2 vector)
    {
        vector = Vector2.Transform(vector, DestinationRectangleMatrix);
    }

    /// <summary>
    /// Transforms coordinates from the window to the <see cref="DestinationRectangle"/>.
    /// </summary>
    public static void TransformVectorInverse(ref Vector2 vector)
    {
        vector = Vector2.Transform(vector, DestinationRectangleMatrixInverse);
    }

    private static void GoWindowed()
    {
        GraphicsManager.DeviceManager.IsFullScreen = false;
        GraphicsManager.DeviceManager.HardwareModeSwitch = false;
        GraphicsManager.DeviceManager.PreferredBackBufferWidth = _windowedResolution.X;
        GraphicsManager.DeviceManager.PreferredBackBufferHeight = _windowedResolution.Y;

        GraphicsManager.DeviceManager.ApplyChanges();
    }

    private static void GoFullscreen(bool borderless)
    {
        GraphicsManager.DeviceManager.IsFullScreen = true;
        GraphicsManager.DeviceManager.HardwareModeSwitch = !borderless;
        GraphicsManager.DeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        GraphicsManager.DeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        GraphicsManager.DeviceManager.ApplyChanges();
    }

    private static void OnClientSizeChanged(object sender, EventArgs e)
    {
        CalculateViewportSize();
    }

    private static void OnDeviceCreated(object sender, EventArgs e)
    {
        CalculateViewportSize();
    }

    private static void OnDeviceReset(object sender, EventArgs e)
    {
        CalculateViewportSize();
    }

    private static void CalculateViewportSize()
    {
        Point viewportSize = GraphicsManager.Viewport.Bounds.Size;

        // Skip calculating the destination rectangle if width or height is zero.
        if (viewportSize.X == 0 || viewportSize.Y == 0)
        {
            _destinationRectangle.Location = new Point(0, 0);
            _destinationRectangle.Size = new Point(1, 1);
            return;
        }

        switch (_activeScalingMode)
        {
            case ScalingMode.ScaleToFit:
                CalculateScalingScaleToFit();
                break;
            case ScalingMode.Stretch:
                CalculateScalingStretch();
                break;
            case ScalingMode.Crop:
                CalculateScalingCrop();
                break;
            case ScalingMode.Integer:
                break;
            default:
                throw new NotImplementedException("Scaling mode not implemented.");
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

    private static void CalculateScalingScaleToFit()
    {
        Point viewportSize = GraphicsManager.Viewport.Bounds.Size;
        float viewportAspect = (float)viewportSize.Y / viewportSize.X;

        if (viewportAspect > NATIVE_ASPECT_RATIO) // Taller screen, letterbox.
        {
            _destinationRectangle.Width = viewportSize.X;
            _destinationRectangle.Height = (int)(viewportSize.X * NATIVE_ASPECT_RATIO);
            _destinationRectangle.X = 0;
            _destinationRectangle.Y = (int)((viewportSize.Y - _destinationRectangle.Height) * 0.5f);
        }
        else if (viewportAspect < NATIVE_ASPECT_RATIO) // Wider screen, pillar box.
        {
            _destinationRectangle.Width = (int)(viewportSize.Y / NATIVE_ASPECT_RATIO);
            _destinationRectangle.Height = viewportSize.Y;
            _destinationRectangle.X = (int)((viewportSize.X - _destinationRectangle.Width) * 0.5f);
            _destinationRectangle.Y = 0;
        }
    }

    private static void CalculateScalingStretch()
    {
        Point viewportSize = GraphicsManager.Viewport.Bounds.Size;
        _destinationRectangle.Width = viewportSize.X;
        _destinationRectangle.Height = viewportSize.Y;
        _destinationRectangle.X = 0;
        _destinationRectangle.Y = 0;
    }

    private static void CalculateScalingCrop()
    {
        Point viewportSize = GraphicsManager.Viewport.Bounds.Size;
        float viewportAspect = (float)viewportSize.Y / viewportSize.X;

        _destinationRectangle.X = 0;
        _destinationRectangle.Y = 0;

        if (viewportAspect > NATIVE_ASPECT_RATIO) // Taller screen
        {
            _destinationRectangle.Width = (int)(viewportSize.Y / NATIVE_ASPECT_RATIO);
            _destinationRectangle.Height = viewportSize.Y;
            _destinationRectangle.X = (int)((viewportSize.X - _destinationRectangle.Width) * 0.5f);
        }
        else if (viewportAspect < NATIVE_ASPECT_RATIO) // Wider screen        
        {
            _destinationRectangle.Width = viewportSize.X;
            _destinationRectangle.Height = (int)(viewportSize.X * NATIVE_ASPECT_RATIO);
            _destinationRectangle.Y = (int)((viewportSize.Y - _destinationRectangle.Height) * 0.5f);
        }
    }
}
