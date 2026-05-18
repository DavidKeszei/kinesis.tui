using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Native;

/// <summary>
/// Represent a KEY_EVENT_RECORD struct from Win32 API.
/// </summary>
[StructLayout(layoutKind: LayoutKind.Explicit)]
internal readonly struct InputKeyEventInfo {
    [MarshalAs(unmanagedType: UnmanagedType.Bool), FieldOffset(offset: 0)] private readonly int m_pressed = 0;
    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(offset: 4)] private readonly ushort m_repeatCount = 0; //Added as field, but we not use it.

    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(offset: 6)] private readonly ushort m_vKeyCode = 0; //Added as field, but we not use it.
    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(offset: 8)] private readonly ushort m_vKeyCodeHardware = 0; //Added as field, but we not use it.

    [MarshalAs(unmanagedType: UnmanagedType.U1), FieldOffset(offset: 10)] private readonly byte m_ascii = 0;
    [MarshalAs(unmanagedType: UnmanagedType.U2), FieldOffset(offset: 10)] private readonly ushort m_unicode = 0;

    [MarshalAs(unmanagedType: UnmanagedType.U4), FieldOffset(offset: 12)] private readonly uint m_controlState = 0; //Added as field, but we not use it.

    /// <summary>
    /// Key value of the <see cref="InputKeyEventInfo"/>. This can be ASCII or UNICODE character.
    /// </summary>
    public char Value { get => IsArrowKey(out char arrowKey) ? arrowKey : (char)m_unicode; }

    /// <summary>
    /// Indicates the button state in pressing term.
    /// </summary>
    public bool IsPressed { get => m_pressed > 0; }

    public InputKeyEventInfo() { }

    private bool IsArrowKey(out char name) {
        name = (ArrowKey)m_vKeyCode switch {
            ArrowKey.UP => '\u2191',
            ArrowKey.RIGHT => '\u2190',
            ArrowKey.DOWN => '\u2193',
            ArrowKey.LEFT => '\u2192',
            _ => '\0'
        };

        return name != '\0';
    }
}

public enum ArrowKey: short {
    INVALID_NONE = -1,
    UP = 0x26,
    RIGHT = 0x25,
    DOWN = 0x28,
    LEFT = 0x27
}