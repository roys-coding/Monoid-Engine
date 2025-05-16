using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using MyMonoGameApp.DearImGui;
using System.IO;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using System.Linq;
using MyMonoGameApp.Audio;
using System.Reflection;
using static MyMonoGameApp.MainGame;

namespace MyMonoGameApp;

/// <summary>
/// Main class of the game.
/// </summary>
public class MainGame : Game
{
    public enum StartupConfig
    {
        None = 0,
        FMODLiveUpdateEnabled = 1,
    }

    /// <summary>
    /// Directory where most resources are located and loaded from.
    /// </summary>
    public const string CONTENT_DIRECTORY = "Content";

    private static MainGame _instance;

    /// <summary>
    /// Singleton instance.
    /// </summary>
    /// <remarks>Attempting to create multiple instances of this class will result in an exception.</remarks>
    public static MainGame Instance
    {
        get
        {
            if (_instance == null) throw new InvalidOperationException("A MainGame instance must be initialized before being referenced.");

            return _instance;
        }
    }

    public static StartupConfig Config { get; private set; }

    /// <summary>
    /// Instantiates a new instance of the <see cref="MainGame"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if more than one instance is instantiated in the same process.</exception>
    public MainGame(StartupConfig config = StartupConfig.None)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException("A MainGame instance is already initialized. Only one instance can exist at a time.");
        }

        _instance = this;
        Config = config;

        Graphics.Initialize(this);
        GameAudio.Initialize();
        GameKeyboard.OnKeyPressed += OnKeyPressed;

        // ImGui classes.
        GameConsole.Initialize();

        Content.RootDirectory = CONTENT_DIRECTORY;
        IsMouseVisible = true;

        // Set inactive sleep time to 0 to stop game from

        InactiveSleepTime = TimeSpan.FromMilliseconds(0);
        //MaxElapsedTime = TimeSpan.FromSeconds(1f / 0.1f);
    }

    /// <summary>
    /// Called after <see cref="Initialize"/> and <see cref="LoadContent"/>, but before the first update call.
    /// </summary>
    protected override void BeginRun()
    {
        base.BeginRun();
    }

    /// <summary>
    /// Called at start-up, before the first update call, and before <see cref="LoadContent"/>.
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        RNG.LoadPerlinNoise();

        // Initialize input.
        GameKeyboard.Initialize();
        GameMouse.Initialize();
        SceneManager.Initialize();

        //SceneManager.RegisterScene("main_menu", new SceneMainMenu());
        //SceneManager.RegisterScene("gameplay", new SceneGameplay());

        //SceneManager.ChangeScene("main_menu");

        SceneManager.RegisterScene("font", new FontTestScene());
        SceneManager.ChangeScene("font");

        DImGui.Initialize(this);

        DImGui.OnDraw += (s, a) =>
        {
            ImGui.Begin($"{Fonts.Lucide.Gamepad2} Game");

            ImGui.SeparatorText("Time");

            float timeScale = (float) GameTime.TimeScale;
            bool timeChanged = ImGui.SliderFloat("Scale", ref timeScale, 0f, 500f, "%.2f x", ImGuiSliderFlags.Logarithmic);
            if (timeChanged) GameTime.TimeScale = timeScale;

            ImGui.Text($"Total: {GameTime.TotalElapsed}");
            ImGui.Text($"TotalRaw: {GameTime.TotalElapsedRaw}");

            bool mainMenuPressed = ImGui.Button("Main menu");
            if (mainMenuPressed) SceneManager.ChangeScene("main_menu");

            ImGui.End();
        };
    }

    /// <summary>
    /// Runs at start-up. All necessary resources should be loaded here.
    /// </summary>
    protected override void LoadContent()
    {
        ResourcePool.LoadContent(Content);
        GameAudio.LoadContent();
    }

    /// <summary>
    /// Updates the game's logic.
    /// </summary>
    /// <param name="gameTime">Information in regards time, such as delta time and total game time.</param>
    protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
        GameTime.OnUpdate(gameTime);

        // Update input.
        GameKeyboard.Update();
        GameMouse.Update();

        GameAudio.Update();

        SceneManager.Update();

        base.Update(gameTime);
    }

    /// <summary>
    /// Called before every call draw.
    /// </summary>
    /// <remarks>Return value determines whether to skip the next draw call.</remarks>
    /// <returns>
    /// <c>false</c> to skip next draw call;<br/>
    /// <c>true</c> to draw normally.
    /// </returns>
    protected override bool BeginDraw()
    {
        return base.BeginDraw();
    }

    /// <summary>
    /// Renders the game to the back-buffer.
    /// </summary>
    /// <param name="gameTime">Information in regards time, such as delta time and total game time.</param>
    protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
    {
        base.Draw(gameTime);

        Graphics.DrawGame();
        DImGui.Render(gameTime);
    }

    /// <summary>
    /// Called after every draw call. Presents the rendered frame in the game window.
    /// </summary>
    protected override void EndDraw()
    {
        base.EndDraw();
    }

    /// <summary>
    /// Called once the game loop has been terminated before exiting.
    /// </summary>
    /// <remarks>Called before <see cref="OnExiting(object, EventArgs)"/>.</remarks>
    protected override void EndRun()
    {
        base.EndRun();
    }

    /// <summary>
    /// Runs right before the game is closed.
    /// </summary>
    /// <remarks>Called before <see cref="UnloadContent"/>.<br/>
    /// Called after <see cref="EndRun"/>.</remarks>
    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        SceneManager.Terminate();
        GameKeyboard.OnKeyPressed -= OnKeyPressed;

        base.OnExiting(sender, args);
    }

    /// <summary>
    /// Runs before the game is closed. All non-previously-unloaded resources should be unloaded here.
    /// </summary>
    /// <remarks>Called after <see cref="OnExiting(object, EventArgs)"/>.</remarks>
    protected override void UnloadContent()
    {
        GameAudio.UnloadContent();
        GameAudio.Release();
        DImGui.Terminate();

        base.UnloadContent();
    }

    /// <summary>
    /// Called when the game gains focus.
    /// </summary>
    protected override void OnActivated(object sender, EventArgs args)
    {
        base.OnActivated(sender, args);
    }

    /// <summary>
    /// Called when the game loses focus.
    /// </summary>
    protected override void OnDeactivated(object sender, EventArgs args)
    {
        base.OnDeactivated(sender, args);
    }

    private void OnKeyPressed(object sender, GameKeyboard.KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case Keys.Enter:
                if (GameKeyboard.IsKeyDown(Keys.LeftAlt)) ToggleFullscreen();
                break;
            case Keys.F11:
                ToggleFullscreen();
                break;
            default:
                break;
        }
    }

    private static void ToggleFullscreen()
    {        
        switch (Graphics.ActiveWindowMode)
        {
            case WindowMode.Windowed:
                Graphics.SetWindowMode(WindowMode.FullscreenBorderless);
                break;
            case WindowMode.Fullscreen:
            case WindowMode.FullscreenBorderless:
                Graphics.SetWindowMode(WindowMode.Windowed);
                break;
            default:
                break;
        }
    }
}