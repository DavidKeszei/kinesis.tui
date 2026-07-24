using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents an <see cref="Entity"/> with multiple children, but nothing more.
/// </summary>
public sealed class UIStack: Entity, IContentable<List<Entity>>, ICopyable<BuildContext> {

    public List<Entity> Content {
        set {
            if (value == null || value.Count == 0)
                return;

            int childCount = this.CountComponent<Hierarchy>() - 1;

            for (int i = 0; i < value.Count; ++i) {
                if (value[i] == null) continue;

                value[i].Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;

                if(childCount <= i) _ = base.Attach<Hierarchy>(ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.DOWN));
                Get<Hierarchy>(Hierarchy.ChildrenStart + i)!.Attached = value[i];
            }
        }
    }

    public UIStack(int capacity = MAX_COMPONENT_COUNT * 2): base(capacity)
        => _ = base.Attach<Hierarchy>(ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.UP));

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);
    }
}
