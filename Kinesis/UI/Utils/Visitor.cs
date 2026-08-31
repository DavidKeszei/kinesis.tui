using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a local visitor on the entity-tree to the bottom.
/// </summary>
public readonly ref struct Visitor {
    private const int MAX_USE_ENTITY_COUNT = 32;

    private static Dictionary<string, Entity> s_mostUsed = null!;
    private readonly Entity? m_pivot = null!;

    internal Visitor(Entity? pivot) {
        m_pivot = pivot;
        s_mostUsed ??= new Dictionary<string, Entity>();
    }

    /// <summary>
    /// Visit a specific <typeparamref name="T"/> entity in the tree.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="name">Unique name of the entity.</param>
    /// <returns>Returns an entity as <typeparamref name="T"/>. If not in the tree, then return <see langword="null"/>.</returns>
    public T? Visit<T>(string name) where T: Entity {
        if (string.IsNullOrEmpty(name) || m_pivot == null) return null!;
        else if (IsSequenceEqual(m_pivot.Name, name) && m_pivot is T ret) return ret;

        if (s_mostUsed.TryGetValue(name, out Entity? result))
            return (T)result;

        result = RecursiveVisit(current: m_pivot, name);

        if (s_mostUsed.Count >= MAX_USE_ENTITY_COUNT)
            s_mostUsed.Clear();

        if(result != null) _ = s_mostUsed.TryAdd(name, result!);
        return result as T;
    }

    internal void ClearCache() => s_mostUsed.Clear();

    private Entity? RecursiveVisit(Entity? current, string name) {
        if (current == null) return null!;

        int childrenCount = current.CountComponent<Hierarchy>();

        for (int i = Hierarchy.ChildrenStart; i < childrenCount; ++i) {
            Entity? child = current.Get<Hierarchy>(index: i)?.Attached;

            if (child != null) {
                if (!string.IsNullOrEmpty(child.Name) && IsSequenceEqual(child.Name, name)) return child;
                else {
                    child = RecursiveVisit(child, name);

                    if (child != null) return child;
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
