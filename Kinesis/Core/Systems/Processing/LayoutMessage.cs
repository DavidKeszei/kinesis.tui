using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Represent a message from the current state of global layout parameters.
/// </summary>
public readonly struct LayoutMessage: IWorkMessage {
    private readonly Vec2 m_layoutGlobalScale = Vec2.Zero;

    public static WorkTag Target { get => WorkTag.LAYOUT; }

    /// <summary>
    /// Scale of the current console window.
    /// </summary>
    public Vec2 Scale { get => m_layoutGlobalScale; }

    /// <summary>
    /// Create new <see cref="LayoutMessage"/> from <paramref name="scale"/>.
    /// </summary>
    /// <param name="scale">Global scale of console window.</param>
    internal LayoutMessage(Vec2 scale) => m_layoutGlobalScale = scale; 
}
