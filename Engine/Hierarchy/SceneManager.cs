using Hexa.NET.ImGui;

using MonoidEngine.DearImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

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
