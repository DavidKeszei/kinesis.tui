using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Represent a portion from the screen.
/// </summary>
public readonly ref struct Canvas {
    private readonly ref ConsoleBuffer m_buffer;
    private readonly Vec2 m_scale = Vec2.Zero;

    private readonly Vec2 m_position = Vec2.Zero;
    private readonly Vec2 m_offsetFromStart = Vec2.Zero;

    /// <summary>
    /// Get a <see cref="ANSIChar"/> reference from the canvas.
    /// </summary>
    /// <param name="x">X coordinate of the reference on the screen.</param>
    /// <param name="y">Y coordinate of the reference on the screen.</param>
    /// <returns>Return a <see cref="ANSIChar"/> reference.</returns>
    public ref ANSIChar this[int x, int y] { get => ref m_buffer[(int)m_position.X + x, (int)m_position.Y + y]; }

    /// <summary>
    /// Scale of the <see cref="Canvas"/>.
    /// </summary>
    public Vec2 Scale { get => m_scale; }

    /// <summary>
    /// Start offset from the negative domain of the screen.
    /// </summary>
    public Vec2 Start { get => m_offsetFromStart; }

    /// <summary>
    /// Create a new <see cref="Canvas"/> from a <paramref name="buffer"/> based on the <paramref name="scale"/>.
    /// </summary>
    /// <param name="buffer">Buffer of the screen.</param>
    /// <param name="scale">Scale of the <see cref="Canvas"/>.</param>
    internal Canvas(ref ConsoleBuffer buffer, Vec2 scale, Vec2 position, Vec2 start) {
        m_buffer = ref buffer;
        m_scale = scale;

        m_position = position;
        m_offsetFromStart = new Vec2(x: start.X < 0 ? start.X * -1 : 0, y: start.Y < 0 ? start.Y * -1 : 0);
    }
}
