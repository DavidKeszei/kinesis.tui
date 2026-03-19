using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Input.Windows;

[StructLayout(layoutKind: LayoutKind.Explicit)]
internal readonly struct InputKeyEventInfo {
    [MarshalAs(unmanagedType: UnmanagedType.Bool), FieldOffset(offset: 0)] private readonly int m_pressed = 0;
    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(offset: 4)] private readonly ushort m_repeatCount = 0; //Added a name, but we not use it.

    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(offset: 6)] private readonly ushort m_vKeyCode = 0;
    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(offset: 8)] private readonly ushort m_vKeyCodeHardware = 0; //Added a name, but we not use it.

    [MarshalAs(unmanagedType: UnmanagedType.U1), FieldOffset(offset: 10)] private readonly byte m_ascii = 0;
    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(offset: 10)] private readonly ushort m_unicode = 0;

    [MarshalAs(unmanagedType: UnmanagedType.U4), FieldOffset(offset: 12)] private readonly uint m_controlState = 0; //Added a name, but we not use it.

    public ushort VirtualKeyCode { get => m_vKeyCode; }

    public char Value { get => (char)m_unicode; }

    public bool IsPressed { get => m_pressed > 0; }

    public InputKeyEventInfo() { }
}
