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
    private readonly List<Entity> m_entities = null!;

    private Island m_root = null!;
    private int m_chunkId = -1;

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

    public Island(int count = MAX_COMPONENT_COUNT): base(count != MAX_COMPONENT_COUNT ? count + 3 : MAX_COMPONENT_COUNT) {
        m_renderSet = new List<Entity>(capacity: 32);
        m_entities = new List<Entity>(capacity: 32);

        this.Attach<Hierarchy>(component: ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static (x) => x.Direction = ConnectionDirection.UP));
        this.Attach<Hierarchy>(component: ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static (x) => x.Direction = ConnectionDirection.DOWN));

        this.Attach<ContentComponent>(component: ComponentPool<ContentComponent>.Instance.Rent<ContentComponent>(), isUnique: true);
        this.Attach<DrawCalls>(component: ComponentPool<DrawCalls>.Instance.Rent<DrawCalls>(), isUnique: true);
    }

    /// <summary>
    /// Rebuilds the current <see cref="Island"/>.
    /// </summary>
    /// <remarks>
    /// This method is destructive and marks an intent-driven layout shift. 
    /// It immediately invalidates and structural-deconstructs the current UI sub-tree.
    /// <para>
    /// <c>WARNING:</c> All entity and component references originating from the old 
    /// tree-segment before this call are considered <b>stale</b> and are returned to their respective pools. 
    /// Do not cache, query, or mutate any objects from the destroyed hierarchy after invoking this method.
    /// </para>
    /// </remarks>
    public void Rebuild() {
        if (!Get<ContentComponent>()!.HasChanged) return;

        bool isTop = m_root == null;

        DrawCalls draws = (isTop ? Get<DrawCalls>()! : m_root!.Get<DrawCalls>()!);
        IReadOnlyList<Island> chunks = draws.ChunkHolders;

        for(int i = chunks.Count - 1; i >= m_chunkId; --i) {
            bool currentChunk = i == m_chunkId;

            // The first entity each chunk the "chunk holder" itself
            for (int j = currentChunk ? 1 : 0; j < chunks[i].m_entities.Count; ++j)
                chunks[i].m_entities[j].Dispose();

            if (!currentChunk) draws.Remove(i);
        }

        m_entities.Clear();
        m_renderSet.Clear();

        if (isTop) {
            Get<DrawCalls>()!.Reset();
            Remove<DrawCalls>();

            _ = Attach<DrawCalls>(ComponentPool<DrawCalls>.Instance.Rent<DrawCalls>(), isUnique: true);
        }

        BuildTree(context: new BuildContext(current: this) { Root = isTop ? this : m_root!, IsTop = isTop });
    }

    /// <summary>
    /// Build the island as <see cref="Entity"/>.
    /// </summary>
    /// <returns>Return <see cref="Entity"/> instance from the current <see cref="Island"/>.</returns>
    protected abstract Entity? Build(ref readonly BuildContext context);

    /// <summary>
    /// Move through the tree and create list from it.
    /// </summary>
    /// <param name="context">Context of the current build period.</param>
    internal void BuildTree(BuildContext context) {
        if (context.Current is Island island) {
            Entity? created = island.Build(in context);

            if (created == null) return;
            context.CurrentIsland = island;

            if (!context.IsTop) {
                context.CurrentIsland.Remove<DrawCalls>();
                context.CurrentIsland.m_root = context.Root;
            }

            if (context.CurrentIsland.m_chunkId == -1) {
                context.CurrentIsland.m_chunkId = context.Root.Get<DrawCalls>()!.ChunkHolders.Count;
                context.Root.Get<DrawCalls>()!.Add(island);
            }

            context.Current.Get<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = created;
            created.Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = context.Current;
        }

        if (context.Current == null) return;
        int childrenCount = context.Current.CountComponent<Hierarchy>();

        context.CurrentIsland.m_entities.Add(context.Current!);

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