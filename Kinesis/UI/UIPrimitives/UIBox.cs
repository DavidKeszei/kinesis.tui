using Kinesis.UI.Components;
using Kinesis.Rendering;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a simple, plain box on the screen with background.
/// </summary>
public class UIBox: Entity {

    /// <summary>
    /// Size of the <see cref="UIBox"/>.
    /// </summary>
    public Vec2 Scale { get => base.GetComponent<Transform>()!.Scale; set => base.GetComponent<Transform>()!.Scale = value; }

    /// <summary>
    /// Background color of the <see cref="UIBox"/>.
    /// </summary>
    public RGB Background { get => base.GetComponent<Style>()!.AsRGB; set => base.GetComponent<Style>()!.AsRGB = value; }

    /// <summary>
    /// Attached <see cref="Entity"/> instance as child.
    /// </summary>
    public Entity Child { 
        set {
            if (value == null) return;

            _ = base.GetComponent<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = value;
            _ = value.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
        } 
    }

    public UIBox() {
        InitRenderEntityWith<BoxRenderer>();

        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.DOWN });
        _ = base.AttachComponent<Style>(Style.CreateFromRGB(StyleTag.BACKGROUND, RGB.White));
    }
}
