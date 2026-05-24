using Kinesis.Core;
using Kinesis.UI.Components;
using Kinesis.Core.Rendering;

using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Provides a foundational structure for a page, defining the layout bounds 
/// and the root visual style (colors, decorations) for its child hierarchy.
/// </summary>
public sealed class Scaffold: Island, IContentable<Entity>, ICopyable<BuildContext> {
    private readonly string m_scaffoldName = string.Empty;

    public Entity Content {
        init {
            if (value == null) return;

            UIBox scaffold = new UIBox() { 
                Name = (m_scaffoldName = $"__scaffold_{Guid.CreateVersion7()}__"),
                Scale = Vec2.Zero,
                Content = value
            };
            scaffold.RemoveComponent<RenderComponent>();

            scaffold.GetComponent<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = scaffold;
        }
    }

    /// <summary>
    /// Background at the root level.
    /// </summary>
    public RGB Background { init => GetComponent<Style>(index: 0)!.AsRGB = value; }

    /// <summary>
    /// Foreground at the root level.
    /// </summary>
    public RGB Foreground { init => GetComponent<Style>(index: 1)!.AsRGB = value; }

    /// <summary>
    /// Text-decoration at the root level.
    /// </summary>
    public TextDecoration TextDecoration { init => GetComponent<Style>(index: 2)!.AsAttribute = value; }

    public Scaffold() {
        _ = AttachComponent<Style>(component: Style.CreateFromRGB(tag: StyleTag.BACKGROUND, color: null!));
        _ = AttachComponent<Style>(component: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: null!));

        _ = AttachComponent<Style>(component: Style.CreateFromAttributes(tag: StyleTag.FONT_ATTR, flag: TextDecoration.NONE));

        _ = AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.UP });
        _ = AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.DOWN });
    }

    public void Copy(ref BuildContext context) {
        context.Inherit<Style>(this, @default: Style.CreateFromRGB(tag: StyleTag.BACKGROUND, RGB.Transparent));
        context.Inherit<Style>(this, @default: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, RGB.Transparent), index: 1);

        context.Inherit<Style>(this, @default: Style.CreateFromAttributes(tag: StyleTag.FONT_ATTR, TextDecoration.NONE), index: 2);
    }

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref tree) => {
                tree.Visit<UIBox>(name: m_scaffoldName)?.Scale = message.Scale;
            },
            Content = GetComponent<Hierarchy>(Hierarchy.ChildrenStart)!.Attached ?? null!
        };
    }
}
