using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using MonoidEngine.DearImGui;
using System.IO;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using System.Linq;
using MonoidEngine.Audio;
using System.Reflection;
using MonoidEngine.Input;
using MonoidEngine.Graphics;

namespace MonoidEngine;

/// <summary>
/// Main class of the game engine.
/// </summary>
public class Monoid : Game
{
    public enum StartupParameters
    {
        None = 0,
        FMODLiveUpdateEnabled = 1,
    }

    /// <summary>
    /// Directory where most resources are located and loaded from.
    /// </summary>
    public const string CONTENT_DIRECTORY = "Content";

    private static Monoid _instance;

    /// <summary>
    /// Singleton instance.
    /// </summary>
    /// <remarks>Attempting to create multiple instances of this class will result in an exception.</remarks>
    public static Monoid Instance
    {
        get
        {
            if (_instance == null) throw new InvalidOperationException("A MainGame instance must be initialized before being referenced.");

            return _instance;
        }
    }

    public static StartupParameters Parameters { get; private set; }

    /// <summary>
    /// Instantiates a new instance of the <see cref="Monoid"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if more than one instance is instantiated in the same process.</exception>
    public Monoid(StartupParameters config = StartupParameters.None)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException("A MainGame instance is already initialized. Only one instance can exist at a time.");
        }

        _instance = this;
        Parameters = config;

        GraphicsManager.Initialize(this);
        GameAudio.Initialize();
        GameInput.Keyboard.OnKeyPressed += OnKeyPressed;

        IsMouseVisible = true;

        // Set inactive sleep time to 0 to stop game from freezing while unfocused.

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

        // Initialize input.
        GameInput.Initialize();

        DImGui.Initialize(this);

        DImGui.OnDraw += (s, a) =>
        {
            ImGui.Begin($"{Fonts.Lucide.Gamepad2} Game");

            bool timeHeaderOpen = ImGui.CollapsingHeader("Time");

            if (timeHeaderOpen)
            {
                float timeScale = (float)GameTimes.GlobalTimeScale;
                bool timeChanged = ImGui.SliderFloat("Scale", ref timeScale, 0f, 100f, "%.2f x", ImGuiSliderFlags.Logarithmic);
                if (timeChanged) GameTimes.GlobalTimeScale = timeScale;

                ImGui.Text($"Total: {GameTimes.TotalElapsed}");
                ImGui.Text($"TotalRaw: {GameTimes.TotalElapsedRaw}");
                ImGui.Separator();
            }

            int scalingMode = (int)DisplayManager.ActiveScalingMode;
            bool comboChanged = ImGui.Combo("Scaling", ref scalingMode, "ScaleToFit\0Stretch\0Crop\0Integer\0");
            if (comboChanged) DisplayManager.ActiveScalingMode = (ScalingMode)scalingMode;

            bool exitPressed = ImGui.Button("Exit Game");
            if (exitPressed)
            {
                Exit();
            }

            ImGui.End();
        };
    }

    /// <summary>
    /// Runs at start-up. All necessary resources should be loaded here.
    /// </summary>
    protected override void LoadContent()
    {
        GameAudio.LoadContent();
    }

    /// <summary>
    /// Updates the game's logic.
    /// </summary>
    /// <param name="gameTime">Information in regards time, such as delta time and total game time.</param>
    protected override void Update(GameTime gameTime)
    {
        GameTimes.OnUpdate(gameTime);

        GameInput.Update();
        GameAudio.Update();

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
    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        GraphicsManager.DrawGame();

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
    /// <remarks>Called before <see cref="OnExiting(object, ExitingEventArgs)"/>.</remarks>
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
        GameInput.Keyboard.OnKeyPressed -= OnKeyPressed;

        base.OnExiting(sender, args);
    }

    /// <summary>
    /// Runs before the game is closed. All non-previously-unloaded resources should be unloaded here.
    /// </summary>
    /// <remarks>Called after <see cref="OnExiting(object, ExitingEventArgs)"/>.</remarks>
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

    private void OnKeyPressed(object sender, GameInput.Keyboard.KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case Keys.Enter:
                if (GameInput.Keyboard.IsKeyDown(Keys.LeftAlt)) ToggleFullscreen();
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
        switch (DisplayManager.ActiveWindowMode)
        {
            case WindowMode.Windowed:
                DisplayManager.SetWindowMode(WindowMode.FullscreenBorderless);
                break;
            case WindowMode.Fullscreen:
            case WindowMode.FullscreenBorderless:
                DisplayManager.SetWindowMode(WindowMode.Windowed);
                break;
            default:
                break;
        }
    }
}