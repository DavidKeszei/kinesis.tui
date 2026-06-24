using Kinesis.UI;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Represent a renderer for the <see cref="Border"/> UI element.
/// </summary>
internal sealed class BorderRenderer: RenderComponent {

    internal protected override void Render(in Canvas buffer, int version, StyleEnumerator styles) {
        if (buffer.Scale == Vec2.Zero) return;

        if (m_entityVersion != version) {
            m_entityVersion = version;
            CacheStyles(styles);
        }

        for (int y = 0; y < buffer.Scale.Y; ++y) {
            for(int x = 0; x < buffer.Scale.X; ++x) {
                ref ANSIChar cell = ref buffer[x, y];

                if (y == 0 && x == 0) cell.Character = m_cache[StyleTag.BORDER_CHAR_TOP_LEFT].AsCharacter;
                else if (y == 0 && x == buffer.Scale.X - 1) cell.Character = m_cache[StyleTag.BORDER_CHAR_TOP_RIGHT].AsCharacter;
                else if (y == buffer.Scale.Y - 1 && x == 0) cell.Character = m_cache[StyleTag.BORDER_CHAR_BOTTOM_LEFT].AsCharacter;
                else if (y == buffer.Scale.Y - 1 && x == buffer.Scale.X - 1) cell.Character = m_cache[StyleTag.BORDER_CHAR_BOTTOM_RIGHT].AsCharacter;

                if (y >= 1 && y < buffer.Scale.Y - 1 && (x == 0 || x == buffer.Scale.X - 1)) cell.Character = m_cache[StyleTag.BORDER_CHAR_VERTICAL].AsCharacter;
                else if (x >= 1 && x < buffer.Scale.X - 1 && (y == 0 || y == buffer.Scale.Y - 1)) cell.Character = m_cache[StyleTag.BORDER_CHAR_HORIZONTAL].AsCharacter;

                cell.Foreground = m_cache[StyleTag.FOREGROUND].AsRGB;
            }
        }
    }

    public override void Reset() {
        base.Reset();
        ComponentPool<RenderComponent>.Instance.Return(this);
    }

    protected override void CacheStyles(StyleEnumerator styles) {
        m_cache.Clear();

        foreach (Style style in styles) {
            bool isBorderStyle = style.Tag switch {
                StyleTag.BORDER_CHAR_TOP_RIGHT or StyleTag.BORDER_CHAR_TOP_LEFT or
                StyleTag.BORDER_CHAR_BOTTOM_RIGHT or StyleTag.BORDER_CHAR_BOTTOM_LEFT or
                StyleTag.BORDER_CHAR_HORIZONTAL or StyleTag.BORDER_CHAR_VERTICAL or StyleTag.FOREGROUND => true,
                _ => false!
            };

            if (!isBorderStyle) continue;
            m_cache.Add(style.Tag, style);
        }
    }
}
