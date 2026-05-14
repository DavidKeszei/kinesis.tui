using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Provides property for defines space requirements based on ratios.
/// </summary>
/// <typeparam name="T">Type of the ratio container. This indicator of the axis count (1D, 2D).</typeparam>
public interface IAdaptiveLayout<T>: IContentable<List<Entity>> where T: IEnumerable<uint> {

    /// <summary>
    /// Indicates the space ratio of the elements.
    /// </summary>
    /// <remarks>
    /// This instance divide the given space from the parent based on this.
    /// </remarks>
    public T Ratios { set; }
}
