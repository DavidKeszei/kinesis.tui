using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents centering UI element on an area.
/// </summary>
public sealed class Center: Island, ICopyable<BuildContext>, IContentable<Entity> {
    private string m_boxName = string.Empty;

    private Scale? m_childScale = null!;
    private readonly Axis m_axis = Axis.X | Axis.Y;

    public Entity Content {
        init {
            if (value == null) return;

            UIBox container = new UIBox { Name = (m_boxName = $"__center__{Guid.CreateVersion7()}__"), Content = value };
            container.RemoveComponent<RenderComponent>();

            container.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = container;

            m_childScale = value.GetComponent<Scale>();
        }
    }

    public Axis Axis { init => m_axis = value; }

    public Center() {
        _ = this.AttachComponent<Position>(component: new Position(), isUnique: true);
        _ = this.AttachComponent<Scale>(component: new Scale(Vec2.One * Scale.Auto), isUnique: true);
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                if (m_childScale == null) return;

                UIBox box = tree.Visit<UIBox>(name: m_boxName)!;
                Position pos = this.GetComponent<Position>()!;

                Vec2 pivot = GetComponent<Scale>()!.Value;
                Vec2 childScale = m_childScale.Value;

                if (pivot.X > message.Scale.X) pivot.X = message.Scale.X;
                if (pivot.Y > message.Scale.Y) pivot.Y = message.Scale.Y;

                Vec2 center = Vec2.Zero;

                if ((m_axis & Axis.X) == Axis.X) center.X = MathF.Round((pivot.X - childScale.X) / 2);
                if ((m_axis & Axis.Y) == Axis.Y) center.Y = MathF.Round((pivot.Y - childScale.Y) / 2);

                pos.Relative = center;
            },
            Content = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached!
        };
    }
}
