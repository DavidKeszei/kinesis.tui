using Kinesis.Core;
using Kinesis.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a style container component.
/// </summary>
public class Style: Component, IStaticType {
    private const string TYPE_NAME = "Style";
    private StyleUnion m_union = default;

    /// <summary>
    /// Name of the <see cref="Style"/> component.
    /// </summary>
    public static string Name { get => TYPE_NAME; }

    /// <summary>
    /// Tagging of the <see cref="Style"/>, which indicates what kind of style property is.
    /// </summary>
    public StyleTag Tag { get => m_union.Tag; }

    /// <summary>
    /// Interact the underlying value as <see cref="int"/>.
    /// </summary>
    public int AsInt { get => m_union.INumber; set => m_union.INumber = value; }

    /// <summary>
    /// Interact the underlying value as <see cref="RGB"/>.
    /// </summary>
    public RGB AsRGB { get => m_union.Color; set => m_union.Color = value; }

    /// <summary>
    /// Interact the underlying value as <see cref="StyleFlag"/>.
    /// </summary>
    public StyleFlag AsAttribute { get => m_union.Flag;  set => m_union.Flag = value; }

    private Style(StyleTag tag, RGB color): base(id: ComponentRegistry.QueryComponent(TYPE_NAME)) {
        m_union = new StyleUnion(tag);
        m_union.Color = color;
    }

    private Style(StyleTag tag, int value): base(id: ComponentRegistry.QueryComponent(TYPE_NAME)) {
        m_union = new StyleUnion(tag);
        m_union.INumber = value;
    }

    private Style(StyleTag tag, StyleFlag flag): base(id: ComponentRegistry.QueryComponent(TYPE_NAME)) {
        m_union = new StyleUnion(tag);
        m_union.Flag = flag;
    }

    private Style(StyleTag tag, char chr) : base(id: ComponentRegistry.QueryComponent(TYPE_NAME)) {
        m_union = new StyleUnion(tag);
        m_union.Character = chr;
    }

    /// <summary>
    /// Create a new <see cref="Style"/> with <see cref="RGB"/> value.
    /// </summary>
    /// <param name="tag">Tag of the style.</param>
    /// <param name="color">The color value itself.</param>
    /// <returns>Return a <see cref="Style"/> instance.</returns>
    public static Style CreateFromRGB(StyleTag tag, RGB color) => new Style(tag, color);

    /// <summary>
    /// Create a new <see cref="Style"/> with <see cref="RGB"/> value.
    /// </summary>
    /// <param name="tag">Tag of the style.</param>
    /// <param name="value">The color value itself.</param>
    /// <returns>Return a <see cref="Style"/> instance.</returns>
    public static Style CreateFromInt(StyleTag tag, int value) => new Style(tag, value);

    /// <summary>
    /// Create a new <see cref="Style"/> with <see cref="StyleFlag"/> value.
    /// </summary>
    /// <param name="tag">Tag of the style.</param>
    /// <param name="flag">The flag value of the VT100 character.</param>
    /// <returns>Return a <see cref="Style"/> instance.</returns>
    public static Style CreateFromAttributes(StyleTag tag, StyleFlag flag) => new Style(tag, flag);

    /// <summary>
    /// Create a new <see cref="Style"/> with <see cref="char"/> value.
    /// </summary>
    /// <param name="tag">Tag of the style.</param>
    /// <param name="chr">Character value of the <see cref="Style"/> instance.</param>
    /// <returns>Return a <see cref="Style"/> instance.</returns>
    public static Style CreateFromChar(StyleTag tag, char chr) => new Style(tag, chr);
}

/// <summary>
/// Simple union for store <see cref="Style"/> values without generic.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal struct StyleUnion {
    [FieldOffset(0)] private int m_integer = 0;
    [FieldOffset(0)] private float m_floating = .0f;

    [FieldOffset(0)] private StyleFlag m_flag = StyleFlag.NONE;
    [FieldOffset(0)] private RGB m_color = RGB.Black;

    [FieldOffset(0)] private char m_char = '\0';
    [FieldOffset(8)] private readonly StyleTag m_tag = StyleTag.BACKGROUND;

    /// <summary>
    /// Delimiter tag of the <see cref="StyleUnion"/>.
    /// </summary>
    public readonly StyleTag Tag { get => m_tag; }

    /// <summary>
    /// Create a new <see cref="StyleUnion"/>.
    /// </summary>
    /// <param name="tag">Tag of the <see cref="StyleUnion"/> instance.</param>
    public StyleUnion(StyleTag tag) => m_tag = tag;

    public char Character { readonly get => m_char; set => m_char = value; }

    public int INumber { readonly get => m_integer; set => m_integer = value; }

    public float FNumber { readonly get => m_floating; set => m_floating = value; }

    public RGB Color { readonly get => m_color; set => m_color = value; }

    public StyleFlag Flag { readonly get => m_flag; set => m_flag = value; }
}

public enum StyleTag: byte {
    BACKGROUND,
    FOREGROUND,
    BORDER_COLOR,
    BORDER_CHAR,
    PADDING,
    FONT_ATTR
}
