using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Kinesis;

/// <summary>
/// Represent a vector in the 2D space.
/// </summary>
public struct Vec2: IAdditionOperators<Vec2, Vec2, Vec2> {
    private float m_x = 0;
    private float m_y = 0;

    /// <summary>
    /// Value of the X axis.
    /// </summary>
    public float X { readonly get => m_x; set => m_x = value; }

    /// <summary>
    /// Value of the Y axis.
    /// </summary>
    public float Y { readonly get => m_y; set => m_y = value; }

    /// <summary>
    /// Represents a (0, 0) vector.
    /// </summary>
    public static Vec2 Zero { get => new Vec2(x: 0, y: 0); }

    /// <summary>
    /// Represents a (1, 1) vector.
    /// </summary>
    public static Vec2 One { get => new Vec2(x: 1, y: 1); }

    public static Vec2 operator *(Vec2 vec, float amount) => new Vec2(x: vec.m_x * amount, y: vec.m_y * amount);

    public static Vec2 operator -(Vec2 vec, float amount) => new Vec2(x: vec.m_x - amount, y: vec.m_y - amount);

    public static Vec2 operator +(Vec2 left, Vec2 right) => new Vec2(x: left.m_x + right.m_x, y: left.m_y + right.m_y);

    public static bool operator ==(Vec2 left, Vec2 right) => left.m_x == right.m_x && left.m_y == right.m_y;

    public static bool operator !=(Vec2 left, Vec2 right) => !(left == right);

    public Vec2(float x, float y) {
        m_x = x;
        m_y = y;
    }

    /// <summary>
    /// Create a new <see cref="Vec2"/> instance from just the <paramref name="x"/> value.
    /// </summary>
    /// <param name="x">Value of the X-axis.</param>
    /// <returns>Returns a <see cref="Vec2"/> instance, which X value is equal with <paramref name="x"/>, but the Y value is equals with <see cref="float.MinValue"/>.</returns>
    public static Vec2 FromX(float x) => new Vec2(x, y: float.MinValue);

    /// <summary>
    /// Create a new <see cref="Vec2"/> instance from just the <paramref name="y"/> value.
    /// </summary>
    /// <param name="y">Value of the Y-axis.</param>
    /// <returns>Returns a <see cref="Vec2"/> instance, which Y value is equal with <paramref name="y"/>, but the X value is equals with <see cref="float.MinValue"/>.</returns>
    public static Vec2 FromY(float y) => new Vec2(x: float.MinValue, y);

    /// <summary>
    /// Correcting the given <see cref="Vec2"/> instance to the terminal display.
    /// </summary>
    /// <param name="scale">Source vector of the correction.</param>
    /// <param name="ratio">Scale ratio of the X axis.</param>
    /// <returns>Returns a new <see cref="Vec2"/> instance, which corrected to the terminal display.</returns>
    public static Vec2 TerminalBasedScale(Vec2 scale, float ratio = 2f) {
        if (scale.m_y % (int)scale.m_y == 0)
            return new Vec2(x: scale.m_x * ratio, y: scale.m_y);

        return new Vec2(x: scale.m_x * ratio, y: (int)scale.m_y);
    }
}
