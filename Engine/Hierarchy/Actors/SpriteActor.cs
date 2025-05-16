using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

public class SpriteActor : Actor2D
{
    public Sprite Sprite { get; protected set; }

    public SpriteActor(Sprite sprite, Transform2D transform) : base(transform)
    {
        Sprite = sprite;
    }

    public override void Initialize()
    {
    }

    public override void OnAdded(Scene scene)
    {
    }

    public override void OnRemoved(Scene scene)
    {
    }

    public override void Update()
    {
    }

    public override void Draw()
    {
        Sprite.Transform = Transform;
        Sprite.Depth = Depth;

        Graphics.Draw.Sprite(Sprite);
    }

    public override void Terminate()
    {
    }
}
