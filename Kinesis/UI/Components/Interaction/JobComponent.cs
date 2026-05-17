using Kinesis.Core;
using Kinesis.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represent a interactive component on an <see cref="Entity"/>.
/// </summary>
public class JobComponent: Component, IStaticType {
    private const string TYPE_NAME = "Interaction";

    /// <summary>
    /// Name of the <see cref="JobComponent"/>.
    /// </summary>
    public static string Name { get => TYPE_NAME; }

    /// <summary>
    /// Create a new <see cref="JobComponent"/>, which fires every input.
    /// </summary>
    /// <param name="onInput">Callback for the inputs.</param>
    public JobComponent(Action<InputMessage> onInput, Island island): base(id: ComponentRegistry.QueryComponent(TYPE_NAME))
        => JobSystem.Current.AddCallback(work: onInput, island);

    /// <summary>
    /// Create a new <see cref="JobComponent"/>, which fires every render frame ends.
    /// </summary>
    /// <param name="onRender">Callback for the end of the frame.</param>
    public JobComponent(Action<RenderMessage> onRender, Island island): base(id: ComponentRegistry.QueryComponent(TYPE_NAME))
        => JobSystem.Current.AddCallback(work: onRender, island);

    /// <summary>
    /// Create a new <see cref="JobComponent"/>, which fires every layout change
    /// </summary>
    /// <param name="onLayoutChange">Handler callback, when the layout change occurs.</param>
    public JobComponent(Action<LayoutMessage> onLayoutChange, Island island): base(id: ComponentRegistry.QueryComponent(TYPE_NAME))
        => JobSystem.Current.AddCallback(work: onLayoutChange, island);
}
