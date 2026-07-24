using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.Native;

using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Kinesis.UI;

/// <summary>
/// Represent an textual input on the screen.
/// </summary>
public sealed class InputField: Island, ICopyable<BuildContext> {
    private readonly string m_textIdentifier = null!;
    private readonly string m_cursorIdentifier = null!;

    private readonly string m_placeholder = string.Empty;
    private readonly uint m_maxLen = 64;

    private int m_headPosition = 0;

    /// <summary>
    /// Maximum length of the text of the <see cref="InputField"/> instance.
    /// </summary>
    public uint MaximumLength { init => m_maxLen = uint.Max(x: value, y: 1); }

    /// <summary>
    /// Placeholder text of the <see cref="InputField"/> instance.
    /// </summary>
    public string Placeholder { init => m_placeholder = value ?? string.Empty; }

    /// <summary>
    /// Create a new <see cref="InputField"/> instance.
    /// </summary>
    public InputField() {
        _ = Attach<Position>(component: ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = Attach<Scale>(component: ComponentPool<Scale>.Instance.Rent<Scale>(static(x) => x.Value = Vec2.Auto), isUnique: true);

        Guid guid = Guid.CreateVersion7();
        m_textIdentifier = $"__input_box_{guid}__";

        m_cursorIdentifier = $"__input_bix_cursor_{guid}__";
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Scale>(this);
        context.SetPivot<Position>(this);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        return new OnUpdate<InputMessage>(context) {
            On = (message, ref readonly tree) => {
                if (!message.IsPressed || message.Key == '\0') return;

                UIText text = tree.Visit<UIText>(name: m_textIdentifier)!;
                int currentLen = (int)text.Get<Scale>()!.Value.X;

                if (message.ToArrowKey() != ArrowKey.INVALID_NONE) {
                    int moveAmount = message.ToArrowKey() switch {
                        ArrowKey.RIGHT => 1,
                        ArrowKey.LEFT  => -1,
                        _ => 0
                    };

                    m_headPosition = int.Clamp(value: m_headPosition + moveAmount, min: 0, max: currentLen);
                    return;
                }

                switch (message.Key) {
                    case '\b': {
                        RemoveChar(text);
                        break;
                    }
                    default: {
                        AddChar(text, message.Key);
                        break;
                    }
                }
            },
            Content = new OnUpdate<RenderMessage>(context) {
                On = (message, ref readonly tree) => {

                    int width = (int)Get<Scale>()!.Value.X; 
                    UIBox cursor = tree.Visit<UIBox>(name: m_cursorIdentifier)!;

                    if(m_headPosition < width)
                        cursor.Move(x: m_headPosition, y: 0);
                },
                Content = new UIStack {
                    Content = [
                        new UIText {
                            Foreground = RGB.White,
                            Text       = m_placeholder,

                            Background = RGB.Transparent,
                            Name       = m_textIdentifier,
                        },
                        new AnimatedArea<RGB, UIBox> {
                            Selector   = static(box)        => box.Background,
                            Applier    = static(box, color) => box.Background = color,

                            To         = RGB.White,
                            Duration   = TimeSpan.FromSeconds(value: .5f),

                            IsPeriodic = true,
                            Content = new UIBox {
                                Name       = m_cursorIdentifier,
                                Scale      = Vec2.One,

                                Background = RGB.Transparent,
                                Filler     = new Filler(color: RGB.Black, character: ' ')
                            }
                        }
                    ]
                }
            }
        };
    }

    private void AddChar(UIText text, char letter) {
        if (m_headPosition >= m_maxLen) return;

        int len = text.Text.Length;

        Span<char> left = stackalloc char[m_headPosition + 1];
        Span<char> rigth = stackalloc char[len - m_headPosition];

        _ = text.Read(destination: left[..m_headPosition]);
        _ = text.Read(destination: rigth, from: m_headPosition);

        left[^1] = letter;

        _ = text.Write(left);
        _ = text.Write(rigth, from: m_headPosition + 1);

        ++m_headPosition;
    }

    private void RemoveChar(UIText text) {
        if (m_headPosition <= 0) return;
        int len = text.Text.Length;

        Span<char> left  = stackalloc char[m_headPosition - 1];
        Span<char> rigth = stackalloc char[len - m_headPosition];

        text.Read(destination: left[..(m_headPosition - 1)]);
        text.Read(destination: rigth, from: m_headPosition);

        text.Write(text: left);
        text.Write(text: rigth, from: m_headPosition - 1);

        --m_headPosition;
    }
}
