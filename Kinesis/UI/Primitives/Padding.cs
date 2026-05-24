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
    private readonly string m_container = string.Empty;

    /// <summary>
    /// Value of the spacing.
    /// </summary>
    public Vec2 Value {
        get => new Vec2(x: this.GetComponent<Style>()!.AsInt, this.GetComponent<Style>(1)!.AsInt);
        set {
            if (value.X < 0 || value.Y < 0) return;

            this.GetComponent<Style>()!.AsInt = (int)value.X;
            this.GetComponent<Style>(1)!.AsInt = (int)value.Y;
        }
    }

    /// <summary>
    /// Content of the current <see cref="Padding"/>.
    /// </summary>
    public Entity Content {
        init {
            if (value == null) return;
            UIBox container = new UIBox {
                Name = (m_container = $"__padding__{Guid.CreateVersion7()}__"),
                Content = value
            };

            this.GetComponent<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = container;
            container.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
        }
    }

    public Padding() {
        _ = base.AttachComponent<Position>(new Position(origin: null!), isUnique: true);
        _ = base.AttachComponent<Scale>(new Scale(scale: Vec2.One * Scale.Auto), isUnique: true);

        _ = base.AttachComponent<Style>(Style.CreateFromInt(StyleTag.PADDING, value: 0));
        _ = base.AttachComponent<Style>(Style.CreateFromInt(StyleTag.PADDING, value: 0));

        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.UP });
        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.DOWN });
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                Position pos = GetComponent<Position>()!;
                Scale scale = GetComponent<Scale>()!;

                UIBox container = tree.Visit<UIBox>(name: m_container)!;
                scale.Inset = new Vec2(x: GetComponent<Style>()!.AsInt, y: GetComponent<Style>(1)!.AsInt);

                Vec2 savedScale = scale.Value;
                container.GetComponent<Position>()!.Relative = scale.Inset;

                container.GetComponent<Scale>()!.ChangeAxisValue(value: savedScale.X - scale.Inset.X, axis: Axis.X);
                container.GetComponent<Scale>()!.ChangeAxisValue(value: savedScale.Y - scale.Inset.Y, axis: Axis.Y);
            },
            Content = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached ?? null!
        };
    }
}
