using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine.Input
{
    /// <summary>
    /// Handles input.
    /// </summary>
    public static partial class GameInput
    {
        /// <summary>
        /// Initializes all input classes. Must be executed during start-up.
        /// </summary>
        public static void Initialize()
        {
            Keyboard.Initialize();
            Mouse.Initialize();
        }

        /// <summary>
        /// Updates all input classes. Must be executed every frame.
        /// </summary>
        public static void Update()
        {
            Keyboard.Update();
            Mouse.Update();
        }
    }
}
