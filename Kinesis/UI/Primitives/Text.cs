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
public sealed class Text: Entity, ICopyable<BuildContext>, IContentable<string> {
    
    /// <summary>
    /// Underlying text value of the <see cref="UI.Text"/>.
    /// </summary>
    public string Content {
        get => base.Get<TextRenderer>()!.Value;
        set {
            if (value != null) Write(text: value);
        }
    }

    /// <summary>
    /// Background of the <see cref="UI.Text"/>.
    /// </summary>
    public RGB Background { get => base.Get<Style>()!.AsRGB; set => base.Get<Style>()!.AsRGB = value; }

    /// <summary>
    /// Foreground/Text color of the <see cref="UI.Text"/>.
    /// </summary>
    public RGB Foreground { get => base.Get<Style>(index: 1)!.AsRGB; set => base.Get<Style>(index: 1)!.AsRGB = value; }

    /// <summary>
    /// Style indicators of the <see cref="UI.Text"/>.
    /// </summary>
    public TextDecoration Decoration { get => base.Get<Style>(index: 2)!.AsAttribute; set => base.Get<Style>(index: 2)!.AsAttribute = value; }

    public Text(): base(count: 7) {
        base.InitRenderEntityWith<TextRenderer>();

        base.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>().As<RGB?>(Style.BACKGROUND, StyleDataType.COLOR, null));
        base.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>().As<RGB?>(Style.FOREGROUND, StyleDataType.COLOR, null));

        base.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>().As<TextDecoration>(Style.FONT_ATTR, StyleDataType.FONT_ATTR, TextDecoration.NONE));

        Content = string.Empty;
    }

    /// <summary>
    /// Write a <paramref name="text"/> directly to the internal buffer <paramref name="from"/> a specific index.
    /// </summary>
    /// <param name="text">Text buffer of the method.</param>
    /// <param name="from">Start index of the write.</param>
    /// <remarks>
    /// <b>Remarks:</b> 
    ///     If the sum of the <paramref name="text"/> length and <paramref name="from"/> greater than the current text length, then that reallocating itself.<br/>
    ///     Otherwise the just copying the given <see cref="text"/>, but not reallocating the underlying buffer of the instance.
    /// </remarks>
    public int Write(ReadOnlySpan<char> text, int from = 0) {
        TextRenderer renderer = Get<TextRenderer>()!;
        int len = renderer.Write(text, from);

        Get<Scale>()!.Value = new Vec2(x: len + from, y: 1);
        return len;
    }

    public int Read(Span<char> destination, int from = 0) {
        TextRenderer renderer = Get<TextRenderer>()!;
        return renderer.Read(destination, from);
    }

    public void Remove(int count) {
        Scale scale = Get<Scale>()!;
        if (count == -1 || scale.Value.X == 0) return;

        TextRenderer renderer = Get<TextRenderer>()!;
        int width = (int)scale.Value.X;

        if (width == renderer.Length)
            scale.ChangeAxisValue(value: width - 1, axis: Axis.X);

        renderer.Remove(count);

    }

    public void Copy(ref BuildContext from) {
        from.InheritStyle(this, @default: Style.CreateFromRGB(name: Style.BACKGROUND, tag: StyleDataType.COLOR, color: RGB.Transparent));
        from.InheritStyle(this, @default: Style.CreateFromRGB(name: Style.FOREGROUND, tag: StyleDataType.COLOR, color: RGB.White), index: 1);

        from.InheritStyle(this, @default: Style.CreateFromAttributes(name: Style.FONT_ATTR, tag: StyleDataType.FONT_ATTR, flag: TextDecoration.NONE), index: 2);

        from.SetPivot<Position>(this);
        from.SetPivot<Scale>(this);
    }
}
