using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Intrinsics;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Represent a <see cref="RGB"/> color on the screen.
/// </summary>
[StructLayout(layoutKind: LayoutKind.Explicit)]
public struct RGB: IEquatable<RGB>, IInterpolatable<RGB, RGB, RGB> {
    [FieldOffset(offset: 0)] private readonly uint m_color = 0x00;
    [FieldOffset(offset: 0)] private byte m_red = 0x0;

    [FieldOffset(offset: 1)] private byte m_green = 0x0;
    [FieldOffset(offset: 2)] private byte m_blue = 0x0;

    [FieldOffset(offset: 3)] private byte m_alpha = 0x0;

    public static implicit operator RGB(uint color) => new RGB(color);

    #region PREDEFINES

    public static RGB White { get => new RGB(r: 255, g: 255, b: 255, a: 255); }

    public static RGB Black { get => new RGB(r: 0, g: 0, b: 0, a: 255); }

    public static RGB Purple { get => new RGB(r: 128, g: 0, b: 128, a: 255); }

    public static RGB Blue { get => new RGB(color: 0x0000FFFF); }

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

    public RGB(uint color) {
        unsafe {
            if (BitConverter.IsLittleEndian) {
                byte* asArray = (byte*)(void*)&color;

                m_red = asArray[3];
                m_green = asArray[2];

                m_blue = asArray[1];
                m_alpha = asArray[0];
                return;
            }
        }

        m_color = color;
    }

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
        if (time <= 0) return left;
        if (time >= 1) return right;

        if (Vector.IsHardwareAccelerated) {
            Vector4 simdLeft = new Vector4(x: left.m_red, y: left.m_green, z: left.m_blue, w: left.m_alpha);
            Vector4 simdRight = new Vector4(x: right.m_red, y: right.m_green, z: right.m_blue, w: right.m_alpha);

            Vector4 lerp = Vector4.Lerp(simdLeft, simdRight, time);
            return new RGB((byte)lerp.X, (byte)lerp.Y, (byte)lerp.Z, (byte)lerp.W);
        }

        float r = left.R + (right.R - left.R) * time;
        float g = left.G + (right.G - left.G) * time;

        float b = left.B + (right.B - left.B) * time;
        float a = left.A + (right.A - left.A) * time;

        return new RGB((byte)r, (byte)g, (byte)b, (byte)a);
    }

    /// <summary>
    /// Blend two <see cref="RGB"/> instance based on thier alpha values.
    /// </summary>
    /// <param name="top">Top/Left of the <see cref="RGB"/> value.</param>
    /// <param name="bottom">Bottom/Rigth of the parameters.</param>
    /// <returns>Returns a <see cref="RGB"/> instance, which represents blend of the two <see cref="RGB"/> instances.</returns>
    public static RGB Blend(RGB top, RGB bottom) {
        if (top.A == byte.MaxValue) return top;
        if (top.A == byte.MinValue) return bottom;

        float topNormalizedAlpha = top.m_alpha / 255f;
        float ratio = 1f -  topNormalizedAlpha;

        /* Use SIMD-feature, if we can */
        if (Vector.IsHardwareAccelerated) {
            Vector4 topSIMD = new Vector4(top.m_red, top.m_green, top.m_blue, top.m_alpha);
            Vector4 bottomSIMD = new Vector4(bottom.m_red, bottom.m_green, bottom.m_blue, bottom.m_alpha);

            topSIMD *= topNormalizedAlpha;
            bottomSIMD *= ratio;

            topSIMD = Vector4.Clamp(topSIMD + bottomSIMD, min: Vector4.Zero, max: Vector4.One * byte.MaxValue);
            return new RGB((byte)topSIMD.X, (byte)topSIMD.Y, (byte)topSIMD.Z, 255);
        }

        byte r = (byte)float.Clamp((top.m_red * topNormalizedAlpha) + (bottom.m_red * ratio), byte.MinValue, byte.MaxValue);
        byte g = (byte)float.Clamp((top.m_green * topNormalizedAlpha) + (bottom.m_green * ratio), byte.MinValue, byte.MaxValue);
        byte b = (byte)float.Clamp((top.m_blue * topNormalizedAlpha) + (bottom.m_blue * ratio), byte.MinValue, byte.MaxValue);

        return new RGB(r, g, b, a: 255);
    }

    public static void Gradient(RGB from, RGB to, Span<RGB> output) {
        if (output.Length < 2) return;

        float ratio = 1f / output.Length;
        int index = 0;

        for (int i = 1; i < output.Length; ++i)
            output[index++] = Lerp(from, to, i * ratio);

        output[0]  = from;
        output[^1] = to;
    }
}
