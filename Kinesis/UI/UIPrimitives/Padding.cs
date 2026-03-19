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
    public uint Value { get => (uint)this.GetComponent<Style>()!.AsInt; set => this.GetComponent<Style>()!.AsInt = (int)value; }

    /// <summary>
    /// Child of the current <see cref="Padding"/>.
    /// </summary>
    public Entity Child {
        set {
            if (value == null) return;

            this.GetComponent<Hierarchy>(index: 1)!.Attached = value;
            value.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;

            Transform? transform = value.GetComponent<Transform>();

            if (transform != null) {
                Transform e_transform = this.GetComponent<Transform>()!;
                e_transform.Position = transform.Position;

                transform.Position = new Vec2(e_transform.Position.X + Value, e_transform.Position.Y + Value);
            }
        }
    }

    public Padding() {
        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.UP });
        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.DOWN });

        _ = base.AttachComponent<Transform>(new Transform(), isUnique: true);
        _ = base.AttachComponent<Style>(Style.CreateFromInt(StyleTag.PADDING, value: 1), true);
    }
}
