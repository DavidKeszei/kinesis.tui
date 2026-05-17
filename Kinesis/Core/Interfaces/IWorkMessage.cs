using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides target recognition for a message somewhere.
/// </summary>
public interface IWorkMessage {

    /// <summary>
    /// Target group of the message.
    /// </summary>
    public abstract static JobTag Target { get; }
}
