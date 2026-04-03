using System;
using System.Collections.Generic;
using System.Text;
using Kinesis.Core;

namespace Kinesis.UI;

public ref struct BuildContext {
    private readonly Island m_root = null!;
    private readonly State<int> m_renderId = null!;

    private readonly Entity m_current = null!;
    private readonly int m_depth = 0;

    public readonly Entity Current { get => m_current; internal init => m_current = value; }

    public readonly Island Root { get => m_root; internal init => m_root = value; }

    public readonly int Depth { get => m_depth; internal init => m_depth = value; }

    public readonly int RenderId { get => m_renderId; internal set => m_renderId.Value = value; }

    public BuildContext(Entity current) {
        m_current = current;
        m_renderId = new ValueState<int>(@default: 0);
    }
}
