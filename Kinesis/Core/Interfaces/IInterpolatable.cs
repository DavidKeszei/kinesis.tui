using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

public interface IInterpolatable<TSelf> {

    public static abstract TSelf Lerp(TSelf from, TSelf to, float time);
}
