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
            JobComponent? interaction = base.GetComponent<JobComponent>();

            if (interaction == null) {
                interaction = T.Target switch {
                    JobTag.RENDERING => new JobComponent(onRender: (message) => SetCallback(value, Unsafe.As<RenderMessage, T>(ref message)), m_island),
                    JobTag.INPUT => new JobComponent(onInput: (message) => SetCallback(value, Unsafe.As<InputMessage, T>(ref message)), m_island),
                    JobTag.LAYOUT => new JobComponent(onLayoutChange: (message) => SetCallback(value, Unsafe.As<LayoutMessage, T>(ref message)), m_island),
                    _ => null!
                };
                base.AttachComponent<JobComponent>(interaction, isUnique: true);
            }
        }
    }

    /// <summary>
    /// Attached child of the <see cref="OnUpdate{T}"/>.
    /// </summary>
    public Entity Content {
        init {
            if (value == null) return;

            Hierarchy connection = base.GetComponent<Hierarchy>(index: Hierarchy.ChildrenStart)!;
            connection!.Attached = value;

            value.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
        }
    }

    public OnUpdate(BuildContext context) {
        _ = base.AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.UP });
        _ = base.AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDirection.DOWN });


        this.m_island = context.Root;
    }

    private void SetCallback(JobCallback<T> func, T message) {
        if (func == null) return;

        IslandEntityVisitor visitor = new IslandEntityVisitor(pivot: this);
        func(message, ref visitor);
        visitor.ClearCache();
    }
}