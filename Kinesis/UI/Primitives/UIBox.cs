using Kinesis.UI.Components;
using Kinesis.Core.Rendering;

using System;
using System.Collections.Generic;
using System.Text;
using Kinesis.Core;

namespace Kinesis.UI;

/// <summary>
/// Represent a simple, plain box on the screen with background.
/// </summary>
public sealed class UIBox: Entity, ICopyable<BuildContext>, IContentable<Entity> {

    /// <summary>
    /// Size of the <see cref="UIBox"/>.
    /// </summary>
    public Vec2 Scale { get => base.Get<Scale>()!.Value; set => base.Get<Scale>()!.Value = value; }

    /// <summary>
    /// Background color of the <see cref="UIBox"/>.
    /// </summary>
    public RGB Background { get => base.Get<Style>()!.AsRGB; set => base.Get<Style>()!.AsRGB = value; }

    /// <summary>
    /// Filler character inside the box.
    /// </summary>
    public Filler Filler { init => value.ToComponents(this); }

    /// <summary>
    /// Attached <see cref="Entity"/> instance as child.
    /// </summary>
    public Entity Content {
        init {
            if (value == null) return;

            _ = base.Get<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = value;
            _ = value.Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
        }
    }

    public UIBox() {
        InitRenderEntityWith<BoxRenderer>();

        _ = base.Attach<Hierarchy>(new Hierarchy() { Direction = ConnectionDirection.DOWN });
        _ = base.Attach<Style>(component: Style.CreateFromRGB(tag: StyleTag.BACKGROUND, color: null!));

        _ = base.Attach<Style>(component: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: null!));
        _ = base.Attach<Style>(component: Style.CreateFromChar(tag: StyleTag.FILLER, chr: ' '));
    }

    public void Copy(ref BuildContext from) {
        from.Inherit<Style>(this, @default: Style.CreateFromRGB(StyleTag.BACKGROUND, RGB.Transparent));
        from.Inherit<Style>(this, @default: Style.CreateFromRGB(StyleTag.FOREGROUND, RGB.White), index: 1);

        from.SetPivot<Scale>(this);
        from.SetPivot<Position>(this);
    }
}

public readonly struct Filler {
    private readonly RGB m_foreground = RGB.Transparent;
    private readonly char m_character = ' ';

    public readonly RGB Color { get => m_foreground; init => m_foreground = value; }

    public readonly char Character { get => m_character; init => m_character = value; }

    public Filler(RGB color, char character) {
        m_character = character;
        m_foreground = color;
    }

    public void ToComponents(UIBox box) {
        box.Get<Style>(index: 1)!.AsRGB = m_foreground;
        box.Get<Style>(index: 2)!.AsCharacter = m_character;
    }
}