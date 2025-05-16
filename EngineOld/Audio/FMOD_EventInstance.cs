using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMOD;
using FMOD.Studio;


namespace MyMonoGameApp.Audio;

public readonly struct FMOD_EventInstance
{
    private readonly EventInstance _native;

    public readonly PLAYBACK_STATE PlaybackState
    {
        get
        {
            GameAudio.CheckFMOD(_native.getPlaybackState(out PLAYBACK_STATE state));
            return state;
        }
    }
    public readonly int TimelinePosition
    {
        get
        {
            GameAudio.CheckFMOD(_native.getTimelinePosition(out int position));
            return position;
        }

        set
        {
            GameAudio.CheckFMOD(_native.setTimelinePosition(value));
        }
    }
    public readonly FMOD_EventDescription Description
    {
        get
        {
            GameAudio.CheckFMOD(_native.getDescription(out EventDescription native));
            return new(native);
        }
    }
    public readonly Vector3 Position
    {
        get
        {
            GameAudio.CheckFMOD(_native.get3DAttributes(out ATTRIBUTES_3D attributes));
            return attributes.position.ToXNA3();
        }
        set
        {
            GameAudio.CheckFMOD(_native.get3DAttributes(out ATTRIBUTES_3D attributes));
            attributes.position = value.ToFMOD();
            GameAudio.CheckFMOD(_native.set3DAttributes(attributes));
        }
    }
    public readonly Vector3 Up
    {
        get
        {
            GameAudio.CheckFMOD(_native.get3DAttributes(out ATTRIBUTES_3D attributes));
            return attributes.up.ToXNA3();
        }

        set
        {
            GameAudio.CheckFMOD(_native.get3DAttributes(out ATTRIBUTES_3D attributes));
            attributes.up = value.ToFMOD();
            GameAudio.CheckFMOD(_native.set3DAttributes(attributes));
        }
    }
    public readonly Vector3 Forward
    {
        get
        {
            GameAudio.CheckFMOD(_native.get3DAttributes(out ATTRIBUTES_3D attributes));
            return attributes.forward.ToXNA3();
        }

        set
        {
            GameAudio.CheckFMOD(_native.get3DAttributes(out ATTRIBUTES_3D attributes));
            attributes.forward = value.ToFMOD();
            GameAudio.CheckFMOD(_native.set3DAttributes(attributes));
        }
    }
    public readonly Vector3 Velocity
    {
        get
        {
            GameAudio.CheckFMOD(_native.get3DAttributes(out ATTRIBUTES_3D attributes));
            return attributes.velocity.ToXNA3();
        }

        set
        {
            GameAudio.CheckFMOD(_native.get3DAttributes(out ATTRIBUTES_3D attributes));
            attributes.velocity = value.ToFMOD();
            GameAudio.CheckFMOD(_native.set3DAttributes(attributes));
        }
    }
    public readonly bool IsValid => _native.isValid();
    public readonly bool Paused
    {
        get
        {
            GameAudio.CheckFMOD(_native.getPaused(out bool paused));
            return paused;
        }

        set
        {
            GameAudio.CheckFMOD(_native.setPaused(value));
        }
    }

    public FMOD_EventInstance(EventInstance native)
    {
        if (!native.isValid())
        {
            throw new ArgumentException("EventInstance is not valid.", nameof(native));
        }

        _native = native;
    }
    public FMOD_EventInstance(IntPtr instancePtr)
    {
        _native = new(instancePtr);

        if (!_native.isValid())
        {
            throw new ArgumentException("EventInstance pointer is not valid.", nameof(instancePtr));
        }
    }
    public readonly void Start() => GameAudio.CheckFMOD(_native.start());
    public readonly void Stop(bool allowFadeOut = true) => GameAudio.CheckFMOD(_native.stop(allowFadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE));
    public readonly void Release() => GameAudio.CheckFMOD(_native.release());
    public readonly void SetParameter(string name, float value, bool ignoreSeekSpeed = false) => GameAudio.CheckFMOD(_native.setParameterByName(name, value, ignoreSeekSpeed));
    public readonly void GetParameter(string name, out float value) => GameAudio.CheckFMOD(_native.getParameterByName(name, out value));
    public readonly void GetParameter(string name, out float value, out float finalValue) => GameAudio.CheckFMOD(_native.getParameterByName(name, out value, out finalValue));
    public readonly void GetVolume(out float volume) => GameAudio.CheckFMOD(_native.getVolume(out volume));
    public readonly void GetVolume(out float volume, out float finalVolume) => GameAudio.CheckFMOD(_native.getVolume(out volume, out finalVolume));
    public readonly void SetVolume(float value) => GameAudio.CheckFMOD(_native.setVolume(value));
    public readonly void GetPitch(out float pitch) => GameAudio.CheckFMOD(_native.getPitch(out pitch));
    public readonly void GetPitch(out float pitch, out float finalPitch) => GameAudio.CheckFMOD(_native.getPitch(out pitch, out finalPitch));
    public readonly void SetPitch(float value) => GameAudio.CheckFMOD(_native.setPitch(value));
    public readonly void AddCallback(FMOD_EventDescription.EventCallback callback, EVENT_CALLBACK_TYPE callbackMask = EVENT_CALLBACK_TYPE.ALL)
    {
        RESULT nativeCallback(EVENT_CALLBACK_TYPE type, IntPtr audioEvent, IntPtr parameters)
        {
            return callback.Invoke(type, new(audioEvent), parameters);
        }

        GameAudio.CheckFMOD(_native.setCallback(nativeCallback, callbackMask));
    }

    public float this[string parameterName]
    {
        get
        {
            GetParameter(parameterName, out float value);
            return value;
        }
        set => SetParameter(parameterName, value);
    }
}
