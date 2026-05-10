using Kinesis.Core;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a space between <see cref="Entity"/> instances.
/// </summary>
public sealed class Padding: Entity, IContentable<Entity> {

    /// <summary>
    /// Value of the spacing.
    /// </summary>
    public uint Value { get => (uint)this.GetComponent<Style>()!.AsInt; set => UpdateChildPadding(value); }

    /// <summary>
    /// Content of the current <see cref="Padding"/>.
    /// </summary>
    public Entity Content {
        init {
            if (value == null) return;

            this.GetComponent<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = value;
            value.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;

            Position? position = value.GetComponent<Position>();

            if (position != null) {
                Position ePosition = this.GetComponent<Position>()!;

                position.Origin = ePosition.Origin;
                position.Relative = new Vec2(x: Value, y: Value);
            }
        }
    }

    public Padding() {
        _ = base.AttachComponent<Position>(new Position(origin: null!), isUnique: true);
        _ = base.AttachComponent<Style>(Style.CreateFromInt(StyleTag.PADDING, value: 0), true);

        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.UP });
        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.DOWN });
    }

    private void UpdateChildPadding(uint value) {
        this.GetComponent<Style>()!.AsInt = (int)value;

        Entity? child = this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached;
        if (child != null) {
            Position position = this.GetComponent<Position>()!;
            Position? childPosition = child.GetComponent<Position>();

            if (childPosition != null) {
                childPosition.Origin = position.Origin;
                childPosition.Relative = position.Relative;
            }
        }
    }
}
