using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.Native;

using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent an textual input on the screen.
/// </summary>
public sealed class InputField: Island, ICopyable<BuildContext> {
    private readonly string m_textIdentifier = $"__input_box_{Guid.CreateVersion7()}__";
    private readonly string m_placeholder = string.Empty;

    private UIText m_text = null!;
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
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Scale>(this);
        context.SetPivot<Position>(this);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        return new OnUpdate<InputMessage>(context) {
            On = (message, ref readonly tree) => {
                if (!message.IsPressed || message.Key == '\0') return;

                m_text = tree.Visit<UIText>(name: m_textIdentifier)!;
                int currentLen = (int)m_text.Get<Scale>()!.Value.X;

                if (message.ToArrowKey() != ArrowKey.INVALID_NONE) {
                    int moveAmount = message.ToArrowKey() switch {
                        ArrowKey.RIGHT => -1,
                        ArrowKey.LEFT  =>  1,
                        _              =>  0
                    };

                    m_headPosition = int.Clamp(value: m_headPosition + moveAmount, min: 0, max: currentLen);
                    return;
                }

                switch(message.Key) {
                    case '\b': {
                        if (m_headPosition != 0) {
                            
                            m_text.Remove(count: 1);
                            --m_headPosition;
                        }
                        break;    
                    }
                    default: {
                        if (m_maxLen != m_headPosition) {
                            Span<char> temp = stackalloc char[currentLen + 1];
                            m_text.Read(destination: temp[..currentLen]);

                            temp[^1] = message.Key;
                            m_text.Write(text: temp);

                            ++m_headPosition;
                        }
                        return;
                    }
                }
            },
            Content = new UIText {
                Foreground = RGB.White, 
                Text       = m_placeholder,

                Background = RGB.Transparent,
                Name       = m_textIdentifier,
            }
        };
    }
}
