using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

public interface ICopyable<TFrom> where TFrom : allows ref struct {

    public void Copy(TFrom from);
}

public interface IImmutableCopyable<TSelf> where TSelf : allows ref struct {

    public TSelf Copy();
}
