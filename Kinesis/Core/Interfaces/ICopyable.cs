using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides method for copy data from a(n) <typeparamref name="TFrom"/> object.
/// </summary>
/// <typeparam name="TFrom">Type of the copy source.</typeparam>
public interface ICopyable<TFrom> where TFrom: allows ref struct {

    /// <summary>
    /// Copy the data from a object.
    /// </summary>
    /// <param name="from">Holder of the copy values.</param>
    public void Copy(ref TFrom from);
}