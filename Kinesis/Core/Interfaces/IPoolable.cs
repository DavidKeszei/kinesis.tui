using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides reset behavior for pooling.
/// </summary>
public interface IPoolable {

    /// <summary>
    /// Reset the component to default state (like <i><b>new T()</b></i>). This method used by the <see cref="Entity"/> class, to reset
    /// and return components to the <see cref="Core.ComponentPool{T}"/>.
    /// </summary>
    /// <remarks>
    /// <b>Example</b>: <br/><br/>
    /// <code>
    /// 
    /// public override void Reset() {
    ///     
    ///     //Before this call, you do some resetting logic, if this behavior required.
    ///     //After that you can return the actual instance to the pool. If not pooled, then method do nothing.
    ///     //
    ///     //The T parameter the current component type of the method.
    ///     ComponentPool{T}.Instance.Return(component: this)
    /// }
    /// 
    /// </code>
    /// </remarks>
    public void Reset();
}
