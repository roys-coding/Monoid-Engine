using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

public class AnimationFrameRandomizer : SpriteAnimation
{
    /// <summary>
    /// Defines the animation's behaviour.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="AlwaysShuffle"/>:<br/>
    /// Continuously jumps between random frames.</item>
    /// <item>
    /// <see cref="ShuffleBursts"/>:<br/>
    /// A 'hold' frame is displayed, with occasional bursts of random frames.
    /// </item>
    /// </list>
    /// </remarks>
    public enum FrameMode
    {
        /// <summary>
        /// Continuously jumps between random frames.
        /// </summary>
        AlwaysShuffle,
        /// <summary>
        /// A 'hold' frame is displayed, with occasional bursts of random frames.
        /// </summary>
        ShuffleBursts
    }

    protected enum BurstState
    {
        Burst,
        Hold
    }

    /// <summary>
    /// Whether this animation allows the same frame to be randomly selected more than once.
    /// </summary>
    /// <remarks>If <c>false</c>, the next random frame will always be different from the current one.</remarks>
    public bool AllowDuplicateFrames = false;

    protected readonly WeightedCollection<int> _randomFrames = new(RNG.Animations);

    protected int _activeFrame = 0;
    protected int _holdFrame = 0;
    protected int _previousFrame = 0;
    protected BurstState _burstState = BurstState.Hold;
    protected FrameMode _frameMode = FrameMode.AlwaysShuffle;

    protected TimeSpan _minHoldTime = TimeSpan.Zero;
    protected TimeSpan _maxHoldTime = TimeSpan.Zero;

    protected TimeSpan _minFrameTime = TimeSpan.Zero;
    protected TimeSpan _maxFrameTime = TimeSpan.Zero;

    protected TimeSpan _minBurstDuration = TimeSpan.Zero;
    protected TimeSpan _maxBurstDuration = TimeSpan.Zero;

    protected TimeSpan _timeUntilNextFrame = TimeSpan.Zero;
    protected TimeSpan _timeUntilHoldStop = TimeSpan.Zero;
    protected TimeSpan _timeUntilBurstStop = TimeSpan.Zero;

    public override int ActiveFrame => _activeFrame;

    public AnimationFrameRandomizer(bool global) : base(global) { }

    /// <summary>
    /// Sets this animation's frame mode.
    /// </summary>
    /// <remarks>
    /// See <see cref="FrameMode"/> for more details about each behaviour.
    /// </remarks>
    public void SetMode(FrameMode mode, bool restartAnimation = true)
    {
        _frameMode = mode;

        if (restartAnimation) Restart();
    }

    /// <summary>
    /// Sets the weight of the specified <paramref name="frames"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if weight is negative.</exception>
    public void SetFrameWeight(int weight, params int[] frames)
    {
        if (weight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
        }

        foreach (int frame in frames)
        {
            _randomFrames.AddOrSetWeight(frame, weight);
        }
    }

    public void SetFrameDuration(double seconds)
    {
        _minFrameTime = TimeSpan.FromSeconds(seconds);
        _maxFrameTime = _minFrameTime;
    }

    public void SetFrameDuration(double minSeconds, double maxSeconds)
    {
        if (minSeconds > maxSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(minSeconds), "Min seconds must not be greater than max seconds.");
        }

        _minFrameTime = TimeSpan.FromSeconds(minSeconds);
        _maxFrameTime = TimeSpan.FromSeconds(maxSeconds);
    }

    public void SetHoldDuration(double seconds)
    {
        _minHoldTime = TimeSpan.FromSeconds(seconds);
        _maxHoldTime = _minHoldTime;

        if (_state == PlaybackState.Stopped)
        {
            _timeUntilHoldStop = RNG.Animations.TimeSpan(_minHoldTime, _maxHoldTime);
        }
    }

    public void SetHoldDuration(double minSeconds, double maxSeconds)
    {
        if (minSeconds > maxSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(minSeconds), "Min seconds must not be greater than max seconds.");
        }

        _minHoldTime = TimeSpan.FromSeconds(minSeconds);
        _maxHoldTime = TimeSpan.FromSeconds(maxSeconds);

        if (_state == PlaybackState.Stopped)
        {
            _timeUntilHoldStop = RNG.Animations.TimeSpan(_minHoldTime, _maxHoldTime);
        }
    }

    public void SetBurstDuration(double seconds)
    {
        _minBurstDuration = TimeSpan.FromSeconds(seconds);
        _maxBurstDuration = _minBurstDuration;
    }

    public void SetBurstDuration(double minSeconds, double maxSeconds)
    {
        if (minSeconds > maxSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(minSeconds), "Min seconds must not be greater than max seconds.");
        }

        _minBurstDuration = TimeSpan.FromSeconds(minSeconds);
        _maxBurstDuration = TimeSpan.FromSeconds(maxSeconds);
    }

    public void SetHoldFrame(int frame)
    {
        if (frame < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be positive.");
        }

        _holdFrame = frame;
    }

    public override bool Pause()
    {
        if (_state == PlaybackState.Paused) return false;

        _state = PlaybackState.Paused;
        return true;
    }

    public override bool Play()
    {
        if (_state == PlaybackState.Playing) return false;

        if (_state == PlaybackState.Stopped)
        {
            Restart();
        }

        _state = PlaybackState.Playing;
        return true;
    }

    public override bool Restart()
    {
        _burstState = BurstState.Hold;
        _timeUntilHoldStop = RNG.Animations.TimeSpan(_minHoldTime, _maxHoldTime);
        _timeUntilBurstStop = TimeSpan.Zero;
        _timeUntilNextFrame = TimeSpan.Zero;

        return true;
    }

    public override bool Stop()
    {
        if (_state == PlaybackState.Stopped) return false;

        _state = PlaybackState.Stopped;

        return true;
    }

    public override void Update()
    {
        // 'Done' state should never be reached for this animation type.
        if (_state == PlaybackState.Done) _state = PlaybackState.Stopped;

        // Exit if animation is not playing.
        if (_state != PlaybackState.Playing) return;

        switch (_frameMode)
        {
            default:
            case FrameMode.AlwaysShuffle:
                UpdateShuffle();
                break;
            case FrameMode.ShuffleBursts:
                UpdateBurst();
                break;
        }
    }

    protected void UpdateBurst()
    {
        switch (_burstState)
        {
            case BurstState.Hold:
                _timeUntilHoldStop -= GameTime.DeltaTime;
                _activeFrame = 0;
                _previousFrame = 0;

                if (_timeUntilHoldStop <= TimeSpan.Zero)
                {
                    _burstState = BurstState.Burst;
                    _timeUntilBurstStop = RNG.Animations.TimeSpan(_minBurstDuration, _maxBurstDuration);
                }

                break;
            case BurstState.Burst:
                _timeUntilBurstStop -= GameTime.DeltaTime;
                _timeUntilNextFrame -= GameTime.DeltaTime;

                if (_timeUntilBurstStop <= TimeSpan.Zero)
                {
                    _burstState = BurstState.Hold;
                    _timeUntilHoldStop = RNG.Animations.TimeSpan(_minHoldTime, _maxHoldTime);
                    _timeUntilNextFrame = TimeSpan.Zero;
                    break;
                }

                if (_timeUntilNextFrame <= TimeSpan.Zero)
                {
                    AdvanceFrame();
                }

                break;
        }
    }

    protected void UpdateShuffle()
    {
        _timeUntilNextFrame -= GameTime.DeltaTime;

        if (_timeUntilNextFrame < TimeSpan.Zero)
        {
            AdvanceFrame();
        }
    }

    protected void AdvanceFrame()
    {
        _timeUntilNextFrame = RNG.Animations.TimeSpan(_minFrameTime, _maxFrameTime);

        do
        {
            _activeFrame = _randomFrames.SelectRandom();
        }
        while (!AllowDuplicateFrames
               && _activeFrame == _previousFrame
               && _randomFrames.Count() > 1);

        _previousFrame = _activeFrame;
    }
}
