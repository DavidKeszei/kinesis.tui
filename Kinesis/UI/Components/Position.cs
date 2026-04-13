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

    private Vec2 m_origin = Vec2.Zero;
    private Vec2 m_offset = Vec2.Zero;

    public static string Name { get => s_type; }

    /// <summary>
    /// Relative distance from a point in the 2D space.
    /// </summary>
    public Vec2 Offset { get => m_offset; set => m_offset = value; }

    /// <summary>
    /// Pivot point of the <see cref="Position"/>.
    /// </summary>
    public Vec2 Origin { get => m_origin; set => m_origin = value; }

    public Position(Vec2 origin) : base(id: ComponentRegistry.QueryComponent(s_type)) {
        m_origin = origin;
        m_offset = Vec2.Zero;
    }
}
