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

    /// <summary>
    /// Created <see cref="Entity"/> instance-tree as "list".
    /// </summary>
    internal List<Entity> Tree { get => m_renderSet; }

    /// <summary>
    /// Indicates the <see cref="Island"/> is active by the <see cref="ImmediateRenderer"/> and the <see cref="INavigator"/>.
    /// </summary>
    internal bool IsActive { get => m_isActive; set => m_isActive = value; }

    public Island() {
        m_renderSet = new List<Entity>(capacity: 32);

        this.AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.UP });
        this.AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.DOWN });
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
            Entity? created = island.Build(context);
            if (created == null) return;

            context.Current.GetComponent<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = created;
            created.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = context.Current;
        }

        if (context.Current == null) return;

        int childrenCount = context.Current.CountComponent<Hierarchy>();
        if (context.Current.GetComponent<RenderComponent>() != null)
            m_renderSet.Add(context.Current!);

        if (context.Current is ICopyable<BuildContext> copyable)
            copyable.Copy(from: ref context);

        for (int i = Hierarchy.ChildrenStart; i < childrenCount; ++i) {
            Hierarchy child = context.Current!.GetComponent<Hierarchy>(i)!;

            if (child.Attached != null) {
                BuildTree(context: context with {
                    Current = child.Attached,
                    ChangeStyleFlag = 0
                });
            }
        }

        context.DropCurrentLevelStyles();
    }
}