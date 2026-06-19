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

    private readonly Action<TData, TTemplate> m_bind = null!;
    private readonly List<TData> m_contentSource = null!;

    private Vec2 m_preScale = Vec2.Zero;
    private readonly int m_height = 1;

    private int m_yOffset = 0;
    private int m_visibleRowCount = 0;

    public List<TData> Source { init => m_contentSource = value; }

    public Action<TData, TTemplate> Bind { init => m_bind = value; }

    public uint RowHeight { init => m_height = (int)(value == 0 ? 1 : value); }

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
            On = (message, ref readonly tree) => {

                ArrowKey key = ArrowKey.INVALID_NONE;
                if (message.IsPressed && (key = message.ToArrowKey()) == ArrowKey.INVALID_NONE) return;

                m_yOffset += key switch {
                    ArrowKey.UP => -1,
                    ArrowKey.DOWN => 1,
                    _ => 0
                };

            },
            Content = new OnUpdate<RenderMessage>(context) {
                On = (message, ref readonly tree) => {
                    Vec2 parentScale = Get<Scale>()?.Maximum.Value ?? message.Scale;
                    m_visibleRowCount = int.Clamp(value: (int)MathF.Ceiling(parentScale.Y / m_height), min: 0, max: m_contentSource.Count);

                    if (m_preScale.Y < parentScale.Y) {
                        CreateRowViewports(y: parentScale.Y);
                        Rebuild();

                        m_preScale = parentScale;
                        return;
                    }

                    m_yOffset = int.Clamp(m_yOffset, min: 0, max: m_contentSource.Count - m_visibleRowCount);
                    SyncData(stack: tree.Visit<UIStack>(name: m_list)!);
                },
                Content = Get<ContentComponent>()?.Content ?? null!
            }
        };
    }

    private void CreateRowViewports(float y) {
        int count = (int)MathF.Round(y / m_height);

        UIStack stack = new UIStack(capacity: 64) {
            Name = (m_list ??= $"__list_{Guid.CreateVersion7()}__")
        };

        for (int i = 0; i < count; ++i) {
            TTemplate template = new TTemplate();

            _ = stack.Attach<Hierarchy>(component: ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.DOWN));
            stack.Get<Hierarchy>(index: Hierarchy.ChildrenStart + i)!.Attached = new Viewport() {
                Name = $"__list_item_{Guid.CreateVersion7()}__",
                Content = template
            };

            template.Move(x: 0, y: i * m_height);
            template.Get<Hierarchy>(Hierarchy.Parent)!.Attached = stack;
        }

        Get<ContentComponent>()!.Content = new Viewport() { Content = stack };
        SetHeigthOfTheChildren(stack, maxHeigth: y);
    }

    private void SetHeigthOfTheChildren(UIStack stack, float maxHeigth) {
        float remainedHeigth = .0f;
        if (m_visibleRowCount == 0) return;

        for (int i = 0; i < m_visibleRowCount && remainedHeigth < maxHeigth; ++i) {
            float calculatedHeigth = float.Clamp(maxHeigth - remainedHeigth, .0f, m_height);

            Scale childScale = stack.Get<Hierarchy>(index: i + Hierarchy.ChildrenStart)!.Attached.Get<Scale>()!;
            childScale.ChangeAxisValue(value: calculatedHeigth, axis: Axis.Y);

            remainedHeigth += m_height;
        }
    }

    private void SyncData(UIStack stack) {
        if (m_visibleRowCount == 0) return;

        for (int i = 0; i < m_contentSource.Count && i < m_visibleRowCount; ++i) {
            TTemplate child = (TTemplate)stack.Get<Hierarchy>(index: i + Hierarchy.ChildrenStart)!.Attached
                                              .Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached;

            int contentIndex = i + m_yOffset;
            m_bind(m_contentSource[contentIndex], child);
        }
    }
}