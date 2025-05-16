using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoidEngine;

namespace FNAFFangame;

public abstract class Animatronic
{
    public const int MAX_AI = 20;

    protected int _AI = 0;
    protected int _location = 0;
    protected TimeSpan _timeUntilNextMove = TimeSpan.Zero;

    protected readonly GameSession _gameSession;
    protected float AINormalized => (float)_AI / MAX_AI;
    public TimeSpan TimeUntilNextMove => _timeUntilNextMove;

    public int AI
    {
        get => _AI;
        set
        {
            if (value > MAX_AI)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"AI must not be greater than {MAX_AI}");
            }

            _AI = value;
        }
    }

    public Animatronic(GameSession gameSession)
    {
        if (gameSession == null)
        {
            throw new ArgumentNullException(nameof(gameSession));
        }

        _gameSession = gameSession;
    }

    public virtual void Update()
    {
        if (AI == 0) return;

        // Decrease movement timer.
        _timeUntilNextMove -= GameTimes.DeltaTime;

        // Move when timer reaches zero.
        if (_timeUntilNextMove <= TimeSpan.Zero)
        {
            PerformMovement();
        }
    }

    public abstract void OnStart(NightInfo nightInfo);
    public abstract void PerformMovement();
    public abstract void Attack();
    public virtual void OnDoorFlashed(OfficeDoors door) { }
    public virtual void OnDoorOpened(OfficeDoors door) { }
    public virtual void OnDoorClosed(OfficeDoors door) { }
}
