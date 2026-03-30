using Kinesis.Processing;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

public sealed class Center: Island {
    private const string CONTANIER_ID = "__box__";
    private readonly Entity? m_child = null!;

    public Entity Child {
        init {
            if (value == null) return;
            m_child = value;

            this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = value;
            value.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
        }
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<LayoutMessage>(island: context.Root) {
            On = (msg, _) => {
                
            },
            Child = new UIBox {
                Name = CONTANIER_ID,
                Child = m_child ?? null!
            }
        };
    }
}
