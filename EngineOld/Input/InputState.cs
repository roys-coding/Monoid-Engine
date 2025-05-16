using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

/// <summary>
/// Describes the state of a keyboard key, mouse button, gamepad button, screen touch, or other input methods.
/// </summary>
public enum InputState
{
    /// <summary>
    /// Not pressed, held down nor released during the current frame.
    /// </summary>
    /// <remarks>The default state of any key/button.</remarks>
    Up = 0,
    /// <summary>
    /// Key/button was pressed during the current frame.
    /// </summary>
    Pressed = 1,
    /// <summary>
    /// Key/button was pressed or held down during the current frame.
    /// </summary>
    Down = 2,
    /// <summary>
    /// Key/button was released during the current frame.
    /// </summary>
    Released = 3
}
