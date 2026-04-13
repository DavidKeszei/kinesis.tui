using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a position in the 2D space.
/// </summary>
public sealed class Position: Component, IStaticType {
    private static readonly string s_type = nameof(Position);

    private Position m_origin = null!;
    private Vec2 m_offset = Vec2.Zero;

    public static string Name { get => s_type; }

    /// <summary>
    /// Relative distance from a point in the 2D space.
    /// </summary>
    public Vec2 Relative { get => m_offset; set => m_offset = value; }

    /// <summary>
    /// Absolute distance from the (0;0) point.
    /// </summary>
    public Vec2 Absolute { get => (m_origin == null ? Vec2.Zero : m_origin.m_offset) + m_offset; }

    /// <summary>
    /// Pivot point of the current <see cref="Position"/>.
    /// </summary>
    public Position Origin { get => m_origin; set => m_origin = value; }

    public Position(Position? origin) : base(id: ComponentRegistry.QueryComponent(s_type)) {
        m_origin = null!;
        m_offset = Vec2.Zero;
    }
}
