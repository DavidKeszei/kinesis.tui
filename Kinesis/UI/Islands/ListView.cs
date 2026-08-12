using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.Native;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents a virtualized list of elements on the screen.
/// </summary>
/// <typeparam name="T">Template object for each row.</typeparam>
/// <typeparam name="U">Encapsulated data target for each row.</typeparam>
public sealed class ListView<T, U>: Island, ICopyable<BuildContext>, IContentable<List<U>> where T: Entity, new() {
    private const byte KEY_ENTER = 13;
    private readonly string m_stackContainerName = null!;

    private readonly Action<T, U> m_onBind = null!;
    private readonly Action<T> m_onFocus   = null!;

    private readonly Func<T, U> m_onSelect = null!;
    private readonly Func<T> m_prototypeBuild = null!;

    private readonly List<U> m_contentSource = null!;
    private readonly int m_contentHeight = 1;

    private float m_maxYScale = .0f;
    private float m_currentYScale = .0f;

    private int m_scrollOffset = 0;
    private int m_listRowHead = 0;

    private int m_visibleRowCount = 0;
    private int m_maxRowCount = 0;

    /// <summary>
    /// Collection of the underlying data instances as <typeparamref name="U"/>.
    /// </summary>
    public List<U> Content {
        get => m_contentSource;
        set {
            if (value == null) return;

            for (int i = 0; i < value.Count; ++i)
                m_contentSource.Add(value[i]);
        }
    }

    public Func<T> Prototype { init => m_prototypeBuild = value; }

    /// <summary>
    /// Current position of the cursor in the <see cref="ListView{TTemplate,TData}"/>.
    /// </summary>
    public int CursorPosition { get => m_listRowHead + m_scrollOffset; }

    /// <summary>
    /// Custom binding logic for attach data to a <typeparamref name="T"/> instance.
    /// </summary>
    public Action<T, U> OnBind { init => m_onBind = value; }

    /// <summary>
    /// Simple callback for react to selecting on a row.
    /// </summary>
    public Func<T, U> OnSelect { init => m_onSelect = value; }

    /// <summary>
    /// Simple callback for react to highlighting on a row.
    /// </summary>
    public Action<T> OnFocus { init => m_onFocus = value; }

    /// <summary>
    /// Used height value of each row.
    /// </summary>
    public uint RowHeight { init => m_contentHeight = (int)(value == 0 ? 1 : value); }

    /// <summary>
    /// Create a new <see cref="ListView{TTemplate,TData}"/> instance.
    /// </summary>
    public ListView(): base(count: 8) {
        _ = Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static(x) => x.Value = Vec2.Auto), isUnique: true);

        m_stackContainerName = $"__list_{Guid.CreateVersion7()}__";
        m_contentSource = new List<U>();
    }

    public void Copy(ref BuildContext from) {
        from.SetPivot<Scale>(this);
        from.SetPivot<Position>(this);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        if (m_prototypeBuild == null) return null;

        return new OnUpdate<InputMessage>(context) {
            On = HandleArrowKeys,
            Content = new OnUpdate<RenderMessage>(context) {
                On = Update,
                Content = Get<RebuildContent>()?.Content ?? null!
            }
        };
    }

    private void HandleArrowKeys(InputMessage message, ref readonly IslandEntityVisitor tree) {
        if (m_contentSource.Count < 1) return;

        if (message.Key == KEY_ENTER) {
            T template = (T)tree.Visit<Stack>(name: m_stackContainerName)!
                                                .Get<Hierarchy>(Hierarchy.ChildrenStart + m_listRowHead)!
                                                .Attached!
                                                .Get<Hierarchy>(Hierarchy.ChildrenStart)!
                                                .Attached!;

            if(m_onSelect != null) m_contentSource[m_listRowHead + m_scrollOffset] = m_onSelect(template);
            return;
        }

        ArrowKey key = message.ToArrowKey();
        if (!message.IsPressed || key == ArrowKey.INVALID_NONE) return;

        int changeValue = key switch {
            ArrowKey.UP => -1,
            ArrowKey.DOWN => 1,
            _ => 0
        };

        int offset = changeValue - m_scrollOffset;

        // If we are at the top ot bottom, then we de- or increase the `m_scrollOffset` value.
        if (m_listRowHead + offset < 0 || m_listRowHead + offset > m_visibleRowCount - 1)
            m_scrollOffset += changeValue;

        m_listRowHead = int.Clamp(m_listRowHead + changeValue, 0, m_contentSource.Count - 1);
    }
    
    private void Update(RenderMessage message, ref readonly IslandEntityVisitor tree) {
        Vec2 parentScale = Get<Scale>()?.Maximum.Value ?? message.Scale;

        // We clamp it, if the content count is smaller than the max. row count
        m_visibleRowCount = int.Clamp(value: (int)MathF.Ceiling(parentScale.Y / m_contentHeight), min: 0, max: m_contentSource.Count);

        if (m_maxYScale < parentScale.Y) {
            CreateRowViewports(y: parentScale.Y);
            Rebuild();

            m_maxYScale = parentScale.Y;
            return;
        }

        if (m_currentYScale != message.Scale.Y) {
            m_scrollOffset  = m_listRowHead;
            m_currentYScale = message.Scale.Y;

            ClipChildren(in tree);
        }

        // Clamp the offset to the diff of content count & visible row count
        m_scrollOffset = int.Clamp(m_scrollOffset, min: 0, max: m_contentSource.Count - m_visibleRowCount);

        SyncData(stack: tree.Visit<Stack>(name: m_stackContainerName)!);
    }

    private void CreateRowViewports(float y) {
        m_maxRowCount = (int)MathF.Round(y / m_contentHeight);
        Stack stack = new Stack(capacity: 64) { Name = m_stackContainerName };

        for (int i = 0; i < m_maxRowCount; ++i) {
            T template = m_prototypeBuild();
            Viewport viewport = new Viewport() { Name = $"__list_item_{Guid.CreateVersion7()}__", Content = template, Scale = Vec2.Auto with { Y = 0 } };

            _ = stack.Attach<Hierarchy>(component: ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.DOWN));

            viewport.Move(x: Get<Position>()!.Relative.X, y: i * m_contentHeight);
            viewport.Get<Hierarchy>(Hierarchy.Parent)!.Attached = stack;

            stack.Get<Hierarchy>(index: Hierarchy.ChildrenStart + i)!.Attached = viewport;
        }

        Get<RebuildContent>()!.Content = new Viewport() { Content = stack };
        SetHeightOfTheChildren(stack, maxHeight: y);
    }

    private void SetHeightOfTheChildren(Stack stack, float maxHeight) {
        float remainedHeight = .0f;
        if (m_visibleRowCount == 0) return;

        for (int i = 0; i < m_visibleRowCount && i < m_maxRowCount; ++i) {
            float calculatedHeight = float.Clamp(maxHeight - remainedHeight, .0f, m_contentHeight);

            Scale childScale = stack.Get<Hierarchy>(index: i + Hierarchy.ChildrenStart)!.Attached.Get<Scale>()!;
            childScale.ChangeAxisValue(value: calculatedHeight, axis: Axis.Y);

            remainedHeight += calculatedHeight;
        }
    }

    private void SyncData(Stack stack) {
        for (int i = 0; i < m_maxRowCount; ++i) {
            Viewport viewPort = (Viewport)stack.Get<Hierarchy>(i + Hierarchy.ChildrenStart)!.Attached;

            T child = (T)stack.Get<Hierarchy>(index: i + Hierarchy.ChildrenStart)!.Attached
                              .Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached;

            int contentIndex = i + m_scrollOffset;

            if (i < m_visibleRowCount) {
                m_onBind?.Invoke(child, m_contentSource[contentIndex]);

                if (m_listRowHead == i + m_scrollOffset)
                    m_onFocus?.Invoke(child);

                viewPort.Get<Scale>()!.ChangeAxisValue(m_contentHeight, Axis.Y);
                continue;
            }

            viewPort.Get<Scale>()!.ChangeAxisValue(.0f, Axis.Y);
        }
    }

    private void ClipChildren(ref readonly IslandEntityVisitor tree) {
        Stack stack = tree.Visit<Stack>(name: m_stackContainerName)!;
        int len = stack.CountComponent<Hierarchy>(static(x) => x.Direction == ConnectionDirection.DOWN);

        for (int i = 0; i < len; ++i)
            stack.Get<Hierarchy>(Hierarchy.ChildrenStart + i)!.Attached!
                 .Get<Hierarchy>()!.Attached!
                 .ClipRenderScale();

    }
}