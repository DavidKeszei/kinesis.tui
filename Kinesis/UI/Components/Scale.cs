using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a dimension in the 2D space.
/// </summary>
public sealed class Scale : Component, IStaticType, ICopyable<Scale>, IDefault<Scale> {
    private static readonly string s_type = nameof(Scale);

    private Scale m_max = null!;
    private Vec2 m_scale = Vec2.Zero;

    public static string Name { get => s_type; }

    /// <summary>
    /// Value of the current <see cref="Scale"/> instance.
    /// </summary>
    public Vec2 Value { get => Limit(); set => m_scale = value; }

    /// <summary>
    /// Parent/Maximum value of the scale of the current <see cref="Scale"/> instance.
    /// </summary>
    public Scale Maximum { get => m_max; set => m_max = value; }

    public Scale(Vec2 scale): base(id: ComponentRegistry.QueryComponent(name: s_type))
        => m_scale = scale;

    public void Copy(ref Scale from) {
        if (from == null) return;

        m_max = from.m_max;

        if(m_scale.X == float.MinValue) m_scale.X = from.m_scale.X;
        if(m_scale.Y == float.MinValue) m_scale.Y = from.m_scale.Y;
    }

    public static bool IsDefault(Scale? instance) {
        if (instance == null) return false;
        return (instance.m_scale.X == float.MinValue || instance.m_scale.Y == float.MinValue) && instance.m_max == null;
    }

    private Vec2 Limit() {
        if (m_max != null) {
            Vec2 result = m_scale;

            if (result.X > m_max.Value.X || m_scale.X == float.MinValue) result.X = m_max.Value.X;
            if (result.Y > m_max.Value.Y || m_scale.Y == float.MinValue) result.Y = m_max.Value.Y;

            return result;
        }

        return m_scale;
    }
}
