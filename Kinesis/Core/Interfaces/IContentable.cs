using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides attach mechanism for a class to an another class.
/// </summary>
/// <typeparam name="T">Type of the attachable content.</typeparam>
public interface IContentable<T> {

    /// <summary>
    /// Attached content of the current instance.
    /// </summary>
    public T Content { set; }
}