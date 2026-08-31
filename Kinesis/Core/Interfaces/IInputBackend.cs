using Kinesis.Native;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides low-latency input resolving on a platform.
/// </summary>
internal interface IInputBackend {

    /// <summary>
    /// Represent a error value for <see cref="IInputBackend"/> implementers.
    /// </summary>
    public const IInputBackend ERR = null!;

    /// <summary>
    /// Reads an input from the native implementation of the input handling.
    /// </summary>
    /// <param name="input">Output/Destination variable of the input.</param>
    /// <returns>Returns <see langword="true"/>, if any input can be read by the caller side, otherwise returns <see langword="false"/>.</returns>
    public bool ReadInput(out InputInfo input);
}