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
    private const float _120FPS_ = 8.3f;

    private const float FPS_CONVERT = 1000f;
    private const int STRING_BUILDER_STACK_SPACE = 16_384;
    #endregion

    private ConsoleBuffer m_backbuffer = default;
    private ConsoleBuffer m_frontbuffer = default;

    private readonly State<LayoutInfo> m_layoutState = null!;
    private readonly State<WorkStateInfo> m_workState = null!;

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

    public ImmediateRenderer(State<WorkStateInfo> workState, State<LayoutInfo> layoutState) {
        m_workState = workState;
        m_layoutState = layoutState;

        m_backbuffer = new ConsoleBuffer(x: (int)layoutState.Value.Scale.X, (int)layoutState.Value.Scale.Y);
        m_frontbuffer = new ConsoleBuffer(x: (int)layoutState.Value.Scale.X, (int)layoutState.Value.Scale.Y);

        m_out = new StreamWriter(stream: Console.OpenStandardOutput(), encoding: Encoding.UTF8) {
            AutoFlush = false
        };

        Console.CursorVisible = false;
    }
        
    public void Run(IReadOnlyList<Entity> list) {
        long start = DateTime.Now.Ticks;

        if (m_workState.Value.State == WorkerSystemState.WAIT_FOR_RENDERER && m_workState.Value.IsWorked) {
            for (int i = 0; i < list.Count; ++i) {

                Transform transform = list[i].GetComponent<Transform>()!;
                if (!InBuffer(position: transform.Position)) continue;

                RenderComponent renderLogic = list[i].GetComponent<RenderComponent>()!;
                Canvas canvas = ConsoleBuffer.Slice(buffer: ref m_backbuffer, from: transform.Position, scale: transform.Scale);

                using StyleEnumerator style = new StyleEnumerator(entity: list[i]);
                renderLogic.Render(buffer: canvas, version: list[i].Version, style);
            }

            Diffing();
            m_workState.Value.IsWorked = false;
        }

        m_workState.Value.State = WorkerSystemState.OPEN_FOR_PROCESSING;
        m_delta = (DateTime.Now.Ticks - start) / NS_TO_MS;

        if (m_delta <= _120FPS_) {

            Thread.Sleep(millisecondsTimeout: (int)(_120FPS_ - m_delta));
            m_delta = _120FPS_;
        }
    }

    public void Diffing() {
        VT100StringBuilder builder = new VT100StringBuilder(buffer: stackalloc char[STRING_BUILDER_STACK_SPACE]);
        int count = 0;

        for (int x = 0; x < m_backbuffer.Scale.X; ++x) {
            for (int y = 0; y < m_backbuffer.Scale.Y; ++y) {

                ref vtchar_t backChar = ref m_backbuffer[x, y];
                ref vtchar_t frontChar = ref m_frontbuffer[x, y];

                if (!frontChar.Equals(backChar)) {
                    count = builder.WritePosition(x, y)
                                    .WriteFontStyles(backChar.Styles)
                                        .WriteColor(color: backChar.Background.A == 0 ? null : backChar.Background, true)
                                        .WriteColor(color: backChar.Foreground.A == 0 ? null : backChar.Foreground, false)
                                    .WriteCharacter(backChar.Character)
                                    .Build(destination: m_out);

                    frontChar = backChar;
                    builder.Position = count;
                }

                if (STRING_BUILDER_STACK_SPACE - count <= VT100StringBuilder.FLUSH_BARRIER) {
                    m_out.Flush();
                    builder.Clear();

                    count = 0;
                }
            }
        }

        m_out.Flush();
        m_backbuffer.Clear();
    }

    private bool InBuffer(Vec2 position) {
        return (m_backbuffer.Scale.X > position.X && m_backbuffer.Scale.X > position.Y) &&
               (position.X >= 0 && position.Y >= 0);
    }

    private RGB? ExtractColor(RGB first, RGB second) {
        if (first.A != 0) return first;
        if (second.A != 0) return second;

        return null;
    }
}