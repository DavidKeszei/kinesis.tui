using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Utils;

/// <summary>
/// Helper class for contains most used ANSI commands.
/// </summary>
internal static class AnsiCommand {

    public static string RESET_STYLES { get; } = "\e[0m";

    public static string CLEAR_BUFFER { get; } = "\e[2J";

    public static string HOME { get; } = "\e[H";

    public static string CLEAR_SAVED_LINES { get; } = "\e[3J";
}
