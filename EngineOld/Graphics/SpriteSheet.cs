using FMOD.Studio;

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

public class SpriteSheet : Sprite
{
    protected SpriteAnimation _animation;
    protected Point _frameSize;
    protected Rectangle[] _frameRectangles;
    protected int _activeFrame = 0;
    protected int _frameCount;

    /// <summary>
    /// The animating being played by this sprite sheet.
    /// </summary>
    public SpriteAnimation Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            OnAnimationChange();
        }
    }

    /// <summary>
    /// The size of a single frame from this sprite sheet.
    /// </summary>
    public Point FrameSize
    {
        get => _frameSize;
        set
        {
            if (value.X < 1 || value.Y < 1)
            {
                throw new InvalidOperationException("Frame size must be greater than zero.");
            }

            if (_frameSize == value) return;

            _frameSize = value;
            OnFrameSizeChange();
        }
    }
    /// <summary>
    /// Frame currently being displayed by this sprite sheet.
    /// </summary>
    public int ActiveFrame => _activeFrame;

    /// <summary>
    /// Amount of frames this sprite sheet has.
    /// </summary>
    public int FrameCount => _frameCount;

    /// <summary>
    /// The size of a single frame from this sprite sheet.
    /// </summary>
    public override Vector2 Size => _frameSize.ToVector2();

    #region CONSTRUCTORS
    /// <summary>
    /// Instantiates a sprite sheet.
    /// </summary>
    public SpriteSheet(Texture2D texture, Point frameSize, Transform2D transform, Vector2 origin = default) : base(texture, transform, origin)
    {
        if (frameSize.X < 1 || frameSize.Y < 1)
        {
            throw new ArgumentException("Frame size must be greater than zero.", nameof(frameSize));
        }

        _frameSize = frameSize;
        CreateFrames();
    }
    /// <summary>
    /// Instantiates a sprite sheet.
    /// </summary>
    public SpriteSheet(Texture2D texture, int frameWidth, int frameHeight, Transform2D transform, Vector2 origin = default)
        : this(texture, new Point(frameWidth, frameHeight), transform, origin) { }
    /// <summary>
    /// Instantiates a sprite sheet.
    /// </summary>
    public SpriteSheet(Texture2D texture, int frameSize, Transform2D transform, Vector2 origin = default)
        : this(texture, new Point(frameSize), transform, origin) { }
    #endregion

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_animation is SpriteAnimation provider && !provider.IsGlobal)
        {
            provider.Update();
        }

        if (_texture == null) return;

        Color drawColor = Color;

        if (_animation != null)
        {
            // Set the active frame to the frame specified by the
            // animation, only if it's within this Sprite's frame count.
            if (_animation.ActiveFrame.InRange(0, _frameCount - 1))
            {
                _activeFrame = _animation.ActiveFrame;
            }
            // DEBUG
            // Set draw color to red if frame specified by the
            // animator is out of bounds.
            // Helps keep track of invalid frames.
            else drawColor = Color.Red;
        }

        SpriteEffects effect = SpriteEffects.None;

        if (FlipVertical) effect |= SpriteEffects.FlipVertically;
        if (FlipHorizontal) effect |= SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(_texture,
                         Transform.Position,
                         _frameRectangles[_activeFrame],
                         drawColor * Alpha,
                         Transform.Radians,
                         Origin,
                         Transform.Scale,
                         effect,
                         Depth);
    }

    /// <summary>
    /// Sets the active frame of this sprite sheet.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if frame is negative.</exception>
    public void SetFrame(int frame)
    {
        if (frame < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be positive.");
        }

        _activeFrame = frame;
    }

    protected override void OnTextureChange()
    {
        base.OnTextureChange();
        CreateFrames();
    }

    protected virtual void OnFrameSizeChange()
    {
        CreateFrames();
    }

    protected virtual void OnAnimationChange()
    {
    }

    protected void CreateFrames()
    {
        if (_texture == null)
        {
            _frameRectangles = null;
            _frameCount = 0;
            return;
        }

        int framesX = (int)MathF.Ceiling((float)_texture.Width / _frameSize.X);
        int framesY = (int)MathF.Ceiling((float)_texture.Height / _frameSize.Y);

        // Create frame rectangles.
        _frameCount = framesX * framesY;
        _frameRectangles = new Rectangle[framesX * framesY];
        Point frameSize = new(_frameSize.X, _frameSize.Y);

        int i = 0;
        for (int y = 0; y < framesY; y++)
        {
            for (int x = 0; x < framesX; x++)
            {
                Point frameCoordinates = new(x, y);
                frameCoordinates *= frameSize;

                _frameRectangles[i] = new Rectangle(frameCoordinates, frameSize);
                i++;
            }
        }
    }
}
