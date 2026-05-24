using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents a status indicator element with numeric indicator.
/// </summary>
public sealed class ProgressBar: Island, ICopyable<BuildContext>, IContentable<Entity> {
    private string m_progressEmpty = string.Empty;
    private string m_progressFilled = string.Empty;

    private readonly Filler m_filled = default;
    private readonly Filler m_empty = default;

    private readonly Action<float, Entity> m_onUpdate = null!;
    private string m_progressIndicator = string.Empty;

    private float m_prePercent = -1.0f;
    private float m_percent = .0f;

    /// <summary>
    /// Decoration of the filled bar of the <see cref="ProgressBar"/>.
    /// </summary>
    public Filler Filled { init => m_filled = value; }

    /// <summary>
    /// Decoration of the empty bar of the <see cref="ProgressBar"/>.
    /// </summary>
    public Filler Empty { init => m_empty = value; }

    /// <summary>
    /// Provides simple update logic to the indicator of the <see cref="ProgressBar"/>.
    /// </summary>
    /// <remarks><b>Remarks:</b> This property can be <see langword="null"/>, if progress indicator (<see cref="ProgressBar.Content"/>) is not requiring it.</remarks>
    public Action<float, Entity> On { init => m_onUpdate = value; }

    /// <summary>
    /// Setting up the loading indicator of the <see cref="ProgressBar"/>.
    /// </summary>
    public Entity Content {
        init {
            if (value == null || value.GetComponent<Scale>() == null) return;

            UIBox container = new UIBox() {
                Name = (m_progressIndicator = $"__progress_indicator_{Guid.CreateVersion7()}__"),
                Content = value
            };

            container.RemoveComponent<RenderComponent>();

            container.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = container;
        }
    }

    /// <summary>
    /// Create a new <see cref="ProgressBar"/> instance.
    /// </summary>
    public ProgressBar() {
        _ = AttachComponent<Position>(new Position(), true);
        _ = AttachComponent<Scale>(new Scale(scale: Vec2.One * Scale.Auto), true);

        m_filled = new Filler() { Character = '━', Color = RGB.White };
        m_empty = new Filler() { Character = '━', Color = RGB.White with { A = 25 } };
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);

        GetComponent<Scale>()!.ChangeAxisValue(value: 1, axis: Axis.Y);
    }

    /// <summary>
    /// Update the underlying <paramref name="percent"/>.
    /// </summary>
    /// <param name="percent">New value of the percent.</param>
    public void Update(float percent) 
        => m_percent = float.Clamp(percent, min: .0f, max: 100f);

    protected override Entity? Build(BuildContext context) {
        /* TODO(2026-05-21T19:00:32): Add chance to change the loading text to any loading animation.
         * 
         * State: Done✅
         */ 	
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                if (m_percent == m_prePercent) return;
                Entity entity = tree.Visit<Entity>(name: m_progressIndicator)?
                                    .GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached ?? null!;

                int len = 0;
                if (entity != null) {
                    m_onUpdate?.Invoke(m_percent, entity);
                    len = (int)entity.GetComponent<Scale>()!.Value.X + 1;
                }

                UIBox filled = tree.Visit<UIBox>(name: m_progressFilled)!;
                UIBox empty = tree.Visit<UIBox>(name: m_progressEmpty)!;

                if (len > 0) {

                    empty.GetComponent<Scale>()!.Inset = new Vec2(x: len, y: 0);
                    empty.GetComponent<Position>()!.Relative = new Vec2(x: len, y: 0);

                    filled.GetComponent<Scale>()!.Inset = new Vec2(x: len, y: 0);
                    filled.GetComponent<Position>()!.Relative = new Vec2(x: len, y: 0);
                }

                float x = empty.GetComponent<Scale>()!.Value.X;

                /* Percent + Text length + Empty Space -> This makes the render flexible & correct */
                filled.GetComponent<Scale>()!.ChangeAxisValue(value: len + (x / 100f) * m_percent, axis: Axis.X);
                m_prePercent = m_percent;
            },
            Content = CreateContainer()
        };
    }

    private UIBox CreateContainer() {
        UIBox box = new UIBox {
            Content = new UIStack {
                Content = [
                        new UIBox {
                            Name = (m_progressEmpty = $"__progress_empty_{Guid.CreateVersion7()}__"),
                            Filler = m_empty
                        },
                        new UIBox {
                            Name = (m_progressFilled = $"__progress_filled_{Guid.CreateVersion7()}__"),
                            Filler = m_filled
                        },
                        GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached ?? null!
                    ]
            }
        };

        box.RemoveComponent<RenderComponent>();
        return box;
    }
}