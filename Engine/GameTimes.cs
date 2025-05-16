using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

/// <summary>
/// Provides information such as delta time and total time running.
/// </summary>
public static class GameTimes
{
    /// <summary>
    /// Global time scale.
    /// </summary>
    /// <remarks>Modify this value to speed-up or slow-down the game without affecting the framerate.</remarks>
    public static double GlobalTimeScale { get; set; } = 1.0;

    private static TimeSpan _totalElapsed;
    private static TimeSpan _totalElapsedRaw;
    private static TimeSpan _deltaTime;
    private static TimeSpan _deltaTimeRaw;

    /// <summary>
    /// Total time elapsed since the game began running.
    /// </summary>
    /// <remarks>Affected by global time scale.</remarks>
    public static TimeSpan TotalElapsed => _totalElapsed;

    /// <summary>
    /// Time elapsed between the last frame, and the frame before it.
    /// </summary>
    /// <remarks>Delta time is always 1 frame behind.<br/>
    /// Affected by global time scale.
    /// </remarks>
    public static TimeSpan DeltaTime => _deltaTime;

    /// <summary>
    /// Seconds elapsed between the last frame, and the frame before it.
    /// </summary>
    /// <remarks>Delta time is always 1 frame behind.<br/>
    /// Affected by global time scale.
    /// </remarks>
    public static double DeltaSeconds => _deltaTime.TotalSeconds;

    /// <summary>
    /// Seconds elapsed between the last frame, and the frame before it.
    /// </summary>
    /// <remarks>Delta time is always 1 frame behind.<br/>
    /// Affected by global time scale.
    /// </remarks>
    public static float DeltaSecondsF => (float)_deltaTime.TotalSeconds;

    /// <summary>
    /// Raw total time elapsed since the game began running.
    /// </summary>
    /// <remarks>Not affected by global time scale.</remarks>
    public static TimeSpan TotalElapsedRaw => _totalElapsedRaw;

    /// <summary>
    /// Time elapsed between the last frame, and the frame before it.
    /// </summary>
    /// <remarks>Delta time is always 1 frame behind.<br/>
    /// Not affected by global time scale.
    /// </remarks>
    public static TimeSpan DeltaTimeRaw => _deltaTimeRaw;

    /// <summary>
    /// Seconds elapsed between the last frame, and the frame before it.
    /// </summary>
    /// <remarks>Delta time is always 1 frame behind.<br/>
    /// Not affected by global time scale.
    /// </remarks>
    public static double DeltaSecondsRaw => _deltaTimeRaw.TotalSeconds;

    /// <summary>
    /// Seconds elapsed between the last frame, and the frame before it.
    /// </summary>
    /// <remarks>Delta time is always 1 frame behind.<br/>
    /// Not affected by global time scale.
    /// </remarks>
    public static float DeltaSecondsFRaw => (float)_deltaTimeRaw.TotalSeconds;
    
    /// <summary>
    /// Must be called on every game update call.
    /// </summary>
    public static void OnUpdate(GameTime gameTime)
    {
        _deltaTimeRaw = gameTime.ElapsedGameTime;
        _totalElapsedRaw = gameTime.TotalGameTime;

        _deltaTime = gameTime.ElapsedGameTime * GlobalTimeScale;
        _totalElapsed += _deltaTime;
    }
}
