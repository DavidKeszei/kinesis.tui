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
    private string m_progressEmpty = null!;
    private string m_progressFilled = null!;

    private readonly Filler m_filled = default;
    private readonly Filler m_empty = default;

    private readonly Action<float, Entity> m_onUpdate = null!;
    private string m_progressIndicator = null!;

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
        set {
            if (value == null || value.Get<Scale>() == null) {
                Get<ContentComponent>()!.Content = CreateContainer(null!);
                return;
            }

            Viewport container = new Viewport() {
                Name = (m_progressIndicator ??= $"__progress_indicator_{Guid.CreateVersion7()}__"),
                Content = value
            };

            container.Get<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = container;

            Get<ContentComponent>()!.Content = CreateContainer(container);
            Rebuild();
        }
    }

    /// <summary>
    /// Create a new <see cref="ProgressBar"/> instance.
    /// </summary>
    public ProgressBar(): base(count: 3) {
        _ = Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static(x) => x.Value = Vec2.Auto), isUnique: true);

        m_filled = new Filler(color: RGB.White, character: '━');
        m_empty = new Filler(color: RGB.White with { A = 25 }, character: '━');

        Content = null!;
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);

        Get<Scale>()!.ChangeAxisValue(value: 1, axis: Axis.Y);
    }

    /// <summary>
    /// Update the underlying <paramref name="percent"/>.
    /// </summary>
    /// <param name="percent">New value of the percent.</param>
    public void Update(float percent) 
        => m_percent = float.Clamp(percent, min: .0f, max: 100f);

    protected override Entity? Build(ref readonly BuildContext context) {
        /* TODO(2026-05-21T19:00:32): Add chance to change the loading text to any loading animation. (State: Done✅)*/ 	
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref readonly tree) => {
                Entity entity = tree.Visit<Viewport>(name: m_progressIndicator)?
                                    .Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached ?? null!;

                int len = 0;
                if (entity != null) {

                    m_onUpdate?.Invoke(m_percent, entity);
                    len = (int)entity.Get<Scale>()!.Value.X + 1;
                }

                UIBox filled = tree.Visit<UIBox>(name: m_progressFilled)!;
                UIBox empty = tree.Visit<UIBox>(name: m_progressEmpty)!;

                empty.Filler  = m_empty;
                filled.Filler = m_filled;

                if (len > 0) {

                    empty.Get<Scale>()!.Inset = new Vec2(x: len, y: 0);
                    empty.Get<Position>()!.Relative = new Vec2(x: len, y: 0);

                    filled.Get<Scale>()!.Inset = new Vec2(x: len, y: 0);
                    filled.Get<Position>()!.Relative = new Vec2(x: len, y: 0);
                }

                float x = empty.Get<Scale>()!.Value.X;

                /* Text length + (Maximum width * Percent) -> This makes the render flexible & correct */
                filled.Get<Scale>()!.ChangeAxisValue(value: len + (x / 100f) * m_percent, axis: Axis.X);
            },
            Content = Get<ContentComponent>()?.Content ?? null!
        };
    }

    private Viewport CreateContainer(Entity content) {
        Viewport box = new Viewport() {
            Content = new UIStack() {
                Content = [
                        new UIBox() {
                            Name = (m_progressEmpty ??= $"__progress_empty_{Guid.CreateVersion7()}__"),
                            Background = RGB.Transparent,
                            Filler = m_empty
                        },
                        new UIBox() {
                            Name = (m_progressFilled ??= $"__progress_filled_{Guid.CreateVersion7()}__"),
                            Background = RGB.Transparent,
                            Filler = m_filled
                        },
                        content
                    ]
            }
        };

        return box;
    }
}