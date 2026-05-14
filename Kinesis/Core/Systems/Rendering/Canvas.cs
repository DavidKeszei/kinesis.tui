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

    /// <summary>
    /// Get a <see cref="vtchar_t"/> reference from the canvas.
    /// </summary>
    /// <param name="x">X coordinate of the reference on the screen.</param>
    /// <param name="y">Y coordinate of the reference on the screen.</param>
    /// <returns>Return a <see cref="vtchar_t"/> reference.</returns>
    public ref vtchar_t this[int x, int y] { get => ref m_buffer[(int)m_position.X + x, (int)m_position.Y + y]; }

    /// <summary>
    /// Scale of the <see cref="Canvas"/>.
    /// </summary>
    public Vec2 Scale { get => m_scale; }

    /// <summary>
    /// Create a new <see cref="Canvas"/> from a <paramref name="buffer"/> based on the <paramref name="scale"/>.
    /// </summary>
    /// <param name="buffer">Buffer of the screen.</param>
    /// <param name="scale">Scale of the <see cref="Canvas"/>.</param>
    internal Canvas(ref ConsoleBuffer buffer, Vec2 scale, Vec2 position) {
        m_buffer = ref buffer;
        m_scale = scale;

        m_position = position;
    }
}
