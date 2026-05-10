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

            value.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = value;
        }
    }

    public RGB Foreground { get => GetComponent<Style>()!.AsRGB; set => GetComponent<Style>()!.AsRGB = value; }

    public BorderDecoration Characters { set => value.CreateStyles(to: this); }

    public Border() {
        InitRenderEntityWith<BorderRenderer>();

        _ = AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDir.DOWN });
        _ = AttachComponent<Style>(component: Style.CreateFromRGB(StyleTag.FOREGROUND, null!));
    }

    public void Copy(ref BuildContext context) {
        context.Set<Position>(this, @default: new Position());
        context.Set<Scale>(this, @default: new Scale(scale: Vec2.Zero));

        context.Set<Style>(this, @default: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: RGB.White));
    }
}

/// <summary>
/// Represents a collection of border-draw characters.
/// </summary>
public readonly struct BorderDecoration {
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

    internal void CreateStyles(Border to) {
        to.AttachComponent<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_TOP_LEFT, m_topLeft));
        to.AttachComponent<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_TOP_RIGHT, m_topRight));

        to.AttachComponent<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_BOTTOM_LEFT, m_bottomLeft));
        to.AttachComponent<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_BOTTOM_RIGHT, m_bottomRight));

        to.AttachComponent<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_HORIZONTAL, m_horizontal));
        to.AttachComponent<Style>(component: Style.CreateFromChar(StyleTag.BORDER_CHAR_VERTICAL, m_vertical));
    }
}