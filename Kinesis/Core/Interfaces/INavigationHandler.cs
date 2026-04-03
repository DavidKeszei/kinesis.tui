using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides function to preparing class to a navigation action.
/// </summary>
public interface INavigationHandler {

    /// <summary>
    /// Handler function, when a navigation action was applied.
    /// </summary>
    /// <param name="isBack">Indicates motion of the navigation.</param>
    public void OnNavigation(bool isBack);
}
