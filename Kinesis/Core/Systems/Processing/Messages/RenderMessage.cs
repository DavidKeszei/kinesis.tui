using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// Represent current state of the rendering.
/// </summary>
public readonly record struct RenderMessage: IJobMessage {
    private readonly float m_frameCount = 0;
    private readonly float m_delta = .0f;

    private readonly int m_fps = 0;

    /*   [Remarks]
     *   If this field became the first field of the struct, that explodes the memory to the roof (10MB => 19MB; +90-100% to memory pressure).
     *   I guess this problems starts from the layout of the Vec2 struct, but I'm not sure about it.
     *
     *   So DON'T move this to the first line of the fields.
     */
    private readonly Vec2 m_scale = Vec2.Zero; 

    /// <summary>
    /// Elapsed time from the previous frame.
    /// </summary>
    public readonly float DeltaTime { get => m_delta; }

    /// <summary>
    /// Current frame per second value.
    /// </summary>
    public readonly int FPS { get => m_fps; }

    /// <summary>
    /// Current scale of the rendering.
    /// </summary>
    public readonly Vec2 Scale { get => m_scale; }

    /// <summary>
    /// Count of the generated frames.
    /// </summary>
    public readonly float FrameCount { get => m_frameCount; }

    /// <summary>
    /// Target callback type of the message.
    /// </summary>
    public static JobTag Target { get => JobTag.RENDERING; }

    public RenderMessage(float deltaTime, int fps, Vec2 scale, float frameCount) {
        m_delta = deltaTime;
        m_fps = fps;

        m_scale = scale;
        m_frameCount = frameCount;
    }
}
