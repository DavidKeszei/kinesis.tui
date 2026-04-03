using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent connection information between two or more <see cref="Entity"/> instances.
/// </summary>
public class Hierarchy: Component, IStaticType {
    private const string TYPE_NAME = "ConnectionComponent";
    private const int PARENT_INDEX = 0;

    private const int CHILDREN_START_INDEX = 1;

    private Entity m_child = null!;
    private ConnectionDir m_direction = ConnectionDir.DOWN;

    /// <summary>
    /// Name of the component.
    /// </summary>
    public static string Name { get => TYPE_NAME; }

    /// <summary>
    /// Index of the parent on every <see cref="Entity"/> instance.
    /// </summary>
    public static int Parent { get => PARENT_INDEX; }

    /// <summary>
    /// Start index of the children on every <see cref="Entity"/> instance.
    /// </summary>
    public static int ChildrenStart { get => CHILDREN_START_INDEX; }

    /// <summary>
    /// Next <see cref="Entity"/> instance from this <see cref="Entity"/>.
    /// </summary>
    public Entity Attached { get => m_child; set => m_child = value; }

    /// <summary>
    /// Direction of the current connection in the hierarchy.
    /// </summary>
    public ConnectionDir Direction { get => m_direction; init => m_direction = value; }

    public Hierarchy(): base(id: ComponentRegistry.QueryComponent(TYPE_NAME)) { }
}

public enum ConnectionDir: byte {
    UP,
    DOWN
}