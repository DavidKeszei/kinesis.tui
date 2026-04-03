using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides functionality for query <see cref="ISystem"/> instances.
/// </summary>
public interface ISystemProvider {

    /// <summary>
    /// Get specific <see cref="ISystem"/> instance as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Implementer type of the <see cref="ISystem"/> interface.</typeparam>
    /// <returns>Return a <typeparamref name="T"/> instance.</returns>
    public T? GetSystem<T>() where T: class, ISystem;
}
