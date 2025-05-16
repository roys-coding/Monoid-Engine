using Hexa.NET.ImGui;
using MonoidEngine.DearImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoidEngine;

namespace FNAFFangame;

public class BonnieAnimatronic : Animatronic
{
    private static int _forceLocation = 0;

    protected const double SNEAK_IN_TIME_SECS = 8;
    protected const double SNEAK_IN_SPOTTED_TIME_SECS = 1.8;
    protected const double ATTACK_TIME_MIN = 5;
    protected const double ATTACK_TIME_MAX = 60;

    private const float MOVE_TWICE_CHANCE_LOWAI = 0.1f;
    private const float MOVE_TWICE_CHANCE_HIGHAI = 0.3f;

    private const double MOVEMENT_INTERVAL_MIN_SECS_LOWAI = 25;
    private const double MOVEMENT_INTERVAL_MIN_SECS_HIGHAI = 7.5;
    private const double MOVEMENT_INTERVAL_MAX_SECS_LOWAI = 90;
    private const double MOVEMENT_INTERVAL_MAX_SECS_HIGHAI = 25;

    protected const double PRESSURE_TIME_MIN_SECS_LOWAI = 5;
    protected const double PRESSURE_TIME_MIN_SECS_HIGHAI = 10;
    protected const double PRESSURE_TIME_MAX_SECS_LOWAI = 15;
    protected const double PRESSURE_TIME_MAX_SECS_HIGHAI = 20;
    protected const double PRESSURE_TIME_CURVE_EXPONENT = 3;

    protected bool _spotted = false;
    protected bool _blocked = false;

    public bool Spotted => _spotted;
    public bool Blocked => _blocked;
    public BonnieLocation Location
    {
        get => (BonnieLocation)_location;
        protected set => _location = (int)value;
    }

    public BonnieAnimatronic(GameSession gameplayState) : base(gameplayState)
    {
        DImGui.OnDraw += (s, a) =>
        {
            ImGui.Begin("Bonnie");

            ImGui.SeparatorText("Bonnie");
            ImGui.Text($"Movement: {TimeUntilNextMove}");
            ImGui.Text($"Location: {Location}");
            ImGui.Text($"Spotted: {Spotted}");
            ImGui.Text($"Blocked: {Blocked}");

            ImGui.SliderInt("AI", ref _AI, 0, MAX_AI);

            ImGui.SetNextItemWidth(120f);
            ImGui.Combo("##locations", ref _forceLocation, string.Join('\0', Enum.GetNames<BonnieLocation>()));

            ImGui.SameLine();
            bool movePressed = ImGui.Button("Move");
            if (movePressed) ForceToRoom((BonnieLocation)_forceLocation);

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
        AI = nightInfo.BonnieAI;
        Location = nightInfo.BonnieStartingLocation;

        // Reset state.
        _spotted = false;
        _blocked = false;
        _timeUntilNextMove = GetRandomMovementTimeSpan();
    }

    public override void PerformMovement()
    {
        _spotted = false;
        _blocked = false;

        // Bonnie is in the office. Attack.
        if (Location == BonnieLocation.Office)
        {
            Attack();
            return;
        }

        int previousLocation = _location;

        // Bonnie is at the left door.
        if (Location == BonnieLocation.LeftDoor)
        {
            if (_gameSession.IsLeftDoorClosed)
            {
                _timeUntilNextMove = GetRandomMovementTimeSpan();

                // Door is closed, Bonnie backs off to the dining room.
                Location = BonnieLocation.DiningRoom1;

                _gameSession.InvokeAnimatronicMoved(Animatronics.Bonnie, previousLocation, _location);
            }
            else
            {
                // Door is open, Bonnie sneaks in.
                SneakIn();
            }

            return;
        }

        int movements = 1;
        float random = RNG.Gameplay.Float();
        float moveTwiceChance = EasingF.Lerp(MOVE_TWICE_CHANCE_LOWAI, MOVE_TWICE_CHANCE_HIGHAI, AINormalized);

        if (random < moveTwiceChance)
        {
            movements++;
        }

        for (int i = 0; i < movements; i++)
        {
            switch (Location)
            {
                case BonnieLocation.ShowStage:
                    // From the show stage,
                    // Bonnie can move to the dining room, or the backstage.
                    _location = RNG.Gameplay.ChooseInt((int)BonnieLocation.DiningRoom1,
                                                       (int)BonnieLocation.Backstage);
                    break;
                case BonnieLocation.DiningRoom1:
                    Location = BonnieLocation.DiningRoom2;
                    break;
                case BonnieLocation.DiningRoom2:
                    Location = BonnieLocation.WestHall1;
                    break;
                case BonnieLocation.Backstage:
                    Location = BonnieLocation.WestHall1;
                    break;
                case BonnieLocation.WestHall1:
                    // From the start of the west hall,
                    // Bonnie can move to the supply closet, or the end of the west hall.
                    _location = RNG.Gameplay.ChooseInt((int)BonnieLocation.SupplyCloset,
                                                       (int)BonnieLocation.WestHall2);
                    break;
                case BonnieLocation.SupplyCloset:
                    Location = BonnieLocation.LeftDoor;
                    break;
                case BonnieLocation.WestHall2:
                    Location = BonnieLocation.LeftDoor;
                    break;
                // Bonnie stops at the left door.
                // Avoids Bonnie from attacking or entering the office immediately if he moves twice.
                case BonnieLocation.LeftDoor:
                case BonnieLocation.Office:
                    break;
                default:
                    throw new NotImplementedException($"Bonnie location {Location} not implemented");
            }
        }

        if (Location == BonnieLocation.LeftDoor)
        {
            if (_gameSession.IsLeftDoorBroken)
            {
                // When Bonnie arrives at the door, and it's closed,
                // he will stay there for a few seconds.
                SpamDoor();
            }
            else
            {
                // If Bonnie is at the left door, and remains unseen,
                // he will sneak in after a fixed amount of time.
                _timeUntilNextMove = TimeSpan.FromSeconds(SNEAK_IN_TIME_SECS);
            }

            // If Bonnie moves to the left door while it is flashed,
            // he is immediately spotted.
            if (_gameSession.IsLeftDoorFlashed && !_spotted)
            {
                _spotted = true;

                // If the door is closed, add some cooldown.
                // This avoids scenarios where the animatronic is spotted and
                // sneaks into the office at the very last second, before the door is open.
                if (_gameSession.IsLeftDoorClosed)
                {
                    _gameSession.SpottedDoorCooldown(OfficeDoors.Left);
                }

                _gameSession.InvokeAnimatronicSpottedAtDoor(Animatronics.Bonnie, OfficeDoors.Left);
            }
        }
        else
        {
            // If Bonnie is in some room that is not the left door, move normally:
            // Select a random time between min and max movement intervals.
            // Keeps animatronics out of sync, and somewhat unpredictable.
            _timeUntilNextMove = GetRandomMovementTimeSpan();
        }

        _gameSession.InvokeAnimatronicMoved(Animatronics.Bonnie, previousLocation, _location);
    }

    public override void Attack()
    {
        _gameSession.AnimatronicAttacked(Animatronics.Bonnie);
    }

    public override void OnDoorFlashed(OfficeDoors door)
    {
        if (door != OfficeDoors.Left) return;

        // If Bonnie is at the left door, and the door is flashed,
        // he is 'spotted'.
        if (Location == BonnieLocation.LeftDoor && !_spotted)
        {
            if (!_gameSession.IsLeftDoorClosed)
            {
                // When Bonnie is spotted at the door, and it is not closed, the player
                // will only have a few seconds to close it before he sneaks in.
                _timeUntilNextMove = TimeSpan.FromSeconds(SNEAK_IN_SPOTTED_TIME_SECS);
            }

            _spotted = true;
            _gameSession.InvokeAnimatronicSpottedAtDoor(Animatronics.Bonnie, OfficeDoors.Left);
        }

        base.OnDoorFlashed(door);
    }

    public override void OnDoorOpened(OfficeDoors door)
    {
        if (door != OfficeDoors.Left) return;
        if (Location != BonnieLocation.LeftDoor) return;

        // If Bonnie has already been spotted, or blocked at the door,
        // and it is opened, Bonnie sneaks in immediately.
        if (_spotted || _blocked)
        {
            SneakIn();
        }

        base.OnDoorOpened(door);
    }

    public override void OnDoorClosed(OfficeDoors door)
    {
        if (door != OfficeDoors.Left) return;
        if (Location != BonnieLocation.LeftDoor) return;

        // When Bonnie is at the door, and it's closed, he will stay
        // at the door for some seconds.
        if (!_blocked)
        {
            SpamDoor();
            _blocked = true;
        }

        base.OnDoorClosed(door);
    }

    public void ForceToRoom(BonnieLocation location)
    {
        if (Location == BonnieLocation.Office || location == BonnieLocation.ShowStage) return;

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
        // After a random interval, Bonnie will attack no matter what.
        float random = RNG.Gameplay.Float();
        double attackTime = Easing.Lerp(ATTACK_TIME_MIN, ATTACK_TIME_MAX, random);

        _timeUntilNextMove = TimeSpan.FromSeconds(attackTime);

        // Door is open, Bonnie enters the office and breaks buttons.
        Location = BonnieLocation.Office;
        _gameSession.BreakDoor(OfficeDoors.Left);

        _gameSession.InvokeAnimatronicSneakIntoOffice(Animatronics.Bonnie);
        _gameSession.InvokeAnimatronicMoved(Animatronics.Bonnie, (int)BonnieLocation.LeftDoor, _location);
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
        // Make Bonnie stay at the door for a random amount of time, putting some pressure on the player.
        float random = RNG.Gameplay.Float();

        // The higher the AI level, the higher the max possible pressure time is.
        double pressureTimeMinSecs = Easing.Lerp(PRESSURE_TIME_MIN_SECS_LOWAI, PRESSURE_TIME_MIN_SECS_HIGHAI, AINormalized);
        double pressureTimeMaxSecs = Easing.Lerp(PRESSURE_TIME_MAX_SECS_LOWAI, PRESSURE_TIME_MAX_SECS_HIGHAI, AINormalized);
        // An exponential function is used to make high pressure times rarer.
        double pressureTime = Easing.InterpolateExponentialIn(pressureTimeMinSecs, pressureTimeMaxSecs, random, PRESSURE_TIME_CURVE_EXPONENT);

        _timeUntilNextMove = TimeSpan.FromSeconds(pressureTime);
    }
}
