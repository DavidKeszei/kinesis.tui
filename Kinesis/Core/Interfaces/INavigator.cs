using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides navigation between <see cref="Island"/> instances.
/// </summary>
public interface INavigator: ISystem {

    /// <summary>
    /// Navigate to a <see cref="Island"/> based on the <paramref name="method"/>.
    /// </summary>
    /// <param name="method">Navigation method of the <see cref="Island"/>.</param>
    public void NavigateTo(Func<ISystemProvider, Island> method);

    /// <summary>
    /// Navigate to a <see cref="Island"/> based on the <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the registered <see cref="Island"/>.</param>
    public void NavigateTo(string name);

    /// <summary>
    /// Navigate back to the previous <see cref="Island"/>. 
    /// </summary>
    public void NavigateBack();
}