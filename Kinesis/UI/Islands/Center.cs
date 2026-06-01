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
    private string m_boxName = null!;

    private Scale? m_childScale = null!;
    private readonly Axis m_axis = Axis.X | Axis.Y;

    public Entity Content {
        set {
            if (value == null) return;

            UIBox container = new UIBox { Name = (m_boxName ??= $"__center__{Guid.CreateVersion7()}__"), Content = value };
            container.Remove<RenderComponent>();

            container.Get<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = container;

            m_childScale = value.Get<Scale>();
        }
    }

    public Axis Axis { init => m_axis = value; }

    public Center() {
        _ = this.Attach<Position>(component: new Position(), isUnique: true);
        _ = this.Attach<Scale>(component: new Scale(Vec2.One * Scale.Auto), isUnique: true);
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
                Position pos = this.Get<Position>()!;

                Vec2 pivot = Get<Scale>()!.Value;
                Vec2 childScale = m_childScale.Value;

                if (pivot.X > message.Scale.X) pivot.X = message.Scale.X;
                if (pivot.Y > message.Scale.Y) pivot.Y = message.Scale.Y;

                Vec2 center = Vec2.Zero;

                if ((m_axis & Axis.X) == Axis.X) center.X = MathF.Round((pivot.X - childScale.X) / 2);
                if ((m_axis & Axis.Y) == Axis.Y) center.Y = MathF.Round((pivot.Y - childScale.Y) / 2);

                pos.Relative = center;
            },
            Content = Get<Hierarchy>(Hierarchy.ChildrenStart)?.Attached!
        };
    }
}
