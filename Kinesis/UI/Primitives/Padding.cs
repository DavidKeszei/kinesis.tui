using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a space between <see cref="Entity"/> instances.
/// </summary>
public sealed class Padding: Entity {

    /// <summary>
    /// Value of the spacing.
    /// </summary>
    public uint Value { get => (uint)this.GetComponent<Style>()!.AsInt; set => UpdateChildPadding(value); }

    /// <summary>
    /// Child of the current <see cref="Padding"/>.
    /// </summary>
    public Entity Child {
        set {
            if (value == null) return;

            this.GetComponent<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = value;
            value.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;

            Position? position = value.GetComponent<Position>();

            if (position != null) {
                Position ePosition = this.GetComponent<Position>()!;

                position.Origin = ePosition.Origin;
                position.Offset = new Vec2(x: Value, y: Value);
            }
        }
    }

    public Padding() {
        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.UP });
        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.DOWN });

        _ = base.AttachComponent<Position>(new Position(origin: Vec2.Zero), isUnique: true);
        _ = base.AttachComponent<Style>(Style.CreateFromInt(StyleTag.PADDING, value: 1), true);
    }

    private void UpdateChildPadding(uint value) {
        this.GetComponent<Style>()!.AsInt = (int)value;

        Entity? child = this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached;
        if (child != null) {
            Position position = this.GetComponent<Position>()!;
            Position? childPosition = child.GetComponent<Position>();

            if (childPosition != null) {
                childPosition.Origin = position.Origin;
                childPosition.Offset = position.Offset;
            }
        }
    }
}
