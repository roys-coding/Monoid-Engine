using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

public class Layer
{
    /// <summary>
    /// Defines how the layer will scale to different resolutions.
    /// </summary>
    public enum ScalingMode
    {
        /// <summary>
        /// The layer will not scale.
        /// </summary>
        NoScaling,
        /// <summary>
        /// The layer will stretch to fit screen.
        /// </summary>
        Stretch,
        /// <summary>
        /// The layer will expand to fit screen.
        /// </summary>
        Expand,
    }

    /// <summary>
    /// Defines how the layer will scale to different resolutions.
    /// </summary>
    public ScalingMode Scaling = ScalingMode.Stretch;
    public Camera Camera;

    public Layer()
    {
    }

    public virtual void OnResolutionChange()
    {

    }
}
