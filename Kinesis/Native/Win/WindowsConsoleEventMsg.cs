using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Native;

/// <summary>
/// Represents a INPUT_RECORD struct from the Win32 API.
/// </summary>
[StructLayout(layoutKind: LayoutKind.Explicit)]
internal readonly struct WindowsConsoleEventMsg {
    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(0)] readonly WindowsConsoleMsgTag m_tag = WindowsConsoleMsgTag.INPUT;
    [FieldOffset(4)] readonly InputKeyEventInfo m_inputInfo = default;
    [FieldOffset(4)] readonly ConsoleScaleInfo m_windowScale = default;

    /// <summary>
    /// Delimiter
    /// </summary>
    public WindowsConsoleMsgTag Tag { get => m_tag; }

    /// <summary>
    /// Captured input information by the <see cref="WindowsInputBackend"/>.
    /// </summary>
    public InputKeyEventInfo KeyInfo { get => m_inputInfo; }

    /// <summary>
    /// Current scale of the console window.
    /// </summary>
    public ConsoleScaleInfo ConsoleWindowScale { get => m_windowScale; }

    public WindowsConsoleEventMsg(WindowsConsoleMsgTag tag) {
        switch (tag) {
            case WindowsConsoleMsgTag.INPUT:
                m_inputInfo = new InputKeyEventInfo();
                break;
            case WindowsConsoleMsgTag.LAYOUT:
                m_windowScale = new ConsoleScaleInfo(x: 0, y: 0);
                break;
        }
    }
}
