using Hexa.NET.ImGui;
using MonoidEngine.DearImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoidEngine;

namespace FNAFFangame;

public class ChicaAnimatronic : Animatronic
{
    private static int _forceLocation = 0;

    protected const double SNEAK_IN_TIME_SECS = 8;
    protected const double SNEAK_IN_SPOTTED_TIME_SECS = 1.8;
    protected const double ATTACK_TIME_MIN = 5;
    protected const double ATTACK_TIME_MAX = 60;

    private const float BACKOFF_CHANCE_LOWAI = 0.5f;
    private const float BACKOFF_CHANCE_HIGHAI = 0.2f;

    private const double MOVEMENT_INTERVAL_MIN_SECS_LOWAI = 20;
    private const double MOVEMENT_INTERVAL_MIN_SECS_HIGHAI = 4;
    private const double MOVEMENT_INTERVAL_MAX_SECS_LOWAI = 60;
    private const double MOVEMENT_INTERVAL_MAX_SECS_HIGHAI = 15;

    protected const double PRESSURE_TIME_MIN_SECS_LOWAI = 10;
    protected const double PRESSURE_TIME_MIN_SECS_HIGHAI = 20;
    protected const double PRESSURE_TIME_MAX_SECS_LOWAI = 20;
    protected const double PRESSURE_TIME_MAX_SECS_HIGHAI = 40;
    protected const double PRESSURE_TIME_CURVE_EXPONENT = 2;

    protected bool _spotted = false;
    protected bool _blocked = false;

    public bool Spotted => _spotted;
    public bool Blocked => _blocked;
    public ChicaLocation Location
    {
        get => (ChicaLocation)_location;
        protected set => _location = (int)value;
    }

    public ChicaAnimatronic(GameSession gameplayState) : base(gameplayState)
    {
        DImGui.OnDraw += (s, a) =>
        {
            ImGui.Begin("Chica");

            ImGui.SeparatorText("Chica");
            ImGui.Text($"Movement: {TimeUntilNextMove}");
            ImGui.Text($"Location: {Location}");
            ImGui.Text($"Spotted: {Spotted}");
            ImGui.Text($"Blocked: {Blocked}");

            ImGui.SliderInt("AI", ref _AI, 0, MAX_AI);

            ImGui.SetNextItemWidth(120f);
            ImGui.Combo("##locations", ref _forceLocation, string.Join('\0', Enum.GetNames<ChicaLocation>()));

            ImGui.SameLine();
            bool movePressed = ImGui.Button("Move");
            if (movePressed) ForceToRoom((ChicaLocation)_forceLocation);

            bool forceMovePressed = ImGui.Button("Force move");
            if (forceMovePressed) _timeUntilNextMove = TimeSpan.Zero;

            ImGui.SameLine();

            bool attackPressed = ImGui.Button("Attack");
            if (attackPressed) Attack();

            ImGui.End();
        };
    }

    public override void OnStart(NightInfo nightInfo)
    {
        AI = nightInfo.ChicaAI;
        Location = nightInfo.ChicaStartingLocation;

        // Reset state.
        _spotted = false;
        _blocked = false;
        _timeUntilNextMove = GetRandomMovementTimeSpan();
    }

    public override void PerformMovement()
    {
        _spotted = false;
        _blocked = false;

        // Chica is in the office. Attack.
        if (Location == ChicaLocation.Office)
        {
            Attack();
            return;
        }

        int previousLocation = _location;

        // Chica is at the right door.
        if (_location == (int)ChicaLocation.RightDoor)
        {
            if (_gameSession.IsRightDoorClosed)
            {
                _timeUntilNextMove = GetRandomMovementTimeSpan();

                // Door is closed, Chica backs off to a random room.
                _location = RNG.Gameplay.ChooseInt((int)ChicaLocation.Kitchen,
                                                    (int)ChicaLocation.Restrooms,
                                                    (int)ChicaLocation.DiningRoom2);

                _gameSession.InvokeAnimatronicMoved(Animatronics.Chica, previousLocation, _location);
            }
            else
            {
                // Door is open, Chica sneaks in.
                SneakIn();
            }

            return;
        }

        // Roll a dice to check if Chica should back off or move forward.
        float backoffChance = EasingF.Lerp(BACKOFF_CHANCE_LOWAI, BACKOFF_CHANCE_HIGHAI, AINormalized);
        float backoffDiceRoll = RNG.Gameplay.Float();

        if (backoffDiceRoll > backoffChance)
        {
            // Move forward.
            _location++;
        }
        else
        {
            _location--;
        }

        // Stop Chica from backing off to the Show Stage or moving past the office (invalid locations).
        _location = GameMath.Clamp(_location, (int)ChicaLocation.DiningRoom1, (int)ChicaLocation.Office);

        if (Location == ChicaLocation.RightDoor)
        {
            if (_gameSession.IsRightDoorClosed)
            {
                // When Chica arrives at the door, and it's closed,
                // she will stay there for a few seconds.
                SpamDoor();
            }
            else
            {
                // If Chica is at the right door, and remains unseen,
                // she will sneak in after a fixed amount of time.
                _timeUntilNextMove = TimeSpan.FromSeconds(SNEAK_IN_TIME_SECS);
            }

            // If Chica moves to the right door while it is flashed,
            // she is immediately spotted.
            if (_gameSession.IsRightDoorFlashed && !_spotted)
            {
                _spotted = true;

                // If the door is closed, add some cooldown.
                // This avoids scenarios where the animatronic is spotted and
                // sneaks into the office at the very last second, before the door is open.
                if (_gameSession.IsRightDoorClosed)
                {
                    _gameSession.SpottedDoorCooldown(OfficeDoors.Right);
                }

                _gameSession.InvokeAnimatronicSpottedAtDoor(Animatronics.Chica, OfficeDoors.Right);
            }
        }
        else
        {
            // If Chica is in some room that is not the right door, move normally:
            // Select a random time between min and max movement intervals.
            // Keeps animatronics out of sync, and somewhat unpredictable.
            _timeUntilNextMove = GetRandomMovementTimeSpan();
        }

        _gameSession.InvokeAnimatronicMoved(Animatronics.Chica, previousLocation, _location);
    }

    public override void Attack()
    {
        _gameSession.AnimatronicAttacked(Animatronics.Chica);
    }

    public override void OnDoorFlashed(OfficeDoors door)
    {
        if (door != OfficeDoors.Right) return;

        // If Chica is at the right door, and the door is flashed,
        // she is 'spotted'.
        if (Location == ChicaLocation.RightDoor && !_spotted)
        {
            if (!_gameSession.IsRightDoorClosed)
            {
                // When Chica is spotted at the door, and it is not closed, the player
                // will only have a few seconds to close it before she sneaks in.
                _timeUntilNextMove = TimeSpan.FromSeconds(SNEAK_IN_SPOTTED_TIME_SECS);
            }

            _spotted = true;
            _gameSession.InvokeAnimatronicSpottedAtDoor(Animatronics.Chica, OfficeDoors.Right);
        }

        base.OnDoorFlashed(door);
    }

    public override void OnDoorOpened(OfficeDoors door)
    {
        if (door != OfficeDoors.Right) return;
        if (Location != ChicaLocation.RightDoor) return;

        // If Chica has already been spotted, or blocked at the door,
        // and it is opened, Chica sneaks in immediately.
        if (_spotted || _blocked)
        {
            SneakIn();
        }

        base.OnDoorOpened(door);
    }

    public override void OnDoorClosed(OfficeDoors door)
    {
        if (door != OfficeDoors.Right) return;
        if (Location != ChicaLocation.RightDoor) return;

        // When Chica is at the door, and it's closed, she will stay
        // at the door for some seconds.
        if (!_blocked)
        {
            SpamDoor();
            _blocked = true;
        }


        base.OnDoorClosed(door);
    }

    public void ForceToRoom(ChicaLocation location)
    {
        if (Location == ChicaLocation.Office || location == ChicaLocation.ShowStage) return;

        int i = 0;

        while (Location != location && i < 10000)
        {
            _timeUntilNextMove = TimeSpan.Zero;
            PerformMovement();
            i++;
        }
    }

    protected void SneakIn()
    {
        // After a random interval, Chica will attack no matter what.
        float random = RNG.Gameplay.Float();
        double attackTime = Easing.Lerp(ATTACK_TIME_MIN, ATTACK_TIME_MAX, random);

        _timeUntilNextMove = TimeSpan.FromSeconds(attackTime);

        // Door is open, Chica enters the office and breaks buttons.
        Location = ChicaLocation.Office;
        _gameSession.BreakDoor(OfficeDoors.Right);

        _gameSession.InvokeAnimatronicSneakIntoOffice(Animatronics.Chica);
        _gameSession.InvokeAnimatronicMoved(Animatronics.Chica, (int)ChicaLocation.RightDoor, _location);
    }

    public TimeSpan GetRandomMovementTimeSpan()
    {
        float random = RNG.Gameplay.Float();

        // The higher the AI, the higher the possible times are.
        double minMovementTime = Easing.Lerp(MOVEMENT_INTERVAL_MIN_SECS_LOWAI, MOVEMENT_INTERVAL_MIN_SECS_HIGHAI, AINormalized);
        double maxMovementTime = Easing.Lerp(MOVEMENT_INTERVAL_MAX_SECS_LOWAI, MOVEMENT_INTERVAL_MAX_SECS_HIGHAI, AINormalized);

        // Get a random movement time.
        double movementTime = Easing.Lerp(minMovementTime, maxMovementTime, random);

        return TimeSpan.FromSeconds(movementTime);
    }

    protected void SpamDoor()
    {
        // Make Chica stay at the door for a random amount of time, putting some pressure on the player.
        float random = RNG.Gameplay.Float();

        // The higher the AI level, the higher the max possible pressure time is.
        double pressureTimeMinSecs = Easing.Lerp(PRESSURE_TIME_MIN_SECS_LOWAI, PRESSURE_TIME_MIN_SECS_HIGHAI, AINormalized);
        double pressureTimeMaxSecs = Easing.Lerp(PRESSURE_TIME_MAX_SECS_LOWAI, PRESSURE_TIME_MAX_SECS_HIGHAI, AINormalized);
        // An exponential function is used to make high pressure times rarer.
        double pressureTime = Easing.InterpolateExponentialIn(pressureTimeMinSecs, pressureTimeMaxSecs, random, PRESSURE_TIME_CURVE_EXPONENT);

        _timeUntilNextMove = TimeSpan.FromSeconds(pressureTime);
    }
}
