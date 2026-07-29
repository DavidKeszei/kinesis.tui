using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a local visitor on the entity-tree to the bottom.
/// </summary>
public ref struct IslandEntityVisitor {
    private const int MAX_USE_ENTITY_COUNT = 32;

    private static Dictionary<string, Entity> m_mostUsed = null!;
    private readonly Entity? m_pivot = null!;

    internal IslandEntityVisitor(Entity? pivot) {
        m_pivot = pivot;
        m_mostUsed ??= new Dictionary<string, Entity>();
    }

    /// <summary>
    /// Visit a specific <typeparamref name="T"/> entity in the tree.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="name">Unique name of the entity.</param>
    /// <returns>Return a entity as <typeparamref name="T"/>. If not in the tree, then return <see langword="null"/>.</returns>
    public T? Visit<T>(string name) where T: Entity {
        if (string.IsNullOrEmpty(name) || m_pivot == null) return null!;
        else if (IsSequenceEqual(m_pivot.Name, name) && m_pivot is T ret) return ret;

        if (m_mostUsed.TryGetValue(name, out Entity? result))
            return (T)result;

        result = RecursiveVisit(current: m_pivot, name);

        if (m_mostUsed.Count >= MAX_USE_ENTITY_COUNT)
            m_mostUsed.Clear();

        if(result != null) _ = m_mostUsed.TryAdd(name, result!);
        return result as T;
    }

    internal readonly void ClearCache() => m_mostUsed.Clear();

    private Entity? RecursiveVisit(Entity? current, string name) {
        if (current == null) return null!;

        int childrenCount = current.CountComponent<Hierarchy>();
        for (int i = 1; i < childrenCount; ++i) {
            Entity? child = current.Get<Hierarchy>(index: i)?.Attached;

            if (child != null) {
                if (!string.IsNullOrEmpty(child.Name) && IsSequenceEqual(child.Name, name)) return child;
                else {
                    child = RecursiveVisit(child, name);

                    if (child != null)
                        return child;
                }
            }
        }

        return null!;
    }

    private bool IsSequenceEqual(ReadOnlySpan<char> left, ReadOnlySpan<char> right) {
        if (left.Length != right.Length) return false;

        for (int i = 0; i < left.Length; ++i)
            if (left[i] != right[i])
                return false;

        return true;
    }
}
