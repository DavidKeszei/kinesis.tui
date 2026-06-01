using Kinesis.Core;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a space between <see cref="Entity"/> instances.
/// </summary>
public sealed class Padding: Island, IContentable<Entity>, ICopyable<BuildContext> {
    private string m_container = null!;

    /// <summary>
    /// Value of the spacing.
    /// </summary>
    public Vec2 Value {
        get => new Vec2(x: this.Get<Style>()!.AsInt, this.Get<Style>(1)!.AsInt);
        set {
            if (value.X < 0 || value.Y < 0) return;

            this.Get<Style>()!.AsInt = (int)value.X;
            this.Get<Style>(1)!.AsInt = (int)value.Y;
        }
    }

    /// <summary>
    /// Content of the current <see cref="Padding"/>.
    /// </summary>
    public Entity Content {
        set {
            if (value == null) return;
            UIBox container = new UIBox {
                Name = (m_container ??= $"__padding__{Guid.CreateVersion7()}__"),
                Content = value
            };

            this.Get<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = container;
            container.Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
        }
    }

    public Padding() {
        _ = base.Attach<Position>(new Position(origin: null!), isUnique: true);
        _ = base.Attach<Scale>(new Scale(scale: Vec2.One * Scale.Auto), isUnique: true);

        _ = base.Attach<Style>(Style.CreateFromInt(StyleTag.PADDING, value: 0));
        _ = base.Attach<Style>(Style.CreateFromInt(StyleTag.PADDING, value: 0));

        _ = base.Attach<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.UP });
        _ = base.Attach<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.DOWN });
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                Position pos = Get<Position>()!;
                Scale scale = Get<Scale>()!;

                UIBox container = tree.Visit<UIBox>(name: m_container)!;
                scale.Inset = new Vec2(x: Get<Style>()!.AsInt, y: Get<Style>(1)!.AsInt);

                Vec2 savedScale = scale.Value;
                container.Get<Position>()!.Relative = scale.Inset;

                container.Get<Scale>()!.ChangeAxisValue(value: savedScale.X - scale.Inset.X, axis: Axis.X);
                container.Get<Scale>()!.ChangeAxisValue(value: savedScale.Y - scale.Inset.Y, axis: Axis.Y);
            },
            Content = Get<Hierarchy>(Hierarchy.ChildrenStart)?.Attached ?? null!
        };
    }
}
