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
public class UIList: Entity, IContentable<List<Entity>> {

    public List<Entity> Content {
        init {
            if (value == null || value.Count == 0)
                return;

            for (int i = 0; i < value.Count; ++i) {
                Hierarchy conn = new Hierarchy() {
                    Direction = ConnectionDirection.DOWN,
                    Attached = value[i]
                };

                value[i].GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
                _ = base.AttachComponent<Hierarchy>(conn);
            }
        }
    }

    public UIList()
        => _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.UP });
}
