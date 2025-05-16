using Microsoft.Xna.Framework.Input;
using System;

namespace MyMonoGameApp;

/// <summary>
/// Methods and events used for capturing keyboard inputs.
/// </summary>
public static class GameKeyboard
{
    /// <summary>
    /// Arguments for events related to keyboard inputs.
    /// </summary>
    public class KeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// Key related to this event.
        /// </summary>
        public readonly Keys Key;
        /// <summary>
        /// Input state of the key related to this event.
        /// </summary>
        public readonly InputState InputState;

        /// <summary>
        /// Arguments for events related to keyboard inputs.
        /// </summary>
        public KeyboardEventArgs(Keys key, InputState inputState)
        {
            Key = key;
            InputState = inputState;
        }
    }

    /// <summary>
    /// Invoked when a key is pressed.
    /// </summary>
    public static event EventHandler<KeyboardEventArgs> OnKeyPressed;
    /// <summary>
    /// Invoked every frame while a key is being held down.
    /// </summary>
    public static event EventHandler<KeyboardEventArgs> OnKeyDown;
    /// <summary>
    /// Invoked when a key is released.
    /// </summary>
    public static event EventHandler<KeyboardEventArgs> OnKeyReleased;

    private static readonly Keys[] _keys = Enum.GetValues<Keys>();

    private static KeyboardState _previousState;
    private static KeyboardState _currentState;

    /// <summary>
    /// Gets whether any <c>Shift</c> key is being held down.
    /// </summary>
    public static bool Shift => IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
    /// <summary>
    /// Gets whether any <c>Alt</c> key is being held down.
    /// </summary>
    public static bool Alt => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);
    /// <summary>
    /// Gets whether any <c>Ctrl</c> key is being held down.
    /// </summary>
    public static bool Control => IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);

    /// <summary>
    /// Must be called at start-up.
    /// </summary>
    public static void Initialize()
    {
        _currentState = Keyboard.GetState();
        _previousState = _currentState;
    }

    /// <summary>
    /// Must be called on every update call.
    /// </summary>
    public static void Update()
    {
        if (!MainGame.Instance.IsActive) return;

        _previousState = _currentState;
        _currentState = Keyboard.GetState();

        foreach (Keys key in _keys)
        {
            if (_currentState.IsKeyDown(key))
            {
                if (_previousState.IsKeyDown(key))
                {
                    // Key is held down.
                    OnKeyDown?.Invoke(null, new(key, InputState.Down));
                }
                else
                {
                    OnKeyPressed?.Invoke(null, new(key, InputState.Pressed));
                }
            }
            else if (_previousState.IsKeyDown(key))
            {
                // Key was just released.
                OnKeyReleased?.Invoke(null, new(key, InputState.Released));
            }
        }
    }

    /// <summary>
    /// Checks if the specified <paramref name="key"/> is up (not pressed).
    /// </summary>
    public static bool IsKeyUp(Keys key) =>
        _currentState.IsKeyUp(key);

    /// <summary>
    /// Checks if the specified <paramref name="key"/> was pressed during this frame.
    /// </summary>
    public static bool IsKeyPressed(Keys key) =>
        _previousState.IsKeyUp(key) && _currentState.IsKeyDown(key);

    /// <summary>
    /// Checks if the specified <paramref name="key"/> is being held down.
    /// </summary>
    public static bool IsKeyDown(Keys key) =>
        _currentState.IsKeyDown(key);

    /// <summary>
    /// Checks if the specified <paramref name="key"/> was released this frame.
    /// </summary>
    public static bool IsKeyReleased(Keys key) =>
        _previousState.IsKeyDown(key) && _currentState.IsKeyUp(key);

    /// <summary>
    /// Gets the current <see cref="InputState"/> of the specified <paramref name="key"/>.
    /// </summary>
    public static InputState GetInputState(Keys key)
    {
        if (IsKeyUp(key)) return InputState.Up;
        if (IsKeyDown(key)) return InputState.Down;
        if (IsKeyPressed(key)) return InputState.Pressed;

        return InputState.Released;
    }
}