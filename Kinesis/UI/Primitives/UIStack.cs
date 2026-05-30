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
        init {
            if (value == null || value.Count == 0)
                return;

            for (int i = 0; i < value.Count; ++i) {
                if (value[i] == null) continue;

                Hierarchy conn = new Hierarchy() {
                    Direction = ConnectionDirection.DOWN,
                    Attached = value[i]
                };

                value[i].Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
                _ = base.Attach<Hierarchy>(conn);
            }
        }
    }

    public UIStack()
        => _ = base.Attach<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.UP });
}
