using Kinesis.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent base-class for all UI elements on the screen.
/// </summary>
public class Entity {
    private readonly Dictionary<int, int> m_uniqueComponents = null!;
    private readonly Queue<int> m_emptySpaces = null!;

    private readonly List<Component> m_components = null!;
    private readonly string m_name = string.Empty;

    private int m_version = 0;

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
    public Entity() {
        m_version = 0;
        m_components = new List<Component>(capacity: 16);

        m_emptySpaces = new Queue<int>(capacity: 16);
        m_uniqueComponents = new Dictionary<int, int>();
    }

    /// <summary>
    /// Attach a(n) <typeparamref name="T"/> component to the current instance.
    /// </summary>
    /// <typeparam name="T">Type of the instance.</typeparam>
    /// <param name="component">Pre-defined value of the component. If this <see langword="null"/>, then the system creates a default component.</param>
    /// <param name="isUnique">Indicates the component is unique on the <see cref="Entity"/>.</param>
    /// <returns>Return <see langword="true"/> if the component is added to the entity. Otherwise return <see langword="false"/>.</returns>
    public bool AttachComponent<T>(T? component = null!, bool isUnique = false) where T: Component, IStaticType {
        if (component == null) return false;
        if(isUnique || component.TypeOf(type: RenderComponent.Name)) {
            if(!m_uniqueComponents.TryAdd(ComponentRegistry.QueryComponent(name: T.Name), m_components.Count))
                return false;
        }

        if (m_emptySpaces.Count == 0) this.m_components.Add(component);
        else this.m_components[m_emptySpaces.Dequeue()] = component;

        ++m_version;
        return true;
    }

    /// <summary>
    /// Get a(n) <typeparamref name="T"/> component from the current instance.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="index">Indicates, which component we wan't from the type. (Example: if the index = 1, then return second component of the <typeparamref name="T"/>.)</param>
    /// <returns>Return <typeparamref name="T"/> component. If not exists, then return <see langword="null"/>.</returns>
    public virtual T? GetComponent<T>(int index = 0) where T: Component, IStaticType {
        if (index < 0) return null!;

        if (m_uniqueComponents.TryGetValue(ComponentRegistry.QueryComponent(T.Name), out int i))
            return (T)m_components[i];

        int current = 0;
        foreach (Component component in m_components) {
            if (component.TypeOf(T.Name) && current++ == index)
                return component as T;
        }

        return default!;
    }

    /// <summary>
    /// Remove a component from the current <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="index">Indicates where we want delete the component.</param>
    public void RemoveComponent<T>(int index = 0) where T: Component, IStaticType {
        if (m_uniqueComponents.TryGetValue(key: ComponentRegistry.QueryComponent(name: T.Name), out int i)) {
            m_components[i] = null!;
            m_uniqueComponents.Remove(key: ComponentRegistry.QueryComponent(name: T.Name));

            m_emptySpaces.Enqueue(i);
            ++m_version;
            return;
        }

        int indexOf = 0;
        for (; i < m_components.Count; ++i) {
            if (m_components[i].TypeOf(type: T.Name) && indexOf++ == index) {
                m_components[i] = null!;

                m_emptySpaces.Enqueue(i);
                ++m_version;
                return;
            }
        }
    }

    public ComponentIterator<Component> GetEnumerator()
        => new ComponentIterator<Component>(components: m_components, count: (uint)m_components.Count);

    /// <summary>
    /// Initialize the current <see cref="Entity"/> instance with some basic render properties.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="RenderComponent"/>.</typeparam>
    protected void InitRenderEntityWith<T>() where T: RenderComponent, IStaticType, new() {
        _ = this.AttachComponent<Transform>(new Transform(), isUnique: true);
        _ = this.AttachComponent<T>(new T(), isUnique: true);

        _ = this.AttachComponent<RenderHierarchy>(new RenderHierarchy(), isUnique: true);
        _ = this.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.UP });
    }
}