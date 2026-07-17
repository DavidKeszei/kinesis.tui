using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Possible character values from the arrow keys. 
/// </summary>
public enum ArrowKey : short {
    /// <summary>
    /// Indicates the underlying character is not arrow key.
    /// </summary>
    INVALID_NONE = -1,
    /// <summary>
    /// Up direction on the arrow key pad. Key as character: ↑.
    /// </summary>
    UP = 0x26,
    /// <summary>
    /// Right direction on the arrow key pad. Key as character: →.
    /// </summary>
    RIGHT = 0x25,
    /// <summary>
    /// Down direction on the arrow key pad. Key as character: ↓.
    /// </summary>
    DOWN = 0x28,
    /// <summary>
    /// Left direction on the arrow key pad. Key as character: ←.
    /// </summary>
    LEFT = 0x27
}
