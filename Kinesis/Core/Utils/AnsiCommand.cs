using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Utils;

/// <summary>
/// Helper class for contains most used ANSI commands.
/// </summary>
internal static class AnsiCommand {

    /// <summary>
    /// Indicates to the terminal emulator for reset all styles.
    /// </summary>
    public static string ResetStyles { get; } = "\e[0m";

    /// <summary>
    /// Clear the buffer screen of the user see.
    /// </summary>
    public static string ClearBuffers { get; } = "\e[2J";

    /// <summary>
    /// Return the cursor to the home position. (0;0)
    /// </summary>
    public static string Home { get; } = "\e[H";

    /// <summary>
    /// Clear saved/scroll/history buffer of the terminal emulator.
    /// </summary>
    public static string ClearSavedLines { get; } = "\e[3J";

    /// <summary>
    /// Disable line wrapping by the terminal emulator.
    /// </summary>
    public static string WrapDisable { get; } = "\e[?7l";

    /// <summary>
    /// Request a empty, non-scrollable screen from the emulator.
    /// </summary>
    public static string EnableAlternateBuffering { get; } = "\e[?1049h";

    /// <summary>
    /// Indicates to the terminal emulator for not rendering the current buffer to the screen.
    /// </summary>
    public static string StartBufferLoad { get; } = "\e[?2026h";

    /// <summary>
    /// Indicates to the terminal emulator for rendering the current buffer to the screen.
    /// </summary>
    public static string EndBufferLoad { get; } = "\e[?2026l";
}
