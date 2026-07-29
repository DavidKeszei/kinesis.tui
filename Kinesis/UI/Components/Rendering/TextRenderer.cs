using Kinesis.Core;
using Kinesis.Core.Rendering;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a text renderer component.
/// </summary>
public class TextRenderer(): RenderComponent, IPoolable {
    private char[] m_buffer = null!;
    private int m_len = 0;

    /// <summary>
    /// Current text of the <see cref="TextRenderer"/>.
    /// </summary>
    public string Value { 
        get => new string(value: m_buffer.AsSpan()[..m_len]);
        set => Write(text: value);
    }

    /// <summary>
    /// Length of the current rendered text as <see cref="int"/>,
    /// </summary>
    public int Length { get => m_len; }

    /// <summary>
    /// Remove characters from internal buffer.
    /// </summary>
    /// <param name="count">Amount of the remove.</param>
    public void Remove(int count) {
        if (count > m_len || m_len - count < 0) {
            m_len = 0;
            return;
        }

        m_len -= count;
    }

    /// <summary>
    /// Write into the internal buffer a specific <paramref name="text"/>.
    /// </summary>
    /// <param name="text">New value of the internal buffer.</param>
    /// <param name="from">Starting point of the write. (If you want replace the text, then leave it at zero.)</param>
    /// <returns>Returns the write count of the call. This <b>always</b> equals with length of the <see cref="text"/>.</returns>
    public int Write(ReadOnlySpan<char> text, int from = 0) {
        if ((m_buffer?.Length ?? m_len) < from + text.Length) {
            Span<char> temp = stackalloc char[from];
            Span<char> bufferView = m_buffer;

            bufferView[..from].CopyTo(destination: temp);
            m_len = text.Length + from;

            if(m_buffer != null) ArrayPool<char>.Shared.Return(m_buffer, true);
            m_buffer = ArrayPool<char>.Shared.Rent(minimumLength: m_len);

            bufferView = m_buffer;
            if (from != 0)
                temp.CopyTo(destination: bufferView[..from]);
        }

        m_len = text.Length + (int)from;

        for (int i = (int)from; i < m_len; ++i) {
            if (text.Length + from <= i) break;
            else m_buffer![i] = text[i - (int)from];
        }

        return text.Length;
    }

    public int Read(Span<char> destination, int from = 0) {
        if (from > m_len || from < 0) return -1;
        int len = m_len > destination.Length ? destination.Length : m_len;

        for (int i = from; i < from + len; ++i)
            destination[i - from] = m_buffer[i];

        return len - from;
    }

    public override void Reset() {
        m_len = 0;
        base.Reset();

        if (m_buffer != null) {
            ArrayPool<char>.Shared.Return(m_buffer, true);
            m_buffer = null!;
        }

        ComponentPool<TextRenderer>.Instance.Return(this);
    }

    internal protected override void Render(in Canvas buffer, int version, StyleEnumerator styles) {
        if (buffer.Scale == Vec2.Zero)
            return;

        if (m_entityVersion != version) {
            m_entityVersion = version;
            CacheStyles(styles);
        }

        Style? bg = null!;
        Style? fg = null!;
        Style? attr = null!;

        bool isMissing = (!m_cache.TryGetValue(key: StyleTag.BACKGROUND, out bg) && !bg!.TypeOf(Style.Name)) ||
                         (!m_cache.TryGetValue(key: StyleTag.FOREGROUND, out fg) && !fg!.TypeOf(Style.Name));

        _ = m_cache.TryGetValue(key: StyleTag.FONT_ATTR, out attr);
        Vec2 requiredScale = new Vec2(x: m_len / buffer.Scale.Y, y: m_len % buffer.Scale.Y);

        for(int x = 0; x < buffer.Scale.X && x <= requiredScale.X; ++x) {
            for(int y = 0; y < buffer.Scale.Y && y <= requiredScale.Y; ++y) {

                ref ANSIChar ch = ref buffer[x, y];

                if(isMissing) {
                    if(y % 2 == 0) ch.Background = x % 2 == 0 ? RGB.Purple : RGB.Black;
                    else ch.Background = x % 2 != 0 ? RGB.Black : RGB.Purple;

                    ch.Character = ' ';
                }
                else {
                    ch.Character = m_buffer[x + (int)buffer.Start.X];
                    ch.Background = RGB.Blend(bg.AsRGB, ch.Background);

                    /* TODO(2026-07-25T00:49:06): Bad foreground blending (Status: Done✅)
                     * 
                     * Inspection(s):
                     *  - Watch for not required foreground coloring, which brake the drawing logic.
                     */ 	
                    ch.Foreground = RGB.Blend(fg.AsRGB, ch.Foreground);

                    if (attr == null) ch.Styles = TextDecoration.NONE;
                    else ch.Styles = ((Style)attr).AsAttribute;
                }
            }
        }
    }

    protected override void CacheStyles(StyleEnumerator styles) {
        m_cache.Clear();
        foreach(Style style in styles) {
            
            switch(style.Tag) {
                case StyleTag.FOREGROUND:
                    m_cache.Add(StyleTag.FOREGROUND, style);
                    break;

                case StyleTag.BACKGROUND:
                    m_cache.Add(StyleTag.BACKGROUND, style);
                    break;

                case StyleTag.FONT_ATTR:
                    m_cache.Add(StyleTag.FONT_ATTR, style);
                    break;
            }
        }
    }
}
