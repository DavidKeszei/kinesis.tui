using Kinesis.Core;
using Kinesis.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a interactive component on an <see cref="Entity"/>.
/// </summary>
public class JobComponent : Component, IStaticType {
    private const string TYPE_NAME = nameof(JobComponent);
    private readonly State<JobRequestIntent> m_status = null!;

    /// <summary>
    /// Name of the <see cref="JobComponent"/>.
    /// </summary>
    public static string Name { get => TYPE_NAME; }

    /// <summary>
    /// Current status of the <see cref="JobComponent"/>.
    /// </summary>
    public JobRequestIntent Status { get => m_status; }

    /// <summary>
    /// Create a new <see cref="JobComponent"/>, which fires every input.
    /// </summary>
    /// <param name="onInput">Callback for the inputs.</param>
    public JobComponent(Action<InputMessage> onInput, Island island) : base(id: ComponentRegistry.QueryComponent(TYPE_NAME))
        => m_status = JobSystem.Current.AddCallback(work: onInput, island);

    /// <summary>
    /// Create a new <see cref="JobComponent"/>, which fires every render frame ends.
    /// </summary>
    /// <param name="onRender">Callback for the end of the frame.</param>
    public JobComponent(Action<RenderMessage> onRender, Island island) : base(id: ComponentRegistry.QueryComponent(TYPE_NAME))
        => m_status = JobSystem.Current.AddCallback(work: onRender, island);

    /// <summary>
    /// Create a new <see cref="JobComponent"/>, which fires every layout change
    /// </summary>
    /// <param name="onLayoutChange">Handler callback, when the layout change occurs.</param>
    public JobComponent(Action<LayoutMessage> onLayoutChange, Island island) : base(id: ComponentRegistry.QueryComponent(TYPE_NAME))
        => m_status = JobSystem.Current.AddCallback(work: onLayoutChange, island);

    /// <summary>
    /// Requets remove from the <see cref="JobSystem"/>.
    /// </summary>
    public void Request(JobRequestIntent status) {
        if (status == m_status.Value) return;
        m_status.Value = status;
    }
}