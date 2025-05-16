using FMOD;
using FMOD.Studio;
using MyMonoGameApp.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

public class SoundEmitterActor : Actor2D
{
    public enum InstancingMode
    {
        CreateInstances,
        ReuseInstance
    }

    public Vector3 Velocity = Vector3.Zero;
    public Vector3 Up = -Vector3.UnitY;
    public Vector3 Forward = Vector3.UnitX;

    // New instance.
    protected FMOD_EventDescription _audioEvent;
    protected List<FMOD_EventInstance> _playedInstances = new();

    // Single instance.
    protected FMOD_EventInstance _singleAudioInstance;

    protected InstancingMode _playbackMode = InstancingMode.CreateInstances;

    public SoundEmitterActor(Transform2D transform, string eventPath) : base(transform)
    {
        _audioEvent = GameAudio.GetEventDescription(eventPath);
        _audioEvent.AddCallback(OnEventStop, EVENT_CALLBACK_TYPE.STOPPED);
    }

    public override void Initialize()
    {
        if (_playbackMode == InstancingMode.CreateInstances)
        {
            _singleAudioInstance = _audioEvent.CreateInstance();
        }

        base.Initialize();
    }

    public override void Terminate()
    {
        // Stop single instance.
        _singleAudioInstance.Stop();

        // Stop all instances.
        foreach (FMOD_EventInstance instance in _playedInstances)
        {
            instance.Stop();
        }

        // Release all instances.
        _audioEvent.ReleaseAllInstances();

        base.Terminate();
    }

    public virtual void Play()
    {
        switch (_playbackMode)
        {
            case InstancingMode.CreateInstances:
                PlayNewInstance();
                break;
            case InstancingMode.ReuseInstance:
                PlaySingleInstance();
                break;
            default:
                throw new NotImplementedException($"Playback mode '{_playbackMode}' not implemented.");
        }
    }

    public virtual void Pause()
    {
        switch (_playbackMode)
        {
            case InstancingMode.CreateInstances:
                foreach (FMOD_EventInstance instance in _playedInstances)
                {
                    instance.Paused = true;
                }
                break;
            case InstancingMode.ReuseInstance:
                _singleAudioInstance.Paused = true;
                break;
            default:
                throw new NotImplementedException($"Playback mode '{_playbackMode}' not implemented.");
        }
    }

    protected void PlaySingleInstance()
    {
        if (!_singleAudioInstance.IsValid)
        {
            _singleAudioInstance = _audioEvent.CreateInstance();
        }

        _singleAudioInstance.Start();
    }

    protected void PlayNewInstance()
    {
        FMOD_EventInstance instance = _audioEvent.CreateInstance();
        instance.Start();

        _playedInstances.Add(instance);
    }

    private RESULT OnEventStop(EVENT_CALLBACK_TYPE type, FMOD_EventInstance instance, IntPtr parameters)
    {
        if (!_playedInstances.Contains(instance))
        {
            return RESULT.OK;
        }

        _playedInstances.Remove(instance);
        instance.Release();
        return RESULT.OK;
    }
}
