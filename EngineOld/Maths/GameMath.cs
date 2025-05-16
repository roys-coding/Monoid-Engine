
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

/// <summary>
/// Useful methods related to Math.
/// </summary>
public static class GameMath
{
    public const float PiF = MathF.PI;
    public const float PiOver2F = MathF.PI * 0.5f;
    public const float PiOver4F = MathF.PI * 0.25f;
    public const float PiOver5F = MathF.PI * 0.2f;
    public const float PiOver8F = MathF.PI * 0.125f;
    public const float TauF = MathF.Tau;

    public const double Pi = Math.PI;
    public const double PiOver2 = Math.PI * 0.5;
    public const double PiOver4 = Math.PI * 0.25;
    public const double PiOver5 = Math.PI * 0.2;
    public const double PiOver8 = Math.PI * 0.125;
    public const double Tau = Math.Tau;

    /// <summary>
    /// Restricts the specified value to a range between 0 and 1 (inclusive).
    /// </summary>
    public static int Clamp01(int value) => Clamp(value, 0, 1);
    /// <summary>
    /// Restricts the specified value to a range between 0 and 1 (inclusive).
    /// </summary>
    public static float Clamp01(float value) => Clamp(value, 0f, 1f);
    /// <summary>
    /// Restricts the specified value to a range between 0 and 1 (inclusive).
    /// </summary>
    public static decimal Clamp01(decimal value) => Clamp(value, 0m, 1m);
    /// <summary>
    /// Restricts the specified value to a range between 0 and 1 (inclusive).
    /// </summary>
    public static double Clamp01(double value) => Clamp(value, 0.0, 1.0);

    /// <summary>
    /// Restricts the specified value to a specified range between <paramref name="min"/> and <paramref name="max"/> (inclusive).
    /// </summary>
    public static int Clamp(int value, int min, int max)
    {
        if (value <= min) return min;
        if (value >= max) return max;
        return value;
    }

    /// <summary>
    /// Restricts the specified value to a specified range between <paramref name="min"/> and <paramref name="max"/> (inclusive).
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        if (value <= min) return min;
        if (value >= max) return max;
        return value;
    }

    /// <summary>
    /// Restricts the specified value to a specified range between <paramref name="min"/> and <paramref name="max"/> (inclusive).
    /// </summary>
    public static decimal Clamp(decimal value, decimal min, decimal max)
    {
        if (value <= min) return min;
        if (value >= max) return max;
        return value;
    }

    /// <summary>
    /// Restricts the specified value to a specified range between <paramref name="min"/> and <paramref name="max"/> (inclusive).
    /// </summary>
    public static double Clamp(double value, double min, double max)
    {
        if (value <= min) return min;
        if (value >= max) return max;
        return value;
    }

    /// <summary>
    /// Converts the specified value from degrees to radians.
    /// </summary>
    public static float ToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180);
    }

    /// <summary>
    /// Converts the specified from radians to degrees.
    /// </summary>
    public static float ToDegrees(float radians)
    {
        return radians * (180 / MathF.PI);
    }

    /// <summary>
    /// Wraps radians to a range between -<see cref="MathF.PI"/> and +<see cref="MathF.PI"/>.
    /// </summary>
    public static float WrapRadians(float radians)
    {
        if (radians > -MathF.PI && radians <= MathF.PI)
        {
            return radians;
        }

        radians %= MathF.PI * 2f;

        if (radians <= -MathF.PI)
        {
            return radians + MathF.PI * 2f;
        }

        if (radians > MathF.PI)
        {
            return radians - MathF.PI * 2f;
        }

        return radians;
    }

    /// <summary>
    /// Checks whether the specified value is within the specified range (exclusive).
    /// </summary>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is greater than <paramref name="min"/>, and less than <paramref name="max"/>;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if min is greater than max.</exception>
    public static bool InRangeExclusive(this float value, float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min cannot be greater than max.");
        }

        return value > min && value < max;
    }

    /// <summary>
    /// Checks whether the specified value is within the specified range.
    /// </summary>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is greater or equal to <paramref name="min"/>, and less or equal to <paramref name="max"/>;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if min is greater than max.</exception>
    public static bool InRange(this float value, float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min cannot be greater than max.");
        }

        return value >= min && value <= max;
    }

    /// <summary>
    /// Checks whether the specified value is within the specified range (exclusive).
    /// </summary>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is greater than <paramref name="min"/>, and less than <paramref name="max"/>;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if min is greater than max.</exception>
    public static bool InRangeExclusive(this double value, double min, double max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min cannot be greater than max.");
        }

        return value > min && value < max;
    }

    /// <summary>
    /// Checks whether the specified value is within the specified range.
    /// </summary>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is greater or equal to <paramref name="min"/>, and less or equal to <paramref name="max"/>;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if min is greater than max.</exception>
    public static bool InRange(this double value, double min, double max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min cannot be greater than max.");
        }

        return value >= min && value <= max;
    }

    /// <summary>
    /// Checks whether the specified value is within the specified range (exclusive).
    /// </summary>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is greater than <paramref name="min"/>, and less than <paramref name="max"/>;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if min is greater than max.</exception>
    public static bool InRangeExclusive(this int value, int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min cannot be greater than max.");
        }

        return value > min && value < max;
    }

    /// <summary>
    /// Checks whether the specified value is within the specified range.
    /// </summary>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is greater or equal to <paramref name="min"/>, and less or equal to <paramref name="max"/>;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if min is greater than max.</exception>
    public static bool InRange(this int value, int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min cannot be greater than max.");
        }

        return value >= min && value <= max;
    }

    /// <summary>
    /// Checks whether the specified value is within the specified range (exclusive).
    /// </summary>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is greater than <paramref name="min"/>, and less than <paramref name="max"/>;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if min is greater than max.</exception>
    public static bool InRangeExclusive(this uint value, uint min, uint max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min cannot be greater than max.");
        }

        return value > min && value < max;
    }

    /// <returns>
    /// <c>true</c> if <paramref name="value"/> is greater or equal to <paramref name="min"/>, and less or equal to <paramref name="max"/>;<br/>
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if min is greater than max.</exception>
    public static bool InRange(this uint value, uint min, uint max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min cannot be greater than max.");
        }

        return value >= min && value <= max;
    }
}

public static partial class ExtensionMethods
{
    /// <summary>
    /// Calculates the angle between two vectors, measured in radians.
    /// </summary>
    public static float AngleRadiansTo(this Vector2 pointA, Vector2 pointB)
    {
        float deltaX = pointB.X - pointA.X;
        float deltaY = pointB.Y - pointA.Y;
        return MathF.Atan2(deltaY, deltaX);
    }

    /// <summary>
    /// Calculates the angle between two vectors, measured in degrees.
    /// </summary>
    public static float AngleDegreesTo(this Vector2 pointA, Vector2 pointB)
    {
        return GameMath.ToDegrees(AngleRadiansTo(pointA, pointB));
    }

    /// <summary>
    /// Applies an offset to a vector.
    /// </summary>
    public static Vector2 ApplyOffset(this ref Vector2 vector, float offset)
    {
        vector.X += offset;
        vector.Y += offset;
        return vector;
    }

    /// <summary>
    /// Applies an offset to a vector.
    /// </summary>
    public static Vector2 ApplyOffsetX(this ref Vector2 vector, float offset)
    {
        vector.X += offset;
        return vector;
    }

    /// <summary>
    /// Applies an offset to a vector.
    /// </summary>
    public static Vector2 ApplyOffsetY(this ref Vector2 vector, float offset)
    {
        vector.Y += offset;
        return vector;
    }

    /// <summary>
    /// Applies an offset to a vector.
    /// </summary>
    public static Vector2 ApplyOffset(this ref Vector2 vector, float offsetX, float offsetY)
    {
        vector.X += offsetX;
        vector.Y += offsetY;
        return vector;
    }

    /// <summary>
    /// Returns a copy of a vector with an offset applied.
    /// </summary>
    public static Vector2 Offset(this Vector2 vector, float offset)
    {
        vector.X += offset;
        vector.Y += offset;
        return vector;
    }

    /// <summary>
    /// Returns a copy of a vector with an offset applied.
    /// </summary>
    public static Vector2 OffsetX(this Vector2 vector, float offset)
    {
        vector.X += offset;
        return vector;
    }

    /// <summary>
    /// Returns a copy of a vector with an offset applied.
    /// </summary>
    public static Vector2 OffsetY(this Vector2 vector, float offset)
    {
        vector.Y += offset;
        return vector;
    }

    /// <summary>
    /// Returns a copy of a vector with an offset applied.
    /// </summary>
    public static Vector2 Offset(this Vector2 vector, float offsetX, float offsetY)
    {
        vector.X += offsetX;
        vector.Y += offsetY;
        return vector;
    }
}
