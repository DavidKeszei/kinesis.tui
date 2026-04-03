using Kinesis.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a text renderer component.
/// </summary>
public class TextRenderer: RenderComponent {
    private char[] m_buffer = null!;
    private int m_len = 0;

    /// <summary>
    /// Current text text of the <see cref="TextRenderer"/>.
    /// </summary>
    public string Value { 
        get => new string(value: m_buffer.AsSpan()[..m_len]);
        set => Replace(text: value);
    }

    public TextRenderer() { }

    public TextRenderer(string text) {
        m_buffer = text.ToCharArray();
        m_len = text.Length;
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

    internal override void Render(in Canvas buffer, int version, StyleEnumerator styles) {
        if (buffer.Scale.Y == 0 || buffer.Scale.X == 0)
            return;

        if(m_entityVersion != version)
            CacheStyles(styles);

        Style? bg = null!;
        Style? fg = null!;
        Style? attr = null!;

        bool isMissing = (!m_cache.TryGetValue(key: StyleTag.BACKGROUND, out bg) && !bg!.TypeOf(Style.Name)) ||
                         (!m_cache.TryGetValue(key: StyleTag.FOREGROUND, out fg) && !fg!.TypeOf(Style.Name));

        _ = m_cache.TryGetValue(key: StyleTag.FONT_ATTR, out attr);
        Vec2 requiredScale = new Vec2(x: m_len / buffer.Scale.Y, y: m_len % buffer.Scale.Y);

        for(int x = 0; x < buffer.Scale.X && x <= requiredScale.X; ++x) {
            for(int y = 0; y < buffer.Scale.Y && y <= requiredScale.Y; ++y) {

                ref vtchar_t ch = ref buffer[x, y];

                if(isMissing) {
                    if(y % 2 == 0) ch.Background = x % 2 == 0 ? RGB.Purple : RGB.Black;
                    else ch.Background = x % 2 != 0 ? RGB.Black : RGB.Purple;

                    ch.Character = ' ';
                }
                else {
                    ch.Character = m_buffer[x];
                    ch.Background = ((Style)bg!).AsRGB;

                    ch.Foreground = ((Style)fg!).AsRGB;

                    if (attr == null) ch.Styles = StyleFlag.NONE;
                    else ch.Styles = ((Style)attr).AsAttribute;
                }
            }
        }

        m_entityVersion = version;
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

    /// <summary>
    /// Replace the internal buffer with a new character sequence.
    /// </summary>
    /// <param name="text">Replace text.</param>
    private void Replace(ReadOnlySpan<char> text) {
        if (m_len < text.Length) {
            m_len = text.Length;
            m_buffer = new char[m_len];
        }

        for (int i = 0; i < m_len; ++i) {
            if (text.Length <= i) break;
            else m_buffer[i] = text[i];
        }

        m_len = text.Length;
    }
}
