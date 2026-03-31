using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Gives information from the building process in an <see cref="Island"/>
/// </summary>
public record struct BuildContext {
    private readonly Island m_root = null!;

    private Entity? m_current = null!;
    private readonly State<int> m_incrementRenderId = null!;

    private readonly int m_depth = 0;

    /// <summary>
    /// Current target of the building.
    /// </summary>
    public Entity? Current { readonly get => m_current; internal set => m_current = value; }

    /// <summary>
    /// Current render id of the building.
    /// </summary>
    public readonly int RenderId { get => m_incrementRenderId; internal set => m_incrementRenderId.Value= value; }

    /// <summary>
    /// Depth of the hierarchy. 
    /// </summary>
    public readonly int Depth { get => m_depth; internal init => m_depth = value; }

    /// <summary>
    /// Root of the build.
    /// </summary>
    public readonly Island Root { get => m_root; }

    public BuildContext(Island root) {
        m_incrementRenderId = new ValueState<int>(@default: 0);
        m_root = root;
    }
}
