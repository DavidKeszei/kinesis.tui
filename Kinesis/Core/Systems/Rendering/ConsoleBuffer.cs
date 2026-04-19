using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Represent a <see cref="ConsoleBuffer"/> on the screen.
/// </summary>
internal readonly unsafe struct ConsoleBuffer: IDisposable {
    private readonly vtchar_t* m_buffer = null!;
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
    public ref vtchar_t this[int x, int y] => ref m_buffer[x + (int)m_startScale.X * y];

    /// <summary>
    /// Create new <see cref="ConsoleBuffer"/> with specific dimension.
    /// </summary>
    /// <param name="x">Width of the from.</param>
    /// <param name="y">Height of the from.</param>
    public ConsoleBuffer(int x, int y) {
        m_scale = new Vec2(x, y);
        m_startScale = new Vec2(x, y);

        m_buffer = Alloc(x, y);
        Clear();
    }

    private ConsoleBuffer(vtchar_t* buffer, Vec2 scale, Vec2 startScale) {
        m_buffer = buffer;
        m_scale = scale;

        m_startScale = startScale;
    }

    /// <summary>
    /// Copy <paramref name="from"/> to this from.
    /// </summary>
    /// <param name="from">Source from.</param>
    public void Copy(in ConsoleBuffer from) {
        int rangeX = (int)(from.Scale.X < m_scale.X ? from.Scale.X : m_scale.X);
        int rangeY = (int)(from.Scale.Y < m_scale.Y ? from.Scale.Y : m_scale.Y);

        for (int x = 0; x < rangeX; ++x) {
            for (int y = 0; y < rangeY; ++y) {
                this[x, y] = from[x, y];
            }
        }
    }

    public void Clear() {
        for (int x = 0; x < m_scale.X; ++x) {
            for (int y = 0; y < m_scale.Y; ++y) {
                ref vtchar_t ch = ref this[x, y];
                ch.Clear();
            }
        }
    }

    public void Dispose()
        => NativeMemory.Free(ptr: m_buffer);

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

    public static ConsoleBuffer Reallocate(in ConsoleBuffer buffer, Vec2 scale) {
        if (buffer.m_startScale.X < scale.X || buffer.m_startScale.Y < scale.Y) {
            ConsoleBuffer allocated = new ConsoleBuffer(buffer: Alloc(x: (int)scale.X, y: (int)scale.Y), scale, scale);

            allocated.Clear();
            allocated.Copy(buffer);

            NativeMemory.Free(ptr: buffer.m_buffer);

            Debug.WriteLine(value: $"Memory was freed up & reallocated... (Scale: {scale.X:f0} x {scale.Y:f0})");
            return allocated;
        }

        return new ConsoleBuffer(buffer.m_buffer, scale, buffer.m_startScale);
    }

    private static vtchar_t* Alloc(int x, int y) 
        => (vtchar_t*)NativeMemory.Alloc(byteCount: (nuint)(Unsafe.SizeOf<vtchar_t>() * (x * y)));
}
