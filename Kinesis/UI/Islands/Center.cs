using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

public sealed class Center: Island, ICopyable<BuildContext> {
    private readonly static string s_box = "__center__";

    public Entity Child {
        set {
            if (value == null) return;

            _ = this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = value;
            _ = value.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
        }
    }

    public Center()
        => _ = this.AttachComponent<Position>(component: new Position() { Relative = new Vec2(x: float.MinValue, y: float.MinValue) });

    public void Copy(BuildContext context)
        => context.Set<Position>(this, @default: new Position());

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<LayoutMessage>(context.Root) {
            On = (message, ref visitor) => {
                UIBox box = visitor.Visit<UIBox>(name: s_box)!;

                Position pos = this.GetComponent<Position>()!;
                Scale scale = box.GetComponent<Scale>()!;

                Vec2 center = new Vec2(x: (message.Scale.X / 2) - (scale.Value.X / 2), y: (message.Scale.Y / 2) - (scale.Value.Y / 2));
                pos.Relative = center;
            },
            Child = new UIBox() {
                Name = s_box,

                Scale = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached.GetComponent<Scale>()?.Value ?? Vec2.Zero,
                Child = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached ?? null!
            }
        };
    }
}
