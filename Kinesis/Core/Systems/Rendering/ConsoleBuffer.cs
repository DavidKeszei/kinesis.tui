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
internal readonly struct ConsoleBuffer: IDisposable {
    private readonly unsafe vtchar_t* m_buffer = null!;
    private readonly Vec2 m_scale = new Vec2(-1, -1);

    private readonly Vec2 m_startScale = new Vec2(-1, -1);

    /// <summary>
    /// Current scale of the <see cref="ConsoleBuffer"/>.
    /// </summary>
    public Vec2 Scale { get => m_scale; }

    /// <summary>
    /// Get a <see cref="ANSIChar"/> reference from the from.
    /// </summary>
    /// <param name="x">X position of the reference.</param>
    /// <param name="y">Y position of the reference.</param>
    /// <returns>Return a <see cref="ANSIChar"/> struct by reference.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    public ref vtchar_t this[int x, int y] { 
        get {
            unsafe { 
                return ref m_buffer[x + (int)m_startScale.X * y]; 
            }
        }
    }

    /// <summary>
    /// Create new <see cref="ConsoleBuffer"/> with specific dimension.
    /// </summary>
    /// <param name="x">Width of the from.</param>
    /// <param name="y">Height of the from.</param>
    public ConsoleBuffer(int x, int y) {
        m_scale = new Vec2(x, y);
        m_startScale = new Vec2(x, y);

        unsafe { m_buffer = Alloc(x, y); }
        Clear();
    }

    private unsafe ConsoleBuffer(ANSIChar* buffer, Vec2 scale, Vec2 startScale) {
        m_buffer = buffer;
        m_scale = scale;

        m_startScale = startScale;
    }

    /// <summary>
    /// Copy <paramref name="from"/> to this from.
    /// </summary>
    /// <param name="from">Content from.</param>
    public void Copy(in Canvas from) {
        int rangeX = (int)(from.Scale.X < m_scale.X ? from.Scale.X : m_scale.X);
        int rangeY = (int)(from.Scale.Y < m_scale.Y ? from.Scale.Y : m_scale.Y);

        for (int x = 0; x < rangeX; ++x) {
            for (int y = 0; y < rangeY; ++y) {
                this[x, y] = from[x, y];
            }
        }
    }

    /// <summary>
    /// Clear the buffer with default values.
    /// </summary>
    public void Clear() {
        for (int x = 0; x < m_scale.X; ++x) {
            for (int y = 0; y < m_scale.Y; ++y) {
                ref vtchar_t ch = ref this[x, y];
                ch.Clear();
            }
        }
    }

    public void Dispose() {
        unsafe { NativeMemory.Free(ptr: m_buffer); }
    }

    /// <summary>
    /// Create a slice from the current <see cref="ConsoleBuffer"/>.
    /// </summary>
    /// <param name="buffer">Content of the from.</param>
    /// <param name="from">Absolute index of the <see cref="Canvas"/> instance on the <paramref name="buffer"/>.</param>
    /// <param name="scale">Requested scale.</param>
    /// <returns>Return a <see cref="Canvas"/> instance.</returns>
    public static Canvas Slice(ref ConsoleBuffer buffer, Vec2 from, Vec2 scale) {
        Vec2 start = from;

        if (from.X < 0) scale.X += from.X;
        else if (from.X + scale.X >= buffer.Scale.X) scale.X = buffer.Scale.X - from.X; // <- This mostly used by the internal Reallocate()

        if (from.Y < 0) scale.Y += from.Y;
        else if (from.Y + scale.Y >= buffer.Scale.Y) scale.Y = buffer.Scale.Y - from.Y; // <- This mostly used by the internal Reallocate()

        from.X = float.Clamp(from.X, 0, buffer.Scale.X);
        from.Y = float.Clamp(from.Y, 0, buffer.Scale.Y);

        return new Canvas(ref buffer, scale, from, start);
    }

    /// <summary>
    /// Reallocate the <paramref name="buffer"/> with new <paramref name="scale"/>.
    /// </summary>
    /// <param name="buffer">Target/Old buffer of the reallocation.</param>
    /// <param name="scale">New scale of the buffer.</param>
    /// <returns>Returns a <see cref="ConsoleBuffer"/> instance.</returns>
    public static ConsoleBuffer Reallocate(ref ConsoleBuffer buffer, Vec2 scale) {
        ConsoleBuffer temp = default;
        unsafe { temp = new ConsoleBuffer(buffer: Alloc(x: (int)scale.X, y: (int)scale.Y), scale, startScale: scale); }

        temp.Clear();
        temp.Copy(from: Slice(ref buffer, from: Vec2.Zero, scale));

        if (buffer.m_startScale.X < scale.X || buffer.m_startScale.Y < scale.Y) {
            unsafe { NativeMemory.Free(ptr: buffer.m_buffer); }

            Debug.WriteLine(value: $"Memory was freed up & reallocated... (Scale: {scale.X:f0} x {scale.Y:f0})");
            return temp;
        }

        /* 
         * Force clear & copy; This forcing the diff for check the new/old cells.
         * If we not do this, then we just cut it out, but if resize back, then the diff
         * was not seen any differences (because the not saw cells not changed), but we on the screen see the not rendered objects.
         */
        buffer.Clear();
        buffer.Copy(Slice(ref temp, from: Vec2.Zero, scale));

        temp.Dispose();
        unsafe { return new ConsoleBuffer(buffer.m_buffer, scale, startScale: buffer.m_startScale); }
    }

    private static unsafe ANSIChar* Alloc(int x, int y) 
        => (ANSIChar*)NativeMemory.Alloc(byteCount: (nuint)(Unsafe.SizeOf<ANSIChar>() * (x * y)));
}
