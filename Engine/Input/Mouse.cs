using FMOD;

using Microsoft.Xna.Framework.Input;
using System;

namespace MonoidEngine.Input;

public static partial class GameInput
{
    /// <summary>
    /// Buttons in a mouse.
    /// </summary>
    public enum MouseButtons
    {
        /// <summary>
        /// Left mouse button.
        /// </summary>
        Left,
        /// <summary>
        /// Middle mouse button.
        /// </summary>
        Middle,
        /// <summary>
        /// Right mouse button.
        /// </summary>
        Right,
        /// <summary>
        /// Forward mouse button.
        /// </summary>
        /// <remarks>Usually located at the side of the mouse, button furthest from the wrist.</remarks>
        Forward,
        /// <summary>
        /// Backward mouse button.
        /// </summary>
        /// <remarks>Usually located at the side of the mouse, button closest to the wrist.</remarks>
        Backward
    }

    /// <summary>
    /// Directions to which the mouse scroll wheel can move.
    /// </summary>
    public enum ScrollDirection
    {
        /// <summary>
        /// Scroll up.
        /// </summary>
        Up,
        /// <summary>
        /// Scroll down.
        /// </summary>
        Down,
        /// <summary>
        /// Scroll left.
        /// </summary>
        Left,
        /// <summary>
        /// Scroll right.
        /// </summary>
        Right
    }

    /// <summary>
    /// Methods and events used for capturing mouse inputs.
    /// </summary>
    public static class Mouse
    {
        /// <summary>
        /// Arguments for events related to mouse movement.
        /// </summary>
        public class MouseMoveEventArgs : EventArgs
        {
            /// <summary>
            /// Position of the mouse cursor when this event was raised.
            /// </summary>
            public readonly Vector2 MousePosition;
            /// <summary>
            /// Position of the mouse cursor one frame before this event was raised.
            /// </summary>
            public readonly Vector2 PreviousMousePosition;

            /// <summary>
            /// Arguments for events related to mouse movement.
            /// </summary>
            public MouseMoveEventArgs(Vector2 mousePosition, Vector2 previousMousePosition)
            {
                MousePosition = mousePosition;
                PreviousMousePosition = previousMousePosition;
            }
        }

        /// <summary>
        /// Arguments for events related to mouse buttons.
        /// </summary>
        public class MouseButtonEventArgs : EventArgs
        {
            /// <summary>
            /// Mouse button related to this event.
            /// </summary>
            public readonly MouseButtons Button;
            /// <summary>
            /// Input state of the mouse button related to this event.
            /// </summary>
            public readonly InputState InputState;
            /// <summary>
            /// Position of the mouse cursor when this event was raised.
            /// </summary>
            public readonly Vector2 MousePosition;

            /// <summary>
            /// Arguments for events related to mouse buttons.
            /// </summary>
            public MouseButtonEventArgs(MouseButtons button, InputState inputState, Vector2 mousePosition)
            {
                Button = button;
                InputState = inputState;
                MousePosition = mousePosition;
            }
        }

        /// <summary>
        /// Arguments for events related to mouse scrolling.
        /// </summary>
        public class MouseScrollEventArgs : EventArgs
        {
            /// <summary>
            /// Direction towards which the mouse scrolled.
            /// </summary>
            public readonly ScrollDirection ScrollDirection;
            /// <summary>
            /// Position of the mouse cursor when this event was raised.
            /// </summary>
            public readonly Vector2 Position;
            /// <summary>
            /// Whether the scroll is horizontal or not (vertical).
            /// </summary>
            public readonly bool IsHorizontalScroll;

            /// <summary>
            /// Arguments for events related to mouse scrolling.
            /// </summary>
            public MouseScrollEventArgs(ScrollDirection direction, Vector2 mousePosition, bool isHorizontalScroll)
            {
                ScrollDirection = direction;
                Position = mousePosition;
                IsHorizontalScroll = isHorizontalScroll;
            }
        }

        /// <summary>
        /// Invoked when the mouse scrolls in any direction.
        /// </summary>
        public static event EventHandler<MouseScrollEventArgs> OnScroll;
        /// <summary>
        /// Invoked when the mouse moves.
        /// </summary>
        public static event EventHandler<MouseMoveEventArgs> OnMoveScreen;
        /// <summary>
        /// Invoked when any mouse button is pressed.
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs> OnButtonPressed;
        /// <summary>
        /// Invoked every frame while any mouse button is being held down.
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs> OnButtonDown;
        /// <summary>
        /// Invoked when any mouse button is released.
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs> OnButtonReleased;

        private static MouseState _previousState;
        private static MouseState _currentState;
        private static Vector2 _position;

        /// <summary>
        /// Gets the current position of the mouse cursor, in game window coordinates.
        /// </summary>
        public static Vector2 Position => _position;

        /// <summary>
        /// Must be called at start-up.
        /// </summary>
        public static void Initialize()
        {
            _currentState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            _previousState = _currentState;
        }


        /// <summary>
        /// Must be called every update call.
        /// </summary>
        public static void Update()
        {
            // Do not update input while the window is inactive.
            if (!Monoid.Instance.IsActive) return;

            // Get mouse state.
            _previousState = _currentState;
            _currentState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            _position = _currentState.Position.ToVector2();

            Vector2 previousPosition = _previousState.Position.ToVector2();

            // Handle events.
            if (_position != previousPosition)
            {
                OnMoveScreen?.Invoke(null, new(_position, previousPosition));
            }

            HandleScrollEvent(_previousState.ScrollWheelValue, _currentState.ScrollWheelValue, false);
            HandleScrollEvent(_previousState.HorizontalScrollWheelValue, _currentState.HorizontalScrollWheelValue, true);

            HandleButtonState(MouseButtons.Left, _previousState.LeftButton, _currentState.LeftButton);
            HandleButtonState(MouseButtons.Middle, _previousState.MiddleButton, _currentState.MiddleButton);
            HandleButtonState(MouseButtons.Right, _previousState.RightButton, _currentState.RightButton);
            HandleButtonState(MouseButtons.Forward, _previousState.XButton1, _currentState.XButton1);
            HandleButtonState(MouseButtons.Backward, _previousState.XButton2, _currentState.XButton2);
        }

        /// <summary>
        /// Checks if a mouse button is not pressed.
        /// </summary>
        public static bool IsButtonUp(MouseButtons button) =>
            GetButtonState(button, _currentState) == ButtonState.Released;

        /// <summary>
        /// Checks if a mouse button has been pressed during this frame.
        /// </summary>
        public static bool IsButtonPressed(MouseButtons button) =>
            GetButtonState(button, _currentState) == ButtonState.Pressed &&
            GetButtonState(button, _previousState) == ButtonState.Released;

        /// <summary>
        /// Checks if a mouse button is being held down.
        /// </summary>
        public static bool IsButtonDown(MouseButtons button) =>
            GetButtonState(button, _currentState) == ButtonState.Pressed;

        /// <summary>
        /// Checks if a mouse button has been released during this frame.
        /// </summary>
        public static bool IsButtonReleased(MouseButtons button) =>
            GetButtonState(button, _currentState) == ButtonState.Released &&
            GetButtonState(button, _previousState) == ButtonState.Pressed;

        /// <summary>
        /// Gets the current input state of a mouse button.
        /// </summary>
        public static InputState GetButtonState(MouseButtons button)
        {
            if (IsButtonUp(button)) return InputState.Up;
            if (IsButtonDown(button)) return InputState.Down;
            if (IsButtonPressed(button)) return InputState.Pressed;

            return InputState.Released;
        }

        private static ButtonState GetButtonState(MouseButtons button, MouseState state)
        {
            return button switch
            {
                MouseButtons.Left => state.LeftButton,
                MouseButtons.Middle => state.MiddleButton,
                MouseButtons.Right => state.RightButton,
                MouseButtons.Forward => state.XButton1,
                MouseButtons.Backward => state.XButton2,
                _ => ButtonState.Released
            };
        }

        private static void HandleButtonState(MouseButtons button, ButtonState previous, ButtonState current)
        {
            if (current == ButtonState.Pressed)
            {
                if (previous == ButtonState.Pressed)
                {
                    OnButtonDown?.Invoke(null, new MouseButtonEventArgs(button, InputState.Down, Position));
                }
                else
                {
                    OnButtonPressed?.Invoke(null, new MouseButtonEventArgs(button, InputState.Pressed, Position));
                }
            }
            else if (previous == ButtonState.Pressed)
            {
                OnButtonReleased?.Invoke(null, new MouseButtonEventArgs(button, InputState.Released, Position));
            }
        }

        private static void HandleScrollEvent(int scrollPrevious, int scrollCurrent, bool horizontal)
        {
            int scrollDelta = scrollCurrent - scrollPrevious;

            if (scrollDelta == 0) return;

            ScrollDirection direction = scrollDelta switch
            {
                > 0 => horizontal ? ScrollDirection.Right : ScrollDirection.Up,
                _ => horizontal ? ScrollDirection.Left : ScrollDirection.Down,
            };

            OnScroll?.Invoke(null, new(direction, Position, horizontal));
        }
    }
}