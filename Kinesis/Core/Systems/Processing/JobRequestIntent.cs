using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

public enum JobRequestIntent {
    ACTIVE,
    SUSPEND,
    REMOVE
}
