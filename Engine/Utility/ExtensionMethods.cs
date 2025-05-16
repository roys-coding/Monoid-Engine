using FMOD;

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

public static partial class ExtensionMethods
{
    #region XNA to FMOD vectors.
    public static VECTOR ToFMOD(this Vector3 value) => new() { x = value.X, y = value.Y, z = value.Z };
    public static VECTOR ToFMOD(this Vector2 value) => new() { x = value.X, y = value.Y};
    public static Vector3 ToXNA3(this VECTOR value) => new(value.x, value.y, value.z);
    public static Vector2 ToXNA2(this VECTOR value) => new(value.x, value.y);
    #endregion

    #region Numerics to XNA vectors.
    public static Vector2 ToXNA(this System.Numerics.Vector2 value) => new(value.X, value.Y);
    public static Vector3 ToXNA(this System.Numerics.Vector3 value) => new(value.X, value.Y, value.Z);
    public static Vector4 ToXNA(this System.Numerics.Vector4 value) => new(value.X, value.Y, value.Z, value.W);
    #endregion

    #region Get/set color channels argb uint.
    /// <summary>
    /// Extracts the alpha channel of an argb color.
    /// </summary>
    public static byte ARGBGetAlpha(this uint argb)
    {
        // Extract the individual color component.
        uint a = (argb >> 24) & 0xFF;
        return (byte)a;
    }

    /// <summary>
    /// Sets the alpha channel of an argb color.
    /// </summary>
    public static uint ARGBSetAlpha(this uint argb, byte alpha)
    {
        // Shift the alpha channel (24 bits to the left) and replace the current alpha value
        return (argb & 0x00FFFFFF) | ((uint)alpha << 24);
    }

    /// <summary>
    /// Extracts the red channel of an argb color.
    /// </summary>
    public static byte ARGBGetRed(this uint argb)
    {
        // Extract the individual color component.
        uint r = (argb >> 16) & 0xFF;
        return (byte)r;
    }

    /// <summary>
    /// Extracts the red channel of an argb color.
    /// </summary>
    public static uint ARGBSetRed(this uint argb, byte red)
    {
        // Shift the red channel (16 bits to the left) and replace the current red value
        return (argb & 0xFF00FFFF) | ((uint)red << 16);
    }

    /// <summary>
    /// Extracts the green channel of an argb color.
    /// </summary>
    public static byte ARGBGetGreen(this uint argb)
    {
        // Extract the individual color component.
        uint g = (argb >> 8) & 0xFF;
        return (byte)g;
    }

    /// <summary>
    /// Extracts the green channel of an argb color.
    /// </summary>
    public static uint ARGBSetGreen(this uint argb, byte green)
    {
        // Shift the green channel (8 bits to the left) and replace the current green value
        return (argb & 0xFFFF00FF) | ((uint)green << 8);
    }

    /// <summary>
    /// Extracts the blue channel of an argb color.
    /// </summary>
    public static byte ARGBGetBlue(this uint argb)
    {
        // Extract the individual color component.
        uint b = argb & 0xFF;
        return (byte)b;
    }

    /// <summary>
    /// Extracts the is  channel of an argb color.
    /// </summary>
    public static uint ARGBSetBlue(this uint argb, byte blue)
    {
        // Replace the current blue value
        return (argb & 0xFFFFFF00) | blue;
    }
    #endregion

    #region UINT to Numerics vectors.
    /// <summary>
    /// Converts a <see cref="uint"/> into a <see cref="System.Numerics.Vector4"/>.
    /// </summary>
    /// <remarks>Color channels are normalized.</remarks>
    public static System.Numerics.Vector4 ARGBToVector4(this uint argb)
    {
        // Extract the individual color components
        float a = (argb >> 24) & 0xFF;
        float r = (argb >> 16) & 0xFF;
        float g = (argb >> 8) & 0xFF;
        float b = argb & 0xFF;

        // Normalize the components to the range of 0 to 1
        return new System.Numerics.Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    /// <summary>
    /// Converts a <see cref="uint"/> into a <see cref="System.Numerics.Vector3"/>.
    /// </summary>
    /// <remarks>Color channels are normalized.</remarks>
    public static System.Numerics.Vector3 RGBToVector3(this uint rgb)
    {
        // Extract the color components from the argb number
        float r = ((rgb >> 16) & 0xFF) / 255f; // Red component (x)
        float g = ((rgb >> 8) & 0xFF) / 255f;  // Green component (y)
        float b = (rgb & 0xFF) / 255f;         // Blue component (z)

        // Create and return the Vector3
        return new System.Numerics.Vector3(r, g, b);
    }

    /// <summary>
    /// Converts a <see cref="uint"/> into a <see cref="System.Numerics.Vector4"/>.
    /// </summary>
    /// <remarks>
    /// Color channels are normalized.<br/>
    /// Alpha channel will equal to <c>1</c>.
    /// </remarks>
    public static System.Numerics.Vector4 RGBToVector4(this uint rgb)
    {
        // Extract the individual color components
        float r = (rgb >> 16) & 0xFF;
        float g = (rgb >> 8) & 0xFF;
        float b = rgb & 0xFF;

        // Normalize the components to the range of 0 to 1
        return new System.Numerics.Vector4(r / 255f, g / 255f, b / 255f, 1f);
    }
    #endregion

    /// <summary>
    /// Converts a <see cref="uint"/> into a <see cref="Color"/>.
    /// </summary>
    /// <param name="argb"></param>
    /// <returns></returns>
    public static Color ARGBToColorXNA(this uint argb)
    {
        // Extract the individual color components
        float a = (argb >> 24) & 0xFF;
        float r = (argb >> 16) & 0xFF;
        float g = (argb >> 8) & 0xFF;
        float b = argb & 0xFF;

        // Normalize the components to the range of 0 to 1
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    /// <summary>
    /// Sets the alpha channel of a vector.
    /// </summary>
    public static System.Numerics.Vector4 ARGBSetAlpha(this System.Numerics.Vector4 vec, float alpha)
    {
        vec.W = alpha;
        return vec;
    }
}
