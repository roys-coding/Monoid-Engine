using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

/// <summary>
/// Provides several functions for easing and interpolation.
/// </summary>
public static class Easing
{
    private const double C1 = 1.70158;
    private const double C2 = 1.70158 * 1.525;
    private const double C3 = 2.70158;
    private const double C4 = Math.Tau / 3.0;
    private const double C5 = Math.Tau / 4.5;

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
    public delegate double EasingFunction(double time);

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
    public static double Interpolate(double start, double end, double time, EasingFunction function)
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
    public static double Lerp(double start, double end, double time) => start + (end - start) * time;

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
    public static double ExponentialIn(double time, double exponent) => Math.Pow(time, exponent);

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
    public static double ExponentialOut(double time, double exponent) => 1.0 - Math.Pow(1.0 - time, exponent);

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
    public static double InterpolateExponentialIn(double start, double end, double time, double exponent) => Lerp(start, end, ExponentialIn(time, exponent));
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
    public static double InterpolateExponentialOut(double start, double end, double time, double exponent) => Lerp(start, end, ExponentialOut(time, exponent));

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
    public static double SineIn(double time) => 1.0 - Math.Cos(time * Math.PI * 0.5);

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
    public static double SineOut(double time) => Math.Sin(time * Math.PI * 0.5);

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
    public static double SineInOut(double time) => -(Math.Cos(Math.PI * time) - 1.0) * 0.5;

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
    public static double QuadIn(double time) => time * time;

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
    public static double QuadOut(double time) => 1.0 - (1.0 - time) * (1.0 - time);

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
    public static double QuadInOut(double time) => time < 0.5 ? 2.0 * time * time : 1.0 - Math.Pow(-2.0 * time + 2.0, 2.0) * 0.5;

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
    public static double CubicIn(double time) => time * time * time;

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
    public static double CubicOut(double time) => 1.0 - (1.0 - time) * (1.0 - time) * (1.0 - time);

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
    public static double CubicInOut(double time) => time < 0.5 ? 4.0 * time * time * time : 1.0 - Math.Pow(-2.0 * time + 2.0, 3.0) * 0.5;

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
    public static double QuartIn(double time) => time * time * time * time;

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
    public static double QuartOut(double time) => 1.0 - (1.0 - time) * (1.0 - time) * (1.0 - time) * (1.0 - time);

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
    public static double QuartInOut(double time) => time < 0.5 ? 8.0 * time * time * time * time : 1.0 - Math.Pow(-2.0 * time + 2.0, 4.0) * 0.5;

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
    public static double QuintIn(double time) => time * time * time * time * time;

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
    public static double QuintOut(double time) => 1.0 - (1.0 - time) * (1.0 - time) * (1.0 - time) * (1.0 - time) * (1.0 - time);

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
    public static double QuintInOut(double time) => time < 0.5 ? 16.0 * time * time * time * time * time : 1.0 - Math.Pow(-2.0 * time + 2.0, 5.0) * 0.5;

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
    public static double ExpoIn(double time) => time == 0.0 ? 0.0 : Math.Pow(2.0, 10.0 * time - 10.0);

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
    public static double ExpoOut(double time) => time == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * time);

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
    public static double ExpoInOut(double time) => time == 0.0 ? 0.0
                                                : time == 1.0 ? 1.0
                                                : time < 0.5 ? Math.Pow(2.0, 20.0 * time - 10.0) * 0.5
                                                : (2.0 - Math.Pow(2.0, -20.0 * time + 10.0)) * 0.5;

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
    public static double CircIn(double time) => 1.0 - Math.Sqrt(1.0 - (time * time));

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
    public static double CircOut(double time) => Math.Sqrt(1.0 - ((time - 1.0) * (time - 1.0)));

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
    public static double CircInOut(double time) => time < 0.5
                                                ? (1.0 - Math.Sqrt(1.0 - Math.Pow(2.0 * time, 2))) * 0.5
                                                : (Math.Sqrt(1.0 - Math.Pow(-2.0 * time + 2.0, 2.0)) + 1.0) * 0.5;

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
    public static double BackIn(double time) => C3 * time * time * time - C1 * time * time;

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
    public static double BackOut(double time) => 1.0 + C3 * Math.Pow(time - 1.0, 3.0) + C1 * Math.Pow(time - 1.0, 2.0);

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
    public static double BackInOut(double time) => time < 0.5
                                                ? Math.Pow(2.0 * time, 2.0) * ((C2 + 1.0) * 2.0 * time - C2) * 0.5
                                                : (Math.Pow(2.0 * time - 2.0, 2.0) * ((C2 + 1.0) * (time * 2.0 - 2.0) + C2) + 2.0) * 0.5;


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
    public static double ElasticIn(double time) => time == 0.0
                                              ? 0.0
                                              : time == 1.0
                                              ? 1.0
                                              : -Math.Pow(2.0, 10.0 * time - 10.0) * Math.Sin((time * 10.0 - 10.75) * C4);

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
    public static double ElasticOut(double time) => time == 0.0
                                               ? 0.0
                                               : time == 1.0
                                               ? 1.0
                                               : Math.Pow(2.0, -10.0 * time) * Math.Sin((time * 10.0 - 0.75) * C4) + 1.0;

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
    public static double ElasticInOut(double time) => time == 0.0
                                              ? 0.0
                                              : time == 1.0
                                              ? 1.0
                                              : time < 0.5
                                              ? -(Math.Pow(2.0, 20.0 * time - 10.0) * Math.Sin((20.0 * time - 11.125) * C5)) / 2.0
                                              : (Math.Pow(2.0, -20.0 * time + 10.0) * Math.Sin((20.0 * time - 11.125) * C5)) / 2.0 + 1.0;
}
