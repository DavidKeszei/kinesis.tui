using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.UI;

public sealed class Center: Island, ICopyable<BuildContext>, IContentable<Entity> {
    private string m_boxName = string.Empty;

    private Vec2 m_previousScale = Vec2.Zero;
    private Scale? m_childScale = null!;

    private bool m_scaleNotDefined = false;

    public Entity Content {
        init {
            if (value == null) return;

            UIBox container = new UIBox() { Name = (m_boxName = $"__center__{Guid.CreateVersion7()}__"), Content = value };
            container.RemoveComponent<RenderComponent>();

            this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = container;
            container.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;

            m_childScale = value.GetComponent<Scale>();
        }
    }

    public Center() {
        _ = this.AttachComponent<Position>(component: new Position() { Relative = Vec2.One * Scale.Auto },isUnique: true);
        _ = this.AttachComponent<Scale>(component: new Scale(Vec2.One * Scale.Auto), isUnique: true);
    }

    public void Copy(ref BuildContext context) {
        context.Set<Position>(this, @default: new Position());
        context.Set<Scale>(this, @default: new Scale(scale: Vec2.One * Scale.Auto));

        m_scaleNotDefined = Scale.IsDefault(instance: GetComponent<Scale>());
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref visitor) => {
                if (m_previousScale.X == message.Scale.X && m_previousScale.Y == message.Scale.Y)
                    return;

                if (m_scaleNotDefined)
                    GetComponent<Scale>()!.Value = message.Scale;

                UIBox box = visitor.Visit<UIBox>(name: m_boxName)!;
                Position pos = this.GetComponent<Position>()!;

                Vec2 pivot = this.GetComponent<Scale>()!.Value;

                if (pivot.X > message.Scale.X) pivot.X = message.Scale.X;
                if (pivot.Y > message.Scale.Y) pivot.Y = message.Scale.Y;

                Vec2 center = new Vec2(x: MathF.Round((pivot.X - m_childScale!.Value.X) / 2), y: MathF.Round((pivot.Y - m_childScale!.Value.Y) / 2));
                pos.Relative = center;

                m_previousScale = message.Scale;
            },
            Content = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached!
        };
    }
}
