using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Represent a callback, which controls the update logic of the <see cref="State{T}"/> instance.
/// </summary>
/// <typeparam name="T">Type of the target.</typeparam>
/// <param name="current">Current reference of the value.</param>
/// <param name="to">Update value of the current.</param>
public delegate void StateUpdate<T>(ref T current, T to);

/// <summary>
/// Represent a state of <typeparamref name="T"/> value.
/// </summary>
/// <typeparam name="T">Target of the state.</typeparam>
public abstract class State<T> {
    private readonly StateUpdate<T> m_update = null!;
    private T m_state = default!;

    /// <summary>
    /// Current value of the <see cref="State{T}"/> as <typeparamref name="T"/>.
    /// </summary>
    public T Value { get => m_state; set => m_update(ref m_state, value); }

    /// <summary>
    /// Implicit conversion from <see cref="State{T}"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="state">The <see cref="State{T}"/> itself.</param>
    public static implicit operator T(State<T> state) => state.Value;

    protected State(StateUpdate<T> update, T @default = default!) {
        m_update = update;
        m_state = @default;
    }
}

/// <summary>
/// Represent a state holder, which only allows struct/value-type.
/// </summary>
/// <typeparam name="T">Value-type of the state.</typeparam>
/// <param name="update">Update logic of state.</param>
public class ValueState<T>(StateUpdate<T> update = default!, T @default = default): State<T>(update ?? (static(ref from, to) => from = to), @default) where T: struct;


/// <summary>
/// Represent a state holder, which only allows class/reference-type.
/// </summary>
/// <typeparam name="T">Reference-type of the state.</typeparam>
/// <param name="update">Update logic of state.</param>
public class RefState<T>(StateUpdate<T> update = default!, T @default = null!): State<T>(update ?? (static (ref from, to) => from = to), @default) where T: class;
