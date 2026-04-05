using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Represent a <see cref="ConsoleBuffer"/> on the screen.
/// </summary>
internal readonly struct ConsoleBuffer {
    private readonly vtchar_t[,] m_buffer = null!;
    private readonly Vec2 m_scale = new Vec2(-1, -1);

    private readonly Vec2 m_startScale = new Vec2(-1, -1);

    /// <summary>
    /// Dimension of the from.
    /// </summary>
    public Vec2 Scale { get => m_scale; }

    /// <summary>
    /// Get a <see cref="vtchar_t"/> reference from the from.
    /// </summary>
    /// <param name="x">X position of the reference.</param>
    /// <param name="y">Y position of the reference.</param>
    /// <returns>Return a <see cref="vtchar_t"/> reference.</returns>
    public ref vtchar_t this[int x, int y] => ref m_buffer[x, y];

    /// <summary>
    /// Create new <see cref="ConsoleBuffer"/> with specific dimension.
    /// </summary>
    /// <param name="x">Width of the from.</param>
    /// <param name="y">Height of the from.</param>
    public ConsoleBuffer(int x, int y) {
        m_scale = new Vec2(x, y);
        m_startScale = new Vec2(x, y);

        m_buffer = new vtchar_t[x, y];
        Clear();
    }

    private ConsoleBuffer(vtchar_t[,] buffer, Vec2 scale) {
        m_buffer = buffer;
        m_scale = scale;

        m_startScale = scale;
    }

    /// <summary>
    /// Copy <paramref name="from"/> to this from.
    /// </summary>
    /// <param name="from">Source from.</param>
    public void Copy(in ConsoleBuffer from) {
        for (int x = 0; x < m_scale.X; ++x) {
            for (int y = 0; y < m_scale.Y; ++y) {
                this[x, y] = from[x, y];
            }
        }
    }

    public void Clear() {
        for (int x = 0; x < m_scale.X; ++x) {
            for (int y = 0; y < m_scale.Y; ++y) {
                ref vtchar_t ch = ref m_buffer[x, y];
                ch.Clear();
            }
        }
    }

    /// <summary>
    /// Create a slice from the current <see cref="ConsoleBuffer"/>.
    /// </summary>
    /// <param name="buffer">Source of the from.</param>
    /// <param name="from">Absolute index of the from.</param>
    /// <param name="scale">Scale of the from.</param>
    /// <returns>Return a <see cref="Canvas"/> instance.</returns>
    public static Canvas Slice(ref ConsoleBuffer buffer, Vec2 from, Vec2 scale) {
        if (from.X < 0) scale.X += from.X;
        else if (from.X + scale.X >= buffer.Scale.X) scale.X = buffer.Scale.X - from.X;

        if (from.Y < 0) scale.Y += from.Y;
        else if (from.Y + scale.Y >= buffer.Scale.Y) scale.Y = buffer.Scale.Y - from.Y;

        from.X = float.Clamp(from.X, 0, buffer.Scale.X - 1);
        from.Y = float.Clamp(from.Y, 0, buffer.Scale.Y - 1);

        return new Canvas(ref buffer, scale, from);
    }

    public static ConsoleBuffer Reallocate(ConsoleBuffer buffer, Vec2 scale) {
        if (buffer.m_startScale.X < scale.X || buffer.m_startScale.Y < scale.Y) {
            vtchar_t[,] _new = new vtchar_t[(int)scale.X, (int)scale.Y];
            return new ConsoleBuffer(_new, scale);
        }

        return new ConsoleBuffer(buffer.m_buffer, scale);
    }
}
