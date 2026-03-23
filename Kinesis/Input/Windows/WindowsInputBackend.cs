using Kinesis.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Kinesis.Input.Windows;

/// <summary>
/// Represent a standard input on the Windows platform. 
/// </summary>
internal partial class WindowsInputBackend: IInputBackend {
    #region CONSTANTS

    private const string KERNEL32_LIB = "kernel32.dll";
    private const string USER32_LIB = "user32.dll";

    private const nint INVALID_HND = -1;
    private const uint MANUAL_PROCESSING = 0x0001;

    private const uint STD_IN = uint.MaxValue - 10 + 1;
    private const string DEDICATED_THREAD_NAME = "Input::Native Thread";

    #endregion

    #region NATIVE_IMPL

    [LibraryImport(libraryName: KERNEL32_LIB, EntryPoint = "GetStdHandle")]
    private static partial nint GetStandardHandle(uint type);

    [LibraryImport(libraryName: KERNEL32_LIB, EntryPoint = "SetConsoleMode")]
    [return: MarshalAs(unmanagedType: UnmanagedType.Bool)]
    private static partial bool SetMode(nint handle, uint flags);

    [LibraryImport(libraryName: USER32_LIB, EntryPoint = "GetAsyncKeyState")]
    private static partial short GetKeyState(int modifier);

    [LibraryImport(libraryName: KERNEL32_LIB, EntryPoint = "ReadConsoleInputW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(unmanagedType: UnmanagedType.Bool)]
    private static partial bool ReadConsole(nint hnd, ref WindowsConsoleEventMsg buffer, uint length, out uint _);

    #endregion

    private readonly CircularBuffer<InputInfo> m_infoBuffer = null!;
    private Task m_rawInputTask = null!;

    private nint m_handle = nint.Zero;
    private WindowsConsoleEventMsg m_msg = new WindowsConsoleEventMsg(tag: WindowsConsoleMsgTag.INPUT);

    /// <summary>
    /// Indicates the user pressed/pressing some key currently.
    /// </summary>
    public bool HasInput { get => m_infoBuffer.Count > 0; }

    public WindowsInputBackend()
        => m_infoBuffer = new CircularBuffer<InputInfo>(capacity: 64);

    /// <summary>
    /// Create new <see cref="WindowsInputBackend"/> instance.
    /// </summary>
    /// <returns>Return a fresh <see cref="WindowsInputBackend"/> instance. If something goes wrong, then return <see cref="IInputBackend.ERR"/>.</returns>
    public static IInputBackend Init() {
        WindowsInputBackend backend = new WindowsInputBackend();
        backend.m_handle = GetStandardHandle(STD_IN);

        if(backend.m_handle == INVALID_HND) return IInputBackend.ERR;
        if(!SetMode(handle: backend.m_handle, flags: MANUAL_PROCESSING))
            return IInputBackend.ERR;

        backend.m_rawInputTask = Task.Run(() => {
            Thread.CurrentThread.Name = DEDICATED_THREAD_NAME;

            while(true) {
                if(backend.Read(out char character, out InputModifier modifiers, out bool isPressed))
                    backend.m_infoBuffer.Write(value: new InputInfo(character, modifiers, isPressed));
            }
        });

        return backend;
    }

    public InputInfo ReadInput() {
        _ = m_infoBuffer.Read(out InputInfo info);
        return info;
    }

    /// <summary>
    /// Read one key from the console input.
    /// </summary>
    /// <returns>If anything in the input, return <see langword="true"/>. Otherwise return <see langword="false"/>.</returns>
    private bool Read(out char character, out InputModifier modifiers, out bool isPressed) {
        Span<int> modifiersCodes = stackalloc int[5] {
            0xA0,
            0xA1,
            0xA2,
            0xA3,
            0x12,
        };

        character = '\0';
        modifiers = InputModifier.NONE;
        isPressed = false;

        bool success = ReadConsole(hnd: m_handle, buffer: ref m_msg, length: (uint)Unsafe.SizeOf<WindowsConsoleEventMsg>(), out uint _) && m_msg.Tag == WindowsConsoleMsgTag.INPUT;

        if (success) {
            character = m_msg.KeyInfo.Value;
            isPressed = m_msg.KeyInfo.IsPressed;

            for (byte i = 0; i < modifiersCodes.Length; ++i) {
                if (GetKeyState(modifiersCodes[i]) < 0)
                    modifiers |= (InputModifier)modifiersCodes[i];
            }
        }

        return success;
    }
}