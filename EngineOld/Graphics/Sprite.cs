using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

namespace MyMonoGameApp;

/// <summary>
/// 2D graphic with a position, scale, rotation, color, depth, and other properties.
/// </summary>
public class Sprite
{
    /// <summary>
    /// The sprite's position, scale and rotation.
    /// </summary>
    public Transform2D Transform;

    /// <summary>
    /// The sprite's origin, relative to its top-left corner.
    /// </summary>
    public Vector2 Origin = Vector2.Zero;

    /// <summary>
    /// The sprite's color.
    /// </summary>
    /// <remarks>Color data is multiplied by this color.</remarks>
    public Color Color = Color.White;

    /// <summary>
    /// Whether this sprite is flipped horizontally.
    /// </summary>
    public bool FlipHorizontal = false;

    /// <summary>
    /// Whether this sprite is flipped vertically.
    /// </summary>
    public bool FlipVertical = false;

    /// <summary>
    /// The sprite's transparency.
    /// </summary>
    /// <remarks>Ranges from 0 to 1, where 0 represents full transparency, and 1 represents opaqueness.</remarks>
    public float Alpha = 1f;

    /// <summary>
    /// The sprite's depth.
    /// </summary>
    /// <remarks>Depending on the <see cref="SpriteBatch"/>'s <see cref="SpriteSortMode"/> this is drawn in, depth is either:<br/>
    /// <list type="bullet">
    /// <item>Unknown (<see cref="SpriteSortMode.Deferred"/>)</item>
    /// <item>Front-to-back (<see cref="SpriteSortMode.FrontToBack"/>).</item>
    /// <item>Back-to-front (<see cref="SpriteSortMode.BackToFront"/>).</item>
    /// <item>Ignored (<see cref="SpriteSortMode.Texture"/>, <see cref="SpriteSortMode.Immediate"/>)).</item>
    /// </list>
    /// </remarks>
    public float Depth = 0f;

    protected Texture2D _texture;

    /// <summary>
    /// Gets the size of this sprite's texture.
    /// </summary>
    public virtual Vector2 Size
    {
        get
        {
            if (_texture == null) return Vector2.Zero;

            return new(_texture.Width, _texture.Height);
        }
    }

    /// <summary>
    /// The sprite's origin, normalized.
    /// </summary>
    /// <remarks>{0, 0} represents the top-left corner, and {1, 1} the bottom-right corner.</remarks>
    public Vector2 OriginNormalized
    {
        get => Origin / Size;
        set => Origin = value * Size;
    }

    /// <summary>
    /// The sprite's texture.
    /// </summary>
    public Texture2D Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            OnTextureChange();
        }
    }

    /// <summary>
    /// Instantiates a sprite.
    /// </summary>
    public Sprite(Texture2D texture, Transform2D transform, Vector2 origin = default)
    {
        _texture = texture;
        Transform = transform;
        Origin = origin;
    }

    /// <summary>
    /// Centers the sprite's origin to its center.
    /// </summary>
    public void CenterOrigin()
    {
        OriginNormalized = new Vector2(0.5f);
    }

    /// <summary>
    /// Draws the sprite to the specified <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="spriteBatch"></param>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        SpriteEffects effect = SpriteEffects.None;

        if (FlipVertical) effect |= SpriteEffects.FlipVertically;
        if (FlipHorizontal) effect |= SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(_texture, Transform.Position, null, Color * Alpha, Transform.Radians, Origin, Transform.Scale, effect, Depth);
    }

    protected virtual void OnTextureChange() { }
}
