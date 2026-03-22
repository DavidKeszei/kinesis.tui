using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Processing;

public enum WorkTag : byte {
    INPUT,
    RENDERING,
    LAYOUT
}