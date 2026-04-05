using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides runnable entry point for a <see cref="ISystem"/>.
/// </summary>
public interface ISystem {
    
    /// <summary>
    /// Indicates behavior the <see cref="ISystem"/> instance.
    /// </summary>
    public SystemBehavior Behavior { get; }
}

public interface IDynamicSystem<T>: ISystem where T: allows ref struct {

    /// <summary>
    /// Run the current <see cref="IDynamicSystem{T}"/> instance.
    /// </summary>
    /// <returns>Return a(n) <typeparamref name="T"/> value.</returns>
    public T Run();
}

public interface IDynamicSystem<TReturn, TParameter> : ISystem where TReturn : allows ref struct where TParameter : allows ref struct {

    /// <summary>
    /// Run the current <see cref="IDynamicSystem{T}"/> instance.
    /// </summary>
    /// <returns>Return a(n) <typeparamref name="T"/> value.</returns>
    public TReturn Run(TParameter parameter);
}

public interface IDynamicSystem: ISystem {
    /// <summary>
    /// Run the current <see cref="IDynamicSystem"/> instance.
    /// </summary>
    public void Run();
}

/// <summary>
/// Indicates, when a <see cref="ISystem"/> instance runs.
/// </summary>
public enum SystemInvocationTime: byte {
    ON_BEGIN,
    ON_END,
    ON_CALL
}

public enum SystemBehavior: byte {
    DYNAMIC,
    STATIC
}

internal readonly record struct SystemInvocationInfo(Func<ISystemProvider, ISystem> Creation, ISystem? System, SystemInvocationTime When);