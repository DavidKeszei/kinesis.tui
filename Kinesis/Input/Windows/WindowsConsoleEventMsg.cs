using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Input.Windows;

/// <summary>
/// Represents a INPUT_RECORD struct from the Win32 API.
/// </summary>
[StructLayout(layoutKind: LayoutKind.Explicit)]
internal readonly struct WindowsConsoleEventMsg {
    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(0)] readonly WindowsConsoleMsgTag m_tag = WindowsConsoleMsgTag.INPUT;
    [FieldOffset(4)] readonly InputKeyEventInfo m_inputInfo = default;

    /// <summary>
    /// Delimiter
    /// </summary>
    public WindowsConsoleMsgTag Tag { get => m_tag; }

    /// <summary>
    /// Captured input information by the <see cref="WindowsInputBackend"/>.
    /// </summary>
    public InputKeyEventInfo KeyInfo { get => m_inputInfo; }

    public WindowsConsoleEventMsg(WindowsConsoleMsgTag tag) {
        switch (tag) {
            case WindowsConsoleMsgTag.INPUT:
                m_inputInfo = new InputKeyEventInfo();
                break;
            case WindowsConsoleMsgTag.RESIZE:
               
                break;
        }
    }
}
