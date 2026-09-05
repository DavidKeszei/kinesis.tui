using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a collection of "draw-calls" as separated <see cref="Island"/>s.
/// </summary>
internal class DrawCalls: Component, IStaticType, IPoolable {
    private const string TYPE = nameof(DrawCalls);
    private readonly List<Island> m_islands = null!;

    public static string TypeName { get => TYPE; }

    /// <summary>
    /// Internal chunks of the draw-calls.
    /// </summary>
    internal IReadOnlyList<Island> ChunkHolders { get => m_islands; } 

    public DrawCalls(): base(id: ComponentRegistry.QueryComponent(name: TYPE))
        => m_islands = new List<Island>(capacity: 16);

    /// <summary>
    /// Add a new <see cref="Island"/> to the draw-calls.
    /// </summary>
    /// <param name="chunk">A chunk of the draw-calls.</param>
    public void Add(Island chunk) => m_islands.Add(item: chunk);

    public void Remove(int chunk) => m_islands.RemoveAt(chunk);

    public void Reset() {
        m_islands.Clear();
        ComponentPool<DrawCalls>.Shared.Return(this);
    }

    /// <summary>
    /// Enumerate through the current drawable <see cref="Entity"/> instances.
    /// </summary>
    /// <returns></returns>
    public DrawCallIterator GetEnumerator() => new DrawCallIterator(this);

    /// <summary>
    /// Simple stack-allocated iterator for drawcalls.
    /// </summary>
    internal ref struct DrawCallIterator {
        private readonly List<Island> m_source = null!;
        private List<Entity> m_currentDrawCallChunk = null!;

        private int m_currentIsland = 0;
        private int m_currentEntity = 0;

        /// <summary>
        /// Current call target to the <see cref="Renderer"/>.
        /// </summary>
        public Entity Current { get => m_currentDrawCallChunk[m_currentEntity++]; }

        public DrawCallIterator(DrawCalls drawCalls) {
            m_source = drawCalls.m_islands;
            m_currentDrawCallChunk = drawCalls.m_islands[0].DrawCalls;
        }

        public bool MoveNext() {
            bool entityCountReached = m_currentEntity >= m_currentDrawCallChunk.Count;
            bool islandCountReached = m_currentIsland >= m_source.Count;

            /* If we reach all of it, then we dont do anything */
            if (entityCountReached && !islandCountReached) {
                do {
                    m_currentEntity = 0;
                    m_currentDrawCallChunk = m_source[m_currentIsland++].DrawCalls;

                    entityCountReached = m_currentEntity >= m_currentDrawCallChunk.Count;
                    islandCountReached = m_currentIsland >= m_source.Count;
                }
                while (entityCountReached && !islandCountReached);
            }

            return !entityCountReached || !islandCountReached;
        }
    }
}
