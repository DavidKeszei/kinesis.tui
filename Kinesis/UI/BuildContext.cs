using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Gives information from the building process in an <see cref="Island"/>
/// </summary>
public record struct BuildContext {
    private Entity? m_current = null!;
    private readonly State<int> m_incrementRenderId = null!;

    private readonly int m_depth = 0;
    private readonly bool m_isTop = true;

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
    /// Indicates the building is working on the "first floor".
    /// </summary>
    public readonly bool IsTop { get => m_isTop; internal init => m_isTop = value; }

    public BuildContext()
        => m_incrementRenderId = new ValueState<int>(@default: 0);
}
