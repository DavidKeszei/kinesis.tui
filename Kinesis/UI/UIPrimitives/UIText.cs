using Kinesis.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a simple text on the screen.
/// </summary>
public class UIText: Entity {

    /// <summary>
    /// Underlying text value of the <see cref="UIText"/>.
    /// </summary>
    public string Text {
        get {
            return base.GetComponent<TextRenderer>()!.Value;
        }
        set {
            if (string.IsNullOrEmpty(value))
                return;

            base.GetComponent<TextRenderer>()!.Value = value;
            base.GetComponent<Transform>()!.Scale = new Vec2(x: value.Length, y: 1);
        }
    }

    /// <summary>
    /// Background of the <see cref="UIText"/>.
    /// </summary>
    public RGB Background { get => base.GetComponent<Style>()!.AsRGB; set => base.GetComponent<Style>()!.AsRGB = value; }

    /// <summary>
    /// Foreground/Text color of the <see cref="UIText"/>.
    /// </summary>
    public RGB Foreground { get => base.GetComponent<Style>(index: 1)!.AsRGB; set => base.GetComponent<Style>(index: 1)!.AsRGB = value; }

    /// <summary>
    /// Style indicators of the <see cref="UIText"/>.
    /// </summary>
    public StyleFlag Styles { get => base.GetComponent<Style>(2)!.AsAttribute; set => base.GetComponent<Style>(index: 2)!.AsAttribute = value; }

    public UIText() {
        base.InitRenderEntityWith<TextRenderer>();
        base.AttachComponent<Style>(component: Style.CreateFromRGB(tag: StyleTag.BACKGROUND, color: RGB.Transparent));

        base.AttachComponent<Style>(component: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: RGB.White));
        base.AttachComponent<Style>(component: Style.CreateFromAttributes(tag: StyleTag.FONT_ATTR, flag: StyleFlag.NONE));
    }
}
