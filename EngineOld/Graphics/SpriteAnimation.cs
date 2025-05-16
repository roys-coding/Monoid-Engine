using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

/// <summary>
/// Playback states of an animation.
/// </summary>
public enum PlaybackState
{
    /// <summary>
    /// Animation is stopped.
    /// </summary>
    /// <remarks>Do not confuse with <see cref="Paused"/>.</remarks>
    Stopped,
    /// <summary>
    /// Animation is currently playing.
    /// </summary>
    Playing,
    /// <summary>
    /// Animation is currently paused.
    /// </summary>
    Paused,
    /// <summary>
    /// Animation is done.
    /// </summary>
    Done
}

public abstract class SpriteAnimation
{
    public readonly bool IsGlobal;
    protected PlaybackState _state = PlaybackState.Stopped;

    /// <summary>
    /// Gets the active animation frame.
    /// </summary>
    public virtual int ActiveFrame => 0;
    /// <summary>
    /// Gets the current playback state.
    /// </summary>
    public PlaybackState State => _state;

    public SpriteAnimation(bool isGlobal)
    {
        IsGlobal = isGlobal;
    }

    public virtual void Update() { }
    public virtual bool Play() => false;
    public virtual bool Pause() => false;
    public virtual bool Stop() => false;
    public virtual bool Restart() => false;
}
