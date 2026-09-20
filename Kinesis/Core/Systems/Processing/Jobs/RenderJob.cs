using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Processing;

/// <summary>
/// Represents a job at rendering.
/// </summary>
/// <param name="Root">Owner of the job.</param>
/// <param name="Callback">Attached method to the job.</param>
/// <param name="State">State information of the job.</param>
internal sealed record RenderJob(Island Root, Action<RenderMessage> Callback, State<JobRequestIntent> State): IJob<RenderMessage> {

    /// <summary>
    /// Indicates the current <see cref="RenderJob"/> instance.
    /// </summary>
    public bool IsActive { get => Root.IsActive; }
}