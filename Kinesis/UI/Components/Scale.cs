using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a dimension in the 2D space.
/// </summary>
public sealed class Scale(): Component(id: ComponentRegistry.QueryComponent(name: TYPE_NAME)), IStaticType, ICopyable<Scale>, IDefault<Scale>, IPoolable {
    private const string TYPE_NAME = nameof(Scale);

    private Scale m_max = null!;
    private Vec2 m_scale = Vec2.Zero;

    private Vec2 m_inset = Vec2.Zero;

    public static string Name { get => TYPE_NAME; }

    /// <summary>
    /// Inset of the current <see cref="Scale"/> instance.
    /// </summary>
    /// <remarks>
    /// <b>Remarks:</b> This not a "user-defined" value on the get-side; this is calculated based on the <see cref="Maximum"/> and <see cref="Inset"/>.
    /// </remarks>
    public Vec2 Value { get => Limit(); set => m_scale = value; }

    /// <summary>
    /// Inset of the inset from the <see cref="Value"/>.
    /// </summary>
    /// <remarks>
    /// Example: If the scale is 10x10 and the inset is 1x1, then the calculated scale is 9x9.
    /// </remarks>
    public Vec2 Inset { get => m_inset; set => m_inset = value; }

    /// <summary>
    /// Parent/Maximum value of the scale of the current <see cref="Scale"/> instance.
    /// </summary>
    public Scale Maximum { get => m_max; set => m_max = value; }

    public void Reset() {
        m_max = null!;
        m_inset = Vec2.Zero;

        m_scale = Vec2.Zero;
        ComponentPool<Scale>.Instance.Return(this);
    }

    /// <summary>
    /// Indicates what axis was setted to <see cref="Scale.AUTO_ON_AXIS"/>.,
    /// </summary>
    /// <returns>Returns a <see cref="bool"/> tuple, which indicates the auto state each axis.</returns>
    public (bool X, bool Y) IsAuto()
        => (m_scale.X == float.MinValue, m_scale.Y == float.MinValue);

    /// <summary>
    /// Change an <paramref name="axis"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">New value of the axis.</param>
    /// <param name="axis">Affected axis of the change.</param>
    public void ChangeAxisValue(float value, Axis axis) {
        if (axis == Axis.X) m_scale.X = value;
        if (axis == Axis.Y) m_scale.Y = value;
    }

    public void Copy(ref Scale from) {
        if (from == null) return;
        m_max = from.m_max;

        if(m_scale.X == float.MinValue) m_scale.X = from.m_scale.X;
        if(m_scale.Y == float.MinValue) m_scale.Y = from.m_scale.Y;
    }

    public static bool IsDefault(Scale? instance) {
        if (instance == null) return false;
        return (instance.m_scale.X == Vec2.Auto.X || instance.m_scale.Y == Vec2.Auto.Y) && instance.m_max == null;
    }

    private Vec2 Limit() {
        if (m_max != null) {

            /* Save these, because parent scale is computed value */
            Vec2 result = m_scale;
            Vec2 parent = m_max.Value;

            /* 
             * TODO(2026-05-10T12:36): Revisit for better scale updating. (Cache current, calculated value)
             */
            if (m_scale.X == Vec2.Auto.X || result.X > parent.X) result.X = parent.X;
            if (m_scale.Y == Vec2.Auto.Y || result.Y > parent.Y) result.Y = parent.Y;

            result.X -= m_inset.X;
            result.Y -= m_inset.Y;

            return result;
        }

        return m_scale;
    }
}