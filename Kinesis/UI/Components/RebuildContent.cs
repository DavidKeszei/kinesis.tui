using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

/// <summary>
/// Represents a content-container for rebuilding <see cref="Island"/> instances.
/// </summary>
public sealed class RebuildContent(): Component(id: ComponentRegistry.QueryComponent(name: TYPE)), IStaticType, IPoolable {
    private const string TYPE = nameof(RebuildContent);

    private Entity m_content = null!;
    private bool m_hasChanged = false;

    public static string TypeName { get => TYPE; }

    /// <summary>
    /// Current content of the <see cref="RebuildContent"/>
    /// </summary>
    /// <remarks>
    /// <b>Remarks:</b> Calling this property setting a <see langword="true"/>/<see langword="false"/> value to <see cref="HasChange"/> property implicitly. 
    ///                 (Get: <see langword="false"/>, Set: <see langword="true"/>)
    /// </remarks>
    public Entity Content {
        get {
            m_hasChanged = false;
            return m_content;
        }
        set {
            m_content = value;
            m_hasChanged = true;
        }
    }

    /// <summary>
    /// Indicates something changed to the <see cref="Content"/> property.
    /// </summary>
    public bool HasChange { get => m_hasChanged; set => m_hasChanged = true; }

    public RebuildContent(Entity content): this() => m_content = content;

    public void Reset() {
        m_content = null!;
        ComponentPool<RebuildContent>.Shared.Return(this);
    }
}
