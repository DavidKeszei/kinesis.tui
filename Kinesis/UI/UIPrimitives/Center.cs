using Kinesis.Processing;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

public sealed class Center: Island {
    private Vec2 m_previosusScale = Vec2.Zero;

    public Entity Child {
        set {
            if (value == null) return;

            this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = value;
            value.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
        }
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<LayoutMessage>(this) {
            On = (msg, _) => {
                Entity? child = this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached;
                if (m_previosusScale == msg.Scale || child == null) return;

                Vec2 scale = child.GetComponent<Transform>()!.Scale;
                Vec2 center = new Vec2(x: (msg.Scale.X / 2) - scale.X, (msg.Scale.Y / 2) - scale.Y);
            },
            Child = this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached
        };
    }
}
