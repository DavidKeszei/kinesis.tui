using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Enumeration of supported character styles.
/// </summary>
[Flags]
public enum StyleFlag: short {
    NONE = (1 << 0),
    /// <summary>
    /// Indicates the character is bold.
    /// </summary>
    BOLD = (1 << 1),
    /// <summary>
    /// Indicates the character is italic.
    /// </summary>
    ITALIC = (1 << 2),
    /// <summary>
    /// Indicates the character has underline.
    /// </summary>
    UNDERLINE = (1 << 4),
    /// <summary>
    /// Indicates the character space is blinking slowly.
    /// </summary>
    BLINK_SLOW = (1 << 5),
    /// <summary>
    /// Indicates the character space is blinking fast.
    /// </summary>
    BLINK_FAST = (1 << 6),
    /// <summary>
    /// Swap the foreground and background colors.
    /// </summary>
    INVERSE = (1 << 7),
    /// <summary>
    /// Hide the current character.
    /// </summary>
    HIDDEN = (1 << 8),
    /// <summary>
    /// Draw a stroke through the character. (Most old terminals doesn't supporting)
    /// </summary>
    STROKE_THROUGH = (1 << 9),
    DOUBLE_UNDERLINE = (1 << 10),
    OVERLINE = (1 << 11)
}
