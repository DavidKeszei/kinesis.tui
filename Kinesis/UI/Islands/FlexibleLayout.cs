using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents a flex-like space-managert on one direction.
/// </summary>
public sealed class FlexibleLayout: Island, ICopyable<BuildContext>, IAdaptiveLayout<List<uint>> {
    private static readonly string s_list = "__flexLayout__";

    private List<string> m_childIds = null!;
    private List<uint> m_ratios = null!;

    private Vec2 m_previousScale = Vec2.Zero;
    private float m_maxCrossAxisValue = float.MinValue;

    private int m_childCount = 0;
    private int m_sumOfRatios = 0;

    private Axis m_direction = Axis.X;

    public List<Entity> Content {
        init {
            if (value == null || value.Count == 0)
                return;

            List<Entity> boxes = new List<Entity>(capacity: value.Count);
            m_childIds ??= new List<string>(capacity: value.Count);
            m_childCount = value.Count;

            for (int i = 0; i < value.Count; ++i) {
                Scale? scale = value[i].GetComponent<Scale>();

                if (m_childIds.Count <= i) m_childIds.Add(item: $"__flexItem{i}__{Guid.CreateVersion7()}__");
                else m_childIds[i] = $"__flexItem{i}__{Guid.CreateVersion7()}__";

                boxes.Add(item: new UIBox {
                    Name = m_childIds[i],
                    Content = value[i],
                });

                boxes[^1].RemoveComponent<RenderComponent>();
                float max = m_direction == Axis.X ? 
                                value[i].GetComponent<Scale>()?.Value.Y ?? Scale.Auto :
                                value[i].GetComponent<Scale>()?.Value.X ?? Scale.Auto;

                if (max > m_maxCrossAxisValue) m_maxCrossAxisValue = max;
            }

            GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = new UIStack {
                Name = s_list,
                Content = boxes
            };
        }
    }

    /// <summary>
    /// Weigths/Ratios of the elements for the dividing.
    /// </summary>
    /// <remarks>
    /// Remark: If given list less than the content count, then remained content weigths is equals with 1.
    /// </remarks>
    public List<uint> Ratios {
        set {
            if (value == null) return;
            m_ratios ??= new List<uint>();

            for (int i = 0; i < m_childCount; ++i) {
                uint ratio = i >= value.Count ? 1 : uint.Max(1, value[i]);

                if (i >= m_ratios.Count) m_ratios.Add(ratio);
                else m_ratios[i] = ratio;

                m_sumOfRatios += (int)ratio;
            }
        }
    }

    /// <summary>
    /// Axis of the dividing.
    /// </summary>
    public Axis Direction { get => m_direction; init => m_direction = value; }

    public FlexibleLayout() {
        _ = AttachComponent<Position>(component: new Position(), isUnique: true);
        _ = AttachComponent<Scale>(component: new Scale(scale: Vec2.One * Scale.Auto), isUnique: true);
    }

    public void Copy(ref BuildContext context) {
        context.Set<Position>(this, @default: new Position());
        context.Set<Scale>(this, @default: new Scale(scale: Vec2.One * Scale.Auto));

        if(m_direction == Axis.X) GetComponent<Scale>()!.Value = GetComponent<Scale>()!.Value with { X = Scale.Auto };
        else if(m_direction == Axis.Y) GetComponent<Scale>()!.Value = GetComponent<Scale>()!.Value with { Y = Scale.Auto };
    }

    protected override Entity? Build(BuildContext context) {
        if (m_ratios == null || m_ratios.Count == 0) CreateDefaultRatios();

        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                if (m_childCount == 0 || (m_previousScale.X == message.Scale.X && m_previousScale.Y == message.Scale.Y)) return;

                Scale scale = GetComponent<Scale>()!;
                Vec2 currentScale = GetCurrentScale(scale, message.Scale);

                float ratio = CalculateRatio(currentScale);
                float error = .0f;

                float usedSpace = .0f;

                for (int i = 0; i < m_childCount; ++i) {
                    UIBox box = tree.Visit<UIBox>(name: m_childIds[i])!;

                    SetOffset(position: box.GetComponent<Position>()!, usedSpace);
                    error = SetRatioScale(box: box.GetComponent<Scale>()!, scale: (ratio * m_ratios![i]) + error, isLast: i == m_childCount - 1, ref usedSpace);
                }

                m_previousScale = message.Scale;
            },
            Content = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)?.Attached ?? null!
        };
    }

    private void SetOffset(Position position, float offset) {
        Vec2 offsetPosition = m_direction switch {
            Axis.Y => position.Relative with { Y = offset },
            Axis.X => position.Relative with { X = offset },
            _ => Vec2.Zero
        };

        position.Relative = offsetPosition;
    }

    private float SetRatioScale(Scale box, float scale, bool isLast, ref float used) {
        float original = scale;

        if (isLast) scale = MathF.Round(scale);
        else scale = MathF.Floor(scale);

        box.ChangeAxisValue(value: scale, axis: m_direction);
        used += scale;

        return original - scale;
    }

    private float CalculateRatio(Vec2 scale) {
        float divideAxisValue = m_direction switch {
            Axis.Y => scale!.Y,
            Axis.X => scale!.X,
            _ => .0f
        };

        return divideAxisValue / m_sumOfRatios;
    }

    private Vec2 GetCurrentScale(Scale scale, Vec2 max) {
        return m_direction switch {
            Axis.X => Vec2.Zero with {
                X = (scale.Value.X == Scale.Auto || scale.Value.X > max.X ? max.X : scale.Value.X),
                Y = (scale.Value.Y < m_maxCrossAxisValue ? m_maxCrossAxisValue : scale.Value.Y)
            },
            Axis.Y => Vec2.Zero with {
                X = (scale.Value.X < m_maxCrossAxisValue ? m_maxCrossAxisValue : scale.Value.X),
                Y = (scale.Value.Y == Scale.Auto || scale.Value.Y > max.Y ? max.Y : scale.Value.Y)
            },
            _ => Vec2.One
        };
    }

    private void CreateDefaultRatios() {
        if (m_childCount == 0) return;

        m_ratios = new List<uint>(capacity: m_childCount - 1);
        for (int i = 0; i < m_childCount; ++i)
            m_ratios.Add(1);

        m_sumOfRatios = m_childCount;
    }
}

public enum Axis: byte {
    Y,
    X,
}