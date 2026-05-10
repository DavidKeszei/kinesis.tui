using Kinesis.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a component, which can drawing a box to the screen.
/// </summary>
public class BoxRenderer: RenderComponent {

    /// <summary>
    /// Render a box to the specific <paramref name="buffer"/> area.
    /// </summary>
    /// <param name="buffer">Target buffer of the rendering. This can be smaller, than the requested scale.</param>
    internal override void Render(in Canvas buffer, int version, StyleEnumerator styles) {
        if(m_entityVersion != version) {
            m_entityVersion = version;
            CacheStyles(styles);
        }

        for (int x = 0; x < buffer.Scale.X; ++x) {
            for (int y = 0; y < buffer.Scale.Y; ++y) {
                ref vtchar_t ch = ref buffer[x, y];

                /* Source-like visual debug, if the background as visual effect not exists or just wrong type/typo. */
                if(!m_cache.TryGetValue(key: StyleTag.BACKGROUND, out Style? bg) || bg is not Style) {
                    if(y % 2 != 0) ch.Background = x % 2 != 0 ? RGB.Purple : RGB.Black;
                    else ch.Background = x % 2 == 0 ? RGB.Purple : RGB.Black;

                    ch.Character = ' ';
                }
                else {
                    ch.Background = RGB.Blend(top: bg.AsRGB, bottom: ch.Background);
                }
            }
        }
    }

    protected override void CacheStyles(StyleEnumerator styles) {
        m_cache.Clear();

        foreach(Style style in styles) {
            switch (style.Tag) {
                case StyleTag.BACKGROUND:
                    m_cache.Add(style.Tag, style);
                    break;
                default:
                    break;
            }
        }
    }
}
