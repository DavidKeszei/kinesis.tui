using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.Native;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents a virtualized list of elements on the screen.
/// </summary>
/// <typeparam name="TEntity">Template object for each row.</typeparam>
/// <typeparam name="TData">Encapsulated data target for each row.</typeparam>
public sealed class ListView<TEntity, TData>: Island, ICopyable<BuildContext>, IContentable<IEnumerable<TData>> where TEntity: Entity {
    private const byte KEY_ENTER = 13;
    
    private readonly string m_stackContainerName = null!;
    private readonly Action<TEntity, TData, int> m_onBind = null!;

    private readonly Action<TEntity> m_onFocus   = null!;
    private readonly Func<TEntity, TData, TData> m_onSelect = null!;
    
    private readonly Func<TEntity> m_prototypeBuild = null!;
    private readonly Action<TEntity> m_onUnfocus = null!;
    
    private List<TData> m_contentSource = null!;
    private readonly int m_contentHeight = 1;
    
    private float m_maxYScale = .0f;
    private float m_currentYScale = .0f;

    private int m_scrollOffset = 1;
    private int m_relativeCursorPosition = -1;
    
    private int m_visibleRowCount = 0;
    private int m_maxRowCount = 0;

    /// <summary>
    /// Collection of the row data, which assigned to the <b>visible</b> rows.
    /// </summary>
    public IEnumerable<TData> Content { get => m_contentSource; set => m_contentSource = new List<TData>(value ?? []); }

    /// <summary>
    /// Prototype function for creating <typeparamref name="TEntity"/> instances for the <see cref="ListView{T, U}"/>.
    /// </summary>
    public Func<TEntity> Prototype { init => m_prototypeBuild = value; }

    /// <summary>
    /// Represents a callback function, which controls the data binding at each row.
    /// </summary>
    public Action<TEntity, TData, int> OnBind { init => m_onBind = value; }

    /// <summary>
    /// Represents a callback function, which run on the <b>focused</b> row, when the user press the ENTER key.
    /// </summary>
    public Func<TEntity, TData, TData> OnSelect { init => m_onSelect = value; }

    /// <summary>
    /// Represents a callback, which run on the <b>currently</b> focused row instance.
    /// </summary>
    public Action<TEntity> OnFocus { init => m_onFocus = value; }
    
    /// <summary>
    /// Represents a callback function, which run on the previous, <b>focused</b> row.
    /// </summary>
    public Action<TEntity> OnUnFocus { init => m_onUnfocus = value; }

    /// <summary>
    /// Heigth of a row in the <see cref="ListView{TEntity,TData}"/> instance.
    /// </summary>
    public uint RowHeight { init => m_contentHeight = (int)(value == 0 ? 1 : value); }

    /// <summary>
    /// Create a new <see cref="ListView{TTemplate,TData}"/> instance.
    /// </summary>
    public ListView(): base(count: 8) {
        _ = Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static(x) => x.Value = Vec2.Auto), isUnique: true);

        m_stackContainerName = $"__list_{Guid.CreateVersion7()}__";
        m_contentSource = new List<TData>();
    }

    public void Copy(ref BuildContext from) {
        from.SetPivot<Scale>(this);
        from.SetPivot<Position>(this);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        if (m_prototypeBuild == null)
            throw new ArgumentNullException(paramName: nameof(Prototype), message: "The .Prototype property of the list can't NULL.");

        return new OnUpdate<InputMessage>(context) {
            On = HandleArrowKeys,
            Content = new OnUpdate<RenderMessage>(context) {
                Pivot = this,
                On = Update,
                Content = Get<RebuildContent>()?.Content ?? null!
            }
        };
    }

    private void HandleArrowKeys(InputMessage message, ref readonly Visitor tree) {
        if (m_contentSource.Count < 1) return;
        if (HandleEnter(message.Key, tree)) return;

        ArrowKey key = message.ToArrowKey();
        if (!message.IsPressed || key == ArrowKey.INVALID_NONE) return;

        int direction = key switch {
            ArrowKey.UP => -1,
            ArrowKey.DOWN => 1,
            _ => 0
        };

        int cursorPosition = m_relativeCursorPosition + direction;
        
        // If we are at the top ot bottom, then we de- or increase the `m_scrollOffset` value.
        if (cursorPosition < 0 || cursorPosition > m_visibleRowCount - 1)
            m_scrollOffset += direction;

        m_relativeCursorPosition = cursorPosition;
    }
    
    private void Update(RenderMessage message, ref readonly Visitor tree) {
        Vec2 parentScale = Get<Scale>()?.Maximum.Value ?? message.Scale;

        // We clamp it, if the content count is smaller than the max. row count
        m_visibleRowCount = int.Clamp(value: (int)MathF.Ceiling(parentScale.Y / m_contentHeight), min: 0, max: m_contentSource.Count);

        if (m_maxYScale < parentScale.Y) {
            CreateRowViewports(y: parentScale.Y);
            Rebuild();

            m_maxYScale = parentScale.Y;
        }

        if (m_currentYScale != message.Scale.Y) {
            int diff = m_currentYScale < message.Scale.Y ? -1 : 1;
            
            if (m_relativeCursorPosition - diff >= 0 && m_visibleRowCount != m_contentSource.Count) {
                
                m_scrollOffset += diff;
                m_relativeCursorPosition -= diff;
            }

            m_currentYScale = message.Scale.Y;
            ClipChildren(in tree);
        }

        m_relativeCursorPosition = int.Clamp(m_relativeCursorPosition, 0, m_visibleRowCount - 1 < 0 ? 1 : m_visibleRowCount - 1);
        m_scrollOffset = int.Clamp(m_scrollOffset, 0, m_contentSource.Count - m_visibleRowCount);
        
        SyncData(stack: tree.Visit<Stack>(name: m_stackContainerName)!);
    }

    private void CreateRowViewports(float y) {
        m_maxRowCount = (int)MathF.Round(y / m_contentHeight);
        Stack stack = new Stack(capacity: 64) { Name = m_stackContainerName };

        for (int i = 0; i < m_maxRowCount; ++i) {
            TEntity template = m_prototypeBuild();
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

            TEntity child = (TEntity)stack.Get<Hierarchy>(index: i + Hierarchy.ChildrenStart)!.Attached
                              .Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached;

            int contentIndex = i + m_scrollOffset;

            if (i < m_visibleRowCount) {
                m_onBind?.Invoke(child, m_contentSource[contentIndex], contentIndex);

                if (m_relativeCursorPosition == i) m_onFocus?.Invoke(child);
                else m_onUnfocus?.Invoke(child);

                viewPort.Get<Scale>()!.ChangeAxisValue(m_contentHeight, Axis.Y);
                continue;
            }

            viewPort.Get<Scale>()!.ChangeAxisValue(.0f, Axis.Y);
        }
    }

    private void ClipChildren(ref readonly Visitor tree) {
        Stack stack = tree.Visit<Stack>(name: m_stackContainerName)!;
        int len = stack.CountComponent<Hierarchy>(static(x) => x.Direction == ConnectionDirection.DOWN);

        for (int i = 0; i < len; ++i)
            stack.Get<Hierarchy>(Hierarchy.ChildrenStart + i)!.Attached!
                 .Get<Hierarchy>()!.Attached!
                 .ClipRenderScale();

    }

    private bool HandleEnter(char key, Visitor tree) {
        if (key != KEY_ENTER) return false;
        
        TEntity template = (TEntity)tree.Visit<Stack>(name: m_stackContainerName)!
                            .Get<Hierarchy>(Hierarchy.ChildrenStart + m_relativeCursorPosition)!
                            .Attached!
                            .Get<Hierarchy>(Hierarchy.ChildrenStart)!
                            .Attached!;

        if (m_onSelect != null) {
            int clamp = int.Clamp(m_relativeCursorPosition + m_scrollOffset, 0, m_contentSource.Count - 1);
            m_contentSource[clamp] = m_onSelect(template, m_contentSource[clamp]);    
        }

        return true;
    }
}