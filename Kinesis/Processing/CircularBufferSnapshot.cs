using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Processing;

/// <summary>
/// Represent a snapshot from <see cref="CircularBuffer{T}"/> instance.
/// </summary>
/// <typeparam name="T">Same with the <see cref="CircularBuffer{T}"/>.</typeparam>
internal ref struct CircularBufferSnapshot<T>: IEnumerator<T> {
    private readonly T[] m_buffer = null!;
    private readonly int m_limit = 0;

    private int m_current = -1;

    public readonly T Current { get => m_buffer[m_current]; }

    readonly object? IEnumerator.Current { get => Current; }

    public CircularBufferSnapshot(ReadOnlySpan<T> buffer) {
        m_buffer = ArrayPool<T>.Shared.Rent(buffer.Length);
        m_limit = buffer.Length;

        for (int i = 0; i < buffer.Length; ++i)
            m_buffer[i] = buffer[i];
    }

    public bool MoveNext() {
        if (++m_current >= m_limit)
            return false;

        return true;
    }

    public readonly void Dispose() => ArrayPool<T>.Shared.Return(m_buffer);

    public void Reset() => m_current = 0;
}
