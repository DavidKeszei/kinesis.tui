using Kinesis.Core.Utils;
using Kinesis.UI;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinesis.Core.Rendering;

/// <summary>
/// Represent a IMGUI/immediate-mode based render-engine.
/// </summary>
internal sealed class ImmediateRenderer {
    #region PREDEFINES
    private const float NS_TO_MS = 1000f;
    private const float LIMIT = 1f;

    private const float FPS_CONVERT = 1000f;
    private const int STRING_BUILDER_STACK_SPACE = 16_384;
    #endregion

    private ConsoleBuffer m_backbuffer = default;
    private ConsoleBuffer m_frontbuffer = default;

    private readonly State<LayoutInfo> m_layoutState = null!;
    private readonly State<JobSystemStateInfo> m_workState = null!;

    private readonly StreamWriter m_out = null!;
    private float m_delta = .0f;

    /// <summary>
    /// Current frame generation time in ms.
    /// </summary>
    public float Time { get => m_delta / FPS_CONVERT; }

    /// <summary>
    /// Current fps count of the renderer.
    /// </summary>
    public float FPS { get => FPS_CONVERT / m_delta; }

    public ImmediateRenderer(State<JobSystemStateInfo> workState, State<LayoutInfo> layoutState) {
        m_workState = workState;
        m_layoutState = layoutState;

        m_backbuffer = new ConsoleBuffer(x: (int)layoutState.Value.Scale.X, (int)layoutState.Value.Scale.Y);
        m_frontbuffer = new ConsoleBuffer(x: (int)layoutState.Value.Scale.X, (int)layoutState.Value.Scale.Y);

        m_out = new StreamWriter(stream: Console.OpenStandardOutput(), encoding: Encoding.UTF8) {
            AutoFlush = false
        };

        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
    }
        
    public void Run(List<Entity> list) {
        long start = Stopwatch.GetTimestamp();

        if (m_workState.Value.State == WorkerSystemState.WAIT_FOR_RENDERER) {
            bool fullRedrawRequested = OnLayoutChange();

            for (int i = 0; i < list.Count; ++i) {

                /*
                 * TODO(2026-05-17T01:41): Save these to Vec2 instances for eliminate not reqiured calculations & gain performance.
                 */
                Scale scale = list[i].GetComponent<Scale>()!;
                Position position = list[i].GetComponent<Position>()!;

                if (!InBuffer(position: position.Absolute, scale: scale.Value)) 
                    continue;

                RenderComponent renderLogic = list[i].GetComponent<RenderComponent>()!;
                Canvas canvas = ConsoleBuffer.Slice(buffer: ref m_backbuffer, from: position.Absolute, scale: SetSafeArea(scale.Value, position.Absolute));

                using StyleEnumerator style = new StyleEnumerator(entity: list[i]);
                renderLogic.Render(buffer: canvas, version: list[i].Version, style);
            }

            Diffing(fullRedrawRequested);
        }

        m_workState.Value.State = WorkerSystemState.OPEN_FOR_PROCESSING;
        float delta = (Stopwatch.GetTimestamp() - start) / NS_TO_MS;

        if (delta <= LIMIT)
            Thread.Sleep(millisecondsTimeout: (int)(LIMIT - delta));

        m_delta = float.Lerp(m_delta, delta, .1f);
    }

    public void Diffing(bool full) {
        Console.Out.Write(value: AnsiCommand.StartBufferLoad);
        VT100StringBuilder builder = new VT100StringBuilder(buffer: stackalloc char[STRING_BUILDER_STACK_SPACE]);

        for (int y = 0; y < m_backbuffer.Scale.Y; ++y) {
            for (int x = 0; x < m_backbuffer.Scale.X; ++x) {
                ref vtchar_t backChar = ref m_backbuffer[x, y];
                ref vtchar_t frontChar = ref m_frontbuffer[x, y];

                if (!frontChar.Equals(backChar) || (full && !backChar.Equals(new vtchar_t()))) {
                    builder.WritePosition(x, y)
                           .WriteFontStyles(backChar.Styles)
                                .WriteColor(color: backChar.Background.A == 0 ? null : backChar.Background, isBackground: true)
                                .WriteColor(color: backChar.Foreground.A == 0 ? null : backChar.Foreground, isBackground: false)
                           .WriteCharacter(backChar.Character);

                    frontChar = backChar;
                }

                if (builder.BarrierReached) {
                    builder.Build(destination: m_out);
                    m_out.Flush();
                };
            }

        }

        builder.Build(destination: m_out);

        m_out.Flush();
        m_backbuffer.Clear();

        Console.Out.Write(value: AnsiCommand.ResetStyles);
        Console.Out.Write(value: AnsiCommand.Home);

        Console.Out.Write(value: AnsiCommand.EndBufferLoad);
        Console.Out.Write(value: AnsiCommand.ClearSavedLines);
    }

    private bool OnLayoutChange() {
        if (!m_layoutState.Value.IsChanged)
            return false;

        Vec2 scale = m_layoutState.Value.Scale;

        m_backbuffer = ConsoleBuffer.Reallocate(buffer: ref m_backbuffer, scale);
        m_frontbuffer = ConsoleBuffer.Reallocate(buffer: ref m_frontbuffer, scale);

        m_layoutState.Value = m_layoutState.Value with { IsChanged = false };
        return true;
    }

    private bool InBuffer(Vec2 position, Vec2 scale) {
        return (m_backbuffer.Scale.X > position.X && m_backbuffer.Scale.X > position.Y) &&
               (position.X + scale.X >= 0 && position.Y + scale.Y >= 0);
    }

    private Vec2 SetSafeArea(Vec2 scale, Vec2 position) {
        if ((scale.X + position.X) >= m_backbuffer.Scale.X) scale.X -= 1;
        if ((scale.Y + position.Y) >= m_backbuffer.Scale.Y) scale.Y -= 1;
        return scale;
    }
}