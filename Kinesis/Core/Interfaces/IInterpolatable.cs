using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

public interface IInterpolatable<TFrom, TTo, TResult> {

    public static abstract TResult Lerp(TFrom from, TTo to, float time);
}