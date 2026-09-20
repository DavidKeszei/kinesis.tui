using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Processing;

/// <summary>
/// Provides properties to act like a job.
/// </summary>
internal interface IJob<TMessage> where TMessage: IJobMessage {

    /// <summary>
    /// Indicates the current job can be processed.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Attached callback to the current job.
    /// </summary>
    public Action<TMessage> Callback { get; }

    /// <summary>
    /// State of the current job.
    /// </summary>
    public State<JobRequestIntent> State { get; }
}
