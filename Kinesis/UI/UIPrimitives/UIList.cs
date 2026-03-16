using Kinesis.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

public class UIList: Entity {

    public List<Entity> Children {
        set {
            if (value == null || value.Count == 0)
                return;

            for (int i = 0; i < value.Count; ++i) {
                Hierarchy conn = new Hierarchy() {
                    Direction = ConnectionDir.DOWN,
                    Attached = value[i]
                };

                value[i].GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
                _ = base.AttachComponent<Hierarchy>(conn);
            }
        }
    }

    public UIList()
        => _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.UP });

    private int LastRenderableIndex(List<Entity> list) {
        int pos = -1;
        for (int i = 0; i < list.Count; ++i) {
            if (list[i].GetComponent<RenderComponent>() != null)
                pos = i;
        }

        return pos;
    }
}
