using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

public class Actor2D : Actor
{
    public Transform2D Transform;
    public float Depth;

    public Actor2D(Transform2D transform = default)
    {
        Transform = transform;
    }
}
