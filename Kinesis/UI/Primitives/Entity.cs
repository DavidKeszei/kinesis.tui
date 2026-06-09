using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent base-class for all UI elements on the screen.
/// </summary>
public class Entity: IDisposable {
    protected const int MAX_COMPONENT_COUNT = 16;

    private readonly Dictionary<int, int> m_uniqueComponents = null!;
    private readonly Queue<int> m_emptySpaces = null!;

    private readonly Component[] m_components = null!;
    private readonly string m_name = string.Empty;

    private int m_version = 0;
    private int m_lastEntityIndex = 0;

    private bool m_disposed = false;

    /// <summary>
    /// Version of the entity.
    /// </summary>
    internal int Version { get => m_version; set => m_version = value; }

    /// <summary>
    /// Name of the entity.
    /// </summary>
    public string Name { get => m_name; init => m_name = value; }

    /// <summary>
    /// Create a new <see cref="Entity"/> instance.
    /// </summary>
    public Entity(int count = MAX_COMPONENT_COUNT) {
        m_version = 0;
        m_components = ArrayPool<Component>.Shared.Rent(minimumLength: count);

        m_emptySpaces = new Queue<int>(capacity: 16);
        m_uniqueComponents = new Dictionary<int, int>();
    }

    ~Entity() => ReturnRendtedComponents();

    /// <summary>
    /// Attach a(n) <typeparamref name="T"/> component to the current instance.
    /// </summary>
    /// <typeparam name="T">Type of the instance.</typeparam>
    /// <param name="component">Pre-defined value of the component. If this <see langword="null"/>, then the system creates a default component.</param>
    /// <param name="isUnique">Indicates the component is unique on the <see cref="Entity"/>.</param>
    /// <returns>Return <see langword="true"/> if the component is added to the entity. Otherwise return <see langword="false"/>.</returns>
    public bool Attach<T>(T component = null!, bool isUnique = false) where T: Component, IStaticType, new() {
        if (component == null) return false;
        bool hasEmptySlot = m_emptySpaces.TryDequeue(out int slot);

        if(isUnique || component.TypeOf(type: RenderComponent.Name)) {
            if(!m_uniqueComponents.TryAdd(ComponentRegistry.QueryComponent(name: T.Name), !hasEmptySlot ? m_lastEntityIndex : slot))
                return false;
        }

        if (!hasEmptySlot) slot = m_lastEntityIndex++;

        this.m_components[slot] = component;
        ++m_version;

        return true;
    }

    /// <summary>
    /// Get a(n) <typeparamref name="T"/> component from the current instance.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="index">Indicates, which component we wan't from the type. (Example: if the index = 1, then return second component of the <typeparamref name="T"/>.)</param>
    /// <returns>Returns a(n) <typeparamref name="T"/> component. If not exists, then return <see langword="null"/>.</returns>
    public T? Get<T>(int index = 0) where T: Component, IStaticType {
        if (index < 0) return null!;

        if (m_uniqueComponents.TryGetValue(ComponentRegistry.QueryComponent(T.Name), out int i))
            return (T)m_components[i];

        int current = 0;
        for(; i < m_lastEntityIndex; ++i) {
            if (m_components[i] == null) continue;

            if (m_components[i].TypeOf(T.Name) && current++ == index)
                return (T)m_components[i];
        }

        return default!;
    }

    /// <summary>
    /// Remove a component from the current <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="index">Indicates where we want delete the component.</param>
    public void Remove<T>(int index = 0) where T: Component, IStaticType {
        if (index < 0) return;

        if (m_uniqueComponents.TryGetValue(key: ComponentRegistry.QueryComponent(name: T.Name), out int i)) {
            if (m_components[i] is IPoolable reset) 
                reset.Reset();

            m_components[i] = null!;
            m_uniqueComponents.Remove(key: ComponentRegistry.QueryComponent(name: T.Name));

            m_emptySpaces.Enqueue(i);
            ++m_version;
            return;
        }

        int indexOf = 0;
        for (; i < m_lastEntityIndex; ++i) {
            if (m_components[i] == null) continue;

            if (m_components[i].TypeOf(type: T.Name) && indexOf++ == index) {

                if (m_components[i] is IPoolable reset) reset.Reset();
                m_components[i] = null!;

                m_emptySpaces.Enqueue(i);
                ++m_version;
                return;
            }
        }
    }

    public void Dispose() {
        ReturnRendtedComponents();
        GC.SuppressFinalize(this);
    }

    public ComponentIterator<Component> GetEnumerator()
        => new ComponentIterator<Component>(components: m_components, count: (uint)m_lastEntityIndex);

    /// <summary>
    /// Initialize the current <see cref="Entity"/> instance with some basic render properties.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="RenderComponent"/>.</typeparam>
    protected void InitRenderEntityWith<T>() where T: RenderComponent, IStaticType, IPoolable, new() {
        _ = this.Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = this.Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static(x) => x.Value = Vec2.One * Scale.Auto), isUnique: true);

        _ = this.Attach<T>(ComponentPool<T>.Instance.Rent<T>(), isUnique: true);
        _ = this.Attach<Hierarchy>(ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.UP));
    }

    private void ReturnRendtedComponents() {
        if (m_disposed) return;

        m_disposed = true;
        foreach (Component comp in m_components) {
            if (comp is IPoolable rent)
                rent.Reset();
        }

        ArrayPool<Component>.Shared.Return(m_components, clearArray: true);
        m_uniqueComponents.Clear();
        m_emptySpaces.Clear();
    }
}