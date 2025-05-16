using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

/// <summary>
/// Represents a text, and a color.
/// </summary>
public readonly struct ColoredText
{
    public string Text { get; }
    public uint ColorARGB { get; }
    public Color Color { get; }

    public ColoredText(string text, uint color = 0xFFFFFFFF)
    {
        Text = text;
        ColorARGB = color;
        Color = color.ARGBToColorXNA();
    }
    public ColoredText(string text, Color color)
    {
        Text = text;
        Color = color;
        ColorARGB = color.PackedValue;
    }

    public static implicit operator ColoredText(string text) => new(text);
    public static implicit operator string(ColoredText coloredText) => coloredText.Text;
}
