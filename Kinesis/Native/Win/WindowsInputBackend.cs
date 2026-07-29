using Kinesis.Core;
using Kinesis.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Kinesis.Native;

/// <summary>
/// Represent a standard input on the Windows platform. 
/// </summary>
internal partial class WindowsInputBackend: IInputBackend {
    #region DEFINES

    private const string KERNEL32_LIB = "kernel32.dll";
    private const string USER32_LIB = "user32.dll";

    private const uint MANUAL_PROCESSING = 0x0001;
    private const string DEDICATED_THREAD_NAME = "Input->Native";

    #endregion
    #region P/INVOKE

    [LibraryImport(libraryName: KERNEL32_LIB, EntryPoint = "SetConsoleMode")]
    [return: MarshalAs(unmanagedType: UnmanagedType.Bool)]
    private static partial bool SetMode(nint handle, uint flags);

    [LibraryImport(libraryName: USER32_LIB, EntryPoint = "GetAsyncKeyState")]
    private static partial short GetKeyState(int modifier);

    //[LibraryImport(libraryName: KERNEL32_LIB, EntryPoint = "ReadConsoleInputW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    //[return: MarshalAs(unmanagedType: UnmanagedType.Bool)]
    //private static partial bool ReadConsole(nint hnd, ref WindowsConsoleEventMsg buffer, uint length, out uint _);

    #endregion

    private readonly RingBuffer<InputInfo> m_infoBuffer = null!;
    private IConsoleSource<InputKeyEventInfo> m_source = null!;

    private InputInfo m_lastInput = default;
    private bool m_isPressed = false;

    /// <summary>
    /// Indicates the user pressed/pressing some key currently.
    /// </summary>
    public bool HasInput { get => m_infoBuffer.Count > 0; }

    public WindowsInputBackend()
        => m_infoBuffer = new RingBuffer<InputInfo>(capacity: 64);

    /// <summary>
    /// Create new <see cref="WindowsInputBackend"/> instance.
    /// </summary>
    /// <returns>Return a fresh <see cref="WindowsInputBackend"/> instance. If something goes wrong, then return <see cref="IInputBackend.ERR"/>.</returns>
    public static WindowsInputBackend Init(IConsoleSource<InputKeyEventInfo> source) {
        WindowsInputBackend backend = new WindowsInputBackend();
        if(!SetMode(handle: StdHandle.Input, flags: MANUAL_PROCESSING))
            return null!;

        backend.m_source = source;
        return backend;
    }

    public bool ReadInput(out InputInfo input) {
        input = default;
        Span<int> modifiersCodes = stackalloc int[5] {
            0xA0,
            0xA1,
            0xA2,
            0xA3,
            0x12,
        };

        char character = '\0';
        bool isPressed = false;

        InputModifier modifiers = QueryModifiers(modifiersCodes);
        InputKeyEventInfo info = default!;

        bool success = m_source.Read(out info) && info.IsPressed != m_isPressed;

        if (success) {
            character = info.Value;
            isPressed = m_isPressed = info.IsPressed;

            m_lastInput = input = new InputInfo(modifiers, info.Value, info.IsPressed);
            return true;
        }

        // If the read was unsuccessfull, but we don't detected any key up, then we think the user holding the specific key
        if (m_isPressed) {
            input = m_lastInput;
            return true;
        }

        m_lastInput = default;
        return false;
    }

    private InputModifier QueryModifiers(ReadOnlySpan<int> modifiersCodes) {
        InputModifier modifiers = InputModifier.NONE;

        for (byte i = 0; i < modifiersCodes.Length; ++i) {
            if (GetKeyState(modifiersCodes[i]) < 0)
                modifiers |= (InputModifier)modifiersCodes[i];
        }

        return modifiers;
    }
}