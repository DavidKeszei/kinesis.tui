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
public abstract class Island: Entity {
    private readonly List<Entity> m_renderSet = null!;
    private bool m_isActive = false;

    /// <summary>
    /// Created <see cref="Entity"/> instance-tree as "list".
    /// </summary>
    internal IReadOnlyList<Entity> Tree { get => m_renderSet; }

    /// <summary>
    /// Indicates the <see cref="Island"/> is active by the <see cref="Renderer"/> and the <see cref="INavigator"/>.
    /// </summary>
    internal bool IsActive { get => m_isActive; set => m_isActive = value; }

    public Island() {
        m_renderSet = new List<Entity>(capacity: 32);

        this.AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDir.UP });
        this.AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDir.DOWN });
    }

    /// <summary>
    /// Build the island as <see cref="Entity"/>.
    /// </summary>
    /// <returns>Return <see cref="Entity"/> instance from the current <see cref="Island"/>.</returns>
    protected abstract Entity? Build(BuildContext context);

    /// <summary>
    /// Move through the tree and create list from it.
    /// </summary>
    /// <param name="context">Context of the current build period.</param>
    internal void CreateRenderSet(BuildContext context) {
        if (context.Current is Island island) {
            Entity? created = island.Build(context);
            if (created == null) return;

            context.Current.GetComponent<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = created;
            created.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = context.Current;
        }

        if(context.Current == null) return;
        int childrenCount = context.Current.CountComponent<Hierarchy>();

        if (context.Current.GetComponent<RenderComponent>() != null) {
            ++context.RenderId;
            m_renderSet.Add(context.Current!);

            context.Current.GetComponent<RenderHierarchy>()!.Depth = context.Depth;
            context.Current.GetComponent<RenderHierarchy>()!.NextRenderElementIndex = context.RenderId;
        }

        for (int i = Hierarchy.ChildrenStart; i < childrenCount; ++i) {
            Hierarchy child = context.Current!.GetComponent<Hierarchy>(i)!;

            if (child.Attached != null) {
                CreateRenderSet(context: context with {
                    Current = child.Attached,
                    Depth = child.Attached.GetComponent<RenderComponent>() == null ? context.Depth + 1 : context.Depth
                });
            }
        }
    }
}