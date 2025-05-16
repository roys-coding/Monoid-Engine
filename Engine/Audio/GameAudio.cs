using FMOD;
using FMOD.Studio;
using Hexa.NET.ImGui;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.Http.Headers;

namespace MonoidEngine.Audio;

public static partial class GameAudio
{
    public const string BANKS_PATH = $"{Monoid.CONTENT_DIRECTORY}/FMOD/Desktop/";

    public enum VCAs
    {
        Master,
        Ambience,
        Animatronics,
        Cameras,
        Office,
        Stingers,
        UI,
        Voiceover
    }

    public enum Buses
    {
        Master,
        Ambience,
        Animatronics,
        Cameras,
        Office,
        Stingers,
        UI,
        Voiceover
    }

    private static FMOD.Studio.System _studioSystem;
    private static FMOD.System _coreSystem;

    private static readonly List<Bank> _cachedBanks = new();
    private static readonly Dictionary<string, FMOD_EventDescription> _cachedEvents = new();
    private static readonly Dictionary<string, FMOD_Bus> _cachedBuses = new();
    private static readonly Dictionary<string, FMOD_VCA> _cachedVCAs = new();

    private static Bank _masterStringsBank;
    private static Bank _masterBank;
    private static Bank _ambienceBank;
    private static Bank _voiceoverBank;

    /// <summary>
    /// Initializes FMOD.
    /// </summary>
    public static void Initialize()
    {
        FMOD.Studio.INITFLAGS flags = FMOD.Studio.INITFLAGS.NORMAL;

        if (Monoid.Parameters.HasFlag(Monoid.StartupParameters.FMODLiveUpdateEnabled))
        {
            flags |= FMOD.Studio.INITFLAGS.LIVEUPDATE;
        }

        CheckFMOD(FMOD.Studio.System.create(out _studioSystem));
        CheckFMOD(_studioSystem.initialize(1024, flags, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));
        CheckFMOD(_studioSystem.getCoreSystem(out _coreSystem));
        CheckFMOD(_studioSystem.setNumListeners(1));
    }

    /// <summary>
    /// Loads all audio sample data.
    /// </summary>
    public static void LoadContent()
    {
        // Load banks.
        CheckFMOD(_studioSystem.loadBankFile($"{BANKS_PATH}Master.strings.bank", LOAD_BANK_FLAGS.NORMAL, out _masterStringsBank));
        CheckFMOD(_studioSystem.loadBankFile($"{BANKS_PATH}Master.bank", LOAD_BANK_FLAGS.NORMAL, out _masterBank));
        CheckFMOD(_studioSystem.loadBankFile($"{BANKS_PATH}Music and Ambience.bank", LOAD_BANK_FLAGS.NORMAL, out _ambienceBank));
        CheckFMOD(_studioSystem.loadBankFile($"{BANKS_PATH}Voiceover.bank", LOAD_BANK_FLAGS.NORMAL, out _voiceoverBank));

        _cachedBanks.Add(_masterBank);
        _cachedBanks.Add(_ambienceBank);
        _cachedBanks.Add(_voiceoverBank);

        // Load all sounds.
        foreach (Bank bank in _cachedBanks)
        {
            CheckFMOD(bank.loadSampleData());

            // Lock until banks have finished loading.
            CheckFMOD(_studioSystem.flushSampleLoading());
        }
    }

    /// <summary>
    /// Must be called on every update call.
    /// </summary>
    public static void Update()
    {
        CheckFMOD(_studioSystem.update());
    }

    public static void UnloadContent()
    {
        // Unload sample data from all banks.
        foreach (Bank bank in _cachedBanks)
        {
            CheckFMOD(bank.unloadSampleData());
        }

        // Lock until banks have finished unloading.
        CheckFMOD(_studioSystem.flushSampleLoading());
    }

    public static void Release()
    {
        CheckFMOD(_studioSystem.release());
    }

    public static FMOD_EventDescription GetEventDescription(string path)
    {
        if (_cachedEvents.ContainsKey(path)) return _cachedEvents[path];

        CheckFMOD(_studioSystem.getEvent(path, out EventDescription nativeEvent));
        FMOD_EventDescription eventDescription = new(nativeEvent);

        _cachedEvents.Add(path, eventDescription);

        return eventDescription;
    }

    public static FMOD_EventDescription GetEventDescription(GUID id)
    {
        CheckFMOD(_studioSystem.getEventByID(id, out EventDescription nativeEvent));
        FMOD_EventDescription eventDescription = new(nativeEvent);

        return eventDescription;
    }

    public static FMOD_VCA GetVCA(VCAs vca)
    {
        string path = vca switch
        {
            VCAs.Master => "vca:/Master",
            VCAs.Ambience => "vca:/Ambience",
            VCAs.Animatronics => "vca:/Animatronics",
            VCAs.Cameras => "vca:/Cameras",
            VCAs.Office => "vca:/Office",
            VCAs.Stingers => "vca:/Stingers",
            VCAs.UI => "vca:/UI",
            VCAs.Voiceover => "vca:/Voiceover",
            _ => "vca:/Master",
        };

        if (_cachedVCAs.ContainsKey(path)) return _cachedVCAs[path];

        CheckFMOD(_studioSystem.getVCA(path, out VCA nativeVCA));
        FMOD_VCA ret = new(nativeVCA);

        _cachedVCAs.Add(path, ret);
        return ret;
    }

    public static FMOD_Bus GetBus(Buses bus)
    {
        string path = bus switch
        {
            Buses.Master => "bus:/",
            Buses.Ambience => "bus:/Ambience",
            Buses.Animatronics => "bus:/Animatronics",
            Buses.Cameras => "bus:/Cameras",
            Buses.Office => "bus:/Office",
            Buses.Stingers => "bus:/Stingers",
            Buses.UI => "bus:/UI",
            Buses.Voiceover => "bus:/Voiceover",
            _ => "bus:/",
        };

        if (_cachedBuses.ContainsKey(path)) return _cachedBuses[path];

        CheckFMOD(_studioSystem.getBus(path, out Bus nativeBus));
        FMOD_Bus ret = new(nativeBus);

        _cachedBuses.Add(path, ret);
        return ret;
    }

    public static void SetParameter(string name, float value)
    {
        CheckFMOD(_studioSystem.setParameterByName(name, value));
    }

    public static float GetParameter(string name)
    {
        CheckFMOD(_studioSystem.getParameterByName(name, out float result));
        return result;
    }

    public static void SetParameter(PARAMETER_ID id, float value)
    {
        CheckFMOD(_studioSystem.setParameterByID(id, value));
    }

    public static float GetParameter(PARAMETER_ID id)
    {
        CheckFMOD(_studioSystem.getParameterByID(id, out float result));
        return result;
    }

    public static FMOD_EventInstance Play(FMOD_EventDescription description, bool oneShot = true)
    {
        if (!description.IsValid) return default;

        FMOD_EventInstance instance = description.CreateInstance();
        instance.Start();

        if (oneShot) instance.Release();

        return instance;
    }

    public static FMOD_EventInstance CreateEventInstance(string path)
    {
        FMOD_EventDescription eventDescription = GetEventDescription(path);
        return eventDescription.CreateInstance();
    }

    public static Vector3 GetListenerPosition(int listener)
    {
        CheckFMOD(_studioSystem.getListenerAttributes(listener, out ATTRIBUTES_3D attributes));
        return attributes.position.ToXNA3();
    }

    public static Vector3 GetListenerUp(int listener)
    {
        CheckFMOD(_studioSystem.getListenerAttributes(listener, out ATTRIBUTES_3D attributes));
        return attributes.up.ToXNA3();
    }

    public static Vector3 GetListenerForward(int listener)
    {
        CheckFMOD(_studioSystem.getListenerAttributes(listener, out ATTRIBUTES_3D attributes));
        return attributes.forward.ToXNA3();
    }

    public static Vector3 GetListenerVelocity(int listener)
    {
        CheckFMOD(_studioSystem.getListenerAttributes(listener, out ATTRIBUTES_3D attributes));
        return attributes.velocity.ToXNA3();
    }

    public static void SetListenerPosition(int listener, Vector3 position)
    {
        CheckFMOD(_studioSystem.getListenerAttributes(listener, out ATTRIBUTES_3D attributes));
        attributes.position.x = position.X;
        attributes.position.y = position.Y;
        attributes.position.z = position.Z;
        CheckFMOD(_studioSystem.setListenerAttributes(listener, attributes));
    }

    public static void SetListenerUp(int listener, Vector3 up)
    {
        CheckFMOD(_studioSystem.getListenerAttributes(listener, out ATTRIBUTES_3D attributes));
        attributes.up.x = up.X;
        attributes.up.y = up.Y;
        attributes.up.z = up.Z;
        CheckFMOD(_studioSystem.setListenerAttributes(listener, attributes));
    }

    public static void SetListenerForward(int listener, Vector3 forward)
    {
        CheckFMOD(_studioSystem.getListenerAttributes(listener, out ATTRIBUTES_3D attributes));
        attributes.forward.x = forward.X;
        attributes.forward.y = forward.Y;
        attributes.forward.z = forward.Z;
        CheckFMOD(_studioSystem.setListenerAttributes(listener, attributes));
    }

    public static void SetListenerVelocity(int listener, Vector3 velocity)
    {
        CheckFMOD(_studioSystem.getListenerAttributes(listener, out ATTRIBUTES_3D attributes));
        attributes.velocity.x = velocity.X;
        attributes.velocity.y = velocity.Y;
        attributes.velocity.z = velocity.Z;
        CheckFMOD(_studioSystem.setListenerAttributes(listener, attributes));
    }

    public static void CheckFMOD(RESULT result)
    {
        if (result != RESULT.OK) throw new Exception("FMOD Failed: " + result);
    }
}