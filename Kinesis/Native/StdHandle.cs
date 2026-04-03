using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Native;

/// <summary>
/// Represent a native handle to standard stream.
/// </summary>
internal readonly partial struct StdHandle {
    #region DEFINES
    private const int  UNIX_STDIN_FILENO = 0;
    private const int  UNIX_STDOUT_FILENO = 1;

    private const int  UNIX_STDERR_FILENO = 2;
    private const uint WIN_STDIN = uint.MaxValue - 10 + 1;

    private const uint WIN_STDOUT = uint.MaxValue - 11 + 1;
    private const uint WIN_STDERR = uint.MaxValue - 12 + 1;
    #endregion

    #region P/INVOKE WIN32
    [LibraryImport(libraryName: "kernel32.dll", EntryPoint = "GetStdHandle")]
    private static partial nint GetHandle(uint type);
    #endregion

    private static StdHandle? m_in = default;
    private static StdHandle? m_out = default;
    private static StdHandle? m_err = default;

    /// <summary>
    /// Implicit cast to <see cref="nint"/> and <see cref="StdHandle"/> structs.
    /// </summary>
    /// <param name="handle">Target handle.</param>
    public static implicit operator nint(StdHandle handle) => handle.m_handle;

    /// <summary>
    /// Standard input handle of the console.
    /// </summary>
    public static StdHandle Input { get => m_in ??= new StdHandle(type: StdType.INPUT); }

    /// <summary>
    /// Standard output handle of the console.
    /// </summary>
    public static StdHandle Output { get => m_out ??= new StdHandle(type: StdType.OUTPUT); }

    /// <summary>
    /// Standard error handle of the console.
    /// </summary>
    public static StdHandle Error { get => m_err ??= new StdHandle(type: StdType.ERROR); }

    private readonly nint m_handle = -1;
    private readonly StdType m_typeOf = StdType.INPUT;

    public StdHandle(StdType type) {
        if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows)) {
            uint _type = type switch {
                StdType.INPUT  => WIN_STDIN,
                StdType.OUTPUT => WIN_STDOUT,
                StdType.ERROR  => WIN_STDERR,
                _ => 0
            };

            if (_type == 0) return;
            else {
                m_typeOf = type;
                m_handle = GetHandle(_type);
            }

            return;
        }

        /* If we are on Linux or any Unix like system */
        m_typeOf = type;
        m_handle = type switch {
            StdType.INPUT => UNIX_STDIN_FILENO,
            StdType.OUTPUT => UNIX_STDOUT_FILENO,
            StdType.ERROR => UNIX_STDERR_FILENO,
            _ => -1
        };
    }
}

internal enum StdType: sbyte {
    NONE  = -1,
    INPUT  = 0,
    OUTPUT = 1,
    ERROR  = 2
}