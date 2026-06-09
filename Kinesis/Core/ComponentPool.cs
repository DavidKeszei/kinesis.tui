using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Represents a bunch of reusable <see cref="T"/> instances.
/// </summary>
/// <typeparam name="T">Type of the component.</typeparam>
internal sealed class ComponentPool<T> where T: Component, IStaticType {
    private readonly static int s_preAllocationCount = 1024;
    private static ComponentPool<T> s_instance = null!;

    private readonly List<T> m_components = null!;
    private readonly Dictionary<int, int> m_rentedComponents = null!;

    private readonly Queue<int> m_freeSpaces = null!;
    private bool m_interlock = false;

    public static ComponentPool<T> Instance { get => s_instance ??= new ComponentPool<T>(s_preAllocationCount); }

    public ComponentPool(int allocationSize) {
        m_components = new List<T>(allocationSize);
        m_freeSpaces = new Queue<int>(allocationSize / 2);

        m_rentedComponents = new Dictionary<int, int>(allocationSize / 2);
    }

    /// <summary>
    /// Rents a(n) <typeparamref name="U"/> instance from the pool.
    /// </summary>
    /// <typeparam name="U">Type of the instance. This must a(n) <typeparamref name="T"/> instance with new() constraint.</typeparam>
    /// <returns>Returns a pooled object as <typeparamref name="U"/>.</returns>
    public U Rent<U>(Action<U> settingUp = null!) where U: T, IPoolable, new() {
        while (Interlocked.CompareExchange<bool>(ref m_interlock, true, false) != false)
            Thread.Sleep(millisecondsTimeout: 1);

        bool hasUnused = m_freeSpaces.TryDequeue(out int slot);
        if (!hasUnused) {
            m_components.Add(item: new U());
            slot = m_components.Count - 1;
        }

        U component = (U)m_components[slot];
        m_rentedComponents.Add(key: component.GetHashCode(), value: slot);

        settingUp?.Invoke(component);

        _ = Interlocked.Exchange<bool>(ref m_interlock, false);
        return component;
    }

    /// <summary>
    /// Return a pooled object to the <see cref="ComponentPool{T}"/>.
    /// </summary>
    /// <param name="component">Pooled component instance.</param>
    public void Return(T component) {
        while (Interlocked.CompareExchange<bool>(ref m_interlock, true, false) != false)
            Thread.Sleep(millisecondsTimeout: 1);

        int hash = component.GetHashCode();
        if (!m_rentedComponents.TryGetValue(hash, out int index)) {
            _ = Interlocked.Exchange<bool>(ref m_interlock, false);
            return;
        }

        m_freeSpaces.Enqueue(index);
        m_rentedComponents.Remove(key: hash);

        Interlocked.Exchange<bool>(ref m_interlock, false);
    }
}

