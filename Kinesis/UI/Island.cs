using Kinesis.Rendering;
using Kinesis.UI.Components;
using Kinesis.Navigation;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a segment on the screen.
/// </summary>
public abstract class Island {
    private readonly List<Entity> m_renderSet = null!;
    private int m_currentRenderIndex = 0;

    private bool m_isTopOfTheTree = true;
    private bool m_isActive = false;

    /// <summary>
    /// Created <see cref="Entity"/> instance-tree as "list".
    /// </summary>
    internal IReadOnlyList<Entity> Tree { get => m_renderSet; }

    /// <summary>
    /// Indicates the <see cref="Island"/> is active by the <see cref="Renderer"/> and the <see cref="INavigator"/>.
    /// </summary>
    internal bool IsActive { get => m_isActive; set => m_isActive = value; }

    /// <summary>
    /// Implicit conversion between <see cref="Island"/> and <see cref="Entity"/>.
    /// </summary>
    /// <param name="island">Current <see cref="Island"/> instance.</param>
    public static implicit operator Entity(Island island) => island.Build() ?? null!;

    public Island()
        => m_renderSet = new List<Entity>(capacity: 32);

    /// <summary>
    /// Build the island as <see cref="Entity"/>.
    /// </summary>
    /// <returns>Return <see cref="Entity"/> instance from the current <see cref="Island"/>.</returns>
    protected abstract Entity? Build();

    /// <summary>
    /// Move through the tree and create list from it.
    /// </summary>
    /// <param name="entity">Current target entity of the call.</param>
    internal void CreateRenderSet(Entity? entity = null!, int depth = 0) {
        if (m_isTopOfTheTree) {
            entity ??= Build();
            m_isTopOfTheTree = false;
        }

        if(entity == null) return;
        int childrenCount = entity.CountComponent<Hierarchy>();

        if (entity.GetComponent<RenderComponent>() != null) {
            ++m_currentRenderIndex;
            m_renderSet.Add(entity!);

            entity.GetComponent<RenderHierarchy>()!.Depth = depth;
            entity.GetComponent<RenderHierarchy>()!.NextRenderElementIndex = m_currentRenderIndex;
        }

        for (int i = Hierarchy.ChildrenStart; i < childrenCount; ++i) {
            Hierarchy child = entity!.GetComponent<Hierarchy>(i)!;

            if (child.Attached != null)
                CreateRenderSet(entity: child.Attached, child.Attached.GetComponent<RenderComponent>() == null ? depth + 1 : depth);
        }
    }
}