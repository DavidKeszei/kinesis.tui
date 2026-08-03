using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents a simple container for primitive, numeric values.
/// </summary>
/// <typeparam name="T">The numeric value of the container.</typeparam>
public readonly struct AnimatedNumber<T>: IInterpolatable<AnimatedNumber<T>, AnimatedNumber<T>, AnimatedNumber<T>> where T: struct, INumber<T> {
    private readonly T m_value = default;

    public static implicit operator T(AnimatedNumber<T> number) => number.Value;

    public static implicit operator AnimatedNumber<T>(T number) => new AnimatedNumber<T>(number);

    /// <summary>
    /// Current value of the container.
    /// </summary>
    public readonly T Value { get => m_value; }

    public AnimatedNumber(T value) => m_value = value;

    public static AnimatedNumber<T> Lerp(AnimatedNumber<T> from, AnimatedNumber<T> to, float time)
        => (from.m_value + T.CreateSaturating<float>(float.CreateSaturating<T>(to.m_value - from.m_value) * time));
}