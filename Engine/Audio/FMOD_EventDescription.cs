using FMOD;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine.Audio;

public readonly struct FMOD_EventDescription
{
    public delegate RESULT EventCallback(EVENT_CALLBACK_TYPE type, FMOD_EventInstance instance, IntPtr parameters);

    private readonly EventDescription _native;

    public readonly string Path
    {
        get
        {
            GameAudio.CheckFMOD(_native.getPath(out string path));
            return path;
        }
    }
    public readonly GUID ID
    {
        get
        {
            GameAudio.CheckFMOD(_native.getID(out GUID id));
            return id;
        }
    }
    public readonly bool IsValid => _native.isValid();
    public readonly bool IsSnapshot
    {
        get
        {
            GameAudio.CheckFMOD(_native.isSnapshot(out bool value));
            return value;
        }
    }
    public readonly bool Is3D
    {
        get
        {
            GameAudio.CheckFMOD(_native.is3D(out bool value));
            return value;
        }
    }
    public readonly bool IsStream
    {
        get
        {
            GameAudio.CheckFMOD(_native.isStream(out bool value));
            return value;
        }
    }
    public readonly bool IsOneshot
    {
        get
        {
            GameAudio.CheckFMOD(_native.isOneshot(out bool value));
            return value;
        }
    }
    public readonly bool DopplerEnabled
    {
        get
        {
            GameAudio.CheckFMOD(_native.isDopplerEnabled(out bool value));
            return value;
        }
    }
    public readonly bool HasSustainPoint
    {
        get
        {
            GameAudio.CheckFMOD(_native.hasSustainPoint(out bool value));
            return value;
        }
    }
    public readonly int InstanceCount
    {
        get
        {
            GameAudio.CheckFMOD(_native.getInstanceCount(out int instances));
            return instances;
        }
    }
    public readonly int Length
    {
        get
        {
            GameAudio.CheckFMOD(_native.getLength(out int length));
            return length;
        }
    }
    public readonly float SoundSize
    {
        get
        {
            GameAudio.CheckFMOD(_native.getSoundSize(out float size));
            return size;
        }
    }

    public FMOD_EventDescription(EventDescription native)
    {
        if (!native.isValid())
        {
            throw new ArgumentException("EventDescription is not valid.", nameof(native));
        }

        _native = native;
    }

    public readonly FMOD_EventInstance CreateInstance()
    {
        GameAudio.CheckFMOD(_native.createInstance(out EventInstance native));
        return new(native);
    }
    public readonly FMOD_EventInstance[] GetInstances()
    {
        GameAudio.CheckFMOD(_native.getInstanceList(out EventInstance[] native));
        var instances = native.Select(instance => new FMOD_EventInstance(instance));
        return instances.ToArray();
    }
    public readonly void ReleaseAllInstances() => GameAudio.CheckFMOD(_native.releaseAllInstances());

    public readonly void GetMinMaxDistance(out float min, out float max) => GameAudio.CheckFMOD(_native.getMinMaxDistance(out min, out max));
    public readonly void AddCallback(EventCallback callback, EVENT_CALLBACK_TYPE callbackMask = EVENT_CALLBACK_TYPE.ALL)
    {
        RESULT nativeCallback(EVENT_CALLBACK_TYPE type, IntPtr audioEvent, IntPtr parameters)
        {
            return callback.Invoke(type, new(audioEvent), parameters);
        }

        GameAudio.CheckFMOD(_native.setCallback(nativeCallback, callbackMask));
    }

    public readonly FMOD_EventInstance this[int instanceIndex] => GetInstances()[instanceIndex];
}
