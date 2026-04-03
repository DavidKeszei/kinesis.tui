using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a internal render hierarchy for the <see cref="Rendering.Renderer"/>.
/// </summary>
internal class RenderHierarchy: Component, IStaticType {
    private const string TYPE_OF = "RenderHierarchy";

    private int m_depth = -1;
    private int m_nextRenderInstance = -1;

    public static string Name { get => TYPE_OF; }
    
    /// <summary>
    /// Next drawable <see cref="Entity"/> from this entity.
    /// </summary>
    public int NextRenderElementIndex { get => m_nextRenderInstance; set => m_nextRenderInstance = value; }

    /// <summary>
    /// Current depth of the <see cref="Entity"/>.
    /// </summary>
    public int Depth { get => m_depth; set => m_depth = value; }

    public RenderHierarchy(): base(ComponentRegistry.QueryComponent(name: TYPE_OF)) { }
}
