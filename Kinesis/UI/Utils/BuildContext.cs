using System;
using System.Collections.Generic;
using System.Text;
using Kinesis.Core;

namespace Kinesis.UI;

/// <summary>
/// Represent a state in the Ui building process.
/// </summary>
public ref struct BuildContext {
    private readonly Island m_root = null!;
    private readonly Entity m_current = null!;

    /// <summary>
    /// Current target entity of the building.
    /// </summary>
    public readonly Entity Current { get => m_current; internal init => m_current = value; }

    /// <summary>
    /// Root <see cref="Island"/> of the building.
    /// </summary>
    public readonly Island Root { get => m_root; internal init => m_root = value; }

    internal BuildContext(Entity current) => m_current = current;
}
