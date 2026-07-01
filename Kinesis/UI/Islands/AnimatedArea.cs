using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents an area/conatiner, which holds target of the animation.
/// </summary>
/// <typeparam name="T">Target type of the animation on the <see cref="Entity"/>.</typeparam>
public class AnimatedArea<T>: Island, IContentable<Entity> where T: notnull, IInterpolatable<T> {
    private string s_box = null!;

    private readonly Func<Entity, T> m_selector = default!;
    private readonly Action<Entity, T> m_applier = default!;

    private T m_start  = default!;
    private T m_target = default!;

    private T m_current = default!;
    private long m_startTimeStamp = 0;

    private long m_duration = TimeSpan.FromSeconds(seconds: 1).Ticks;
    private AnimationState m_state = AnimationState.Animate;

    private bool m_isFirstSet = true;
    private bool m_isPeriodic = false;

    /// <summary>
    /// Selector function, which helps querying the specific value.
    /// </summary>
    public Func<Entity, T> Selector { init => m_selector = value; }

    /// <summary>
    /// Applier/InheritStyle function, which applying the animated value back to the <see cref="Entity"/>.
    /// </summary>
    public Action<Entity, T> Applier { init => m_applier = value; }

    /// <summary>
    /// Duration of the animation.
    /// </summary>
    public TimeSpan Duration { init => m_duration = value.Ticks; }

    /// <summary>
    /// Target value of the animation at the end.
    /// </summary>
    public T To { set => m_target = value; }

    /// <summary>
    /// State of the animation.
    /// </summary>
    public AnimationState State { get => m_state; }

    /// <summary>
    /// Indicates the animation run periodicly; always.
    /// </summary>
    public bool IsPeriodic { init => m_isPeriodic = value; }

    public Entity Content {
        set {
            if (value == null) return;
            Viewport box = new Viewport { Name = (s_box ??= $"__{nameof(AnimatedArea<>)}__{Guid.CreateVersion7()}__"), Content = value };

            box.Get<Hierarchy>(Hierarchy.Parent)!.Attached = this;
            this.Get<Hierarchy>(Hierarchy.ChildrenStart)!.Attached = box;

            Get<ContentComponent>()!.Content = box;

            /*
             * The scale contstraints is different here: the AnimatedArea<TSelf> is not animate a value on the parent;
             * the class animates the given content on the given scale/area. 
             */
            Scale? scale = null!;
            if ((scale = value.Get<Scale>()) != null) {
                _ = Attach<Scale>(scale, isUnique: true);
            }

            Rebuild();
        }
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        return new OnUpdate<RenderMessage>(context) {
            On = (message, ref readonly tree) => {
                if (m_state != AnimationState.Animate) return;
                Entity content = tree.Visit<Viewport>(name: s_box)?
                                     .Get<Hierarchy>(Hierarchy.ChildrenStart) ?? null!;

                if(content == null) return;
                long currentTimeStamp = Stopwatch.GetTimestamp();

                if (m_startTimeStamp == 0) m_startTimeStamp = currentTimeStamp;
                if (m_isFirstSet) {
                    m_current = m_selector(content);
                    m_isFirstSet = false;
                }
                
                float time = (currentTimeStamp - m_startTimeStamp) / (float)m_duration;
                T interpolated =  T.Lerp(from: m_current, to: m_target, time);

                m_applier(content, interpolated);

                if (m_isPeriodic && time >= 1f) {
                    Reset();
                    Start();

                    return;
                }

                if (m_state == AnimationState.Animate && time >= 1f)
                    m_state = AnimationState.End;
            },
            Content = Get<ContentComponent>()!.Content ?? null!
        };
    }

    public void Reset() {
        m_current = m_start;
        m_startTimeStamp = 0;

        m_state = AnimationState.Begin;
    }

    public void Start() => m_state = AnimationState.Animate;
}

/// <summary>
/// Represents a simple container for primitive, numeric values.
/// </summary>
/// <typeparam name="T">The numeric value of the container.</typeparam>
public readonly struct AnimatedNumber<T>: IInterpolatable<AnimatedNumber<T>> where T: struct, INumber<T> {
    private readonly T m_value = default;

    public static implicit operator T(AnimatedNumber<T> number) => number.Value;

    public static implicit operator AnimatedNumber<T>(T number) => new AnimatedNumber<T>(number);

    /// <summary>
    /// Current value of the container.
    /// </summary>
    public readonly T Value { get => m_value; }

    public AnimatedNumber(T value) => m_value = value;

    public static AnimatedNumber<T> Lerp(AnimatedNumber<T> from, AnimatedNumber<T> to, float time) 
        => (from.m_value + T.CreateSaturating<float>(float.CreateSaturating<T>(to.m_value - from.m_value) * time));
}

public enum AnimationState: byte {
    Begin,
    Animate,
    End
}