using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Input.Windows;

internal enum WindowsConsoleMsgTag: ushort {
    INPUT = 0x0001,
    RESIZE = 0x0004
}
