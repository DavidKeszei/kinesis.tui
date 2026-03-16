using Kinesis.Input;
using Kinesis.Processing;
using Kinesis.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Interacts, when a frame was rendered.
/// </summary>
public class OnUpdate<T>: Entity where T: IWorkMessage {
    private readonly Island m_island = null!;

    /// <summary>
    /// Callback, when a frame was rendered.
    /// </summary>
    public Action<T, PageEntityVisitor> On {
        set {
            InteractionComponent? interaction = base.GetComponent<InteractionComponent>();

            if (interaction == null) {
                interaction = T.Target switch {
                    WorkTag.RENDERING => new InteractionComponent(onRender: (message) => SetCallback(value, Unsafe.As<RenderMessage, T>(ref message)), m_island),
                    WorkTag.INPUT => new InteractionComponent(onInput: (message) => SetCallback(value, Unsafe.As<InputMessage, T>(ref message)), m_island),
                    _ => null!
                };
                base.AttachComponent<InteractionComponent>(interaction, isUnique: true);
            }
        }
    }

    /// <summary>
    /// Attached child of the <see cref="OnUpdate{T}"/>.
    /// </summary>
    public Entity Child {
        set {
            Hierarchy connection = base.GetComponent<Hierarchy>(index: 1)!;
            connection!.Attached = value;

            value.GetComponent<Hierarchy>(index: Hierarchy.Parent)!.Attached = this;
        }
    }

    public OnUpdate(Island island) {
        base.AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDir.UP });
        base.AttachComponent<Hierarchy>(component: new Hierarchy() { Direction = ConnectionDir.DOWN });

        this.m_island = island;
    }

    private void SetCallback(Action<T, PageEntityVisitor> func, T message) {
        if (func == null) return;
        func(message, new PageEntityVisitor(this));
    }
}