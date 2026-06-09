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
public class UIStack: Entity, IContentable<List<Entity>> {

    public List<Entity> Content {
        set {
            if (value == null || value.Count == 0)
                return;

            for (int i = 0; i < value.Count; ++i) {
                if (value[i] == null) continue;

                value[i].Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;

                _ = base.Attach<Hierarchy>(ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.DOWN));
                Get<Hierarchy>(Hierarchy.ChildrenStart + i)!.Attached = value[i];
            }
        }
    }

    public UIStack()
        => _ = base.Attach<Hierarchy>(ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.UP));
}
