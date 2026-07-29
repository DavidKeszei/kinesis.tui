using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;

using System.Diagnostics;

namespace Kinesis.UI;

/// <summary>
/// Represent an textual input on the screen.
/// </summary>
public sealed class InputField: Island, ICopyable<BuildContext> {
    private const int LAST_UNPRINTBALE_CHR = 31;

    private readonly string m_textIdentifier = null!;
    private readonly string m_cursorIdentifier = null!;

    private readonly string m_animatedAreaIdentifier = null!;
    private string m_placeholder = string.Empty;

    private readonly int m_maxLen = 64;
    private readonly float m_blinkTime = 1f;

    private readonly char[] m_rawBuffer = null!;
    private readonly Func<char, char> m_transform = null!;

    private int m_headPosition = 0;
    private int m_charCount = -1;

    /// <summary>
    /// Maximum length of the text of the <see cref="InputField"/> instance.
    /// </summary>
    public int MaximumLength { 
        init {
            m_maxLen = int.Max(x: value, y: 1);
            m_rawBuffer = new char[m_maxLen];

            Get<Scale>()!.Value = new Vec2(x: value + 1, y: 1);
        }
    }

    /// <summary>
    /// Placeholder text of the <see cref="InputField"/> instance.
    /// </summary>
    public string Placeholder { init => m_placeholder = value ?? string.Empty; }

    /// <summary>
    /// Duration of the cursor blinking in seconds.
    /// </summary>
    public float BlinkDuration { init => m_blinkTime = float.Clamp(value, .1f, float.MaxValue); }

    /// <summary>
    /// Represents a function, which transform a <see langword="char"/> before render it on the screen.
    /// </summary>
    public Func<char, char> Transform { init => m_transform = value; }

    /// <summary>
    /// Current character length of the input as <see langword="int"/>.
    /// </summary>
    public int Length { get => m_charCount; }

    /// <summary>
    /// Create a new <see cref="InputField"/> instance.
    /// </summary>
    public InputField(): base(count: MAX_COMPONENT_COUNT) {
        _ = Attach<Position>(component: ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = Attach<Scale>(component: ComponentPool<Scale>.Instance.Rent<Scale>(static(x) => x.Value = Vec2.Auto), isUnique: true);

        _ = Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<RGB?>(tag: StyleTag.BACKGROUND, value: null!)));

        Guid guid        = Guid.CreateVersion7();
        m_textIdentifier = $"__input_box_text_{guid}__";

        m_cursorIdentifier       = $"__input_box_cursor_{guid}__";
        m_animatedAreaIdentifier = $"__input_box_animArea_{guid}__";
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Scale>(this);
        context.SetPivot<Position>(this);

        context.InheritStyle(this, @default: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, RGB.White));
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        if (m_placeholder.Length > m_maxLen) {
            Debug.WriteLine(message: $"[Info] Placeholder text (value: {m_placeholder}) larger than the maximum length of the input field.");
            m_placeholder = m_placeholder[..m_maxLen];
        }

        return new OnUpdate<InputMessage>(context) {
            On = (message, ref readonly tree) => {
                if (!message.IsPressed || (message.Key != '\b' && message.Key < LAST_UNPRINTBALE_CHR)) return;

                UIText text = tree.Visit<UIText>(name: m_textIdentifier)!;
                int currentLen = (int)text.Get<Scale>()!.Value.X;

                if (message.ToArrowKey() != ArrowKey.INVALID_NONE) {
                    int moveAmount = message.ToArrowKey() switch {
                        ArrowKey.RIGHT =>  1,
                        ArrowKey.LEFT  => -1,
                        _ => 0
                    };

                    moveAmount += m_charCount == 0 ? moveAmount * -1 : 0;
                    m_headPosition = int.Clamp(value: m_headPosition + moveAmount, min: 0, max: currentLen);
                    return;
                }

                switch (message.Key) {
                    case '\b': {
                        RemoveChar(text);

                        if(m_charCount == 0)
                            ShowPlaceholder(text, inputAdded: false);
                        break;
                    }
                    default: {
                        AddChar(text, message.Key);

                        if(m_charCount == 1)
                            ShowPlaceholder(text, inputAdded: true);
                        break;
                    }
                }
            },
            Content = new OnUpdate<RenderMessage>(context) {
                On = (message, ref readonly tree) => {
                    /* At the first run, we set the inherited color to the cursor & placeholder/text */
                    if (m_charCount == -1) {
                        m_charCount = 0;
                        RGB color = Get<Style>()!.AsRGB;

                        UIText txt = tree.Visit<UIText>(name: m_textIdentifier)!;
                        txt.Foreground = color with { A = 128 };

                        AnimatedArea<RGB, UIBox> box = tree.Visit<AnimatedArea<RGB, UIBox>>(name: m_animatedAreaIdentifier)!;
                        box.To = color;
                    }

                    int width = (int)Get<Scale>()!.Value.X; 
                    UIBox cursor = tree.Visit<UIBox>(name: m_cursorIdentifier)!;

                    if (m_headPosition < width && m_charCount >= 0) 
                        cursor.Move(x: m_headPosition, y: 0);
                },
                Content = new UIStack {
                    Content = [
                        new UIText {
                            Name       = m_textIdentifier,
                            Text       = m_placeholder,

                            Decoration = TextDecoration.ITALIC,
                            Foreground = Get<Style>()!.AsRGB with { A = 128 },
                        },
                        new AnimatedArea<RGB, UIBox> {
                            Name       = m_animatedAreaIdentifier,

                            Selector   = static(box)        => box.Background,
                            Applier    = static(box, color) => box.Background = color,

                            To         = RGB.White,
                            Duration   = TimeSpan.FromSeconds(value: m_blinkTime),

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

    /// <summary>
    /// Read the entire content to the specified <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">Destination buffer of the reading.</param>
    /// <remarks>
    /// Remarks: 
    ///     If the destination was smaller than the internal buffer, then read process not read <br/>
    ///     all content of the current <see cref="InputField"/> instance into the <paramref name="destination"/>.
    /// </remarks>
    /// <returns>Returns the count of the reading as <see langword="int"/>.</returns>
    public int Read(Span<char> destination) {
        int len = int.Min(destination.Length, m_charCount);

        for (int i = 0; i < len; ++i)
            destination[i] = m_rawBuffer[i];

        return len;
    }

    private void AddChar(UIText text, char letter) {
        if (m_headPosition >= m_maxLen) return;
        int len = text.Text.Length;

        Span<char> left = stackalloc char[m_headPosition + 1];
        Span<char> rigth = stackalloc char[len - m_headPosition];

        m_rawBuffer[m_charCount] = letter;
        left[^1] = m_transform == null ? letter : m_transform(letter);

        _ = text.Read(destination: left[..m_headPosition]);
        _ = text.Read(destination: rigth, from: m_headPosition);

        _ = text.Write(left);
        _ = text.Write(rigth, from: m_headPosition + 1);

        ++m_headPosition;
        ++m_charCount;
    }

    private void RemoveChar(UIText text) {
        if (m_headPosition - 1 < 0 || m_charCount == 0) return;
        int len = text.Text.Length;

        Span<char> left  = stackalloc char[m_headPosition - 1];
        Span<char> rigth = stackalloc char[len - m_headPosition];

        text.Read(destination: left[..(m_headPosition - 1)]);
        text.Read(destination: rigth, from: m_headPosition);

        text.Write(text: left);
        text.Write(text: rigth, from: m_headPosition - 1);

        --m_headPosition;
        --m_charCount;
    }

    private void ShowPlaceholder(UIText text, bool inputAdded) {
        if (inputAdded) {

            //[0][...] -> First: Real input; From the seocnd: Placeholder
            text.Write(text: [], from: 1);

            text.Decoration = TextDecoration.NONE;
            text.Foreground = text.Foreground with { A = 255 };

            return;
        }

        text.Write(text: m_placeholder);

        text.Decoration = TextDecoration.ITALIC;
        text.Foreground = text.Foreground with { A = 128 };
    }
}
