using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis;

/// <summary>
/// Represent a simple registry for <see cref="Component"/> instances.
/// </summary>
internal static class ComponentRegistry {
    private static readonly Dictionary<string, int> m_registeredComponents = null!;

    static ComponentRegistry()
        => m_registeredComponents = new Dictionary<string, int>();

    /// <summary>
    /// Register component type to the <see cref="ComponentRegistry"/>.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="name">Name of the component. (Mostly <see cref="IStaticType.Name"/>)</param>
    /// <returns>Return <see langword="true"/>, if component registration was successful. Otherwise return <see langword="false"/>.</returns>
    internal static bool RegisterComponent<T>(string name) where T: Component 
        => m_registeredComponents.TryAdd(key: name, value: m_registeredComponents.Count);
    
    /// <summary>
    /// Query the registered assigned integer id based on the <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the component type. (Mostly: <see cref="IStaticType.Name"/>)</param>
    /// <returns>Return a non-negative number as <see cref="int"/>.</returns>
    internal static int QueryComponent(string name) {
        if (m_registeredComponents.TryGetValue(name, out int id))
            return id;

        return -1;
    }
}
