using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a border on an area like a stickers.
/// </summary>
public class Border: Entity, ICopyable<BuildContext>, IContentable<Entity> {
    public Entity Content {
        init {
            if (value == null) return;

            value.Get<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = value;
        }
    }

    public RGB Foreground { get => Get<Style>()!.AsRGB; set => Get<Style>()!.AsRGB = value; }

    public BorderDecoration Decoration { set => value.CreateStyles(to: this); }

    public Border() {
        InitRenderEntityWith<BorderRenderer>();

        _ = Attach<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.DOWN });
        _ = Attach<Style>(component: Style.CreateFromRGB(StyleTag.FOREGROUND, null!));

        BorderDecoration.None.CreateStyles(to: this, init: true);
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);

        context.Inherit<Style>(this, @default: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: RGB.White));

        /* 
         * A border not has specific scale, always query the actual parent scale. (Scale.Auto indicates this -> float.MinValue)
         * This gives to it some flexiblity, when the scale of the parent occurs, like a stricker.
         */
        Get<Scale>()!.Value = Vec2.One * Scale.Auto;
    }
}

/// <summary>
/// Represents a collection of border-draw characters.
/// </summary>
public readonly struct BorderDecoration {
    private readonly int OFFSET = 3;

    private readonly char m_topLeft = ' ';
    private readonly char m_topRight = ' ';

    private readonly char m_bottomRight = ' ';
    private readonly char m_bottomLeft = ' ';

    private readonly char m_vertical = ' ';
    private readonly char m_horizontal = ' ';

    /// <summary>
    /// Modern, unicode border characters.
    /// </summary>
    public static BorderDecoration Arc { 
        get => new BorderDecoration {
            TopRigth = '╮',
            TopLeft = '╭',

            BottomRight = '╯',
            BottomLeft = '╰',

            Horizontal = '─',
            Vertical = '│',
        }; 
    }

    public static BorderDecoration None {
        get => new BorderDecoration {
            TopRigth = ' ',
            TopLeft = ' ',

            BottomRight = ' ',
            BottomLeft = ' ',

            Horizontal = ' ',
            Vertical = ' ',
        };
    }

    public static BorderDecoration Square {
        get => new BorderDecoration {
            TopRigth = '┐',
            TopLeft = '┌',

            BottomRight = '┘',
            BottomLeft = '└',

            Horizontal = '─',
            Vertical = '│'
        };
    }

    /// <summary>
    /// Top-right character of the border.
    /// </summary>
    public char TopRigth { init => m_topRight = value; }

    /// <summary>
    /// Top-left character of the border.
    /// </summary>
    public char TopLeft { init => m_topLeft = value; }

    /// <summary>
    /// Bottom-right character of the border.
    /// </summary>
    public char BottomRight { init => m_bottomRight = value; }

    /// <summary>
    /// Bottom-left character of the border.
    /// </summary>
    public char BottomLeft { init => m_bottomLeft = value; }

    /// <summary>
    /// Vertical filler character of the border.
    /// </summary>
    public char Vertical { init => m_vertical = value; }

    /// <summary>
    /// Horizontal filler character of the border.
    /// </summary>
    public char Horizontal { init => m_horizontal = value; }

    public BorderDecoration() { }

    internal void CreateStyles(Border to, bool init = false) {
        if (init) {
            to.Attach<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_TOP_RIGHT, m_topRight));
            to.Attach<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_TOP_LEFT, m_topLeft));

            to.Attach<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_BOTTOM_RIGHT, m_bottomRight));
            to.Attach<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_BOTTOM_LEFT, m_bottomLeft));

            to.Attach<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_HORIZONTAL, m_horizontal));
            to.Attach<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_VERTICAL, m_vertical));
            return;
        }

        to.Get<Style>(index: (int)StyleTag.BORDER_CHAR_TOP_RIGHT - OFFSET)!.AsCharacter = m_topRight;
        to.Get<Style>(index: (int)StyleTag.BORDER_CHAR_TOP_LEFT - OFFSET)!.AsCharacter = m_topLeft;

        to.Get<Style>(index: (int)StyleTag.BORDER_CHAR_BOTTOM_RIGHT - OFFSET)!.AsCharacter = m_bottomRight;
        to.Get<Style>(index: (int)StyleTag.BORDER_CHAR_BOTTOM_LEFT - OFFSET)!.AsCharacter = m_bottomLeft;

        to.Get<Style>(index: (int)StyleTag.BORDER_CHAR_HORIZONTAL - OFFSET)!.AsCharacter = m_horizontal;
        to.Get<Style>(index: (int)StyleTag.BORDER_CHAR_VERTICAL - OFFSET)!.AsCharacter = m_vertical;
    }
}