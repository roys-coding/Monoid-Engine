using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

public class AnimationFrameSequence : SpriteAnimation
{
    public readonly struct Animation
    {
        public int[] Frames { get; }
        public float FramesPerSecond { get; }
        public bool Loop { get; }
        public int LoopCount { get; }
        public TimeSpan SecondsPerFrame { get; }

        public Animation(float framesPerSecond, bool loop, int loopCount, params int[] frames)
        {
            if (framesPerSecond < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(framesPerSecond), "Frames per second must be positive.");
            }

            if (loopCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(loopCount), "Loop count must be positive.");
            }

            if (frames == null)
            {
                throw new ArgumentNullException(nameof(frames));
            }

            if (frames.Length == 0)
            {
                throw new ArgumentException("Frames must not be empty.", nameof(frames));
            }

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] < 0)
                {
                    throw new ArgumentException("All frames must be positive.", nameof(frames));
                }
            }

            Frames = frames;
            FramesPerSecond = framesPerSecond;
            Loop = loop;
            LoopCount = loopCount == 0 ? -1 : loopCount;
            SecondsPerFrame = framesPerSecond == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(1.00 / framesPerSecond);
        }
    }

    protected readonly Dictionary<string, Animation> _animations = new();

    protected Animation? _activeAnimation;
    protected string _activeAnimationID = "";

    protected TimeSpan _timeSinceFrameChange = TimeSpan.Zero;
    protected int _frameIndex;
    protected int _remainingLoops;

    public override int ActiveFrame => _activeAnimation?.Frames[_frameIndex] ?? _frameIndex;
    /// <summary>
    /// Gets the active animation.
    /// </summary>
    public Animation? ActiveAnimation => _activeAnimation;
    /// <summary>
    /// Gets the remaining loops for the current animation playback.
    /// </summary>
    public int RemainingLoops => _remainingLoops;
    /// <summary>
    /// Gets the ID of the active animation.
    /// </summary>
    public string ActiveAnimationID => _activeAnimationID;

    public AnimationFrameSequence(bool global) : base(global) { }

    public override void Update()
    {
        if (_activeAnimation is not Animation animation) return;
        if (_state != PlaybackState.Playing) return;

        _timeSinceFrameChange += GameTime.DeltaTime;

        if (_timeSinceFrameChange < animation.SecondsPerFrame) return;

        TimeSpan secondsPerFrame = animation.SecondsPerFrame;

        int framesElapsed = 0;

        while (_timeSinceFrameChange >= secondsPerFrame)
        {
            framesElapsed++;
            _timeSinceFrameChange -= secondsPerFrame;
        }

        AdvanceFrames(framesElapsed);
    }

    /// <summary>
    /// Changes the active animation.
    /// </summary>
    /// <param name="ID">ID of the target animation.</param>
    /// <param name="keepProgress">Whether to retain the current progress or reset to the starting point.</param>
    /// <returns>
    /// <c>true</c> if the animation was successfully changed;<br/>
    /// <c>false</c> if:
    /// <list type="bullet">
    /// <item>The active animation is the same as the specified animation.</item>
    /// <item>No animation was found under the given <paramref name="ID"/>.</item>
    /// </list>
    /// </returns>
    public bool SetAnimation(string ID, bool keepProgress = false)
    {
        if (_activeAnimationID == ID) return false;
        if (!_animations.ContainsKey(ID)) return false;

        Animation targetAnimation = _animations[ID];
        _activeAnimationID = ID;

        if (_activeAnimation is Animation currentAnimation
            && keepProgress)
        {
            float currentProgress = (float)_frameIndex / currentAnimation.Frames.Length;
            _frameIndex = (int)MathF.Floor(targetAnimation.Frames.Length * currentProgress);
        }
        else _frameIndex = 0;

        _timeSinceFrameChange = TimeSpan.Zero;
        _activeAnimation = targetAnimation;

        return true;
    }

    /// <summary>
    /// Clears the active animation.
    /// </summary>
    /// <param name="resetToFrameZero">Whether to reset the animation to frame 0 or keep the last played frame.</param>
    /// <returns>
    /// <c>true</c> if the active animation was successfully cleared;<br/>
    /// <c>false</c> if no active animation was set.
    /// </returns>
    public bool ClearActiveAnimation(bool resetToFrameZero = true)
    {
        if (_activeAnimation == null) return false;

        if (resetToFrameZero)
        {
            _frameIndex = 0;
        }

        _activeAnimation = null;
        _activeAnimationID = null;
        _state = PlaybackState.Stopped;
        _timeSinceFrameChange = TimeSpan.Zero;
        return true;
    }

    /// <summary>
    /// Removes all stored animations.
    /// </summary>
    /// <returns>
    /// <c>true</c> if any animations were successfuly removed;<br/>
    /// <c>false</c> if there were no animations to remove.
    /// </returns>
    public bool ClearAnimations()
    {
        if (_animations.Count == 0) return false;

        ClearActiveAnimation();

        _frameIndex = 0;
        _timeSinceFrameChange = TimeSpan.Zero;
        _animations.Clear();
        return true;
    }

    /// <summary>
    /// Sets the active animation's progress.
    /// </summary>
    /// <param name="progressNormalized">Value between 0 and 1, where 0 represents the start and 1 the end of the animation.</param>
    /// <returns>
    /// <c>true</c> if the progress was successfuly set.
    /// <c>false</c> if:
    /// <list type="bullet">
    /// <item><paramref name="progressNormalized"/> was out of bounds (less than 0 or greater than 1)</item>
    /// <item>No active animation set.</item>
    /// </list>
    /// </returns>
    public bool SetAnimationProgress(float progressNormalized)
    {
        if (!progressNormalized.InRange(0, 1)) return false;
        if (_activeAnimation is not Animation animation) return false;

        int targetFrame = (int)MathF.Floor((animation.Frames.Length - 1) * progressNormalized);

        if (progressNormalized == 1f)
        {
            _state = PlaybackState.Done;
        }

        _frameIndex = targetFrame;
        return true;
    }

    /// <summary>
    /// Skips the active animation's progress to it's end.
    /// </summary>
    /// <remarks>Sets the playback state to <see cref="PlaybackState.Done"/>.</remarks>
    /// <returns>
    /// <c>true</c> if the animation was successfully skipped.<br/>
    /// <c>false</c> if no active animation is set.
    /// </returns>
    public bool SkipToEnd()
    {
        return SetAnimationProgress(1f);
    }

    /// <summary>
    /// Plays/Resumes the active animation.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the animation was successfully played;<br/>
    /// <c>false</c> if:
    /// <list type="bullet">
    /// <item>No active animation set.</item>
    /// <item>The animation was already playing.</item>
    /// </list>
    /// </returns>
    public override bool Play()
    {
        if (_activeAnimation is not Animation animation) return false;
        if (_state == PlaybackState.Playing) return false;

        if (_state == PlaybackState.Done) Restart();

        _state = PlaybackState.Playing;
        _remainingLoops = animation.LoopCount;
        return true;
    }

    /// <summary>
    /// Pauses the active animation.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the animation was successfully paused;<br/>
    /// <c>false</c> if:
    /// <list type="bullet">
    /// <item>No active animation set.</item>
    /// <item>The animation was not in the playing state.</item>
    /// </list>
    /// </returns>
    public override bool Pause()
    {
        if (_activeAnimation == null) return false;
        if (_state != PlaybackState.Playing) return false;

        _state = PlaybackState.Paused;
        return true;
    }

    /// <summary>
    /// Stops the active animation, resetting it to it's starting point.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the animation was successfully stopped;<br/>
    /// <c>false</c> if:
    /// <list type="bullet">
    /// <item>No active animation set.</item>
    /// <item>Animation was already stopped.</item>
    /// </list>
    /// </returns>
    public override bool Stop()
    {
        if (_activeAnimation is not Animation animation) return false;
        if (_state == PlaybackState.Stopped) return false;

        _frameIndex = 0;
        _timeSinceFrameChange = TimeSpan.Zero;
        _state = PlaybackState.Stopped;
        _remainingLoops = animation.LoopCount;
        return true;
    }

    /// <summary>
    /// Restarts the animation playback.
    /// </summary>
    /// <remarks>The animation will keep it's current playback state.</remarks>
    /// <returns>
    /// <c>true</c> if the animation was successfully reset;<br/>
    /// <c>false</c> if:
    /// <list type="bullet">
    /// <item>No active animation set.</item>
    /// <item>The animation is already at it's starting point.</item>
    /// </list>
    /// </returns>
    public override bool Restart()
    {
        if (_activeAnimation is not Animation animation) return false;
        if (_frameIndex == 0 && _timeSinceFrameChange == TimeSpan.Zero) return false;

        _frameIndex = 0;
        _timeSinceFrameChange = TimeSpan.Zero;
        _remainingLoops = animation.LoopCount;
        return true;
    }

    /// <summary>
    /// Forces the animation to finish, skipping to the last frame.
    /// </summary>
    /// <remarks></remarks>
    /// <returns>
    /// <c>true</c> if the animation was successfully ended;<br/>
    /// <c>false</c> if:
    /// <list type="bullet">
    /// <item>No active animation set.</item>
    /// <item>The animation was already finished.</item>
    /// </list>
    /// </returns>
    public bool ForceFinish()
    {
        if (_activeAnimation is not Animation animation) return false;
        if (_state == PlaybackState.Done) return false;

        _frameIndex = animation.Frames.Length - 1;
        _timeSinceFrameChange = TimeSpan.Zero;
        _state = PlaybackState.Done;
        _remainingLoops = 0;
        return true;
    }

    /// <summary>
    /// Sets the current frame.
    /// </summary>
    /// <param name="frame">Zero-based index of frame to be displayed.</param>
    /// <param name="force">Forces the frame to be set, clearing the active animation.</param>
    /// <remarks>
    /// If <paramref name="force"/> is <c>true</c>, the active animation will be cleared to display the specified frame.
    /// </remarks>
    /// <returns>
    /// <c>true</c> if frame was successfully set;<br/>
    /// <c>false</c> if:
    /// <list type="bullet">
    /// <item><paramref name="frame"/> is out of bounds (less than 0).</item>
    /// <item>An animation is active and <paramref name="force"/> is <c>false</c>.</item>
    /// </list>
    /// </returns>
    public bool SetFrame(int frame, bool force = false)
    {
        if (force) ClearActiveAnimation();
        else if (_activeAnimation != null) return false;
        if (frame < 0) return false;

        _frameIndex = frame;
        return true;
    }

    /// <summary>
    /// Adds an animation.
    /// </summary>
    /// <param name="ID">ID used to play the animation.</param>
    /// <param name="framesPerSecond">Animation speed.</param>
    /// <param name="loop">Whether this animation will loop.</param>
    /// <param name="loopCount">How many times the animation will loop before being done. Set to 0 to loop indefinitely.</param>
    /// <param name="frames">Frames to play back, in order.</param>
    /// <returns>
    /// <c>true</c> if the animation was successfully added;<br/>
    /// <c>false</c> if an animation under the same ID already existed.
    /// </returns>
    public bool AddAnimation_Frames(string ID, int framesPerSecond, bool loop, int loopCount = 0, params int[] frames)
    {
        if (string.IsNullOrEmpty(ID))
        {
            throw new ArgumentException("ID must not be null nor empty.", nameof(ID));
        }

        if (_animations.ContainsKey(ID)) return false;

        _animations.Add(ID, new(framesPerSecond, loop, loopCount, frames));
        return true;
    }

    /// <summary>
    /// Adds an animation.
    /// </summary>
    /// <param name="ID">ID used to play the animation.</param>
    /// <param name="framesPerSecond">Animation speed.</param>
    /// <param name="loop">Whether this animation will loop.</param>
    /// <param name="loopCount">How many times the animation will loop before being done. Set to 0 to loop indefinitely.</param>
    /// <param name="startFrame">First frame of the animation.</param>
    /// <param name="endFrame">Last frame of the animation.</param>
    /// <returns>
    /// <c>true</c> if the animation was successfully added;<br/>
    /// <c>false</c> if an animation under the same ID already existed.
    /// </returns>
    public bool AddAnimation_StartEndFrames(string ID, int framesPerSecond, bool loop, int loopCount = 0, int startFrame = 0, int endFrame = 0)
    {
        if (string.IsNullOrEmpty(ID))
        {
            throw new ArgumentException("ID must not be null nor empty.", nameof(ID));
        }

        if (_animations.ContainsKey(ID)) return false;

        if (startFrame < 0)
        {
            throw new ArgumentException("Start frame must be positive", nameof(startFrame));
        }

        if (endFrame < 0)
        {
            throw new ArgumentException("End frame must be positive", nameof(endFrame));
        }

        int totalFrames = (int)MathF.Abs(endFrame - startFrame) + 1;

        int[] frames = new int[totalFrames];

        if (startFrame <= endFrame)
        {
            for (int i = startFrame; i <= endFrame; i++)
            {
                frames[i] = i;
            }
        }
        else
        {
            for (int i = endFrame; i <= startFrame; i++)
            {
                frames[i] = startFrame - i;
            }
        }

        _animations.Add(ID, new(framesPerSecond, loop, loopCount, frames));
        return true;
    }

    /// <summary>
    /// Creates a copy of this handler.
    /// </summary>
    /// <param name="copyPlaybackState">Whether to copy the current playback state, or restart to the starting point.</param>
    /// <returns>A copy of this <see cref="AnimationFrameSequence"/>.</returns>
    public AnimationFrameSequence CreateCopy(bool copyPlaybackState = false)
    {
        AnimationFrameSequence copy = new(IsGlobal);

        // Copy playback state.
        if (copyPlaybackState)
        {
            copy._activeAnimation = _activeAnimation;
            copy._activeAnimationID = _activeAnimationID;
            copy._state = _state;
            copy._timeSinceFrameChange = _timeSinceFrameChange;
            copy._frameIndex = _frameIndex;
            copy._remainingLoops = _remainingLoops;
        }

        // Copy all animations into the new copy.
        foreach (var kvp in _animations)
        {
            copy._animations.Add(kvp.Key, kvp.Value);
        }

        return copy;
    }

    protected void AdvanceFrames(int count)
    {
        if (_activeAnimation is not Animation animation) return;

        _timeSinceFrameChange = TimeSpan.Zero;
        _frameIndex += count;

        if (_frameIndex >= animation.Frames.Length)
        {
            if (animation.Loop)
            {
                // Loop indefinitely.
                if (_remainingLoops == -1)
                {
                    _frameIndex %= animation.Frames.Length;
                }
                // Loop n number of times.
                else if (_remainingLoops > 0)
                {
                    _remainingLoops--;
                    _frameIndex %= animation.Frames.Length;
                }
                // Looping finished.
                else
                {
                    _state = PlaybackState.Done;
                    _frameIndex = animation.Frames.Length - 1;
                }
            }
            // Finish the animation otherwise.
            else
            {
                _state = PlaybackState.Done;
                _frameIndex = animation.Frames.Length - 1;
            }
        }
    }
}
