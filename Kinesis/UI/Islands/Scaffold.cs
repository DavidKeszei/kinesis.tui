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
    private string m_scaffoldName = null!;

    public Entity Content {
        set {
            if (value == null) return;

            Viewport scaffold = new Viewport() { 
                Name = (m_scaffoldName ??= $"__scaffold_{Guid.CreateVersion7()}__"),
                Scale = Vec2.Zero,
                Content = value
            };

            scaffold.Get<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = scaffold;

            Get<RebuildContent>()!.Content = scaffold;
            Rebuild();
        }
    }

    /// <summary>
    /// Background at the root level.
    /// </summary>
    public RGB Background { init => Get<Style>(index: 0)!.AsRGB = value; }

    /// <summary>
    /// Foreground at the root level.
    /// </summary>
    public RGB Foreground { init => Get<Style>(index: 1)!.AsRGB = value; }

    /// <summary>
    /// Text-decoration at the root level.
    /// </summary>
    public TextDecoration TextDecoration { init => Get<Style>(index: 2)!.AsAttribute = value; }

    public Scaffold(): base(count: 4) {
        _ = Attach<Style>(component: ComponentPool<Style>.Shared.Rent(static(x) => x.As<RGB?>(name: Style.BACKGROUND, tag: StyleDataType.COLOR, value: null!)));
        _ = Attach<Style>(component: ComponentPool<Style>.Shared.Rent(static(x) => x.As<RGB?>(name: Style.FOREGROUND, tag: StyleDataType.COLOR, value: null!)));

        _ = Attach<Style>(component: ComponentPool<Style>.Shared.Rent(static (x) => x.As<TextDecoration>(name: Style.FONT_ATTR, tag: StyleDataType.FONT_ATTR, value: TextDecoration.NONE)));
        _ = Attach<RebuildContent>(component: ComponentPool<RebuildContent>.Shared.Rent(), isUnique: true);
    }

    public void Copy(ref BuildContext context) {
        context.InheritStyle(this, @default: Style.CreateFromRGB(name: Style.BACKGROUND, tag: StyleDataType.COLOR, RGB.Transparent));
        context.InheritStyle(this, @default: Style.CreateFromRGB(name: Style.FOREGROUND, tag: StyleDataType.COLOR, RGB.Transparent), index: 1);

        context.InheritStyle(this, @default: Style.CreateFromAttributes(name: Style.FONT_ATTR, tag: StyleDataType.FONT_ATTR, TextDecoration.NONE), index: 2);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref readonly tree) => {
                tree.Visit<Viewport>(name: m_scaffoldName)?.Scale = message.Scale;
            },
            Content = Get<RebuildContent>()!.Content ?? null!
        };
    }
}
