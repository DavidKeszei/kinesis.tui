using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

public sealed class Row: Island, ICopyable<BuildContext> {
    private static readonly string s_list = "__row__";

    private string[] m_childIds = null!;
    private int m_childCount = 0;

    private float m_maxHeight = .0f;
    private float m_ratio = .0f;

    public List<Entity> Content {
        set {
            if (value == null || value.Count == 0)
                return;

            List<Entity> boxes = new List<Entity>(capacity: value.Count);
            m_childIds = new string[value.Count];

            m_childCount = value.Count;

            for (int i = 0; i < value.Count; ++i) {
                boxes.Add(item: new UIBox {
                    Name = (m_childIds[i] = $"__rowItem_{i}__"),
                    Child = value[i],
                });

                float max = value[i].GetComponent<Scale>()?.Value.Y ?? float.MinValue;

                if (max > m_maxHeight)
                    m_maxHeight = max;
            }

            GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = new UIList {
                Name = s_list,
                Children = boxes
            };
        }
    }

    public Row() {
        _ = AttachComponent<Position>(component: new Position() { Origin = null!, Relative = Vec2.One * float.MinValue }, isUnique: true);
        _ = AttachComponent<Scale>(component: new Scale(scale: Vec2.One * float.MinValue), isUnique: true);
    }

    public void Copy(ref BuildContext context) {
        context.Set<Position>(this, @default: new Position());
        context.Set<Scale>(this, @default: new Scale(scale: Vec2.One * float.MinValue));

        GetComponent<Scale>()!.Value = GetComponent<Scale>()!.Value with { X = float.MinValue };
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                Scale scale = GetComponent<Scale>()!;

                Vec2 currentScale = Vec2.Zero with {
                    X = (scale.Value.X == float.MinValue ? message.Scale.X : scale.Value.X),
                    Y = (scale.Value.Y < m_maxHeight ? m_maxHeight : scale.Value.Y)
                };

                UIBox box = null!;
                m_ratio = scale.Value.X / m_childCount;

                for (int i = 0; i < m_childCount; ++i) {
                    box = tree.Visit<UIBox>(name: m_childIds[i])!;

                    Vec2 relative = box.GetComponent<Position>()!.Relative;
                    box.Scale = new Vec2(x: m_ratio, y: scale.Value.X);

                    relative.X = MathF.Round(m_ratio * i);
                    box.GetComponent<Position>()!.Relative = relative;
                    box.GetComponent<Scale>()!.Value = new Vec2(x: MathF.Round(m_ratio), y: m_maxHeight);
                }
            },
            Child = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached ?? null!
        };
    }
}
