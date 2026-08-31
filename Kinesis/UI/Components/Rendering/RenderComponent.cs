using Kinesis.Core;
using Kinesis.UI;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Represent a helper component in the rendering.
/// </summary>
public abstract class RenderComponent: Component, IStaticType, IPoolable {
    #region DEFINES
    private const string TYPE_NAME = nameof(RenderComponent);
    #endregion

    protected readonly Dictionary<string, Style> m_cache = null!;
    protected int m_entityVersion = 0;

    /// <summary>
    /// Name of the <see cref="RenderComponent"/> type.
    /// </summary>
    public static string TypeName { get => TYPE_NAME; }

    /// <summary>
    /// Version of the entity, which targeting the current <see cref="RenderComponent"/>.
    /// </summary>
    internal int EntityVersion { get => m_entityVersion; set => m_entityVersion = value; }

    protected RenderComponent(): base(id: ComponentRegistry.QueryComponent(TYPE_NAME)) 
        => m_cache = new Dictionary<string, Style>(capacity: 8);

    public virtual void Reset() {
        m_cache.Clear();
        m_entityVersion = 0;
    }

    /// <summary>
    /// Render the component to the screen.
    /// </summary>
    /// <param name="buffer">Portion of the screen buffer.</param>
    /// <param name="version">Current version number of the <see cref="Entity"/>.</param>
    /// <param name="styles">Decoration of the renderer.</param>
    internal protected abstract void Render(in Canvas buffer, int version, StyleEnumerator styles);

    /// <summary>
    /// Cache the required <see cref="Style"/>s.
    /// </summary>
    /// <param name="styles">Non-filtered <see cref="Style"/>s.</param>
    protected abstract void CacheStyles(StyleEnumerator styles);
}