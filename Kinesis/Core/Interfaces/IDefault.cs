using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides default checking for a object.
/// </summary>
/// <typeparam name="TSelf">The class itself.</typeparam>
public interface IDefault<TSelf> {

    /// <summary>
    /// Check if the <paramref name="instance"/> is default by the method implementation.
    /// </summary>
    /// <param name="instance">Target of this method as <typeparamref name="TSelf"/>.</param>
    /// <returns>Returns <see langword="true"/>, if the <paramref name="instance"/> is default. Otherwise returns <see langword="false"/>.</returns>
    public abstract static bool IsDefault(TSelf instance);
}
