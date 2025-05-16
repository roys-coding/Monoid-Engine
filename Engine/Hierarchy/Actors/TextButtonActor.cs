using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoidEngine.Audio;
using FMOD;
using static System.Net.Mime.MediaTypeNames;

namespace MonoidEngine;

public class TextButtonActor : ButtonActor
{
    protected SpriteFont _font;
    protected string _text;
    protected string _hoveredPrefix;
    protected Vector2 _textOrigin;
    protected Vector2 _textSize;

    public SpriteFont Font
    {
        get => _font;
        set
        {
            _font = value ?? throw new InvalidOperationException("Font cannot be null");
            _textSize = _font.MeasureString(_text);
        }
    }
    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? throw new InvalidOperationException("Text cannot be null");
            _textSize = _font.MeasureString(value);
        }
    }

    public TextButtonActor(SpriteFont font, string text, Point position, Point size = default, float originX = 0.5f, float originY = 0.5f) : base(position, size)
    {
        _textOrigin = new(originX, originY);
        _font = font;
        _text = text;
        _textSize = font.MeasureString(_text);
    }
    public TextButtonActor(SpriteFont font, string text, int x, int y, int width = 0, int height = 0, float originX = 0.5f, float originY = 0.5f) : base(x, y, width, height)
    {
        _textOrigin = new(originX, originY);
        _font = font;
        _text = text;
        _textSize = font.MeasureString(_text);
    }

    public void SetHoveredPrefix(string prefix)
    {
        _hoveredPrefix = prefix;
    }

    public override void Draw()
    {
        Vector2 textPosition = _interactiveRegion.Position.ToVector2();
        textPosition += _interactiveRegion.Size.ToVector2() * _textOrigin;
        textPosition -= _textSize * _textOrigin;

        if (!string.IsNullOrEmpty(_hoveredPrefix) && _interactiveRegion.Hovered)
        {
            Vector2 prefixPosition = textPosition;
            prefixPosition.X -= _font.MeasureString(_hoveredPrefix).X;
            Graphics.Draw.Text(_font, _hoveredPrefix, prefixPosition, Color.White);
        }

        Graphics.Draw.Text(_font, _text, textPosition, Color.White);

        base.Draw();
    }

    public void AutoCalculateSize()
    {
        _interactiveRegion.Size = _font.MeasureString(_text).ToPoint();
    }
}
