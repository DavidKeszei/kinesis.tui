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
        base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.UP });
        base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.DOWN });

        m_renderSet = new List<Entity>(32);
    }

    /// <summary>
    /// Move through the tree and create list from it.
    /// </summary>
    /// <param name="context">Current target entity of the call.</param>
    internal void CreateRenderSet(BuildContext context = default) {
        /* If the Current is an Island, then build it & switch to the created entity */
        if (context.Current is Island island) {
            Hierarchy parent = island.GetComponent<Hierarchy>(Hierarchy.Parent)!;
            context.Current = (context.IsTop ? this.Build(context) : island.Build(context));

            if (context.Current == null) return;
            context.Current.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = parent.Attached;
        }

        int childrenCount = context.Current.CountComponent<Hierarchy>();

        if (context.Current.GetComponent<RenderComponent>() != null) {
            ++context.RenderId;
            m_renderSet.Add(context.Current!);

            context.Current.GetComponent<RenderHierarchy>()!.Depth = context.Depth;
            context.Current.GetComponent<RenderHierarchy>()!.NextRenderElementIndex = context.RenderId;
        }

        for (int i = Hierarchy.ChildrenStart; i < childrenCount; ++i) {
            Hierarchy child = context.Current!.GetComponent<Hierarchy>(i)!;

            if (child.Attached != null){
                BuildContext childContext = context with {
                    Current = child.Attached,
                    Depth = child.Attached.GetComponent<RenderComponent>() == null ? context.Depth + 1 : context.Depth
                };
                CreateRenderSet(childContext);
            }
        }
    }

    /// <summary>
    /// Build a <typeparamref name="T"/> island with custom <paramref name="prop"/>s.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="Island"/> instance.</typeparam>
    /// <param name="context">Global context of the building.</param>
    /// <param name="prop">Configuration callback.</param>
    /// <returns>Return a builded <see cref="Entity"/> from the <typeparamref name="T"/> instance.</returns>
    protected static Entity? BuildWith<T>(BuildContext context, Action<T> prop = null!) where T: Island, new() {
        T island = new T();
        prop?.Invoke(island);

        return island.Build(context);
    }

    /// <summary>
    /// Build the island as <see cref="Entity"/>.
    /// </summary>
    /// <returns>Return <see cref="Entity"/> instance from the current <see cref="Island"/>.</returns>
    protected abstract Entity? Build(BuildContext context);
}