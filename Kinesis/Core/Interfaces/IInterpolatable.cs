using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

public interface IInterpolatable<T> {

    public static abstract T Lerp(T from, T to, float time);
}
