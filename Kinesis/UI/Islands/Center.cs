using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.UI;

public sealed class Center: Island, ICopyable<BuildContext> {
    private readonly static string s_box = "__center__";
    private Scale? m_childScale = null!;

    public Entity Child {
        init {
            if (value == null) return;

            this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = value;
            value.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
        }
    }

    public Center() {
        _ = this.AttachComponent<Position>(component: new Position() { Relative = new Vec2(x: float.MinValue, y: float.MinValue) },isUnique: true);
        _ = this.AttachComponent<Scale>(component: new Scale(new Vec2(x: float.MinValue, y: float.MinValue)), isUnique: true);
    }

    public void Copy(BuildContext context) {
        context.Set<Position>(this, @default: new Position());
        context.Set<Scale>(this, @default: new Scale(scale: new Vec2(x: float.MinValue, y: float.MinValue)));
    }

    protected override Entity? Build(BuildContext context) {
        if ((m_childScale = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached.GetComponent<Scale>()) == null) {
            Trace.Fail(
                message: $"[UI::Warning] Given child not has Scale component; this can be lead to miscalculation. ({context.Current.Name} as {context.Current.Name.GetType().Name})"
            );
        }

        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref visitor) => {
                if (Scale.IsDefault(instance: GetComponent<Scale>()!))
                    GetComponent<Scale>()!.Value = message.Scale;

                UIBox box = visitor.Visit<UIBox>(name: s_box)!;
                Position pos = GetComponent<Position>()!;

                Vec2 pivot = GetComponent<Scale>()!.Value;

                if (pivot.X > message.Scale.X) pivot.X = message.Scale.X;
                if (pivot.Y > message.Scale.Y) pivot.Y = message.Scale.Y;

                Vec2 center = new Vec2(x: (pivot.X / 2) - (m_childScale.Value.X / 2), y: (pivot.Y / 2) - (m_childScale.Value.Y / 2));

                pos.Relative = center;
            },
            Child = new UIBox() {
                Name = s_box,

                Scale = m_childScale?.Value ?? Vec2.Zero,
                Child = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached ?? null!
            }
        };
    }
}
