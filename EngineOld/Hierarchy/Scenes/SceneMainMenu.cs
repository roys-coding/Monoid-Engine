
using Microsoft.Xna.Framework.Graphics;
using MyMonoGameApp.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

public class SceneMainMenu : Scene
{
    private readonly FMOD_EventInstance _musicInstance;
    private readonly Sprite _staticSprite;

    public SceneMainMenu()
    {
        _musicInstance = GameAudio.CreateEventInstance("event:/music/main_menu");

        SpriteSheet backgroundSpritesheet = ResourcePool.CreateSpriteSheet("main_menu", Transform2D.Default);
        SpriteSheet staticSpritesheet = ResourcePool.CreateSpriteSheet("static", Transform2D.Default);

        AnimationFrameRandomizer backgroundAnimation = new(false);
        backgroundAnimation.SetMode(AnimationFrameRandomizer.FrameMode.ShuffleBursts);
        backgroundAnimation.SetHoldFrame(0);
        backgroundAnimation.SetFrameWeight(8, 0);
        backgroundAnimation.SetFrameWeight(20, 1, 2);
        backgroundAnimation.SetFrameWeight(5, 3);

        backgroundAnimation.SetFrameDuration(0.05, 0.1);
        backgroundAnimation.SetHoldDuration(0.5, 10);
        backgroundAnimation.SetBurstDuration(0.1, 0.25);
        backgroundAnimation.Play();

        AnimationFrameSequence staticAnimation = new(false);
        staticAnimation.AddAnimation_Frames("default", 60, true, 0, 0, 1, 2, 3, 4, 5, 6, 7);
        staticAnimation.SetAnimation("default");
        staticAnimation.Play();

        backgroundSpritesheet.Animation = backgroundAnimation;
        staticSpritesheet.Animation = staticAnimation;
        staticSpritesheet.Alpha = 0.5f;

        SpriteActor backgroundActor = new(backgroundSpritesheet, Transform2D.Default);
        SpriteActor staticActor = new(staticSpritesheet, Transform2D.Default);

        //SpriteFont consolaFont = ResourcePool.GetFont("consola");
        //TextButtonActor newGameButton = new(consolaFont, "New Game", 175, 400, 200, 40);
        //TextButtonActor continueButton = new(consolaFont, "Continue", 175, 480, 200, 40);

        //newGameButton.AddCallback(ButtonAction.Pressed, action =>
        //{
        //    SceneManager.ChangeScene("gameplay");
        //});

        //newGameButton.SetHoveredPrefix(">> ");
        //continueButton.SetHoveredPrefix(">> ");
        //newGameButton.SetSound(ButtonAction.MouseEnter | ButtonAction.Pressed, "event:/ui/select");
        //continueButton.SetSound(ButtonAction.MouseEnter | ButtonAction.Pressed, "event:/ui/select");

        AddActor(backgroundActor);
        AddActor(staticActor);
        //AddActor(newGameButton);
        //AddActor(continueButton);

        _staticSprite = staticSpritesheet;
    }

    public override void Initialize()
    {
        _musicInstance.Start();
        base.Initialize();
    }

    public override void Update()
    {
        _staticSprite.Alpha = 0.25f + (0.8f - 0.25f) * RNG.Perlin();
        base.Update();
    }

    public override void PreDraw()
    {
        base.PreDraw();
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override void PostDraw()
    {
        base.PostDraw();
    }

    public override void Terminate()
    {
        _musicInstance.Stop();
        base.Terminate();
    }
}
