using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Base class of component-like object.
/// </summary>
public abstract class Component {
    private readonly int m_id = -1;

    protected Component(int id) => m_id = id;

    /// <summary>
    /// Check if the component static type name is equal with <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type name of the component.</param>
    /// <returns>Return <see langword="true"/>, if the <paramref name="type"/> is equal with the underlying name. Otherwise return <see langword="false"/>.</returns>
    public bool TypeOf(string type) {
        if (string.IsNullOrEmpty(type)) return false;
        return m_id == ComponentRegistry.QueryComponent(name: type);
    }
}
