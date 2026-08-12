using Kinesis.Core;
using Kinesis.Core.Rendering;
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
    /// Inset of the spacing.
    /// </summary>
    public Vec2 Inset {
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
            Viewport container = new Viewport {
                Name = (m_container ??= $"__padding__{Guid.CreateVersion7()}__"),
                Content = value
            };

            this.Get<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = container;
            container.Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;

            Get<RebuildContent>()!.Content = container;
            Rebuild();
        }
    }

    public Padding(): base(count: 7) {
        _ = base.Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = base.Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static(x) => x.Value = Vec2.Auto), isUnique: true);

        _ = base.Attach<Style>(ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<int>(Style.PADDING, tag: StyleDataType.NUMERIC_I, value: 0)));
        _ = base.Attach<Style>(ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<int>(Style.PADDING, tag: StyleDataType.NUMERIC_I, value: 0)));

        _ = base.Attach<Hierarchy>(ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.UP));
        _ = base.Attach<Hierarchy>(ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.DOWN));
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref readonly tree) => {
                Viewport container = tree.Visit<Viewport>(name: m_container)!;

                Position pos = Get<Position>()!;
                Scale scale = Get<Scale>()!;

                scale.Inset = new Vec2(x: Get<Style>()!.AsInt, y: Get<Style>(index: 1)!.AsInt);

                Vec2 savedScale = scale.Value;
                container.Get<Position>()!.Relative = scale.Inset;

                container.Get<Scale>()!.ChangeAxisValue(value: savedScale.X - scale.Inset.X, axis: Axis.X);
                container.Get<Scale>()!.ChangeAxisValue(value: savedScale.Y - scale.Inset.Y, axis: Axis.Y);
            },
            Content = Get<RebuildContent>()!.Content ?? null!
        };
    }
}
