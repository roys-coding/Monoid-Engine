using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMonoGameApp;

public class Actor3D : Actor
{
    public Transform3D Transform;
    public float Depth;

    public Actor3D(Transform3D transform = default)
    {
        Transform = transform;
    }
}
