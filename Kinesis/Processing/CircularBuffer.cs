using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Processing;

/// <summary>
/// Represent a circular buffer for <typeparamref name="T"/> instances.
/// </summary>
internal class CircularBuffer<T> {
    #region CONSTS

    private const byte TRUE = 1;
    private const byte FALSE = 0;

    #endregion

    private readonly T[] m_buffer = null!;
    private readonly int m_limit = 0;

    private int m_writePosition = 0;
    private int m_readPosition = 0;

    private int m_count = 0;
    private byte m_interlock = FALSE;

    /// <summary>
    /// Current count of the messages.
    /// </summary>
    public int Count { get => m_count; }

    /// <summary>
    /// Capacity of the current buffer.
    /// </summary>
    public int Capacity { get => m_limit; }

    public CircularBuffer(int capacity) {
        m_buffer = new T[capacity];
        m_limit = capacity;
    }

    /// <summary>
    /// Write a(n) <typeparamref name="T"/> message to the buffer. If the buffer is full, then the message must be rejected.
    /// </summary>
    /// <param name="value">Target message.</param>
    public void Write(T? value) {
        if (value == null || Interlocked.CompareExchange(location1: ref m_interlock, value: TRUE, comparand: FALSE) != FALSE) 
            return;

        if (m_count == m_limit) {
            _ = Interlocked.Exchange(ref m_interlock, FALSE);
            return;
        }

        m_buffer[m_writePosition] = value;
        _ = Interlocked.Increment(location: ref m_writePosition);

        _ = Interlocked.Increment(location: ref m_count);
        m_writePosition %= m_buffer.Length;

        _ = Interlocked.Exchange(location1: ref m_interlock, FALSE);
    }

    /// <summary>
    /// Read a(n) <typeparamref name="T"/> message from the buffer.
    /// </summary>
    /// <param name="message">Output message.</param>
    /// <returns>If the buffer has any unread message, then return <see langword="true"/>. Otherwise return <see langword="false"/>.</returns>
    public bool Read(out T? message) {
        if (m_count == 0 || Interlocked.CompareExchange(ref m_interlock, value: TRUE, comparand: FALSE) != FALSE) {
            message = default;
            return false;
        }

        message = m_buffer[m_readPosition];
        _ = Interlocked.Increment(location: ref m_readPosition);

        _ = Interlocked.Decrement(location: ref m_count);
        m_readPosition %= m_buffer.Length;

        _ = Interlocked.Exchange(ref m_interlock, FALSE);
        return true;
    }

    public CircularBufferSnapshot<T> GetEnumerator() {
        while(Interlocked.CompareExchange(ref m_interlock, TRUE, TRUE) == TRUE);

        _ = Interlocked.Exchange(ref m_interlock, TRUE);
        CircularBufferSnapshot<T> snapshot = new CircularBufferSnapshot<T>(buffer: m_buffer.AsSpan()[..m_count]);
        _ = Interlocked.Exchange(ref m_interlock, FALSE);

        return snapshot;
    }
}