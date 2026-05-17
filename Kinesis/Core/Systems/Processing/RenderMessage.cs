using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Represent current state of the rendering.
/// </summary>
public readonly record struct RenderMessage: IWorkMessage {
    private readonly float m_delta = .0f;
    private readonly int m_fps = 0;

    private readonly Vec2 m_scale = Vec2.Zero;

    /// <summary>
    /// Elapsed time from the previous frame.
    /// </summary>
    public float DeltaTime { get => m_delta; }

    /// <summary>
    /// Current frame per second value.
    /// </summary>
    public int FPS { get => m_fps; }

    /// <summary>
    /// Current scale of the rendering.
    /// </summary>
    public Vec2 Scale { get => m_scale; }

    /// <summary>
    /// Target callback type of the message.
    /// </summary>
    public static JobTag Target { get => JobTag.RENDERING; }

    public RenderMessage(float deltaTime, int fps, Vec2 scale) {
        m_delta = deltaTime;
        m_fps = fps;

        m_scale = scale;
    }
}
