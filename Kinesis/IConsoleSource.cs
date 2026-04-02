using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis;

/// <summary>
/// Provides function for read specific console information as <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Type of the information.</typeparam>
internal interface IConsoleSource<T> {

    /// <summary>
    /// Read console information as <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">Container of the result.</param>
    /// <returns>Return <see langword="true"/>, if the read was successful. Otherwise return <see langword="false"/>.</returns>
    public bool Read(out T? result);
}
