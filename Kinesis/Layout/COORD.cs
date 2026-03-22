using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Layout;

/// <summary>
/// Represent a COORD structure in the Win32 API.
/// </summary>
[StructLayout(layoutKind: LayoutKind.Sequential)]
internal readonly struct COORD {
    private readonly short m_x = 0;
    private readonly short m_y = 0;

    public short X { get => m_x; }

    public short Y { get => m_y; }

    public COORD(short x, short y) {
        m_x = x;
        m_y = y;
    }
}
