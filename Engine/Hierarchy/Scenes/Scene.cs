using Hexa.NET.ImGui;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

public abstract class Scene
{
    protected readonly List<Actor> _actors = new();
    protected readonly Dictionary<string, Actor> _taggedActors = new();
    public readonly Camera Camera = new(Vector2.Zero, Graphics.GameBounds.Location, Graphics.GameBounds.Size);

    public Scene()
    {
        BuildScene();
    }

    /// <summary>
    /// Called only once after the scene is instantiated.
    /// </summary>
    /// <remarks>Actors, resources, and other necessary objects should be created here.</remarks>
    public virtual void BuildScene()
    {

    }

    /// <summary>
    /// Should initialize actors, load dynamic resources, create necessary objects, etc.
    /// </summary>
    public virtual void Initialize()
    {
        foreach (Actor actor in _actors)
        {
            actor.Initialize();
        }
    }

    /// <summary>
    /// Updates all actors.
    /// </summary>
    public virtual void Update()
    {
        Camera.Update();

        foreach (Actor actor in _actors)
        {
            actor.Update();
        }
    }

    /// <summary>
    /// Draws all actors.
    /// </summary>
    public virtual void Draw()
    {
        Graphics.Viewport = Camera.Viewport;
        Graphics.Begin(null, Camera.Matrix);

        PreDraw();
        foreach (Actor actor in _actors) actor.Draw();
        PostDraw();

        Graphics.ResetViewport();
        Graphics.End();
    }

    /// <summary>
    /// Called before all actors are drawn.
    /// </summary>
    public virtual void PreDraw()
    {

    }

    /// <summary>
    /// Called after all actors are drawn.
    /// </summary>
    public virtual void PostDraw()
    {

    }

    /// <summary>
    /// Should terminate actors, release dynamic resources, dispose objects, etc.
    /// </summary>
    public virtual void Terminate()
    {
        foreach(Actor actor in _actors)
        {
            actor.Terminate();
        }
    }

    /// <summary>
    /// Adds the specified <paramref name="actor"/> to the scene.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the actor was successfully added;<br/>
    /// <c>false</c> if the actor was already in the scene.
    /// </returns>
    public bool AddActor(Actor actor)
    {
        if (_actors.Contains(actor)) return false;

        _actors.Add(actor);
        actor.OnAdded(this);

        return true;
    }

    /// <summary>
    /// Removes the specified <paramref name="actor"/> to the scene.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the actor was successfully removed;<br/>
    /// <c>false</c> if the actor was not in the scene.
    /// </returns>
    public bool RemoveActor(Actor actor)
    {
        if (!_actors.Contains(actor)) return false;

        _actors.Remove(actor);
        actor.OnRemoved(this);

        return true;
    }

    /// <summary>
    /// Tags an actor, making it accessible by it's tag.
    /// </summary>
    /// <param name="actor">Actor that will be tagged.</param>
    /// <param name="uniqueTag">Tag that will be assigned to the actor.</param>
    /// <remarks>Use <see cref="TryGetActor{T}(string, out T)"/> to retrieve an actor.</remarks>
    /// <returns>
    /// <c>true</c> if the actor was tagged successfully.<br/>
    /// <c>false</c> if an actor under the same ID already existed.
    /// </returns>
    public bool TagActor(Actor actor, string uniqueTag)
    {
        if (_taggedActors.ContainsKey(uniqueTag)) return false;

        _taggedActors.Add(uniqueTag, actor);
        return true;
    }

    /// <summary>
    /// Retrieves an actor by it's tag.
    /// </summary>
    /// <param name="actor">When this method returns, will equal to the retrieved actor, or <c>null</c> if no actor could be retrieved.</param>
    /// <param name="uniqueTag">Tag previously assigned to the desired actor.</param>
    /// <returns>
    /// <c>true</c> if the actor was retrieved successfully.<br/>
    /// <c>false</c> if there was no actor under the specified tag.
    /// </returns>
    public bool TryGetActor<T>(string uniqueTag, out T actor) where T : Actor
    {
        actor = null;
        if (!_taggedActors.ContainsKey(uniqueTag)) return false;

        actor = _taggedActors[uniqueTag] as T;
        return true;
    }
}
