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
public class UIBox: Entity, ICopyable<BuildContext>, IContentable<Entity> {

    /// <summary>
    /// Size of the <see cref="UIBox"/>.
    /// </summary>
    public Vec2 Scale { get => base.GetComponent<Scale>()!.Value; set => base.GetComponent<Scale>()!.Value = value; }

    /// <summary>
    /// Background color of the <see cref="UIBox"/>.
    /// </summary>
    public RGB Background { get => base.GetComponent<Style>()!.AsRGB; set => base.GetComponent<Style>()!.AsRGB = value; }

    /// <summary>
    /// Attached <see cref="Entity"/> instance as child.
    /// </summary>
    public Entity Content {
        init {
            if (value == null) return;

            _ = base.GetComponent<Hierarchy>(index: Hierarchy.ChildrenStart)!.Attached = value;
            _ = value.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
        }
    }

    public UIBox() {
        InitRenderEntityWith<BoxRenderer>();

        _ = base.AttachComponent<Hierarchy>(new Hierarchy() { Direction = ConnectionDir.DOWN });
        _ = base.AttachComponent<Style>(component: Style.CreateFromRGB(tag: StyleTag.BACKGROUND, color: RGB.Transparent), isUnique: true);
    }

    public void Copy(ref BuildContext from) {
        from.Set<Position>(this, @default: new Position());
        from.Set<Scale>(this, @default: new Components.Scale(scale: Vec2.One * float.MinValue));

        from.Set<Style>(this, @default: Style.CreateFromRGB(StyleTag.BACKGROUND, RGB.Transparent));
    }
}
