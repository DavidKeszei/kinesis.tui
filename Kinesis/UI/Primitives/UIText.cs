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
public sealed class UIText: Entity, ICopyable<BuildContext> {
    /* TODO(2026-07-16T23:40:36): Add more controll over the text buffer through the UIText class. (Status: Done✅)
     * 
     * INSPECTIONS:
     * 	- TextRenderer already gives to us some "low-level" methods for manipulate the buffer.
     * 	- The Scale component must be syncronized with these "low-level" manipulation.
     */ 	

    /// <summary>
    /// Underlying text value of the <see cref="UIText"/>.
    /// </summary>
    public string Text {
        get {
            return base.Get<TextRenderer>()!.Value;
        }
        set {
            if (value != null) Write(text: value);
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
    public TextDecoration Decoration { get => base.Get<Style>(index: 2)!.AsAttribute; set => base.Get<Style>(index: 2)!.AsAttribute = value; }

    public UIText(): base(count: 7) {
        base.InitRenderEntityWith<TextRenderer>();

        base.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>().As<RGB?>(StyleTag.BACKGROUND, null));
        base.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>().As<RGB?>(StyleTag.FOREGROUND, null));

        base.Attach<Style>(component: ComponentPool<Style>.Instance.Rent<Style>().As<TextDecoration>(StyleTag.FONT_ATTR, TextDecoration.NONE));

        Text = string.Empty;
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
        from.InheritStyle(this, @default: Style.CreateFromRGB(tag: StyleTag.BACKGROUND, color: RGB.Transparent));
        from.InheritStyle(this, @default: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: RGB.White), index: 1);

        from.InheritStyle(this, @default: Style.CreateFromAttributes(tag: StyleTag.FONT_ATTR, flag: TextDecoration.NONE), index: 2);

        from.SetPivot<Position>(this);
        from.SetPivot<Scale>(this);
    }
}
