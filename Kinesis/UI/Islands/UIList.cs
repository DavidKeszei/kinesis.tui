using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.Native;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.UI;

public sealed class UIList<TTemplate, TData>: Island, ICopyable<BuildContext> where TTemplate: Entity, new() {
    private string m_list = null!;

    private readonly Action<TData, TTemplate, bool> m_bind = null!;
    private readonly List<TData> m_contentSource = null!;

    private readonly int m_rowHeight = 1;
    private float m_maxYScale = .0f;

    private float m_currentYScale = .0f;
    private int m_scrollOffset = 0;

    private int m_listRowHead = 0;
    private int m_visibleRowCount = 0;

    private int m_maxRowCount = 0;

    public List<TData> Source { init => m_contentSource = value; }

    public Action<TData, TTemplate, bool> Bind { init => m_bind = value; }

    public uint RowHeight { init => m_rowHeight = (int)(value == 0 ? 1 : value); }

    public UIList(): base(count: 8) {
        _ = Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static(x) => x.Value = Vec2.Auto), isUnique: true);
    }

    public void Copy(ref BuildContext from) {
        from.SetPivot<Scale>(this);
        from.SetPivot<Position>(this);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        if (m_bind == null || m_contentSource == null) return null!;

        return new OnUpdate<InputMessage>(context) {
            On = HandleArrowKeys,
            Content = new OnUpdate<RenderMessage>(context) {
                On = Update,
                Content = Get<ContentComponent>()?.Content ?? null!
            }
        };
    }

    private void HandleArrowKeys(InputMessage message, ref readonly IslandEntityVisitor tree) {
        if (m_contentSource.Count < 1) return;

        ArrowKey key = message.ToArrowKey();
        if (!message.IsPressed || key == ArrowKey.INVALID_NONE) return;

        int changeValue = key switch {
            ArrowKey.UP => -1,
            ArrowKey.DOWN => 1,
            _ => 0
        };

        int offset = changeValue - m_scrollOffset;

        if (m_listRowHead + offset < 0 || m_listRowHead + offset > m_visibleRowCount - 1)
            m_scrollOffset += changeValue;

        m_listRowHead = int.Clamp(m_listRowHead + changeValue, 0, m_contentSource.Count - 1);
    }

    private void Update(RenderMessage message, ref readonly IslandEntityVisitor tree) {
        Vec2 parentScale = Get<Scale>()?.Maximum.Value ?? message.Scale;

        // We clamp it, if the content count is smaller than the max. row count
        m_visibleRowCount = int.Clamp(value: (int)MathF.Ceiling(parentScale.Y / m_rowHeight), min: 0, max: m_contentSource.Count);

        if (m_maxYScale < parentScale.Y) {
            CreateRowViewports(y: parentScale.Y);
            Rebuild();

            m_maxYScale = parentScale.Y;
            return;
        }

        if (m_currentYScale != message.Scale.Y) {
            m_scrollOffset += (m_listRowHead - m_scrollOffset);
            m_currentYScale = message.Scale.Y;
        }

        // Clamp the offset to the diff of content count & visible row count
        m_scrollOffset = int.Clamp(m_scrollOffset, min: 0, max: m_contentSource.Count - m_visibleRowCount);
        SyncData(stack: tree.Visit<UIStack>(name: m_list)!);
    }

    private void CreateRowViewports(float y) {
        int count = (int)MathF.Round(y / m_rowHeight);

        UIStack stack = new UIStack(capacity: 64) {
            Name = (m_list ??= $"__list_{Guid.CreateVersion7()}__")
        };

        for (int i = 0; i < count; ++i) {
            TTemplate template = new TTemplate();

            _ = stack.Attach<Hierarchy>(component: ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.DOWN));
            stack.Get<Hierarchy>(index: Hierarchy.ChildrenStart + i)!.Attached = new Viewport() {
                Name = $"__list_item_{Guid.CreateVersion7()}__",
                Content = template,
            };

            template.Move(x: Get<Position>()!.Relative.X, y: i * m_rowHeight);
            template.Get<Hierarchy>(Hierarchy.Parent)!.Attached = stack;
        }

        m_maxRowCount = count;

        Get<ContentComponent>()!.Content = new Viewport() { Content = stack };
        SetHeigthOfTheChildren(stack, maxHeigth: y);
    }

    private void SetHeigthOfTheChildren(UIStack stack, float maxHeigth) {
        float remainedHeigth = .0f;
        if (m_visibleRowCount == 0) return;

        for (int i = 0; i < m_visibleRowCount && i < m_maxRowCount; ++i) {
            float calculatedHeigth = float.Clamp(maxHeigth - remainedHeigth, .0f, m_rowHeight);

            Scale childScale = stack.Get<Hierarchy>(index: i + Hierarchy.ChildrenStart)!.Attached.Get<Scale>()!;
            childScale.ChangeAxisValue(value: calculatedHeigth, axis: Axis.Y);

            remainedHeigth += calculatedHeigth;
        }
    }

    private void SyncData(UIStack stack) {
        for (int i = 0; i < m_maxRowCount; ++i) {
            Viewport viewPort = (Viewport)stack.Get<Hierarchy>(index: i + Hierarchy.ChildrenStart)!.Attached;
            TTemplate child = (TTemplate)stack.Get<Hierarchy>(index: i + Hierarchy.ChildrenStart)!.Attached
                                              .Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached;

            int contentIndex = i + m_scrollOffset;

            if (i < m_visibleRowCount) {
                m_bind(m_contentSource[contentIndex], child, i + m_scrollOffset == m_listRowHead);
                viewPort.Get<Scale>()!.ChangeAxisValue(value: m_rowHeight, axis: Axis.Y);
            }
            else {
                viewPort.Get<Scale>()!.ChangeAxisValue(value: .0f, axis: Axis.Y);
            }
        }
    }
}