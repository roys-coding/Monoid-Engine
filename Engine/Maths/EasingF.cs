using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

/// <summary>
/// Provides several functions for easing and interpolation.
/// </summary>
public static class EasingF
{
    private const float C1 = 1.70158f;
    private const float C2 = 1.70158f * 1.525f;
    private const float C3 = 2.70158f;
    private const float C4 = MathF.Tau / 3.0f;
    private const float C5 = MathF.Tau / 4.5f;

    /// <summary>
    /// Mathematical easing function that calculates a smoothed output based on the given <paramref name="time"/> input.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public delegate float EasingFunction(float time);

    /// <summary>
    /// Interpolates between a starting and an ending value 
    /// based on the specified easing function and input time.
    /// </summary>
    /// <param name="start">The starting value of the interpolation.</param>
    /// <param name="end">The ending value of the interpolation.</param>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <param name="function">
    /// An easing function that defines the rate of change of the interpolation 
    /// and smooths the transition between the starting and ending values.
    /// </param>
    /// <returns>The interpolated value at the specified time.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified <paramref name="function"/> is <c>null</c>.
    /// </exception>
    public static float Interpolate(float start, float end, float time, EasingFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        return start + (end - start) * function(time);
    }

    /// <summary>
    /// Linearly interpolates between a starting and an ending value.
    /// </summary>
    /// <param name="start">The starting value of the interpolation.</param>
    /// <param name="end">The ending value of the interpolation.</param>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>The interpolated value at the specified time.</returns>
    public static float Lerp(float start, float end, float time) => start + (end - start) * time;

    /// <summary>
    /// Calculates an exponential ease-in value for the given time and exponent.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1, 
    /// where 0 corresponds to the start and 1 to the end of the easing.
    /// </param>
    /// <param name="exponent">
    /// The exponent used to control the acceleration of the easing curve.
    /// Higher values result in a sharper transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time and exponent.
    /// </returns>
    public static float ExponentialIn(float time, float exponent) => MathF.Pow(time, exponent);

    /// <summary>
    /// Calculates an exponential ease-out value for the given time and exponent.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1, 
    /// where 0 corresponds to the start and 1 to the end of the easing.
    /// </param>
    /// <param name="exponent">
    /// The exponent used to control the acceleration of the easing curve.
    /// Higher values result in a sharper transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time and exponent.
    /// </returns>
    public static float ExponentialOut(float time, float exponent) => 1.0f - MathF.Pow(1.0f - time, exponent);

    /// <summary>
    /// Interpolates between a starting and an ending value using an exponential ease-in function.
    /// </summary>
    /// <param name="start">The starting value of the interpolation.</param>
    /// <param name="end">The ending value of the interpolation.</param>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1, 
    /// where 0 corresponds to the start and 1 to the end of the easing.
    /// </param>
    /// <param name="exponent">
    /// The exponent used to control the acceleration of the easing curve.
    /// Higher values result in a sharper transition.
    /// </param>
    /// <returns>The interpolated value at the specified time.</returns>
    public static float InterpolateExponentialIn(float start, float end, float time, float exponent) => Lerp(start, end, ExponentialIn(time, exponent));
    /// <summary>
    /// Interpolates between a starting and an ending value using an exponential ease-out function.
    /// </summary>
    /// <param name="start">The starting value of the interpolation.</param>
    /// <param name="end">The ending value of the interpolation.</param>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1, 
    /// where 0 corresponds to the start and 1 to the end of the easing.
    /// </param>
    /// <param name="exponent">
    /// The exponent used to control the acceleration of the easing curve.
    /// Higher values result in a sharper transition.
    /// </param>
    /// <returns>The interpolated value at the specified time.</returns>
    public static float InterpolateExponentialOut(float start, float end, float time, float exponent) => Lerp(start, end, ExponentialOut(time, exponent));

    /// <summary>
    /// Calculates an easing value based on a sine-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float SineIn(float time) => 1f - MathF.Cos(time * MathF.PI * 0.5f);

    /// <summary>
    /// Calculates an easing value based on a sine-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float SineOut(float time) => MathF.Sin(time * MathF.PI * 0.5f);

    /// <summary>
    /// Calculates an easing value based on a sine in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float SineInOut(float time) => -(MathF.Cos(MathF.PI * time) - 1f) * 0.5f;

    /// <summary>
    /// Calculates an easing value based on a quad-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuadIn(float time) => time * time;

    /// <summary>
    /// Calculates an easing value based on a quad-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuadOut(float time) => 1.0f - (1.0f - time) * (1.0f - time);

    /// <summary>
    /// Calculates an easing value based on a quad in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuadInOut(float time) => time < 0.5f ? 2.0f * time * time : 1.0f - MathF.Pow(-2.0f * time + 2.0f, 2.0f) * 0.5f;

    /// <summary>
    /// Calculates an easing value based on a cubic-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float CubicIn(float time) => time * time * time;

    /// <summary>
    /// Calculates an easing value based on a cubic-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float CubicOut(float time) => 1.0f - (1.0f - time) * (1.0f - time) * (1.0f - time);

    /// <summary>
    /// Calculates an easing value based on a cubic in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float CubicInOut(float time) => time < 0.5f ? 4.0f * time * time * time : 1.0f - MathF.Pow(-2.0f * time + 2.0f, 3.0f) * 0.5f;

    /// <summary>
    /// Calculates an easing value based on a quart-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuartIn(float time) => time * time * time * time;

    /// <summary>
    /// Calculates an easing value based on a quart-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuartOut(float time) => 1.0f - (1.0f - time) * (1.0f - time) * (1.0f - time) * (1.0f - time);

    /// <summary>
    /// Calculates an easing value based on a quart in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuartInOut(float time) => time < 0.5f ? 8.0f * time * time * time * time : 1.0f - MathF.Pow(-2.0f * time + 2.0f, 4.0f) * 0.5f;

    /// <summary>
    /// Calculates an easing value based on a quart-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuintIn(float time) => time * time * time * time * time;

    /// <summary>
    /// Calculates an easing value based on a quint-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuintOut(float time) => 1.0f - (1.0f - time) * (1.0f - time) * (1.0f - time) * (1.0f - time) * (1.0f - time);

    /// <summary>
    /// Calculates an easing value based on a quint in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float QuintInOut(float time) => time < 0.5f ? 16.0f * time * time * time * time * time : 1.0f - MathF.Pow(-2.0f * time + 2.0f, 5.0f) * 0.5f;

    /// <summary>
    /// Calculates an easing value based on a expo-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float ExpoIn(float time) => time == 0.0f ? 0.0f : MathF.Pow(2.0f, 10.0f * time - 10.0f);

    /// <summary>
    /// Calculates an easing value based on a expo-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float ExpoOut(float time) => time == 1.0f ? 1.0f : 1.0f - MathF.Pow(2.0f, -10.0f * time);

    /// <summary>
    /// Calculates an easing value based on a expo in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float ExpoInOut(float time) => time == 0.0f ? 0.0f
                                                : time == 1.0f ? 1.0f
                                                : time < 0.5f ? MathF.Pow(2.0f, 20.0f * time - 10.0f) * 0.5f
                                                : (2.0f - MathF.Pow(2.0f, -20.0f * time + 10.0f)) * 0.5f;

    /// <summary>
    /// Calculates an easing value based on a circ-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float CircIn(float time) => 1.0f - MathF.Sqrt(1.0f - (time * time));

    /// <summary>
    /// Calculates an easing value based on a circ-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float CircOut(float time) => MathF.Sqrt(1.0f - ((time - 1.0f) * (time - 1.0f)));

    /// <summary>
    /// Calculates an easing value based on a circ in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float CircInOut(float time) => time < 0.5f
                                                ? (1.0f - MathF.Sqrt(1.0f - MathF.Pow(2.0f * time, 2))) * 0.5f
                                                : (MathF.Sqrt(1.0f - MathF.Pow(-2.0f * time + 2.0f, 2.0f)) + 1.0f) * 0.5f;

    /// <summary>
    /// Calculates an easing value based on a back-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float BackIn(float time) => C3 * time * time * time - C1 * time * time;

    /// <summary>
    /// Calculates an easing value based on a back-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float BackOut(float time) => 1.0f + C3 * MathF.Pow(time - 1.0f, 3.0f) + C1 * MathF.Pow(time - 1.0f, 2.0f);

    /// <summary>
    /// Calculates an easing value based on a back in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float BackInOut(float time) => time < 0.5f
                                                ? MathF.Pow(2.0f * time, 2.0f) * ((C2 + 1.0f) * 2.0f * time - C2) * 0.5f
                                                : (MathF.Pow(2.0f * time - 2.0f, 2.0f) * ((C2 + 1.0f) * (time * 2.0f - 2.0f) + C2) + 2.0f) * 0.5f;


    /// <summary>
    /// Calculates an easing value based on a elastic-in easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float ElasticIn(float time) => time == 0.0f
                                              ? 0.0f
                                              : time == 1.0f
                                              ? 1.0f
                                              : -MathF.Pow(2.0f, 10.0f * time - 10.0f) * MathF.Sin((time * 10.0f - 10.75f) * C4);

    /// <summary>
    /// Calculates an easing value based on a elastic-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float ElasticOut(float time) => time == 0.0f
                                               ? 0.0f
                                               : time == 1.0f
                                               ? 1.0f
                                               : MathF.Pow(2.0f, -10.0f * time) * MathF.Sin((time * 10.0f - 0.75f) * C4) + 1.0f;

    /// <summary>
    /// Calculates an easing value based on a elastic in-out easing function.
    /// </summary>
    /// <param name="time">
    /// A normalized value, ranging from 0 to 1,
    /// where 0 corresponds to the start and 1 to the end of the easing transition.
    /// </param>
    /// <returns>
    /// Eased output based on the input time.
    /// </returns>
    public static float ElasticInOut(float time) => time == 0.0f
                                              ? 0.0f
                                              : time == 1.0f
                                              ? 1.0f
                                              : time < 0.5f
                                              ? -(MathF.Pow(2.0f, 20.0f * time - 10.0f) * MathF.Sin((20.0f * time - 11.125f) * C5)) / 2.0f
                                              : (MathF.Pow(2.0f, -20.0f * time + 10.0f) * MathF.Sin((20.0f * time - 11.125f) * C5)) / 2.0f + 1.0f;
}
