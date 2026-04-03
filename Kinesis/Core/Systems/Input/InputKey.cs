using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

[Flags]
public enum InputModifier: int {
    NONE,
    L_SHIFT = 0xA0,
    R_SHIFT = 0xA1,
    L_CTRL  = 0xA2,
    R_CTRL  = 0xA3,
    ALT     = 0x12 
}