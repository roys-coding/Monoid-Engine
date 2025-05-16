using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoidEngine.Audio;

namespace MonoidEngine;

[Flags]
public enum ButtonAction
{
    All = MouseEnter | MouseLeave | Pressed | Released,
    MouseEnter = 1 << 0,
    MouseLeave = 1 << 1,
    Pressed = 1 << 2,
    Released = 1 << 3
}

public class ButtonActor : Actor2D
{
    public delegate void Callback(ButtonAction action);

    protected readonly List<Callback> _listeners = new();

    protected MouseInteractiveRegion _interactiveRegion;
    protected FMOD_EventDescription _mouseEnterSoundDescription;
    protected FMOD_EventDescription _mouseLeaveSoundDescription;
    protected FMOD_EventDescription _pressedSoundDescription;
    protected FMOD_EventDescription _releasedSoundDescription;

    public ButtonActor(Rectangle region)
    {
        _interactiveRegion = new(region);
    }
    public ButtonActor(Point position, Point size) : this(new Rectangle(position, size)) { }
    public ButtonActor(int x, int y, int width, int height) : this(new Rectangle(x, y, width, height)) { }

    public override void Initialize()
    {
        _interactiveRegion.Initialize();
        _interactiveRegion.OnMouseEnter += OnMouseEnter;
        _interactiveRegion.OnMouseLeave += OnMouseLeave;
        _interactiveRegion.OnPressed += OnPressed;
        _interactiveRegion.OnReleased += OnReleased;
        base.Initialize();
    }

    public override void Terminate()
    {
        _interactiveRegion.Terminate();
        _interactiveRegion.OnMouseEnter -= OnMouseEnter;
        _interactiveRegion.OnMouseLeave -= OnMouseLeave;
        _interactiveRegion.OnPressed -= OnPressed;
        _interactiveRegion.OnReleased -= OnReleased;
        base.Terminate();
    }

    public override void OnAdded(Scene scene)
    {
        _interactiveRegion.Camera = scene.Camera;
        base.OnAdded(scene);
    }

    public override void OnRemoved(Scene scene)
    {
        _interactiveRegion.Camera = null;
        base.OnRemoved(scene);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Draw()
    {
        Graphics.Draw.Rectangle(_interactiveRegion.Position.ToVector2(), _interactiveRegion.Size.ToVector2(), Color.White * 0.5f);
        base.Draw();
    }

    public void SetSound(ButtonAction action, string soundPath)
    {
        FMOD_EventDescription eventDescription = GameAudio.GetEventDescription(soundPath);

        if (action.HasFlag(ButtonAction.MouseEnter))
            _mouseEnterSoundDescription = eventDescription;

        if (action.HasFlag(ButtonAction.MouseLeave))
            _mouseLeaveSoundDescription = eventDescription;

        if (action.HasFlag(ButtonAction.Pressed))
            _pressedSoundDescription = eventDescription;

        if (action.HasFlag(ButtonAction.Released)) 
            _releasedSoundDescription = eventDescription;
    }

    public void AddCallback(ButtonAction action, Callback callback)
    {
        _listeners.Add(currentAction =>
        {
            if (currentAction.HasFlag(action)) callback.Invoke(currentAction);
        });
    }

    private void OnMouseEnter(object sender, EventArgs e)
    {
        GameAudio.Play(_mouseEnterSoundDescription);

        foreach(Callback listener in _listeners)
        {
            listener.Invoke(ButtonAction.MouseEnter);
        }
    }

    private void OnMouseLeave(object sender, EventArgs e)
    {
        GameAudio.Play(_mouseLeaveSoundDescription);

        foreach (Callback listener in _listeners)
        {
            listener.Invoke(ButtonAction.MouseLeave);
        }
    }

    private void OnPressed(object sender, EventArgs e)
    {
        GameAudio.Play(_pressedSoundDescription);

        foreach (Callback listener in _listeners)
        {
            listener.Invoke(ButtonAction.Pressed);
        }
    }

    private void OnReleased(object sender, EventArgs e)
    {
        GameAudio.Play(_releasedSoundDescription);

        foreach (Callback listener in _listeners)
        {
            listener.Invoke(ButtonAction.Released);
        }
    }
}
