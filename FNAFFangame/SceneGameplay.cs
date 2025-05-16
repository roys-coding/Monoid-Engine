using FMOD;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoidEngine.Audio;
using MonoidEngine.DearImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoidEngine;

namespace FNAFFangame;

public class SceneGameplay : Scene
{
    const double WINDOW_SCARE_COOLDOWN_SECS = 30;
    const float BONNIE_FLASH_SCARE_CHANCE = 0.05f;
    const double KITCHEN_COOLDOWN_SECS = 10;
    const float KITCHEN_CHANCE_INC_PERSEC = (90f) / 60f * 0.01f;

    private GameSession _gameSession;

    private TimeSpan _windowScareCooldown = TimeSpan.Zero;
    private TimeSpan _kitchenCooldown = TimeSpan.Zero;
    private float _kitchenSoundChance = 1f;
    private bool _bonnieFlashScared = false;

    private SpriteSheet _officeSprite;
    private SpriteSheet _leftFlashSprite;
    private SpriteSheet _bonnieFlashLeaveSprite;
    private SpriteSheet _bonnieFlashSprite;
    private SpriteSheet _bonnieOfficeSprite;
    private SpriteSheet _rightFlashSprite;
    private Effect _perspectiveEffect;

    private AnimationFrameSequence _fanAnimation;
    private AnimationFrameSequence _leftDoorAnimation;
    private AnimationFrameSequence _rightDoorAnimation;
    private AnimationFrameSequence _bonnieLeaveAnimation;
    private AnimationFrameSequence _bonnieFlashLeaveAnimation;

    private FMOD_EventInstance _ambienceSound;
    private FMOD_EventInstance _fanSound;
    private FMOD_EventInstance _windowScareSound;
    private FMOD_EventInstance _screamSound;
    private FMOD_EventInstance _screamSlowSound;
    private SoundEmitterActor _leftDoorSound;
    private FMOD_EventInstance _leftFlashSound;
    private FMOD_EventInstance _rightDoorSound;
    private FMOD_EventInstance _rightFlashSound;
    private FMOD_EventInstance _chicaStepsSound;
    private FMOD_EventInstance _chicaKitchenSound;
    private FMOD_EventInstance _bonnieStepsSound;
    private FMOD_EventInstance _bonnieFlashScareSound;
    private FMOD_EventInstance _officeScareSound;
    private FMOD_EventInstance _errorSound;

    public SceneGameplay()
    {
        Camera.CenterOrigin();

        _gameSession = new();
        _gameSession.OnGameStart += OnGameStart;
        _gameSession.OnDoorOpenedOrClosed += OnDoorChange;
        _gameSession.OnDoorFlash += OnDoorFlash;
        _gameSession.OnDoorBreak += OnDoorBreak;
        _gameSession.OnAnimatronicMove += OnAnimatronicMove;
        _gameSession.OnAnimatronicSpottedAtDoor += OnAnimatronicSpottedAtDoor;
        _gameSession.OnAnimatronicAttack += OnAnimatronicAttack;
    }

    private void OnGameStart(object sender, GameSession.OnGameStartEventArgs e)
    {
        StopAllSounds();

        // Restart cooldowns and others.
        _windowScareCooldown = TimeSpan.Zero;
        _kitchenCooldown = TimeSpan.Zero;
        _kitchenSoundChance = 1f;
        _bonnieFlashScared = false;

        // Restart door sprites.
        _leftDoorAnimation.SetAnimation("open");
        _leftDoorAnimation.SkipToEnd();

        _rightDoorAnimation.SetAnimation("open");
        _rightDoorAnimation.SkipToEnd();

        // Restart flash sprites.
        _rightFlashSprite.SetFrame(0);

        // Restart office sprite.
        _officeSprite.SetFrame(0);

        // Restart fan animation.
        _fanAnimation.SetAnimation("on");
        _fanAnimation.Restart();
        _fanAnimation.Play();

        // Restart bonnie leave animation.
        _bonnieFlashLeaveAnimation.SkipToEnd();
        _bonnieLeaveAnimation.SkipToEnd();

        // Set default listener position.
        GameAudio.SetListenerPosition(0, Vector3.Zero);
        GameAudio.SetListenerUp(0, Vector3.UnitY);
        GameAudio.SetListenerForward(0, Vector3.UnitZ);

        // Start sounds.
        _fanSound.Start();
        _ambienceSound.Start();
    }

    public override void BuildScene()
    {
        // Create sound instances.
        _leftDoorSound = new(Transform2D.FromPosition(-1.85f, 0.5f), "event:/office/door");
        _leftFlashSound = GameAudio.CreateEventInstance("event:/office/flash");
        _rightDoorSound = GameAudio.CreateEventInstance("event:/office/door");
        _rightFlashSound = GameAudio.CreateEventInstance("event:/office/flash");
        _fanSound = GameAudio.CreateEventInstance("event:/ambience/fan_loud");
        _windowScareSound = GameAudio.CreateEventInstance("event:/stingers/window_scare");
        _screamSound = GameAudio.CreateEventInstance("event:/jump_scares/scream");
        _screamSlowSound = GameAudio.CreateEventInstance("event:/jump_scares/scream_slow");
        _ambienceSound = GameAudio.CreateEventInstance("event:/ambience/ambience");
        _chicaStepsSound = GameAudio.CreateEventInstance("event:/cues/steps");
        _chicaKitchenSound = GameAudio.CreateEventInstance("event:/cues/dishes_kitchen");
        _bonnieStepsSound = GameAudio.CreateEventInstance("event:/cues/steps");
        _bonnieFlashScareSound = GameAudio.CreateEventInstance("event:/stingers/flash_scare");
        _officeScareSound = GameAudio.CreateEventInstance("event:/stingers/office_scare");
        _errorSound = GameAudio.CreateEventInstance("event:/office/error");

        // Set sound positions.
        //_leftDoorSound.Position = new Vector3(-1.85f, 0f, 0.5f);
        _leftFlashSound.Position = new Vector3(-1.85f, 0f, 0.5f);
        _rightDoorSound.Position = new Vector3(1.85f, 0f, 0.5f);
        _rightFlashSound.Position = new Vector3(1.85f, 0f, 0.5f);
        _fanSound.Position = new Vector3(0.2f, 0.1f, 1.96f);
        _bonnieStepsSound.Position = new Vector3(-1.5f, 0f, 1f);
        _chicaStepsSound.Position = new Vector3(1.5f, 0f, 1f);
        _chicaKitchenSound.Position = new Vector3(4.33f, 0f, 5.65f);

        // Create sprites, sprite actors and animations.
        _perspectiveEffect = ResourcePool.GetEffect("perspective");

        _officeSprite = ResourcePool.CreateSpriteSheet("office", Transform2D.Default);
        _officeSprite.CenterOrigin();

        _leftFlashSprite = ResourcePool.CreateSpriteSheet("flash_left", Transform2D.Default);
        SpriteActor leftFlashActor = new(_leftFlashSprite, Transform2D.FromPosition(-730, -360));

        _bonnieFlashLeaveSprite = ResourcePool.CreateSpriteSheet("bonnie_leave_flash", Transform2D.Default);
        _bonnieFlashLeaveAnimation = new(false);
        _bonnieFlashLeaveAnimation.AddAnimation_StartEndFrames("default", 30, false, 0, 0, 12);
        _bonnieFlashLeaveAnimation.SetAnimation("default");
        _bonnieFlashLeaveSprite.Animation = _bonnieFlashLeaveAnimation;

        SpriteActor bonnieFlashLeaveActor = new(_bonnieFlashLeaveSprite, new(-704f, -360f));

        _bonnieFlashSprite = ResourcePool.CreateSpriteSheet("bonnie_flash", Transform2D.Default);
        SpriteActor bonnieFlashActor = new(_bonnieFlashSprite, new(-704f, -316f));

        _bonnieOfficeSprite = ResourcePool.CreateSpriteSheet("bonnie_office", Transform2D.Default);
        SpriteActor bonnieOfficeActor = new(_bonnieOfficeSprite, new(-703f, -360f));

        _rightFlashSprite = ResourcePool.CreateSpriteSheet("flash_right", Transform2D.Default);
        SpriteActor rightFlashActor = new(_rightFlashSprite, Transform2D.FromPosition(260, -360));

        SpriteSheet fanSprite = ResourcePool.CreateSpriteSheet("fan", Transform2D.Default);
        fanSprite.CenterOrigin();

        _fanAnimation = new(false);
        _fanAnimation.AddAnimation_Frames("on", 60, true, 0, 0, 1, 2);
        _fanAnimation.AddAnimation_StartEndFrames("off", 0, false, 0, 0);

        fanSprite.Animation = _fanAnimation;
        SpriteActor fanActor = new(fanSprite, Transform2D.FromPosition(57, 33));

        SpriteSheet leftDoorSprite = ResourcePool.CreateSpriteSheet("door", Transform2D.Default);
        leftDoorSprite.FlipHorizontal = true;

        _leftDoorAnimation = new(false);
        _leftDoorAnimation.AddAnimation_StartEndFrames("open", 30, false, 0, 14, 0);
        _leftDoorAnimation.AddAnimation_StartEndFrames("close", 30, false, 0, 0, 14);

        leftDoorSprite.Animation = _leftDoorAnimation;
        SpriteActor leftDoorActor = new(leftDoorSprite, Transform2D.FromPosition(-726, -360));

        SpriteSheet rightDoorSprite = ResourcePool.CreateSpriteSheet("door", Transform2D.Default);

        _rightDoorAnimation = new(false);
        _rightDoorAnimation.AddAnimation_StartEndFrames("open", 30, false, 0, 14, 0);
        _rightDoorAnimation.AddAnimation_StartEndFrames("close", 30, false, 0, 0, 14);

        rightDoorSprite.Animation = _rightDoorAnimation;
        SpriteActor rightDoorActor = new(rightDoorSprite, Transform2D.FromPosition(472, -360));

        SpriteSheet leftButtonSprite = ResourcePool.CreateSpriteSheet("buttons", Transform2D.Default);

        SpriteActor leftButtonActor = new(leftButtonSprite, Transform2D.FromPosition(-790, -67));

        SpriteSheet rightButtonSprite = ResourcePool.CreateSpriteSheet("buttons", Transform2D.Default);
        rightButtonSprite.SetFrame(4);

        SpriteActor rightButtonActor = new(rightButtonSprite, Transform2D.FromPosition(700, -67));

        ButtonActor leftDoorButton = new(-800, -100, 80, 120);

        leftDoorButton.AddCallback(ButtonAction.Pressed, (action) =>
        {
            DoorResult result = _gameSession.ToggleDoor(OfficeDoors.Left);

            if (result == DoorResult.FailedBroken)
            {
                _errorSound.Start();
            }
        });

        ButtonActor rightDoorButton = new(693, -100, 80, 120);

        rightDoorButton.AddCallback(ButtonAction.Pressed, (action) =>
        {
            DoorResult result = _gameSession.ToggleDoor(OfficeDoors.Right);

            if (result == DoorResult.FailedBroken)
            {
                _errorSound.Start();
            }
        });

        ButtonActor leftFlashButton = new(-800, 20, 80, 120);

        leftFlashButton.AddCallback(ButtonAction.Pressed, (action) =>
        {
            DoorResult result = _gameSession.ToggleDoorFlash(OfficeDoors.Left);

            if (result == DoorResult.FailedBroken)
            {
                _errorSound.Start();
            }
        });

        ButtonActor rightFlashButton = new(693, 20, 80, 120);

        rightFlashButton.AddCallback(ButtonAction.Pressed, (action) =>
        {
            DoorResult result = _gameSession.ToggleDoorFlash(OfficeDoors.Right);

            if (result == DoorResult.FailedBroken)
            {
                _errorSound.Start();
            }
        });

        SpriteSheet bonnieLeaveSprite = ResourcePool.CreateSpriteSheet("bonnie_leave", Transform2D.Default);

        _bonnieLeaveAnimation = new(false);
        _bonnieLeaveAnimation.AddAnimation_StartEndFrames("default", 30, false, 0, 0, 29);
        _bonnieLeaveAnimation.SetAnimation("default");

        bonnieLeaveSprite.Animation = _bonnieLeaveAnimation;
        SpriteActor bonnieLeaveActor = new(bonnieLeaveSprite, new(-706, -360));

        // Add actors.
        AddActor(_leftDoorSound);
        AddActor(fanActor);
        AddActor(leftFlashActor);
        AddActor(rightFlashActor);
        AddActor(bonnieFlashLeaveActor);
        AddActor(bonnieFlashActor);
        AddActor(leftDoorActor);
        AddActor(rightDoorActor);
        AddActor(leftButtonActor);
        AddActor(rightButtonActor);
        AddActor(leftDoorButton);
        AddActor(rightDoorButton);
        AddActor(leftFlashButton);
        AddActor(rightFlashButton);
        AddActor(bonnieLeaveActor);
        AddActor(bonnieOfficeActor);

        base.BuildScene();
    }

    public override void Initialize()
    {
        base.Initialize();

        _gameSession.Start(GameNight.Seven);
    }

    public override void Terminate()
    {
        // Stop and release sounds.
        StopAllSounds();

        //_leftDoorSound.Release();
        _leftFlashSound.Release();
        _rightDoorSound.Release();
        _rightFlashSound.Release();
        _fanSound.Release();
        _windowScareSound.Release();
        _ambienceSound.Release();
        _chicaStepsSound.Release();
        _bonnieStepsSound.Release();
        _bonnieFlashScareSound.Release();
        _errorSound.Release();
        _chicaKitchenSound.Release();
        _screamSound.Release();
        _screamSlowSound.Release();
        _officeScareSound.Release();

        base.Terminate();
    }

    public override void Update()
    {
        const int CAMERA_MAX_X = 160;
        const float LISTENER_MAX_ROTATION = GameMath.PiOver5F * 0.5f;
        const float SAFE_AREA = 1.1f;

        _gameSession.Update();

        if (GameKeyboard.IsKeyPressed(Keys.B))
        {
            _bonnieLeaveAnimation.Restart();
            _bonnieLeaveAnimation.Play();
            _officeScareSound.Start();
        }

        if (_windowScareCooldown > TimeSpan.Zero)
        {
            _windowScareCooldown -= GameTimes.DeltaTime;
        }

        if (_gameSession.IsLeftDoorFlashed)
        {
            _leftFlashSprite.Alpha = 1f;
        }
        else
        {
            _leftFlashSprite.Alpha = 0f;
        }

        if (_kitchenCooldown > TimeSpan.Zero)
        {
            _kitchenCooldown -= GameTimes.DeltaTime;
        }
        else
        {
            if (_kitchenSoundChance < 1f)
            {
                _kitchenSoundChance += KITCHEN_CHANCE_INC_PERSEC * GameTimes.DeltaSecondsF;
            }
            else if (_kitchenSoundChance > 1f)
            {
                _kitchenSoundChance = 1f;
            }
        }

        if (_gameSession.IsRightDoorFlashed)
        {
            if (_gameSession.Chica.Location == ChicaLocation.RightDoor)
            {
                _rightFlashSprite.SetFrame(1);
            }
            else
            {
                _rightFlashSprite.SetFrame(0);
            }

            _rightFlashSprite.Alpha = 1f;
        }
        else
        {
            _rightFlashSprite.Alpha = 0f;
        }

        if (_gameSession.IsLeftDoorFlashed)
        {
            if (_gameSession.Bonnie.Location == BonnieLocation.LeftDoor)
            {
                _bonnieFlashSprite.Alpha = 1f;
                _bonnieFlashSprite.SetFrame(1);
                _leftFlashSprite.SetFrame(1);
            }
            else
            {
                _bonnieFlashSprite.Alpha = 0f;
                _leftFlashSprite.SetFrame(0);
            }

            _leftFlashSprite.Alpha = 1f;
            _bonnieFlashLeaveSprite.Alpha = 1f;
        }
        else
        {
            if (_gameSession.Bonnie.Location == BonnieLocation.LeftDoor)
            {
                _bonnieFlashSprite.Alpha = 1f;
                _bonnieFlashSprite.SetFrame(0);
            }
            else
            {
                _bonnieFlashSprite.Alpha = 0f;
            }

            _leftFlashSprite.Alpha = 0f;
            _bonnieFlashLeaveSprite.Alpha = 0f;
        }

        if (_gameSession.Bonnie.Location == BonnieLocation.Office)
        {
            _bonnieOfficeSprite.Alpha = 1f;
            _bonnieOfficeSprite.SetFrame(1);
        }
        else
        {
            _bonnieOfficeSprite.Alpha = 0f;
        }

        Vector2 mousePositionNormalized = (-Graphics.GameBounds.Location.ToVector2() + GameMouse.Position) / Graphics.GameBounds.Size.ToVector2();
        mousePositionNormalized -= new Vector2(0.5f);
        mousePositionNormalized *= 2f;
        mousePositionNormalized *= 1f + SAFE_AREA * SAFE_AREA;
        mousePositionNormalized = Vector2.Clamp(mousePositionNormalized, -Vector2.One, Vector2.One);

        float cameraX = CAMERA_MAX_X;
        cameraX *= 1 + (Camera.Transform.ScaleX - 1f) * 5f;
        cameraX *= mousePositionNormalized.X;

        Camera.Transform.PositionX = Lerp.TowardsSmooth(Camera.Transform.PositionX, cameraX, 5f, GameTimes.DeltaSecondsF);

        float cameraXNormalized = Camera.Transform.PositionX / CAMERA_MAX_X;
        float listenerRotation = cameraXNormalized * LISTENER_MAX_ROTATION;

        GameAudio.SetListenerForward(0, Vector3.Transform(Vector3.UnitZ, Matrix.CreateRotationY(listenerRotation)));

        base.Update();
    }

    public override void PreDraw()
    {
        Graphics.Draw.Sprite(_officeSprite);

        base.PreDraw();
    }

    private void StopAllSounds()
    {
        //_leftDoorSound.Stop();
        _leftFlashSound.Stop();
        _rightDoorSound.Stop();
        _rightFlashSound.Stop();
        _fanSound.Stop();
        _windowScareSound.Stop();
        _ambienceSound.Stop();
        _chicaStepsSound.Stop();
        _bonnieStepsSound.Stop();
        _bonnieFlashScareSound.Stop();
        _errorSound.Stop();
        _chicaKitchenSound.Stop();
        _screamSound.Stop();
        _screamSlowSound.Stop();
        _officeScareSound.Stop();
    }

    private void OnDoorChange(object sender, GameSession.DoorOpenedOrClosedEventArgs e)
    {
        string animation = e.IsDoorClosed ? "close" : "open";

        switch (e.Door)
        {
            case OfficeDoors.Left:
                _leftDoorSound.Play();
                _leftDoorAnimation.SetAnimation(animation);
                _leftDoorAnimation.Restart();
                _leftDoorAnimation.Play();
                break;
            case OfficeDoors.Right:
                _rightDoorSound.Start();
                _rightDoorAnimation.SetAnimation(animation);
                _rightDoorAnimation.Restart();
                _rightDoorAnimation.Play();
                break;
            default:
                return;
        }
    }

    private void OnDoorFlash(object sender, GameSession.DoorFlashEventArgs e)
    {
        switch (e.Door)
        {
            case OfficeDoors.Left:
                if (e.IsDoorFlashed) _leftFlashSound.Start();
                else _leftFlashSound.Stop();

                if (e.IsDoorFlashed
                    && (_gameSession.Bonnie.Location == BonnieLocation.WestHall2
                    || _gameSession.Bonnie.Location == BonnieLocation.SupplyCloset))
                {
                    BonnieFlashScare();
                }
                break;
            case OfficeDoors.Right:
                if (e.IsDoorFlashed) _rightFlashSound.Start();
                else _rightFlashSound.Stop();
                break;
            default:
                return;
        }
    }

    private void BonnieFlashScare()
    {
        //if (_bonnieFlashScared) return;

        float random = RNG.Gameplay.Float();

        if (random <= BONNIE_FLASH_SCARE_CHANCE)
        {
            _bonnieFlashScared = true;
            _bonnieFlashScareSound.Start();
            _bonnieFlashLeaveAnimation.Restart();
            _bonnieFlashLeaveAnimation.Play();
        }
    }

    private void OnDoorBreak(object sender, GameSession.DoorBreakEventArgs e)
    {
        switch (e.Door)
        {
            case OfficeDoors.Left:
                _leftFlashSound.Stop();
                _leftDoorAnimation.SetAnimation("open");
                _leftDoorAnimation.SkipToEnd();
                break;
            case OfficeDoors.Right:
                _rightFlashSound.Stop();
                _rightDoorAnimation.SetAnimation("open");
                _rightDoorAnimation.SkipToEnd();
                break;
            default:
                throw new NotImplementedException($"Door {e.Door} not implemented.");
        }
    }

    private void OnAnimatronicMove(object sender, GameSession.AnimatronicMoveEventArgs e)
    {
        switch (e.Animatronic)
        {
            case Animatronics.Chica:
                // Play steps sound if Chica moved away from the door.
                if (e.PreviousLocation == (int)ChicaLocation.RightDoor
                    && e.CurrentLocation != (int)ChicaLocation.Office)
                {
                    _chicaStepsSound.SetVolume(1f);
                    _chicaStepsSound.Start();
                }

                // Play steps sounds when advancing from kitchen to hall.
                if (e.PreviousLocation == (int)ChicaLocation.Kitchen &&
                    e.CurrentLocation == (int)ChicaLocation.EastHall1)
                {
                    _chicaStepsSound.SetVolume(0.5f);
                    _chicaStepsSound.Start();
                }

                // Play steps sounds when advancing from hall1 to hall2.
                if (e.PreviousLocation == (int)ChicaLocation.EastHall1 &&
                    e.CurrentLocation == (int)ChicaLocation.EastHall2)
                {
                    _chicaStepsSound.SetVolume(0.75f);
                    _chicaStepsSound.Start();
                }

                // Start playing Chica's kitchen sound if she is in the kitchen.
                if (e.CurrentLocation == (int)ChicaLocation.Kitchen)
                {
                    float random = RNG.Gameplay.Float();
                     
                    if (_kitchenCooldown <= TimeSpan.Zero && random < _kitchenSoundChance)
                    {
                        _chicaKitchenSound.Start();
                        
                        // Add some cooldown to the kitchen sound.
                        // Avoids the sound to keep playing repeatedly when Chica has high AI and moves a lot.
                        _kitchenSoundChance = 0f;
                        _kitchenCooldown = TimeSpan.FromSeconds(KITCHEN_COOLDOWN_SECS);
                    }
                }

                // Stop playing Chica's kitchen sound if she left the kitchen.
                if (e.PreviousLocation == (int)ChicaLocation.Kitchen)
                {
                    _chicaKitchenSound.Stop();
                }

                break;
            case Animatronics.Bonnie:
                // Play steps sound if Bonnie moved away from the door.
                if (e.PreviousLocation == (int)BonnieLocation.LeftDoor
                    && e.CurrentLocation != (int)BonnieLocation.Office)
                {
                    _bonnieStepsSound.SetVolume(1f);
                    _bonnieStepsSound.Start();
                }

                // Play steps sounds when advancing to hall.
                if ((e.PreviousLocation < e.CurrentLocation)
                    && e.CurrentLocation == (int)BonnieLocation.WestHall1)
                {
                    _bonnieStepsSound.SetVolume(0.5f);
                    _bonnieStepsSound.Start();
                }

                // Play steps sounds when advancing to supply closet/hall2.
                if (e.PreviousLocation < e.CurrentLocation
                    && e.CurrentLocation == (int)BonnieLocation.WestHall2)
                {
                    _bonnieStepsSound.SetVolume(0.75f);
                    _bonnieStepsSound.Start();
                }
                break;
            case Animatronics.Freddy:
                break;
            case Animatronics.Foxy:
                break;
            default:
                return;
        }
    }

    private void OnAnimatronicSpottedAtDoor(object sender, GameSession.AnimatronicSpottedAtDoorEventArgs e)
    {
        if (_windowScareCooldown > TimeSpan.Zero) return;
        if (e.DoorState.HasFlag(DoorStates.Closed)) return;

        _windowScareCooldown = TimeSpan.FromSeconds(WINDOW_SCARE_COOLDOWN_SECS);
        _windowScareSound.Start();
    }

    private void OnAnimatronicAttack(object sender, GameSession.AnimatronicAttackEventArgs e)
    {
        switch (e.Animatronic)
        {
            case Animatronics.Chica:
                break;
            case Animatronics.Bonnie:
                break;
            case Animatronics.Freddy:
                break;
            case Animatronics.Foxy:
                break;
            default:
                throw new NotImplementedException($"Animatronic {e.Animatronic} not implemented.");
        }

        _screamSound.Start();
    }
}
