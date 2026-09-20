using Kinesis.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Kinesis.Core.Processing;

/// <summary>
/// Represents unit-of-work like job processor.
/// </summary>
/// <typeparam name="T">Type of the message.</typeparam>
/// <typeparam name="V">Type of the job.</typeparam>
internal sealed class JobHandler<T, V> where T: struct, IJobMessage, IEquatable<T> where V: notnull, IJob<T> {
    private readonly RingBuffer<T> m_messages = null!;
    private readonly List<V> m_targets = null!;

    private readonly ConcurrentQueue<V> m_addQeueu = null!;

    /// <summary>
    /// Registered targets of the current <see cref="JobHandler{T, V}"/> as <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    public IReadOnlyList<V> Targets { get => m_targets; }

    /// <summary>
    /// Current queue of the addition to the current <see cref="JobHandler{T, V}"/> instance.
    /// </summary>
    public ConcurrentQueue<V> AddQueue { get => m_addQeueu; }

    /// <summary>
    /// Create new <see cref="JobHandler{T, V}"/> instance.
    /// </summary>
    /// <param name="preAllocateTargetCount">Pre-allocation count for the jobs.</param>
    /// <param name="capacity">Maximum capacity of the message buffer.</param>
    public JobHandler(int preAllocateTargetCount = 512, int capacity = 32) {
        m_targets  = new List<V>(preAllocateTargetCount);
        m_messages = new RingBuffer<T>(capacity);

        m_addQeueu = new ConcurrentQueue<V>();
    }

    /// <summary>
    /// Add new message to the <see cref="JobHandler{T, V}"/> instance.
    /// </summary>
    /// <param name="message">The message value itself.</param>
    public void AddMessage(T message) {
        if(message.Equals(default)) return;
        m_messages.Write(message);
    }

    /// <summary>
    /// Process one message from the message buffer.
    /// </summary>
    public void Process() {
        AddTargets();
        RemoveTargets();

        if (!m_messages.Read(out T message))
            return;

        foreach (V target in m_targets) {
            if (!target.IsActive)
                continue;
            target.Callback(message);
        }
    }

    private void AddTargets() {
        if(m_addQeueu.IsEmpty) return;

        while(!m_addQeueu.IsEmpty) {
             _= m_addQeueu.TryDequeue(out V? target);
            m_targets.Add(target!);
        }
    }

    private void RemoveTargets() {
        for (int i = 0; i < m_targets.Count; ++i) {
            if (m_targets[i].State == JobRequestIntent.REMOVE) {
                m_targets.Remove(m_targets[i]);
            }
        }
    }
}
