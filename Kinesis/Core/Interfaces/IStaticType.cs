using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides static "type-detection" for an object.
/// </summary>
public interface IStaticType {

    /// <summary>
    /// Name of the "type".
    /// </summary>
    public abstract static string TypeName { get; }
}
