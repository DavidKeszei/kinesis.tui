using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

public enum JobTag : byte {
    INPUT,
    RENDERING,
    LAYOUT
}