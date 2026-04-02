using Kinesis.Input.Windows;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Input;

/// <summary>
/// Provides low-latency input resolving on a platform.
/// </summary>
internal interface IInputBackend {

    /// <summary>
    /// Represent a error value for <see cref="IInputBackend"/> instances.
    /// </summary>
    public const IInputBackend ERR = null;

    /// <summary>
    /// Indicates has any buffer on the input.
    /// </summary>
    public bool HasInput { get; }

    /// <summary>
    /// Read the current buffer from the input.
    /// </summary>
    /// <returns>Return the key and the modifiers as <see cref="InputInfo"/>.</returns>
    public bool ReadInput(out InputInfo input);
}