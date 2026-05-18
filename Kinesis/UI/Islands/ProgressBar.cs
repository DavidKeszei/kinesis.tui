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
public sealed class ProgressBar: Island, ICopyable<BuildContext> {
    private readonly string m_format = "f0";
    private string m_progressEmpty = string.Empty;

    private readonly Filler m_filled = default;
    private readonly Filler m_empty = default;

    private string m_progressFilled = string.Empty;
    private string m_progressText = string.Empty;

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
    /// Format of the number status of the <see cref="ProgressBar"/>.
    /// </summary>
    public string Format { init => m_format = value; }

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
        context.Set<Position>(this, @default: new Position());
        context.Set<Scale>(this, @default: new Scale(scale: Vec2.One * Scale.Auto));

        GetComponent<Scale>()!.ChangeAxisValue(value: 1, axis: Axis.Y);
    }

    /// <summary>
    /// Update the underlying <paramref name="percent"/>.
    /// </summary>
    /// <param name="percent">New value of the percent.</param>
    public void Update(float percent) 
        => m_percent = float.Clamp(percent, min: .0f, max: 100f);

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                if (m_percent == m_prePercent) return;

                tree.Visit<UIText>(name: m_progressText)!.Text = m_percent.ToString(format: m_format);
                int len = tree.Visit<UIText>(name: m_progressText)!.Text.Length;

                UIBox filled = tree.Visit<UIBox>(name: m_progressFilled)!;
                UIBox empty = tree.Visit<UIBox>(name: m_progressEmpty)!;

                if (len > 0) {

                    empty.GetComponent<Scale>()!.Inset = new Vec2(x: len + 1, y: 0);
                    empty.GetComponent<Position>()!.Relative = new Vec2(x: len + 1, y: 0);

                    filled.GetComponent<Scale>()!.Inset = new Vec2(x: len + 1, y: 0);
                    filled.GetComponent<Position>()!.Relative = new Vec2(x: len + 1, y: 0);
                }

                float x = empty.GetComponent<Scale>()!.Value.X;

                /*
                 * Percent + Text length + Empty Space -> This makes the render flexible & correct
                 */
                filled.GetComponent<Scale>()!.ChangeAxisValue(value: (len + 1) + (x / 100f) * m_percent, axis: Axis.X);

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
                        new UIText {
                            Name = (m_progressText = $"__progress_text_{Guid.CreateVersion7()}__"),
                            Foreground = RGB.White,
                            Text = string.Empty
                        }
                    ]
            }
        };

        box.RemoveComponent<RenderComponent>();
        return box;
    }
}