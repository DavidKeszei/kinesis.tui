using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a simple text on the screen.
/// </summary>
public class UIText: Entity, ICopyable<BuildContext> {

    /// <summary>
    /// Underlying text value of the <see cref="UIText"/>.
    /// </summary>
    public string Text {
        get {
            return base.Get<TextRenderer>()!.Value;
        }
        set {
            if (value == null)
                return;

            base.Get<TextRenderer>()!.Value = value;
            base.Get<Scale>()!.Value = new Vec2(x: value.Length, y: 1);
        }
    }

    /// <summary>
    /// Background of the <see cref="UIText"/>.
    /// </summary>
    public RGB Background { get => base.Get<Style>()!.AsRGB; set => base.Get<Style>()!.AsRGB = value; }

    /// <summary>
    /// Foreground/Text color of the <see cref="UIText"/>.
    /// </summary>
    public RGB Foreground { get => base.Get<Style>(index: 1)!.AsRGB; set => base.Get<Style>(index: 1)!.AsRGB = value; }

    /// <summary>
    /// Style indicators of the <see cref="UIText"/>.
    /// </summary>
    public TextDecoration Decoration { get => base.Get<Style>(2)!.AsAttribute; set => base.Get<Style>(index: 2)!.AsAttribute = value; }

    public UIText() {
        base.InitRenderEntityWith<TextRenderer>();
        base.Attach<Style>(component: Style.CreateFromRGB(tag: StyleTag.BACKGROUND, color: null!));

        base.Attach<Style>(component: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: null!));
        base.Attach<Style>(component: Style.CreateFromAttributes(tag: StyleTag.FONT_ATTR, flag: TextDecoration.NONE));
    }

    public void Copy(ref BuildContext from) {
        from.Inherit<Style>(this, @default: Style.CreateFromRGB(tag: StyleTag.BACKGROUND, color: RGB.Transparent));
        from.Inherit<Style>(this, @default: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: RGB.White), index: 1);

        from.Inherit<Style>(this, @default: Style.CreateFromAttributes(tag: StyleTag.FONT_ATTR, flag: TextDecoration.NONE), index: 2);

        from.SetPivot<Position>(this);
        from.SetPivot<Scale>(this);
    }
}
