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
    private Vec2 m_scale = Vec2.Zero;

    public static string Name { get => s_type; }

    /// <summary>
    /// Value of the current <see cref="Scale"/> instance.
    /// </summary>
    public Vec2 Value { get => m_scale; set => m_scale = value; }

    public Scale(Vec2 scale) : base(id: ComponentRegistry.QueryComponent(name: s_type))
        => m_scale = scale;

    public void Copy(Scale? from) {
        if (from == null) return;
        m_scale = from.m_scale;
    }

    public static bool IsDefault(Scale instance) {
        if (instance == null) return false;
        return instance.m_scale == new Vec2(x: float.MinValue, y: float.MinValue);
    }
}
