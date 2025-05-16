using FMOD;
using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine.Audio;

public readonly struct FMOD_Bus
{
    private readonly Bus _native;

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
    public readonly bool Mute
    {
        get
        {
            GameAudio.CheckFMOD(_native.getMute(out bool mute));
            return mute;
        }

        set
        {
            GameAudio.CheckFMOD(_native.setMute(value));
        }
    }
    public FMOD_Bus(Bus native)
    {
        if (!native.isValid())
        {
            throw new ArgumentException("Bus is not valid.", nameof(native));
        }

        _native = native;
    }

    public readonly void StopAllEvents(bool allowFadeOut = true)
    {
        GameAudio.CheckFMOD(_native.stopAllEvents(allowFadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE));
    }
    public readonly void GetVolume(out float volume) => GameAudio.CheckFMOD(_native.getVolume(out volume));
    public readonly void GetVolume(out float volume, out float finalVolume) => GameAudio.CheckFMOD(_native.getVolume(out volume, out finalVolume));
    public readonly void SetVolume(float value) => GameAudio.CheckFMOD(_native.setVolume(value));
}
