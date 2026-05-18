using Kinesis.Core;
using Kinesis.Core.Rendering;
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
    public bool AttachComponent<T>(T component, bool isUnique = false) where T: Component, IStaticType {
        if (component == null) return false;

        bool hasEmptySlot = m_emptySpaces.TryDequeue(out int slot);

        if(isUnique || component.TypeOf(type: RenderComponent.Name)) {
            if(!m_uniqueComponents.TryAdd(ComponentRegistry.QueryComponent(name: T.Name), !hasEmptySlot ? m_components.Count : slot))
                return false;
        }

        if (!hasEmptySlot) this.m_components.Add(component);
        else this.m_components[slot] = component;

        ++m_version;
        return true;
    }

    /// <summary>
    /// Get a(n) <typeparamref name="T"/> component from the current instance.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="index">Indicates, which component we wan't from the type. (Example: if the index = 1, then return second component of the <typeparamref name="T"/>.)</param>
    /// <returns>Return <typeparamref name="T"/> component. If not exists, then return <see langword="null"/>.</returns>
    public T? GetComponent<T>(int index = 0) where T: Component, IStaticType {
        if (index < 0) return null!;

        if (m_uniqueComponents.TryGetValue(ComponentRegistry.QueryComponent(T.Name), out int i))
            return (T)m_components[i];

        int current = 0;
        foreach (Component component in m_components) {
            if (component == null) continue;

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
        _ = this.AttachComponent<Position>(new Position(origin: null!), isUnique: true);
        _ = this.AttachComponent<Scale>(new Scale(scale: Vec2.One * Scale.Auto), isUnique: true);

        _ = this.AttachComponent<T>(new T(), isUnique: true);
        _ = this.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.UP });
    }

    /// <summary>
    /// Create a new <see cref="Entity"/> instance with scale, position and basic up and down connection.
    /// </summary>
    /// <param name="name">Name of the instance.</param>
    /// <param name="content">Content of the instance.</param>
    /// <returns>Return an <see cref="Entity"/> with scale, position and connections.</returns>
    protected static Entity CreatePlaceholder(string name, Entity? content = null!) {
        Entity entity = new Entity() { Name = name };
        entity.AttachComponent<Position>(new Position(origin: null!), isUnique: true);
        entity.AttachComponent<Scale>(new Scale(scale: Vec2.One * Scale.Auto), isUnique: true);

        entity.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.UP });
        entity.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.DOWN, Attached = content ?? null! });

        return entity;
    }
}