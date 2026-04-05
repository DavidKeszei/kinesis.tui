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
public abstract class RenderComponent: Component, IStaticType {
    private const string TYPE_NAME = "RenderComponent";

    protected readonly Dictionary<StyleTag, Style> m_cache = null!;
    protected int m_entityVersion = 0;

    /// <summary>
    /// Name of the <see cref="RenderComponent"/>.
    /// </summary>
    public static string Name { get => TYPE_NAME; }

    /// <summary>
    /// Version of the entity, which targeting the current <see cref="RenderComponent"/>.
    /// </summary>
    internal int EntityVersion { get => m_entityVersion; set => m_entityVersion = value; }

    protected RenderComponent(): base(id: ComponentRegistry.QueryComponent(TYPE_NAME)) 
        => m_cache = new Dictionary<StyleTag, Style>(capacity: 8);

    /// <summary>
    /// Render the component to the screen.
    /// </summary>
    /// <param name="buffer">Portion of the screen buffer.</param>
    /// <param name="styles">Styles of the renderer.</param>
    internal abstract void Render(in Canvas buffer, int version, StyleEnumerator styles);

    /// <summary>
    /// Cache the required <see cref="Style"/>s.
    /// </summary>
    /// <param name="styles">Non-filtered <see cref="Style"/>s.</param>
    protected abstract void CacheStyles(StyleEnumerator styles);
}