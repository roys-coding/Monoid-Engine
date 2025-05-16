using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MonoidEngine;

public class RNG
{
    public readonly static RNG Animations = new(0);
    public readonly static RNG Gameplay = new();

    private readonly Random _random;
    
    /// <summary>
    /// Instantiates a random number generator using the specified <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed"></param>
    public RNG(int seed)
    {
        _random = new(seed);
    }

    /// <summary>
    /// Instantiates a random number generator using a random time-dependent seed.
    /// </summary>
    /// <remarks>Since the seed is time-dependent, creating multiple instances in the same frame might result in them having the same seed.</remarks>
    public RNG()
    {
        _random = new();
    }

    /// <returns> A <see cref="System.TimeSpan"/> that is greater or equal to <paramref name="min"/> and less than <paramref name="max"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public TimeSpan TimeSpan(TimeSpan min, TimeSpan max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min must not be greater than max.");
        }

        if (min == max) return min;

        return min + (max - min) * Double();
    }
    /// <returns> A <see cref="System.TimeSpan"/> that is greater or equal to <paramref name="minSeconds"/> and less than <paramref name="maxSeconds"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minSeconds"/> is greater than <paramref name="maxSeconds"/>.</exception>
    public TimeSpan TimeSpan(double minSeconds, double maxSeconds)
    {
        if (minSeconds > maxSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(minSeconds), "Min must not be greater than max.");
        }

        if (minSeconds == maxSeconds) return System.TimeSpan.FromSeconds(minSeconds);

        TimeSpan min = System.TimeSpan.FromSeconds(minSeconds);
        TimeSpan max = System.TimeSpan.FromSeconds(maxSeconds);

        return min + (max - min) * Double();
    }
    /// <returns>A <see cref="float"/> that is greater or equal to <paramref name="min"/> and less than <paramref name="max"/>. If <paramref name="max"/> equals <paramref name="min"/>, <paramref name="max"/> will be returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public float Float(float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min must not be greater than max.");
        }

        if (min == max) return min;

        return min + ((max - min) * _random.NextSingle());
    }
    /// <returns>A non-negative <see cref="float"/> that is greater or equal to 0 and less than <paramref name="max"/>.<br/>
    /// If <paramref name="max"/> equals 0, 0 will be returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="max"/> is negative.</exception>
    public float Float(float max) => _random.NextSingle() * max;
    /// <returns>A non-negative <see cref="float"/> that is greater or equal to 0.0 and less than 1.0.</returns>
    public float Float() => _random.NextSingle();
    /// <returns>A <see cref="double"/> that is greater or equal to <paramref name="min"/> and less than <paramref name="max"/>. If <paramref name="max"/> equals <paramref name="min"/>, <paramref name="max"/> will be returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public double Double(double min, double max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Min must not be greater than max.");
        }

        if (min == max) return min;

        return min + ((max - min) * _random.NextDouble());
    }
    /// <returns>A non-negative <see cref="double"/> that is greater or equal to 0 and less than <paramref name="max"/>.<br/>
    /// If <paramref name="max"/> equals 0, 0 will be returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="max"/> is negative.</exception>
    public double Double(double max) => _random.NextDouble() * max;
    /// <returns>A non-negative <see cref="double"/> that is greater or equal to 0.0 and less than 1.0.</returns>
    public double Double() => _random.NextDouble();
    /// <returns>An <see cref="int"/> signed integer that is greater or equal to <paramref name="min"/> and less than <paramref name="max"/>. If <paramref name="max"/> equals <paramref name="min"/>, <paramref name="max"/> will be returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public int Int(int min, int max) => _random.Next(min, max);
    /// <returns>A non-negative <see cref="int"/> that is greater or equal to 0 and less than <paramref name="max"/>.<br/>
    /// If <paramref name="max"/> equals 0, 0 will be returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="max"/> is negative.</exception>
    public int Int(int max) => _random.Next(max);
    /// <returns>A non-negative <see cref="int"/> that is greater or equal to 0 and less than <see cref="int.MaxValue"/>.</returns>
    public int Int() => _random.Next();
    /// <returns>A random integer from the provided <paramref name="values"/></returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="values"/> is null or empty.</exception>
    public int ChooseInt(params int[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty", nameof(values));
        }

        return values[Int(values.Length)];
    }
    /// <returns>A <see cref="long"/> that is greater or equal to <paramref name="min"/> and less than <paramref name="max"/>.<br/>
    /// If <paramref name="max"/> equals <paramref name="min"/>, <paramref name="max"/> will be returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public long Long(long min, long max) => _random.NextInt64(min, max);
    /// <returns>A non-negative <see cref="long"/> that is greater or equal to 0 and less than <paramref name="max"/>.<br/>
    /// If <paramref name="max"/> equals 0, 0 will be returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="max"/> is negative.</exception>
    public long Long(long max) => _random.NextInt64(max);
    /// <returns>A non-negative <see cref="long"/> that is greater or equal to 0 and less than <see cref="long.MaxValue"/>.</returns>
    public long Long() => _random.NextInt64();
}