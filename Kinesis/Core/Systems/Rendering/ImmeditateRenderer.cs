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
    private const float LIMIT = 1.0f;

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

        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = true;
    }
        
    public void Run(IReadOnlyList<Entity> list) {
        long start = DateTime.Now.Ticks;

        if (m_workState.Value.State == WorkerSystemState.WAIT_FOR_RENDERER) {
            OnLayoutChange();

            for (int i = 0; i < list.Count; ++i) {

                Scale scale = list[i].GetComponent<Scale>()!;
                Position position = list[i].GetComponent<Position>()!;

                if (!InBuffer(position: position.Absolute)) continue;

                RenderComponent renderLogic = list[i].GetComponent<RenderComponent>()!;
                Canvas canvas = ConsoleBuffer.Slice(buffer: ref m_backbuffer, from: position.Absolute, scale: scale.Value);

                using StyleEnumerator style = new StyleEnumerator(entity: list[i]);
                renderLogic.Render(buffer: canvas, version: list[i].Version, style);
            }

            Diffing();
            m_workState.Value.IsWorked = false;
        }

        m_workState.Value.State = WorkerSystemState.OPEN_FOR_PROCESSING;
        m_delta = (DateTime.Now.Ticks - start) / NS_TO_MS;

        if (m_delta <= LIMIT) {

            Thread.Sleep(millisecondsTimeout: (int)(LIMIT - m_delta));
            m_delta = LIMIT;
        }
    }

    public void Diffing() {
        VT100StringBuilder builder = new VT100StringBuilder(buffer: stackalloc char[STRING_BUILDER_STACK_SPACE]);

        for (int x = 0; x < m_backbuffer.Scale.X; ++x) {
            for (int y = 0; y < m_backbuffer.Scale.Y; ++y) {

                ref vtchar_t backChar = ref m_backbuffer[x, y];
                ref vtchar_t frontChar = ref m_frontbuffer[x, y];

                if (!frontChar.Equals(backChar)) {
                    builder.WritePosition(x, y)
                           .WriteFontStyles(backChar.Styles)
                                .WriteColor(color: backChar.Background.A == 0 ? null : backChar.Background, true)
                                .WriteColor(color: backChar.Foreground.A == 0 ? null : backChar.Foreground, false)
                           .WriteCharacter(backChar.Character)
                           .Build(destination: m_out);

                    frontChar = backChar;
                }

                if (builder.BarrierReached) {
                    builder.Clear();
                    m_out.Flush();
                }
            }
        }

        m_out.Flush();
        m_backbuffer.Clear();

        Console.Out.Write(value: AnsiCommand.RESET_STYLES);
        Console.Out.Write(value: AnsiCommand.HOME);
        Console.Out.Write(value: AnsiCommand.CLEAR_SAVED_LINES);
    }

    private void OnLayoutChange() {
        if (m_layoutState.Value.IsChanged)
            return;

        m_backbuffer = ConsoleBuffer.Reallocate(buffer: m_backbuffer, scale: m_layoutState.Value.Scale);
        m_frontbuffer = ConsoleBuffer.Reallocate(buffer: m_frontbuffer, scale: m_layoutState.Value.Scale);

        m_layoutState.Value = m_layoutState.Value with { IsChanged = false };
    }

    private bool InBuffer(Vec2 position) {
        return (m_backbuffer.Scale.X > position.X && m_backbuffer.Scale.X > position.Y) &&
               (position.X >= 0 && position.Y >= 0);
    }
}