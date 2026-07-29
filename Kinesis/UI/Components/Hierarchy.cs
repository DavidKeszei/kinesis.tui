using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent connection information between two <see cref="Entity"/> instances.
/// </summary>
public class Hierarchy(): Component(id: ComponentRegistry.QueryComponent(name: TYPE_NAME)), IStaticType, IPoolable {
    private const string TYPE_NAME = nameof(Hierarchy);
    private const int PARENT_INDEX = 0;

    private const int CHILDREN_START_INDEX = 1;

    private Entity m_child = null!;
    private ConnectionDirection m_direction = ConnectionDirection.DOWN;

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
    /// Simply implicit cast between <see cref="Hierarchy"/> and <see cref="Entity"/> classes.
    /// </summary>
    /// <param name="hierarchy">Holder of the child <see cref="Entity"/> instance. This can be <see langword="null"/>.</param>
    public static implicit operator Entity(Hierarchy hierarchy) => hierarchy.Attached;

    /// <summary>
    /// Next <see cref="Entity"/> instance from this <see cref="Entity"/>.
    /// </summary>
    public Entity Attached { get => m_child; set => m_child = value; }

    /// <summary>
    /// Direction of the current connection in the hierarchy.
    /// </summary>
    public ConnectionDirection Direction { get => m_direction; set => m_direction = value; }

    public void Reset() {
        m_child = null!;
        m_direction = ConnectionDirection.DOWN;

        ComponentPool<Hierarchy>.Instance.Return(this);
    }
}

public enum ConnectionDirection: byte {
    UP,
    DOWN
}