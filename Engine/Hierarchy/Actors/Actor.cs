using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

public class Actor
{
    protected Scene _scene;

    public Scene Scene => _scene;

    public Actor() { }

    public virtual void Initialize() { }
    public virtual void OnAdded(Scene scene)
    {
        _scene = scene;
    }
    public virtual void OnRemoved(Scene scene)
    {
        _scene = null;
    }
    public virtual void Update() { }
    public virtual void Draw() { }
    public virtual void Terminate() { }
}
