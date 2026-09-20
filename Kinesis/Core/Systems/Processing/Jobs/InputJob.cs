using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Processing;

/// <summary>
/// Represents a job target for handling  <see cref="InputMessage"/>s.
/// </summary>
/// <param name="Root">Owner of the job.</param>
/// <param name="Callback">Attached method to the job.</param>
/// <param name="State">State information of the job.</param>
/// <param name="IsGlobal">Indicates the current job can be called without focus managment.</param>
internal sealed record InputJob(Island Root, Action<InputMessage> Callback, State<JobRequestIntent> State, bool IsGlobal): IJob<InputMessage> {
    private bool m_isFocused = false;

    /// <summary>
    /// Indicates the current job is focused. If <see cref="IsGlobal"/> equal with <see langword="true"/>, then this property is ignored.
    /// </summary>
    public bool IsFocused { get => m_isFocused; set => m_isFocused = value; }

    /// <summary>
    /// Indicates the current job can be processed.
    /// </summary>
    public bool IsActive { get => Root.IsActive && (IsGlobal || m_isFocused); }
}
