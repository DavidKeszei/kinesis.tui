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

    public RGB Foreground { get => Get<Style>()!.AsRGB; set => Get<Style>()!.AsRGB = value; }

    public BorderDecoration Decoration { set => value.CreateStyles(to: this); }

    public Entity Content {
        set {
            if (value == null) return;

            value.Get<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = value;
        }
    }

    public Border(): base(count: MAX_COMPONENT_COUNT) {
        InitRenderEntityWith<BorderRenderer>();

        _ = Attach<Hierarchy>(component: ComponentPool<Hierarchy>.Instance.Rent<Hierarchy>(static(x) => x.Direction = ConnectionDirection.DOWN));
        _ = Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<RGB?>(name: Style.FOREGROUND, tag: StyleDataType.COLOR, value: null)));

        BorderDecoration.None.CreateStyles(to: this, init: true);
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Position>(this);
        context.SetPivot<Scale>(this);

        context.InheritStyle(this, @default: Style.CreateFromRGB(name: Style.FOREGROUND, tag: StyleDataType.COLOR, color: RGB.White));

        /* 
         * A border not has specific scale, always query the actual parent scale. (Scale.Auto indicates this -> float.MinValue)
         * This gives to it some flexiblity, when the scale of the parent occurs, like a stricker.
         */
        Get<Scale>()!.Value = Vec2.Auto;
    }
}

/// <summary>
/// Represents a collection of border-draw characters.
/// </summary>
public readonly struct BorderDecoration {
    private readonly int OFFSET = 1;

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
            to.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<char>(Style.BORDER_CHAR_TOP_RIGHT, StyleDataType.CHAR, ' ')));
            to.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<char>(Style.BORDER_CHAR_TOP_LEFT, StyleDataType.CHAR, ' ')));

            to.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<char>(Style.BORDER_CHAR_BOTTOM_RIGHT, StyleDataType.CHAR, ' ')));
            to.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<char>(Style.BORDER_CHAR_BOTTOM_LEFT, StyleDataType.CHAR, ' ')));

            to.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<char>(Style.BORDER_CHAR_HORIZONTAL, StyleDataType.CHAR, ' ')));
            to.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>(static(x) => x.As<char>(Style.BORDER_CHAR_VERTICAL, StyleDataType.CHAR, ' ')));
        }

        to.Get<Style>(index: OFFSET)!.AsCharacter = m_topRight;
        to.Get<Style>(index: OFFSET + 1)!.AsCharacter = m_topLeft;

        to.Get<Style>(index: OFFSET + 2)!.AsCharacter = m_bottomRight;
        to.Get<Style>(index: OFFSET + 3)!.AsCharacter = m_bottomLeft;

        to.Get<Style>(index: OFFSET + 4)!.AsCharacter = m_horizontal;
        to.Get<Style>(index: OFFSET + 5)!.AsCharacter = m_vertical;
    }
}