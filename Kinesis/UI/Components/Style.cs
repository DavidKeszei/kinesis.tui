using Kinesis.Core;
using Kinesis.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent style information on an <see cref="Entity"/>,
/// </summary>
public sealed class Style(): Component(id: ComponentRegistry.QueryComponent(name: TYPE_NAME)), IStaticType, ICopyable<Style>, IDefault<Style>, IPoolable {
    #region __NAMES__

    private const string TYPE_NAME = nameof(Style);

    public const string BACKGROUND = "BACKGROUND";
    public const string FOREGROUND = "FOREGROUND";

    public const string PADDING   = "PADDING";
    public const string FONT_ATTR = "FONT_ATTR";

    public const string BORDER_CHAR_TOP_RIGHT    = "BORDER_CHAR_TOP_RIGHT";
    public const string BORDER_CHAR_TOP_LEFT     = "BORDER_CHAR_TOP_LEFT";

    public const string BORDER_CHAR_BOTTOM_RIGHT = "BORDER_CHAR_BOTTOM_RIGHT";
    public const string BORDER_CHAR_BOTTOM_LEFT  = "BORDER_CHAR_BOTTOM_LEFT";

    public const string BORDER_CHAR_VERTICAL     = "BORDER_CHAR_VERTICAL";
    public const string BORDER_CHAR_HORIZONTAL   = "BORDER_CHAR_HORIZONTAL";

    public const string FILLER   = "FILLER";

    #endregion

    private StyleUnion m_union = default;
    private string m_name = null!;

    /// <summary>
    /// Name of the <see cref="Style"/> component type.
    /// </summary>
    public static string TypeName { get => TYPE_NAME; }

    /// <summary>
    /// Name of the current <see cref="Style"/> instance.
    /// </summary>
    public string Name { get => m_name; } 

    /// <summary>
    /// Tagging of the <see cref="Style"/>, which indicates what kind of other property is.
    /// </summary>
    public StyleDataType Tag { get => m_union.Tag; }

    /// <summary>
    /// Interact the underlying value as <see cref="int"/>.
    /// </summary>
    public int AsInt { get => m_union.INumber; set => m_union.INumber = value; }

    /// <summary>
    /// Interact the underlying value as <see cref="RGB"/>.
    /// </summary>
    public RGB AsRGB { get => m_union.Color ?? RGB.Transparent; set => m_union.Color = value; }

    /// <summary>
    /// Interact the underlying value as <see cref="TextDecoration"/>.
    /// </summary>
    public TextDecoration AsAttribute { get => m_union.Flag; set => m_union.Flag = value; }

    /// <summary>
    /// Interact the underlying value as <see cref="char"/>.
    /// </summary>
    public char AsCharacter { get => m_union.Character; set => m_union.Character = value; }

    private Style(string name, StyleDataType tag, RGB? color): this() {
        m_union = new StyleUnion(tag);
        m_union.Color = color;

        m_name = name;
    }

    private Style(string name, StyleDataType tag, int value): this() {
        m_union = new StyleUnion(tag);
        m_union.INumber = value;

        m_name = name;
    }

    private Style(string name, StyleDataType tag, TextDecoration flag): this() {
        m_union = new StyleUnion(tag);
        m_union.Flag = flag;

        m_name = name;
    }

    private Style(string name, StyleDataType tag, char chr): this() {
        m_union = new StyleUnion(tag);
        m_union.Character = chr;

        m_name = name;
    }

    /// <summary>
    /// Change the current <see cref="Style"/> instance to other <see cref="Style"/>.
    /// </summary>
    /// <typeparam name="T">Type of the underlying style.</typeparam>
    /// <param name="tag">New tag/discriminator value of the <see cref="Style"/>.</param>
    /// <param name="value">New value of the <see cref="Style"/>.</param>
    /// <remarks>
    /// <b>Remarks:</b> This method (now) can be run with any parameter types; so there is the supported types: <see langword="int"/>, <see langword="char"/>
    ///                 <see cref="RGB"/>, <see cref="TextDecoration"/>.
    /// </remarks>
    public Style As<T>(string name, StyleDataType tag, T value) {
        m_union = tag switch {
            StyleDataType.COLOR => new StyleUnion(tag) { Color = value != null ? Unsafe.As<T, RGB>(ref value) : null! },
            StyleDataType.CHAR  => new StyleUnion(tag) { Character = Unsafe.As<T, char>(ref value) },

            StyleDataType.FONT_ATTR => new StyleUnion(tag) { Flag = Unsafe.As<T, TextDecoration>(ref value) },
            StyleDataType.NUMERIC_I => new StyleUnion(tag) { INumber = Unsafe.As<T, int>(ref value) },

            _ => m_union
        };

        m_name = name;
        return this;
    }

    /// <summary>
    /// Create a new <see cref="Style"/> with <see cref="RGB"/> value.
    /// </summary>
    /// <param name="tag">Tag of the other.</param>
    /// <param name="color">The color value itself.</param>
    /// <returns>Return a <see cref="Style"/> instance.</returns>
    public static Style CreateFromRGB(string name, StyleDataType tag, RGB? color) => new Style(name, tag, color);

    /// <summary>
    /// Create a new <see cref="Style"/> with <see cref="RGB"/> value.
    /// </summary>
    /// <param name="tag">Tag of the other.</param>
    /// <param name="value">The color value itself.</param>
    /// <returns>Return a <see cref="Style"/> instance.</returns>
    public static Style CreateFromInt(string name, StyleDataType tag, int value) => new Style(name, tag, value);

    /// <summary>
    /// Create a new <see cref="Style"/> with <see cref="TextDecoration"/> value.
    /// </summary>
    /// <param name="tag">Tag of the other.</param>
    /// <param name="flag">The flag value of the VT100 character.</param>
    /// <returns>Return a <see cref="Style"/> instance.</returns>
    public static Style CreateFromAttributes(string name, StyleDataType tag, TextDecoration flag) => new Style(name, tag, flag);

    /// <summary>
    /// Create a new <see cref="Style"/> with <see cref="char"/> value.
    /// </summary>
    /// <param name="tag">Tag of the other.</param>
    /// <param name="chr">Character value of the <see cref="Style"/> instance.</param>
    /// <returns>Return a <see cref="Style"/> instance.</returns>
    public static Style CreateFromChar(string name, StyleDataType tag, char chr) => new Style(name, tag, chr);

    public static bool IsDefault(Style instance) {
        if (instance == null) return false;

        return instance.m_union.Tag switch {
            StyleDataType.COLOR => instance.m_union.Color == null,

            StyleDataType.NUMERIC_I => instance.m_union.INumber == int.MinValue,
            StyleDataType.FONT_ATTR => instance.m_union.Flag == TextDecoration.NONE,

            StyleDataType.CHAR => instance.m_union.Character == '\0',

            _ => false
        };
    }

    public void Reset() {
        m_union = default!;
        ComponentPool<Style>.Shared.Return(this);
    }

    public void Copy(ref Style from) {
        if (from == null || from.m_union.Equals(default)) return;
        m_union = from.m_union;
    }
}

/// <summary>
/// Simple union for store <see cref="Style"/> values without generic.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal struct StyleUnion: IEquatable<StyleUnion> {
    [FieldOffset(0)] private float m_floating = .0f;
    [FieldOffset(0)] private int m_integer    = 0;

    [FieldOffset(0)] private TextDecoration m_flag = TextDecoration.NONE;
    [FieldOffset(0)] private RGB? m_color = null!;

    [FieldOffset(0)] private char m_char = '\0';
    [FieldOffset(8)] private readonly StyleDataType m_tag = StyleDataType.COLOR;

    /// <summary>
    /// Delimiter tag of the <see cref="StyleUnion"/>.
    /// </summary>
    public readonly StyleDataType Tag { get => m_tag; }

    /// <summary>
    /// Create a new <see cref="StyleUnion"/>.
    /// </summary>
    /// <param name="tag">Tag of the <see cref="StyleUnion"/> instance.</param>
    public StyleUnion(StyleDataType tag) => m_tag = tag;

    public char Character { readonly get => m_char; set => m_char = value; }

    public int INumber { readonly get => m_integer; set => m_integer = value; }

    public float FNumber { readonly get => m_floating; set => m_floating = value; }

    public RGB? Color { readonly get => m_color; set => m_color = value; }

    public TextDecoration Flag { readonly get => m_flag; set => m_flag = value; }

    public bool Equals(StyleUnion union) {
        return m_tag == union.m_tag && m_tag switch {
            StyleDataType.COLOR => m_color.Equals(union.m_color),
            StyleDataType.CHAR => m_char == union.m_char,

            StyleDataType.FONT_ATTR => m_flag == union.m_flag,
            StyleDataType.NUMERIC_I => m_integer == union.m_integer,
            _ => false
        };
    }
}

public enum StyleDataType: byte {
    NUMERIC_F,
    NUMERIC_I,
    CHAR,
    COLOR,
    FONT_ATTR
}
