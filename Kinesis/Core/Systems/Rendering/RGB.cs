using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Represent a <see cref="RGB"/> color on the screen.
/// </summary>
[StructLayout(layoutKind: LayoutKind.Explicit)]
public struct RGB: IEquatable<RGB> {
    [FieldOffset(offset: 0)] private readonly uint m_color = 0x00;
    [FieldOffset(offset: 0)] private byte m_red = 0x0;

    [FieldOffset(offset: 1)] private byte m_green = 0x0;
    [FieldOffset(offset: 2)] private byte m_blue = 0x0;

    [FieldOffset(offset: 3)] private byte m_alpha = 0x0;

    public static implicit operator RGB(uint color) => new RGB(color);

    #region STATICS

    public static RGB White { get => new RGB(r: 255, g: 255, b: 255, a: 255); }

    public static RGB Black { get => new RGB(r: 0, g: 0, b: 0, a: 255); }

    public static RGB Purple { get => new RGB(r: 128, g: 0, b: 128, a: 255); }

    public static RGB Yellow { get => new RGB(r: 255, g: 255, b: 0, a: 255); }

    public static RGB Green { get => new RGB(r: 0, g: 255, b: 0, a: 255); }

    public static RGB Red { get => new RGB(r: 255, g: 0, b: 0, a: 255); }

    public static RGB Transparent { get => new RGB(color: 0x0); }

    #endregion

    public byte R { readonly get => m_red; set => m_red = value; }

    public byte G { readonly get => m_green; set => m_green = value; }

    public byte B { readonly get => m_blue; set => m_blue = value; }

    public byte A { readonly get => m_alpha; set => m_alpha = value; }

    public RGB(byte r, byte g, byte b, byte a = 0) {
        m_red = r;
        m_green = g;

        m_blue = b;
        m_alpha = a;
    }

    public RGB(uint color) => m_color = color;

    public readonly bool Equals(RGB rgb)
        => rgb.m_red == m_red && rgb.m_green == m_green && 
           rgb.m_blue == m_blue && m_alpha == rgb.m_alpha;

    /// <summary>
    /// Generate random <see cref="RGB"/> value between the given colors..
    /// </summary>
    /// <returns>Return a <see cref="RGB"/> value.</returns>
    public static RGB Random(RGB min, RGB max) => Lerp(min, max, time: System.Random.Shared.NextSingle());

    /// <summary>
    /// Interpolate between to <see cref="RGB"/> values.
    /// </summary>
    /// <param name="left">Start value of the interpolation.</param>
    /// <param name="right">End of value of the interpolation.</param>
    /// <param name="time">Interpolation between the two <see cref="RGB"/> values.</param>
    /// <returns>Return a new <see cref="RGB"/> between <paramref name="left"/> and <paramref name="right"/> based on the <paramref name="time"/>.</returns>
    public static RGB Lerp(RGB left, RGB right, float time) {
        float r = left.R + (right.R - left.R) * time;
        float g = left.G + (right.G - left.G) * time;

        float b = left.B + (right.B - left.B) * time;
        float a = left.A + (right.A - left.A) * time;

        return new RGB((byte)r, (byte)g, (byte)b, (byte)a);
    }

    public static RGB Blend(RGB top, RGB bottom) {
        if (top.A == byte.MaxValue) return top;
        if (top.A == byte.MinValue) return bottom;

        float ratio = 1f - (top.m_alpha / 255f);

        byte r = (byte)float.Clamp((top.m_red * (top.m_alpha / 255f)) + (bottom.m_red * ratio), byte.MinValue, byte.MaxValue);
        byte g = (byte)float.Clamp((top.m_green * (top.m_alpha / 255f)) + (bottom.m_green * ratio), byte.MinValue, byte.MaxValue);
        byte b = (byte)float.Clamp((top.m_blue * (top.m_alpha / 255f)) + (bottom.m_blue * ratio), byte.MinValue, byte.MaxValue);

        return new RGB(r, g, b, a: 255);
    }
}
