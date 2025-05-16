using Hexa.NET.ImGui;
using MonoidEngine.DearImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoidEngine;

namespace FNAFFangame;

public readonly struct NightInfo
{
    public GameNight Night { get; }
    public float PassivePowerConsumption { get; }
    public float StartingPower { get; }
    public int FreddyAI { get; }
    public int BonnieAI { get; }
    public int ChicaAI { get; }
    public int FoxyAI { get; }

    public FreddyLocation FreddyStartingLocation { get; }
    public BonnieLocation BonnieStartingLocation { get; }
    public ChicaLocation ChicaStartingLocation { get; }
    public FoxyState FoxyStartingLocation { get; }

    public NightInfo(GameNight night, float passivePowerConsumption, float startingPower, int freddyAI, int bonnieAI, int chicaAI, int foxyAI, FreddyLocation freddyStartingLocation, BonnieLocation bonnieStartingLocation, ChicaLocation chicaStartingLocation, FoxyState foxyStartingLocation)
    {
        Night = night;
        PassivePowerConsumption = passivePowerConsumption;
        StartingPower = startingPower;
        FreddyAI = freddyAI;
        BonnieAI = bonnieAI;
        ChicaAI = chicaAI;
        FoxyAI = foxyAI;
        FreddyStartingLocation = freddyStartingLocation;
        BonnieStartingLocation = bonnieStartingLocation;
        ChicaStartingLocation = chicaStartingLocation;
        FoxyStartingLocation = foxyStartingLocation;
    }
}

#region ENUMS
public enum Animatronics
{
    Freddy,
    Bonnie,
    Chica,
    Foxy,
    GoldenFreddy
}

public enum Cameras
{
    None,
    DiningRoom,
    Scenario,
    Kitchen,
    HallLeft,
    HallRight,
    DoorLeft,
    DoorRight,
    ServiceRoom,
    PiratesCove,
    Restrooms,
    Parts,
}

[Flags]
public enum DoorStates
{
    Open = 0,
    Closed = 1,
    Flashing = 2,
    Broken = 4,
}

public enum ChicaLocation
{
    ShowStage = 0,
    DiningRoom1 = 1,
    DiningRoom2 = 2,
    Restrooms = 3,
    Restrooms2 = 4,
    Kitchen = 5,
    EastHall1 = 6,
    EastHall2 = 7,
    RightDoor = 8,
    Office = 9
}

public enum BonnieLocation
{
    ShowStage = 0,
    DiningRoom1 = 1,
    DiningRoom2 = 2,
    Backstage = 3,
    WestHall1 = 4,
    SupplyCloset = 5,
    WestHall2 = 6,
    LeftDoor = 7,
    Office = 8
}

public enum FoxyState
{
    Hiding = 0,
    Peaking = 1,
    Preparing = 2,
    WillRun = 3,
    Running = 4
}

public enum FreddyLocation
{
    ShowStage = 0,
    DiningRoom = 1,
    Restrooms = 2,
    Kitchen = 3,
    EastHall1 = 4,
    EastHall2 = 5,
    Office = 6
}

public enum GameNight
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7
}

public enum GameState
{
    NotPlaying = 0,
    Playing = 1,
    Victory = 2,
    GameOver = 3,
    GameOverGoldenFreddy = 4
}

public enum OfficeDoors
{
    Left,
    Right,
}

public enum DoorResult
{
    Failed = 0,
    FailedCooldown = 1,
    FailedBroken = 2,
    Success = 3,
}
#endregion

public class GameSession
{
    private static readonly Dictionary<GameNight, NightInfo> _nightsInfo = new(7)
    {
        {GameNight.One  , new(GameNight.One  , 0.0f, MAX_POWER,  0,  0,  0,  0, FreddyLocation.ShowStage, BonnieLocation.ShowStage, ChicaLocation.ShowStage, FoxyState.Hiding) },
        {GameNight.Two  , new(GameNight.Two  , 1/6f, MAX_POWER,  0,  3,  1,  1, FreddyLocation.ShowStage, BonnieLocation.ShowStage, ChicaLocation.ShowStage, FoxyState.Hiding) },
        {GameNight.Three, new(GameNight.Three, 1/5f, MAX_POWER,  1,  0,  5,  2, FreddyLocation.ShowStage, BonnieLocation.ShowStage, ChicaLocation.ShowStage, FoxyState.Hiding) },
        {GameNight.Four , new(GameNight.Four , 1/4f, MAX_POWER,  2,  2,  4,  6, FreddyLocation.ShowStage, BonnieLocation.ShowStage, ChicaLocation.ShowStage, FoxyState.Peaking) },
        {GameNight.Five , new(GameNight.Five , 1/3f, MAX_POWER,  3,  5,  7,  5, FreddyLocation.ShowStage, BonnieLocation.ShowStage, ChicaLocation.DiningRoom2, FoxyState.Hiding) },
        {GameNight.Six  , new(GameNight.Six  , 1/2f, MAX_POWER,  4, 10, 12, 16, FreddyLocation.ShowStage, BonnieLocation.Backstage, ChicaLocation.Restrooms, FoxyState.Hiding) },
        {GameNight.Seven, new(GameNight.Seven, 1/2f, MAX_POWER, 20, 20, 20, 20, FreddyLocation.ShowStage, BonnieLocation.ShowStage, ChicaLocation.ShowStage, FoxyState.Hiding) }
    };

    private const float MAX_POWER = 1000f;
    private const double HOUR_DURATION = 90.0;

    private const double SPOTTED_DOOR_COOLDOWN = 1.0;
    private const double DOOR_COOLDOWN_SECS = 0.5;
    private const double DOOR_INPUT_BUFFER_PERCENT_THRESH = 0.25;

    private const double FLASH_COOLDOWN_SECS = 0.2;

    private const float FLASH_POWER_CONSUMPTION = 1.5f;
    private const float DOOR_POWER_CONSUMPTION = 2f;

    #region EVENT ARGS
    public class OnGameStartEventArgs
    {
        public readonly GameNight Night;

        public OnGameStartEventArgs(GameNight night)
        {
            Night = night;
        }
    }

    public class AnimatronicMoveEventArgs : EventArgs
    {
        public readonly Animatronics Animatronic;
        public readonly int PreviousLocation;
        public readonly int CurrentLocation;

        public AnimatronicMoveEventArgs(Animatronics animatronic, int previousLocation, int currentLocation)
        {
            Animatronic = animatronic;
            PreviousLocation = previousLocation;
            CurrentLocation = currentLocation;
        }
    }

    public class AnimatronicSneakIntoOfficeEventArgs : EventArgs
    {
        public readonly Animatronics Animatronic;

        public AnimatronicSneakIntoOfficeEventArgs(Animatronics animatronic)
        {
            Animatronic = animatronic;
        }
    }

    public class AnimatronicSpottedAtDoorEventArgs : EventArgs
    {
        public readonly Animatronics Animatronic;
        public readonly DoorStates DoorState;

        public AnimatronicSpottedAtDoorEventArgs(Animatronics animatronic, DoorStates doorState)
        {
            Animatronic = animatronic;
            DoorState = doorState;
        }
    }

    public class AnimatronicAttackEventArgs
    {
        public readonly Animatronics Animatronic;
        public readonly bool PowerOutage;

        public AnimatronicAttackEventArgs(Animatronics animatronic, bool powerOutage)
        {
            Animatronic = animatronic;
            PowerOutage = powerOutage;
        }
    }

    public class DoorOpenedOrClosedEventArgs : EventArgs
    {
        public readonly OfficeDoors Door;
        public readonly bool IsDoorClosed;

        public DoorOpenedOrClosedEventArgs(OfficeDoors doors, bool isDoorClosed)
        {
            Door = doors;
            IsDoorClosed = isDoorClosed;
        }
    }

    public class DoorBreakEventArgs : EventArgs
    {
        public readonly OfficeDoors Door;
        
        public DoorBreakEventArgs(OfficeDoors doors)
        {
            Door = doors;
        }
    }
    
    public class DoorFlashEventArgs : EventArgs
    {
        public readonly OfficeDoors Door;
        public readonly bool IsDoorFlashed;

        public DoorFlashEventArgs(OfficeDoors doors, bool flashOn)
        {
            Door = doors;
            IsDoorFlashed = flashOn;
        }
    }
    #endregion

    #region EVENTS
    public event EventHandler<OnGameStartEventArgs> OnGameStart;
    public event EventHandler<AnimatronicMoveEventArgs> OnAnimatronicMove;
    public event EventHandler<AnimatronicSneakIntoOfficeEventArgs> OnAnimatronicSneakIntoOffice;
    public event EventHandler<AnimatronicSpottedAtDoorEventArgs> OnAnimatronicSpottedAtDoor;
    public event EventHandler<AnimatronicAttackEventArgs> OnAnimatronicAttack;
    public event EventHandler<DoorOpenedOrClosedEventArgs> OnDoorOpenedOrClosed;
    public event EventHandler<DoorBreakEventArgs> OnDoorBreak;
    public event EventHandler<DoorFlashEventArgs> OnDoorFlash;
    #endregion

    // State.
    protected GameState _state = GameState.NotPlaying;
    protected GameNight _night = GameNight.One;
    protected TimeSpan _time = TimeSpan.Zero;

    // Animatronics.
    protected readonly Animatronic[] _animatronics;
    protected ChicaAnimatronic _chica;
    protected BonnieAnimatronic _bonnie;

    // Power.
    protected float _powerRemaining = MAX_POWER;
    protected float _passivePowerConsumption = 0;
    protected float _powerConsumption = 0;

    // Doors.
    protected DoorStates _leftDoor = DoorStates.Open;
    protected TimeSpan _leftDoorCooldown = TimeSpan.Zero;
    protected TimeSpan _leftFlashCooldown = TimeSpan.Zero;
    protected bool _leftDoorTogglePending = false;
    protected bool _leftDoorFlashPending = false;

    protected DoorStates _rightDoor = DoorStates.Open;
    protected TimeSpan _rightDoorCooldown = TimeSpan.Zero;
    protected TimeSpan _rightFlashCooldown = TimeSpan.Zero;
    protected bool _rightDoorTogglePending = false;
    protected bool _rightDoorFlashPending = false;

    // State.
    public GameState State => _state;
    public GameNight Night => _night;
    public TimeSpan Time => _time;
    public double Hour => _time.TotalSeconds / HOUR_DURATION;

    // Animatronics.
    public ChicaAnimatronic Chica => _chica;
    public BonnieAnimatronic Bonnie => _bonnie;

    // Left door.
    public DoorStates LeftDoor => _leftDoor;
    public bool IsLeftDoorClosed => _leftDoor.HasFlag(DoorStates.Closed);
    public bool IsLeftDoorFlashed => _leftDoor.HasFlag(DoorStates.Flashing);
    public bool IsLeftDoorBroken => _leftDoor.HasFlag(DoorStates.Broken);

    // Right door.
    public DoorStates RightDoor => _rightDoor;
    public bool IsRightDoorClosed => _rightDoor.HasFlag(DoorStates.Closed);
    public bool IsRightDoorFlashed => _rightDoor.HasFlag(DoorStates.Flashing);
    public bool IsRightDoorBroken => _rightDoor.HasFlag(DoorStates.Broken);

    // Power
    public float PowerRemaining => _powerRemaining;
    public float NightPowerConsumption => _passivePowerConsumption;
    public float PowerConsumption => _powerConsumption;

    public GameSession()
    {
        _chica = new ChicaAnimatronic(this);
        _bonnie = new BonnieAnimatronic(this);

        _animatronics = new Animatronic[]
        {
            _chica,
            _bonnie
        };

        DImGui.OnDraw += (s, a) =>
        {
            ImGui.Begin("Gameplay state");

            ImGui.Text($"Night: {_night}");
            ImGui.Text($"State: {_state}");
            ImGui.Text($"Time: {TimeSpan.FromHours(Hour)}");
            ImGui.Spacing();
            ImGui.Text($"Power: {_powerRemaining}");
            ImGui.Text($"Consumption: {_passivePowerConsumption} + {_powerConsumption} / sec");

            bool resetPressed = ImGui.Button("Reset");
            if (resetPressed) Start(GameNight.Seven);

            ImGui.End();
        };
    }

    public void Start(GameNight night)
    {
        NightInfo nightInfo = _nightsInfo[night];

        _state = GameState.Playing;
        _night = night;
        _time = TimeSpan.Zero;

        // Notify animatronics that night has begun.
        foreach (Animatronic animatronic in _animatronics)
        {
            animatronic.OnStart(nightInfo);
        }

        // Set power consumption for the night.
        _passivePowerConsumption = nightInfo.PassivePowerConsumption;

        // Reset state.
        _leftDoor = DoorStates.Open;
        _leftDoorCooldown = TimeSpan.Zero;
        _rightDoor = DoorStates.Open;
        _rightDoorCooldown = TimeSpan.Zero;

        _powerRemaining = MAX_POWER;
        _powerConsumption = 0;

        OnGameStart?.Invoke(this, new(night));
    }

    public void Update()
    {
        if (State != GameState.Playing) return;

        _time += GameTimes.DeltaTime;

        // Decrease cooldowns.
        _leftDoorCooldown -= GameTimes.DeltaTime;
        _leftFlashCooldown -= GameTimes.DeltaTime;
        _rightDoorCooldown -= GameTimes.DeltaTime;
        _rightFlashCooldown -= GameTimes.DeltaTime;
        
        if (_leftDoorCooldown <= TimeSpan.Zero && _leftDoorTogglePending)
        {
            ToggleDoor(OfficeDoors.Left);
            _leftDoorTogglePending = false;
        }
        if (_rightDoorCooldown <= TimeSpan.Zero && _rightDoorTogglePending)
        {
            ToggleDoor(OfficeDoors.Right);
            _rightDoorTogglePending = false;
        }

        if (_leftFlashCooldown <= TimeSpan.Zero && _leftDoorFlashPending)
        {
            ToggleDoorFlash(OfficeDoors.Left);
            _leftDoorFlashPending = false;
        }
        if (_rightFlashCooldown <= TimeSpan.Zero && _rightDoorFlashPending)
        {
            ToggleDoorFlash(OfficeDoors.Right);
            _rightDoorFlashPending = false;
        }

        // Drain power.
        _powerRemaining -= (_passivePowerConsumption + _powerConsumption) * GameTimes.DeltaSecondsF;

        // Update animatronics.
        foreach (Animatronic animatronic in _animatronics)
        {
            animatronic.Update();
        }
    }

    public void AnimatronicAttacked(Animatronics animatronic)
    {
        if (_state != GameState.Playing) return;

        _state = GameState.GameOver;

        OnAnimatronicAttack?.Invoke(this, new(animatronic, false));
    }

    public void BreakDoor(OfficeDoors door)
    {
        switch (door)
        {
            case OfficeDoors.Left:
                if (IsLeftDoorBroken) return;
                _leftDoor = DoorStates.Broken;
                break;
            case OfficeDoors.Right:
                if (IsRightDoorBroken) return;
                _rightDoor = DoorStates.Broken;
                break;
            default:
                throw new NotImplementedException($"Door {door} not implemented.");
        }

        OnDoorBreak?.Invoke(this, new(door));
    }

    public void SpottedDoorCooldown(OfficeDoors door)
    {
        switch (door)
        {
            case OfficeDoors.Left:
                _leftDoorCooldown = TimeSpan.FromSeconds(SPOTTED_DOOR_COOLDOWN);
                break;
            case OfficeDoors.Right:
                _rightDoorCooldown = TimeSpan.FromSeconds(SPOTTED_DOOR_COOLDOWN);
                break;
            default:
                throw new NotImplementedException($"Door {door} not implemented.");
        }
    }

    public DoorResult ToggleDoor(OfficeDoors door)
    {
        bool isDoorClosed = door switch
        {
            OfficeDoors.Left => IsLeftDoorClosed,
            OfficeDoors.Right => IsRightDoorClosed,
            _ => throw new NotImplementedException($"Door {door} not implemented"),
        };

        if (isDoorClosed) return OpenDoor(door);
        else return CloseDoor(door);
    }

    public DoorResult CloseDoor(OfficeDoors door)
    {
        if (_state != GameState.Playing) return DoorResult.Failed;

        // Close corresponding door.
        DoorResult result = door switch
        {
            OfficeDoors.Left => SetDoorClosed(ref _leftDoor, ref _leftDoorCooldown, ref _leftDoorTogglePending, true),
            OfficeDoors.Right => SetDoorClosed(ref _rightDoor, ref _rightDoorCooldown, ref _rightDoorTogglePending, true),
            _ => throw new NotImplementedException($"Door {door} not implemented"),
        };

        if (result == DoorResult.Success)
        {
            // Notify animatronics that door has been closed.
            foreach (Animatronic animatronic in _animatronics)
            {
                animatronic.OnDoorClosed(door);
            }

            // Increase power consumption.
            _powerConsumption += DOOR_POWER_CONSUMPTION;
            OnDoorOpenedOrClosed?.Invoke(this, new(door, true));
        }

        return result;
    }

    public DoorResult OpenDoor(OfficeDoors door)
    {
        if (_state != GameState.Playing) return DoorResult.Failed;

        // Open corresponding door.
        DoorResult result = door switch
        {
            OfficeDoors.Left => SetDoorClosed(ref _leftDoor, ref _leftDoorCooldown, ref _leftDoorTogglePending, false),
            OfficeDoors.Right => SetDoorClosed(ref _rightDoor, ref _rightDoorCooldown, ref _rightDoorTogglePending, false),
            _ => throw new NotImplementedException($"Door {door} not implemented"),
        };

        if (result == DoorResult.Success)
        {
            // Notify animatronics that door has been opened.
            foreach (Animatronic animatronic in _animatronics)
            {
                animatronic.OnDoorOpened(door);
            }

            // Reduce power consumption.
            _powerConsumption -= DOOR_POWER_CONSUMPTION;
            OnDoorOpenedOrClosed?.Invoke(this, new(door, false));
        }

        return result;
    }

    public DoorResult ToggleDoorFlash(OfficeDoors door)
    {
        bool isDoorFlashed = door switch
        {
            OfficeDoors.Left => IsLeftDoorFlashed,
            OfficeDoors.Right => IsRightDoorFlashed,
            _ => throw new NotImplementedException($"Door {door} not implemented"),
        };

        if (!isDoorFlashed) return FlashDoor(door);
        else return StopFlashDoor(door);
    }

    public DoorResult FlashDoor(OfficeDoors door)
    {
        if (_state != GameState.Playing) return DoorResult.Failed;

        // Flash corresponding door.
        DoorResult result = door switch
        {
            OfficeDoors.Left => SetDoorFlashed(ref _leftDoor, ref _leftFlashCooldown, ref _leftDoorFlashPending, true),
            OfficeDoors.Right => SetDoorFlashed(ref _rightDoor, ref _rightFlashCooldown, ref _rightDoorFlashPending, true),
            _ => throw new NotImplementedException($"Door {door} not implemented"),
        };

        if (result == DoorResult.Success)
        {
            // Notify animatronics that door has been flashed.
            foreach (Animatronic animatronic in _animatronics)
            {
                animatronic.OnDoorFlashed(door);
            }

            // Increase power consumption.
            _powerConsumption += FLASH_POWER_CONSUMPTION;
            OnDoorFlash?.Invoke(this, new(door, true));
        }

        return result;
    }

    public DoorResult StopFlashDoor(OfficeDoors door)
    {
        if (_state != GameState.Playing) return DoorResult.Failed;

        // Stop flashing corresponding door.
        DoorResult result = door switch
        {
            OfficeDoors.Left => SetDoorFlashed(ref _leftDoor, ref _leftFlashCooldown, ref _leftDoorFlashPending, false),
            OfficeDoors.Right => SetDoorFlashed(ref _rightDoor, ref _rightFlashCooldown, ref _rightDoorFlashPending, false),
            _ => throw new NotImplementedException($"Door {door} not implemented"),
        };

        if (result == DoorResult.Success)
        {
            // Reduce power consumption.
            _powerConsumption -= FLASH_POWER_CONSUMPTION;
            OnDoorFlash?.Invoke(this, new(door, false));
        }

        return result;
    }

    protected static DoorResult SetDoorClosed(ref DoorStates door, ref TimeSpan cooldown, ref bool inputBuffer, bool closed)
    {
        // Early exit, door is broken.
        if (door.HasFlag(DoorStates.Broken)) return DoorResult.FailedBroken;

        // Early exit, could not set door state due to cooldown.
        if (cooldown > TimeSpan.Zero)
        {
            if (cooldown.TotalSeconds < DOOR_COOLDOWN_SECS * DOOR_INPUT_BUFFER_PERCENT_THRESH)
            {
                inputBuffer = true;
            }

            return DoorResult.FailedCooldown;
        }

        if (closed)
        {
            door |= DoorStates.Closed;
        }
        else
        {
            door &= ~DoorStates.Closed;
        }

        cooldown = TimeSpan.FromSeconds(DOOR_COOLDOWN_SECS);
        return DoorResult.Success;
    }

    protected static DoorResult SetDoorFlashed(ref DoorStates door, ref TimeSpan cooldown, ref bool inputBuffer, bool flashed)
    {
        // Early exit, door is broken.
        if (door.HasFlag(DoorStates.Broken)) return DoorResult.FailedBroken;

        // Early exit, could not set door state due to cooldown.
        if (cooldown > TimeSpan.Zero)
        {
            if (cooldown.TotalSeconds < FLASH_COOLDOWN_SECS * DOOR_INPUT_BUFFER_PERCENT_THRESH)
            {
                inputBuffer = true;
            }

            return DoorResult.FailedCooldown;
        }

        if (flashed)
        {
            door |= DoorStates.Flashing;
        }
        else
        {
            door &= ~DoorStates.Flashing;
        }

        cooldown = TimeSpan.FromSeconds(FLASH_COOLDOWN_SECS);
        return DoorResult.Success;
    }

    #region INVOKE EVENTS METHODS
    public void InvokeAnimatronicMoved(Animatronics animatronic, int previousLocation, int location)
    {
        OnAnimatronicMove?.Invoke(this, new(animatronic, previousLocation, location));
    }
    public void InvokeAnimatronicSneakIntoOffice(Animatronics animatronic)
    {
        OnAnimatronicSneakIntoOffice?.Invoke(this, new(animatronic));
    }
    public void InvokeAnimatronicSpottedAtDoor(Animatronics animatronic, OfficeDoors door)
    {
        DoorStates doorState = door switch
        {
            OfficeDoors.Left => _leftDoor,
            OfficeDoors.Right => _rightDoor,
            _ => throw new NotImplementedException($"Door {door} not implemented"),
        };

        OnAnimatronicSpottedAtDoor?.Invoke(this, new(animatronic, doorState));
    }
    #endregion
}
