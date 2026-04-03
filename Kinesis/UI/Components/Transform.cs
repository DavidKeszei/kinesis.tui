using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a component, which contains the transform of a <see cref="Entity"/>.
/// </summary>
public class Transform: Component, IStaticType {
    private const string TYPE_NAME = "Transform";

    private readonly State<Vec2> m_oldPosition = new ValueState<Vec2>(@default: Vec2.Zero);
    private readonly State<Vec2> m_oldScale =  new ValueState<Vec2>(@default: Vec2.Zero);

    private Vec2 m_position = Vec2.Zero;
    private Vec2 m_scale = Vec2.Zero;

    /// <summary>
    /// Name of the component.
    /// </summary>
    public static string Name { get => TYPE_NAME; }

    /// <summary>
    /// Position of the current <see cref="Transform"/> instance.
    /// </summary>
    public Vec2 Position { get => m_position; set => m_position = value; }

    /// <summary>
    /// Scale of the current <see cref="Transform"/> instance.
    /// </summary>
    public Vec2 Scale { get => m_scale; set => m_scale = value; }

    /// <summary>
    /// Old position of the <see cref="Transform"/> component.
    /// </summary>
    internal Vec2 OldPosition { get => m_oldPosition; set => m_oldPosition.Value = value; }

    /// <summary>
    /// Old scale of the <see cref="Transform"/> component.
    /// </summary>
    internal Vec2 OldScale { get => m_oldScale; set => m_oldScale.Value = value; }

    public Transform(): base(id: ComponentRegistry.QueryComponent(TYPE_NAME)) { }
}