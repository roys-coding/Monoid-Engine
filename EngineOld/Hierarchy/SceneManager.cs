using Hexa.NET.ImGui;

using MyMonoGameApp.DearImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

public static class SceneManager
{
    private static readonly Dictionary<string, Scene> _scenes = new();
    private static Scene _activeScene;
    private static bool _isInitialized = false;

    public static Scene ActiveScene => _activeScene;

    public static void Initialize()
    {
        if (_isInitialized) return;

        _isInitialized = true;
    }

    [ConsoleCommand("scenes", "Shows a list of all the scenes registered in the Scene Manager.")]
    public static bool Command_Scenes()
    {
        GameConsole.Info($"Registered scenes: {string.Join(", ", _scenes.Keys)}");
        return true;
    }

    [ConsoleCommand("reloadscene", "Reloads the active scene.")]
    public static bool Command_SceneReload()
    {
        if (_activeScene == null)
        {
            GameConsole.Error("Could not reload scene. No active scene set.");
            return false;
        }

        _activeScene.Terminate();
        _activeScene.Initialize();
        return true;
    }

    [ConsoleCommand("setscene", "Changes the active scene.")]
    [ConsoleCommandParameter(GameConsole.ParameterType.String, "Scene ID", "ID of the desired scene", true, "")]
    public static bool Command_Scene(string sceneID)
    {
        if (!_scenes.ContainsKey(sceneID))
        {
            GameConsole.Error($"Could not change scene. Scene under the ID '{sceneID}' not found.");
            return false;
        }

        ChangeScene(sceneID);

        return true;
    }
    
    public static void RegisterScene(string ID, Scene scene)
    {
        if (_scenes.ContainsKey(ID))
        {
            throw new InvalidOperationException($"A scene under the ID '{ID}' already exists.");
        }

        _scenes.Add(ID, scene);
    }

    public static void ChangeScene(string ID)
    {
        if (!_scenes.ContainsKey(ID))
        {
            throw new InvalidOperationException($"Could not find a scene under the ID '{ID}'.");
        }

        Scene scene = _scenes[ID];

        // Swap scene.
        _activeScene?.Terminate();
        _activeScene = scene;
        scene.Initialize();
    }

    public static void Update()
    {
        ActiveScene?.Update();
    }

    public static void Draw()
    {
        ActiveScene?.Draw();
    }

    public static void Terminate()
    {
        _activeScene?.Terminate();
    }
}
