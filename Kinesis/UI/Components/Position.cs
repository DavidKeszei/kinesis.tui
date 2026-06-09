using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a point in the 2D space.
/// </summary>
public sealed class Position(): Component(id: ComponentRegistry.QueryComponent(name: TYPE_NAME)), IStaticType, ICopyable<Position>, IDefault<Position>, IPoolable {
    private const string TYPE_NAME = nameof(Position);

    private Position m_origin = null!;
    private Vec2 m_offset = Vec2.Zero;

    public static string Name { get => TYPE_NAME; }

    /// <summary>
    /// Relative distance from a point in the 2D space.
    /// </summary>
    public Vec2 Relative { get => m_offset; set => m_offset = value; }

    /// <summary>
    /// Absolute distance from the (0;0) point.
    /// </summary>
    public Vec2 Absolute { get => (m_origin == null ? Vec2.Zero : m_origin.Absolute) + m_offset; }

    /// <summary>
    /// Pivot point of the current <see cref="Position"/>.
    /// </summary>
    public Position Origin { get => m_origin; set => m_origin = value; }

    public Position(Position? origin = null!): this() {
        m_origin = origin!;
        m_offset = Vec2.Zero;
    }

    public void Copy(ref Position position) {
        if (position == null) return;

        m_offset = position.m_offset;
        m_origin = position.m_origin;
    }

    public void Reset() {
        m_offset = Vec2.Zero;
        m_origin = null!;

        ComponentPool<Position>.Instance.Return(this);
    }

    public static bool IsDefault(Position instance) {
        if (instance == null) return false;
        return instance.m_offset == (Vec2.One * Scale.Auto) && instance.m_origin == null!;
    }
}
