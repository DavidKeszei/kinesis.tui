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
    /// Remove characters from internal buffer.
    /// </summary>
    /// <param name="count">Amount of the remove.</param>
    public void Remove(int count) {
        if (count > m_len) {
            m_len = 0;
            return;
        }

        m_len -= count;
    }

    /// <summary>
    /// Write into the internal buffer a specific <paramref name="text"/>.
    /// </summary>
    /// <param name="text">New value of the internal buffer.</param>
    public void Write(ReadOnlySpan<char> text) {
        if ((m_buffer?.Length ?? m_len) < text.Length) {
            m_len = text.Length;

            if(m_buffer != null) ArrayPool<char>.Shared.Return(m_buffer, true);
            m_buffer = ArrayPool<char>.Shared.Rent(minimumLength: m_len);
        }

        for (int i = 0; i < m_len; ++i) {
            if (text.Length <= i) break;
            else m_buffer![i] = text[i];
        }

        m_len = text.Length;
    }

    public override void Reset() {
        m_len = 0;
        base.Reset();

        ArrayPool<char>.Shared.Return(m_buffer, true);
        m_buffer = null!;

        ComponentPool<TextRenderer>.Instance.Return(this);
    }

    internal protected override void Render(in Canvas buffer, int version, StyleEnumerator styles) {
        if (buffer.Scale.Y == 0 || buffer.Scale.X == 0)
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
