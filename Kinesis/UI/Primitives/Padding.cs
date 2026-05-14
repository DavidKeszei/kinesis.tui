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

    private Scale? m_childScale = null!;
    private Position? m_childPosition = null!;

    /// <summary>
    /// Value of the spacing.
    /// </summary>
    public uint Value { get => (uint)this.GetComponent<Style>()!.AsInt; set => this.GetComponent<Style>()!.AsInt = (int)value; }

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

            m_childScale = container.GetComponent<Scale>();
            m_childPosition = container.GetComponent<Position>();
        }
    }

    public Padding() {
        _ = base.AttachComponent<Position>(new Position(origin: null!), isUnique: true);
        _ = base.AttachComponent<Scale>(new Scale(scale: Vec2.One * Scale.Auto), isUnique: true);

        _ = base.AttachComponent<Style>(Style.CreateFromInt(StyleTag.PADDING, value: 0), true);
        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.UP });

        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.DOWN });
    }

    public void Copy(ref BuildContext context) {
        context.Set<Position>(this, @default: new Position());
        context.Set<Scale>(this, @default: new Scale(scale: Vec2.One * Scale.Auto));

        GetComponent<Scale>()!.Value = Vec2.One * Scale.Auto;
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                if (m_childPosition == null || m_childScale == null) return;

                Position pos = GetComponent<Position>()!;
                Scale scale = GetComponent<Scale>()!;

                scale.Inset = Vec2.One * GetComponent<Style>()!.AsInt;

                Vec2 savedScale = scale.Value;
                m_childPosition.Relative = scale.Inset;

                m_childScale.ChangeAxisValue(value: savedScale.X - scale.Inset.X, axis: Axis.X);
                m_childScale.ChangeAxisValue(value: savedScale.Y - scale.Inset.Y, axis: Axis.Y);
            },
            Content = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached ?? null!
        };
    }
}
