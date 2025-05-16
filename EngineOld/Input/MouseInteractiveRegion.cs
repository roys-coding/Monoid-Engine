using FMOD;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

public class MouseInteractiveRegion
{
    [Flags]
    public enum MouseRegionOptions
    {
        None = 0,
        /// <summary>
        /// If the region is pressed, and the mouse leaves it's boundaries, the region will be released.
        /// </summary>
        /// <remarks>If disabled, the region will stay pressed even if the mouse leaves it's boundaries.</remarks>
        ReleaseOnLeave = 1 << 1,
        /// <summary>
        /// If the mouse enters the region while any mouse button is held down, the region will be pressed.
        /// </summary>
        AllowPressOnEnter = 1 << 2,
    }

    /// <summary>
    /// Invoked when any mouse button is pressed while inside this region's boundaries.
    /// </summary>
    public event EventHandler OnPressed;
    /// <summary>
    /// Invoked when any mouse button is released while inside this region's boundaries, only if the region was previously pressed.
    /// </summary>
    public event EventHandler OnReleased;
    /// <summary>
    /// Invoked when the mouse cursor enters this region's boundaries.
    /// </summary>
    public event EventHandler OnMouseEnter;
    /// <summary>
    /// Invoked when the mouse cursor leaves this region's boundaries.
    /// </summary>
    public event EventHandler OnMouseLeave;

    /// <summary>
    /// Options that define how this region will behave.
    /// </summary>
    public MouseRegionOptions Options = MouseRegionOptions.None;

    protected Camera _camera;
    protected Rectangle _bounds;
    protected bool _holdDownAfterMouseLeave = true;
    protected bool _initialized = false;
    protected bool _hovered = false;
    protected bool _pressed = false;

    /// <summary>
    /// Gets whether this region is being hovered.
    /// </summary>
    public bool Hovered => _hovered;
    /// <summary>
    /// Gets whether this region is being pressed.
    /// </summary>
    public bool Pressed => _pressed;
    /// <summary>
    /// Gets or sets the camera where this region resides.
    /// </summary>
    /// <remarks>The camera's transformation matrix is used to transform from window coordinates to camera-specific coordinates.</remarks>
    public Camera Camera
    {
        get => _camera;
        set
        {
            _camera = value;
        }
    }
    /// <summary>
    /// Gets or sets the boundaries of this region.
    /// </summary>
    public Rectangle Bounds
    {
        get => _bounds;
        set
        {
            _bounds = value;
        }
    }
    /// <summary>
    /// Gets or sets the position of this region.
    /// </summary>
    public Point Position
    {
        get => _bounds.Location;
        set
        {
            _bounds.Location = value;
        }
    }
    /// <summary>
    /// Gets or sets the size of this region.
    /// </summary>
    public Point Size
    {
        get => _bounds.Size;
        set
        {
            _bounds.Size = value;
        }
    }

    public MouseInteractiveRegion(Rectangle region, Camera camera = null)
    {
        _bounds = region;
        _camera = camera;
    }
    public MouseInteractiveRegion(Point position, Point size) : this(new Rectangle(position, size)) { }
    public MouseInteractiveRegion(int x, int y, int widht, int height) : this(new Rectangle(x, y, widht, height)) { }

    public void Initialize()
    {
        if (_initialized) return;

        GameMouse.OnMoveScreen += OnMouseMovedScreen;
        GameMouse.OnButtonPressed += OnMouseButtonPressed;
        GameMouse.OnButtonDown += OnMouseButtonDown;
        GameMouse.OnButtonReleased += OnMouseButtonReleased;

        _initialized = true;
    }

    public void Terminate()
    {
        if (!_initialized) return;

        GameMouse.OnMoveScreen -= OnMouseMovedScreen;
        GameMouse.OnButtonPressed -= OnMouseButtonPressed;
        GameMouse.OnButtonDown -= OnMouseButtonDown;
        GameMouse.OnButtonReleased -= OnMouseButtonReleased;

        _initialized = false;
    }

    protected void CheckHovered(Vector2 mousePosition)
    {
        // Transform from a position within the camera to a position
        // within the graphics destination rectangle.
        Graphics.TransformVectorInverse(ref mousePosition);

        // Apply camera transformation.
        if (_camera != null)
        {
            mousePosition = _camera.TransformVectorInverse(mousePosition);
        }

        bool hovered = _bounds.Contains(mousePosition);

        if (hovered && !_hovered)
        {
            OnMouseEnter?.Invoke(null, EventArgs.Empty);
        }
        else if (!hovered && _hovered)
        {
            OnMouseLeave?.Invoke(null, EventArgs.Empty);
        }
        _hovered = hovered;

        if (Options.HasFlag(MouseRegionOptions.ReleaseOnLeave)
            && !_hovered
            && _pressed)
        {
            _pressed = false;
            OnReleased?.Invoke(null, EventArgs.Empty);
        }
    }

    protected void CheckPressed()
    {
        if (!_hovered || _pressed) return;

        OnPressed?.Invoke(null, EventArgs.Empty);
        _pressed = true;
    }

    protected void CheckDown()
    {
        if (!Options.HasFlag(MouseRegionOptions.AllowPressOnEnter)) return;
        if (!_hovered || _pressed) return;

        OnPressed?.Invoke(null, EventArgs.Empty);
        _pressed = true;
    }

    protected void CheckReleased() {
        if (!_pressed) return;

        OnReleased?.Invoke(null, EventArgs.Empty);
        _pressed = false;
    }

    private void OnMouseMovedScreen(object sender, GameMouse.MouseMoveEventArgs e)
    {
        CheckHovered(e.MousePosition);
    }

    private void OnMouseButtonPressed(object sender, GameMouse.MouseButtonEventArgs e)
    {
        CheckPressed();
    }

    private void OnMouseButtonDown(object sender, GameMouse.MouseButtonEventArgs e)
    {
        CheckDown();
    }

    private void OnMouseButtonReleased(object sender, GameMouse.MouseButtonEventArgs e)
    {
        CheckReleased();
    }
}
