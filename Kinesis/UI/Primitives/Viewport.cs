using Kinesis.Core;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a clipped view on the screen.
/// </summary>
public sealed class Viewport: Entity, IContentable<Entity>, ICopyable<BuildContext> {

    public Entity Content {
        set {
            Hierarchy? childParent = null!;
            if (value == null || (childParent = value.Get<Hierarchy>(index: Hierarchy.Parent)) == null) return;

            childParent.Attached = this;
            Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = value;
        }
    }

    /// <summary>
    /// Current scale of the <see cref="Viewport"/>.
    /// </summary>
    public Vec2 Scale { get => Get<Scale>()!.Value; set => Get<Scale>()!.Value = value; }

    public Viewport(): base(count: MAX_COMPONENT_COUNT) {
        _ = Attach<Position>(component: ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = Attach<Scale>(component: ComponentPool<Scale>.Instance.Rent<Scale>(static x => x.Value = Vec2.Auto), isUnique: true);

        _ = Attach<Hierarchy>(component: ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static x => x.Direction = ConnectionDirection.UP));
        _ = Attach<Hierarchy>(component: ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static x => x.Direction = ConnectionDirection.DOWN));
    }

    public void Copy(ref BuildContext from) {
        from.SetPivot<Scale>(this);
        from.SetPivot<Position>(this);
    }
}
