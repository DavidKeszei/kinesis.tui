using Kinesis.Core.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Helper structure for building VT100/ANSI strings without any heap-allocation.
/// </summary>
internal ref struct ANSIStringBuilder {
    private const string ESC = "\e[";
    private const string ESC_CLEAR = "\e[0m";

    private const string ESC_BG = "\e[48;2;";
    private const string ESC_FG = "\e[38;2;";

    private const string ESC_BG_DEFAULT = "\e[49m";
    private const string ESC_FG_DEFAULT = "\e[39m";

    /// <summary>
    /// Barrier region of the <see cref="ANSIStringBuilder"/>, where the flushing can be done.
    /// </summary>
    public const int FLUSH_BARRIER = 128;

    private readonly Span<char> m_stack = default!;
    private Vec2 m_lastWritePosition = Vec2.Zero;

    private RGB m_foreground = RGB.Transparent;
    private RGB m_background = RGB.Transparent;

    private int m_position = 0;
    private TextDecoration m_flag = TextDecoration.NONE;

    public readonly bool BarrierReached { get => m_stack.Length - m_position <= FLUSH_BARRIER; }

    public ANSIStringBuilder(Span<char> buffer) {
        m_stack = buffer;
        m_stack.Clear();
    }

    /// <summary>
    /// Write the position to the screen.
    /// </summary>
    /// <param name="x">X axis value of the position.</param>
    /// <param name="y">Y axis value of the position.</param>
    /// <returns>Return the current <see cref="ANSIStringBuilder"/> instance.</returns>
    [UnscopedRef]
    public ref ANSIStringBuilder WritePosition(int x, int y) {
        /* VT100 indexes starting from 1..n */
        ++x;
        ++y;

        /* We use the automatic cursor moving of the reminal in the X direction */
        if (m_lastWritePosition.Y == y && x - m_lastWritePosition.X == 1) {
            m_lastWritePosition.X = x;
            m_lastWritePosition.Y = y;
            return ref this;
        }

        m_stack[m_position++] = '\e';
        m_stack[m_position++] = '[';

        _ = y.TryFormat(m_stack[m_position..], out int written);
        m_position += written;

        m_stack[m_position++] = ';';

        _ = x.TryFormat(m_stack[m_position..], out written);
        m_position += written;

        m_stack[m_position++] = 'f';
        return ref this;
    }

    /// <summary>
    /// Write VT100 color to the screen.
    /// </summary>
    /// <param name="color">The color itself.</param>
    /// <param name="isBackground">The color is background color or not?</param>
    /// <returns>Return the current <see cref="ANSIStringBuilder"/> instance.</returns>
    [UnscopedRef]
    public ref ANSIStringBuilder WriteColor(RGB? color, bool isBackground) {
        if (color == null) {
            (isBackground ? ESC_BG_DEFAULT : ESC_FG_DEFAULT).TryCopyTo(m_stack[m_position..]);
            m_position += (isBackground ? ESC_BG_DEFAULT : ESC_FG_DEFAULT).Length;

            if (isBackground) m_background = RGB.Transparent;
            else m_foreground = RGB.Transparent;

            return ref this;
        }

        if ((m_background.Equals(rgb: color.Value) && isBackground) || (m_foreground.Equals(rgb: color.Value) && !isBackground))
            return ref this;

        (isBackground ? ESC_BG : ESC_FG).CopyTo(m_stack[m_position..]);

        m_position += (isBackground ? ESC_BG : ESC_FG).Length;
        float alpha = color.Value.A / 255f;

        ((byte)(color.Value.R * alpha)).TryFormat(m_stack[m_position..], out int written);
        m_position += written;
        m_stack[m_position++] = ';';

        ((byte)(color.Value.G * alpha)).TryFormat(m_stack[m_position..], out written);
        m_position += written;
        m_stack[m_position++] = ';';

        ((byte)(color.Value.B * alpha)).TryFormat(m_stack[m_position..], out written);
        m_position += written;
        m_stack[m_position++] = 'm';

        if (isBackground) m_background = color.Value;
        else m_foreground = color.Value;

        return ref this;
    }

    [UnscopedRef]
    public ref ANSIStringBuilder WriteFontStyles(TextDecoration flags) {
        if(m_flag == flags || flags == TextDecoration.NONE || flags == 0)
            return ref this;

        /* Static stack allocated flags, which give us information about the supported flags. */
        Span<TextDecoration> supportedFlags = stackalloc TextDecoration[] {
            TextDecoration.BOLD, TextDecoration.ITALIC, TextDecoration.UNDERLINE,
            TextDecoration.BLINK_SLOW, TextDecoration.BLINK_FAST, TextDecoration.INVERSE,

            TextDecoration.HIDDEN, TextDecoration.STROKE_THROUGH, TextDecoration.DOUBLE_UNDERLINE,
            TextDecoration.OVERLINE
        };

        ESC.CopyTo(destination: m_stack[m_position..]);
        m_position += ESC.Length;

        foreach(TextDecoration flag in supportedFlags) {
            if((flags & flag) == flag) {
                int code = flag switch {
                    TextDecoration.BOLD => 1,
                    TextDecoration.ITALIC => 3,

                    TextDecoration.UNDERLINE => 4,
                    TextDecoration.BLINK_SLOW => 5,

                    TextDecoration.BLINK_FAST => 6,
                    TextDecoration.INVERSE => 7,

                    TextDecoration.HIDDEN => 8,
                    TextDecoration.STROKE_THROUGH => 9,

                    TextDecoration.DOUBLE_UNDERLINE => 21,
                    TextDecoration.OVERLINE => 53,

                    _ => 0
                };

                flags &= ~(flag);

                code.TryFormat(destination: m_stack[m_position..], out int written);
                m_position += written;

                m_stack[m_position++] = ';';
            }
        }

        m_stack[m_position - 1] = 'm';
        m_flag = flags;

        return ref this;
    }

    /// <summary>
    /// Add a character to the screen.
    /// </summary>
    /// <param name="value">Inset of the character.</param>
    /// <returns>Return the current <see cref="ANSIStringBuilder"/> instance.</returns>
    [UnscopedRef]
    public ref ANSIStringBuilder WriteCharacter(char value) {
        m_stack[m_position++] = value;
        _ = AnsiCommand.ResetFontStyles.TryCopyTo(destination: m_stack[m_position..]);

        m_position += AnsiCommand.ResetFontStyles.Length;
        return ref this;
    }

    [UnscopedRef]
    public ref ANSIStringBuilder WriteRaw(ReadOnlySpan<char> sequence) {
        sequence.TryCopyTo(m_stack[m_position..]);
        m_position += sequence.Length;

        return ref this;
    }

    /// <summary>
    /// Build the underlying buffer and send to the <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">Destination of console screen.</param>
    /// <returns>Return the command length as <see cref="int"/>.</returns>
    public void Build(StreamWriter destination) {
        for (int i = 0; i < m_position; ++i)
            destination.Write(value: m_stack[i]);

        m_position = 0;
    }
}
