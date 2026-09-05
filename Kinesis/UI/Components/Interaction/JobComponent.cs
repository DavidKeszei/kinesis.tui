using Kinesis.Core;
using Kinesis.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a interactive component on an <see cref="Entity"/>.
/// </summary>
public sealed class JobComponent(): Component(id: ComponentRegistry.QueryComponent(TYPE_NAME)), IStaticType, IPoolable {
    private const string TYPE_NAME = nameof(JobComponent);

    private State<JobRequestIntent> m_status = null!;

    /// <summary>
    /// Name of the <see cref="JobComponent"/>.
    /// </summary>
    public static string TypeName { get => TYPE_NAME; }

    /// <summary>
    /// Current status of the <see cref="JobComponent"/>.
    /// </summary>
    public JobRequestIntent Status { get => m_status; }

    /// <summary>
    /// Create a new <see cref="JobComponent"/>, which fires every input.
    /// </summary>
    /// <param name="onInput">Callback for the inputs.</param>
    /// <param name="island">Root island instance of the callback.</param>
    /// <param name="focusBased">Indcates the current callback globally listening input messages or not.</param>
    public JobComponent(Action<InputMessage> onInput, Island island, bool focusBased): this()
        => m_status = JobSystem.Current.AddCallback(onInput, island, focusBased);

    /// <summary>
    /// Create a new <see cref="JobComponent"/>, which fires every render frame ends.
    /// </summary>
    /// <param name="onRender">Callback for the end of the frame.</param>
    /// <param name="island">Root island instance of the callback.</param>
    public JobComponent(Action<RenderMessage> onRender, Island island): this()
        => m_status = JobSystem.Current.AddCallback(onRender, island, false);

    /// <summary>
    /// Request a intent to the <see cref="JobSystem"/>.
    /// </summary>
    public void Request(JobRequestIntent status) {
        if (status == m_status.Value) return;
        m_status.Value = status;
    }

    public void Reset() {
        Request(JobRequestIntent.REMOVE);
        m_status = null!;

        ComponentPool<JobComponent>.Shared.Return(this);
    }
}