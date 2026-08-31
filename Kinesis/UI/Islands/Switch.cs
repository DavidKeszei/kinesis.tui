using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Kinesis.UI;

/// <summary>
/// Represents a simple state-machine for rendering different states with different UI elements.
/// </summary>
/// <typeparam name="TEntity">The renderable entity as <typeparam name="TEntity"/></typeparam>
/// <typeparam name="TData">The state information of current <see cref="Switch{TEntity,TData}"/>.</typeparam>
public sealed class Switch<TEntity, TData>: Island, ICopyable<BuildContext> where TEntity: notnull, Entity {
    private const string ERR_PROTOTYPE = "[Error] The Switch.Prototype property should be set to non-null value.";
    private const string ERR_ONCHANGE  = "[Error] The Switch.OnChange property should be set to non-null value.";
    
    private readonly string m_toggleIndicatorName  = null!;
    private readonly Func<TEntity> m_prototype = null!;
    
    private readonly Action<TEntity, TData> m_onChange = null!;
    private readonly TData[] m_stateInfos = null!;
    
    private int m_preStateIndex = 0;
    private int m_stateIndex = 0;
    
    /// <summary>
    /// Represents a template callback for create UI elements.
    /// </summary>
    public Func<TEntity> Prototype {
        init {
            if (value != null) {
                m_prototype = value;
                
                Get<RebuildContent>()!.Content = new Viewport {
                    Name = m_toggleIndicatorName,
                    Content = m_prototype()
                };
            }
        }
    }
    
    /// <summary>
    /// Represents a callback function, which runs at every value change.
    /// </summary>
    public Action<TEntity, TData> OnChange { init => m_onChange = value; }
    
    /// <summary>
    /// All possible state of the current <see cref="Switch{TEntity,TData}"/> instance.
    /// </summary>
    public TData[] Content { init => m_stateInfos = value; }
    
    /// <summary>
    /// Current active state of the <see cref="Switch{TEntity,TData}"/> instance.
    /// </summary>
    public TData Value { get => m_stateInfos[m_stateIndex]; }

    /// <summary>
    /// Create a new <see cref="Switch{TEntity,TData}"/> instance.
    /// </summary>
    public Switch() {
        m_toggleIndicatorName = $"__toggle_{Guid.CreateVersion7()}__";

        _ = Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>());
        _ = Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static(scale) => scale.Value = Vec2.Auto with { Y = 1 }));
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Scale>(this);
        context.SetPivot<Position>(this);
    }
    
    /// <summary>
    /// Move the internal state to the next possible state.
    /// </summary>
    public void Next()
        => m_stateIndex = ++m_stateIndex % m_stateInfos.Length;

    protected override Entity? Build(ref readonly BuildContext context) {
        if (m_prototype == null) return new Text { Content = ERR_PROTOTYPE, Foreground = RGB.Red, Decoration = TextDecoration.UNDERLINE};
        if (m_onChange == null)  return new Text { Content = ERR_ONCHANGE, Foreground = RGB.Red,  Decoration = TextDecoration.UNDERLINE };
        
        return new Viewport {
            Content = new OnUpdate<RenderMessage>(context) {
                Pivot = this,
                On = (_, ref readonly tree) => {
                    if (m_stateIndex != m_preStateIndex) {
                        Get<RebuildContent>()?.Content = new Viewport {
                            Name = m_toggleIndicatorName,
                            Content = m_prototype()
                        };
                        Rebuild();
                        m_preStateIndex = m_stateIndex;
                    }
                    
                    TEntity indicator = (TEntity)tree.Visit<Viewport>(name: m_toggleIndicatorName)!
                                                     .Get<Hierarchy>(Hierarchy.ChildrenStart)!
                                                     .Attached!;

                    if (indicator.Get<Scale>() == null) return;
                    
                    m_onChange(indicator, m_stateInfos[m_stateIndex]);
                    float width = indicator.Get<Scale>()!.Value.X;

                    Get<Scale>()!.Value = new Vec2(x: width, y: 1);
                    indicator.ClipRenderScale();
                },
                Content = Get<RebuildContent>()?.Content ?? null!
            }
        };
    }
}