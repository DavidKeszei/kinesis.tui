using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;

namespace Kinesis.UI;

/// <summary>
/// Represent a segment on the screen. Acts like a container for complex objects.
/// </summary>
public abstract class Island: Entity {
    private readonly List<Entity> m_renderSet = null!;
    private bool m_isActive = false;
    private bool m_isBuilded = false;

    /// <summary>
    /// Created <see cref="Entity"/> instance-tree as "list".
    /// </summary>
    internal List<Entity> DrawCalls { get => m_renderSet; }

    /// <summary>
    /// Indicates the <see cref="Island"/> is active by the <see cref="Renderer"/> and the <see cref="INavigator"/>.
    /// </summary>
    internal bool IsActive { get => m_isActive; set => m_isActive = value; }

    /// <summary>
    /// Indicates the current <see cref="Island"/> was built. This only <see cref="true"/>, if the island a root island.
    /// </summary>
    internal bool IsBuilt { get => m_isBuilded; }

    public Island() {
        m_renderSet = new List<Entity>(capacity: 32);

        this.Attach<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.UP });
        this.Attach<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.DOWN });

        this.Attach<DrawCalls>(component: new DrawCalls(), isUnique: true);
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
    internal void BuildTree(BuildContext context) {
        if (context.Current is Island island) {
            context.CurrentIsland = island;

            Entity? created = island.Build(context);
            if (created == null) return;

            if (!context.IsTop) context.CurrentIsland.Remove<DrawCalls>();
            context.Root.Get<DrawCalls>()!.Add(island);

            context.Current.Get<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = created;
            created.Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = context.Current;
        }

        if (context.Current == null) return;
        int childrenCount = context.Current.CountComponent<Hierarchy>();

        if (context.Current.Get<RenderComponent>() != null)
            context.CurrentIsland.DrawCalls.Add(context.Current!);

        if (context.Current is ICopyable<BuildContext> copyable)
            copyable.Copy(from: ref context);

        for (int i = Hierarchy.ChildrenStart; i < childrenCount; ++i) {
            Hierarchy child = context.Current!.Get<Hierarchy>(i)!;

            if (child.Attached != null) {
                BuildTree(context: context with {
                    IsTop = false,
                    ChangeStyleFlag = 0,
                    Current = child.Attached,
                });
            }
        }

        context.DropCurrentLevelStyles();
        if(context.IsTop) m_isBuilded = true;
    }
}