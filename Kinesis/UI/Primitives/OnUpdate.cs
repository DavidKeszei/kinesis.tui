using Kinesis.Core;
using Kinesis.Utils;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinesis.UI;

public delegate void JobCallback<T>(T message, ref IslandEntityVisitor visitor) where T: IJobMessage;

/// <summary>
/// Interacts, when a frame was rendered.
/// </summary>
public class OnUpdate<T>: Entity, IContentable<Entity> where T: IJobMessage {
    private readonly Island m_island = null!;

    /// <summary>
    /// Job of the current <see cref="OnUpdate{T}"/> instance, which ran by the <see cref="JobSystem"/>.
    /// </summary>
    public JobCallback<T> On {
        init {
            JobComponent? interaction = base.Get<JobComponent>();

            if (interaction == null) {
                interaction = T.Target switch {
                    JobTag.RENDERING => new JobComponent(onRender: (message) => SetCallback(value, Unsafe.As<RenderMessage, T>(ref message)), m_island),
                    JobTag.INPUT => new JobComponent(onInput: (message) => SetCallback(value, Unsafe.As<InputMessage, T>(ref message)), m_island),
                    JobTag.LAYOUT => new JobComponent(onLayoutChange: (message) => SetCallback(value, Unsafe.As<LayoutMessage, T>(ref message)), m_island),
                    _ => null!
                };
                _ = base.Attach<JobComponent>(interaction, isUnique: true);
            }
        }
    }

    /// <summary>
    /// Current request of the <see cref="OnUpdate{T}"/>.
    /// </summary>
    public JobRequestIntent Status { get => Get<JobComponent>()!.Status; }

    /// <summary>
    /// Attached child of the <see cref="OnUpdate{T}"/>.
    /// </summary>
    public Entity Content {
        init {
            if (value == null) return;

            Hierarchy connection = base.Get<Hierarchy>(index: Hierarchy.ChildrenStart)!;
            connection!.Attached = value;

            value.Get<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
        }
    }

    public OnUpdate(BuildContext context) {
        _ = base.Attach<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.UP });
        _ = base.Attach<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.DOWN });

        this.m_island = context.Root;
    }

    /// <summary>
    /// Indicates change intent to the <see cref="JobSystem"/> state of the <see cref="JobComponent"/>.
    /// </summary>
    /// <remarks>
    /// <b>Remark:</b> This call effect is delayed and inreversable; so if the request was sent to the <see cref="JobSystem"/>,
    ///                then this system take act to this job in the "next" round based on the <paramref name="request"/>.
    /// </remarks>
    /// <param name="request">Requested intent to the <see cref="JobSystem"/>.</param>
    public void Request(JobRequestIntent request) => Get<JobComponent>()!.Request(request);

    private void SetCallback(JobCallback<T> func, T message) {
        if (func == null) return;

        IslandEntityVisitor visitor = new IslandEntityVisitor(pivot: this);
        func(message, ref visitor);
        visitor.ClearCache();
    }
}