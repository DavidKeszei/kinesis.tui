using Kinesis.Processing;
using Kinesis.Input.Windows;
using Kinesis.UI;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Kinesis.Input;

/// <summary>
/// Single input message from the standard input stream.
/// </summary>
public readonly record struct InputMessage: IWorkMessage {
    private readonly char m_key = '\0';
    private readonly InputModifier m_modifiers = InputModifier.NONE;

    private readonly InputAction m_action = InputAction.PRESS;
    private readonly bool m_isPressed = false;

    /// <summary>
    /// Represent a empty <see cref="InputMessage"/>.
    /// </summary>
    public static InputMessage Empty { get => new InputMessage('\0', InputModifier.NONE, InputAction.PRESS, false); }

    /// <summary>
    /// Target key of the input.
    /// </summary>
    public char Key { get => m_key; }

    /// <summary>
    /// Pressed info.Modifiers of the input (SHIFT, ALT, CTR).
    /// </summary>
    public InputModifier Modifiers { get => m_modifiers; }

    /// <summary>
    /// Type of the input-action.
    /// </summary>
    public InputAction Action { get => m_action; }

    /// <summary>
    /// Indicates the current input was pressed.
    /// </summary>
    public bool IsPressed { get => m_isPressed; }

    /// <summary>
    /// Target callback type of the message.
    /// </summary>
    public static WorkTag Target { get => WorkTag.INPUT; }

    /// <summary>Create a new <see cref="InputMessage"/>.</summary>
    /// <param name="key">Actual key value as <see cref="char"/>.</param>
    /// <param name="info.Modifiers">Currently pressed info.Modifiers with the <see cref="Key"/>.</param>
    /// <param name="action">Current action of the message.</param>
    internal InputMessage(char key, InputModifier modifiers, InputAction action, bool isPress) {
        m_key = key;
        m_modifiers = modifiers;

        m_action = action;
        m_isPressed = isPress;
    }
}

/// <summary>
/// Represent a unified source of the inputs.
/// </summary>
internal class InputSystem: IDynamicSystem {
    private const string DEDICATED_THREAD_NAME = "Input Thread";

    /// <summary>
    /// Indicates the wait time between two sampling. (10ms)
    /// </summary>
    private const int POOLING_TIME = 5;

    /// <summary>
    /// Minimum time, when we think no input was happened and we fire that. (10ms)
    /// </summary>
    private const int DEAD_ZONE = 10;

    /// <summary>
    /// Minimum time, when we think the press is long-press. (75ms)
    /// </summary>
    private const int HOLD_THRESHHOLD = 75;

    public SystemBehavior Behavior { get => SystemBehavior.DYNAMIC; }

    private readonly IInputBackend m_backend = null!;
    private (char Key, InputModifier Modifier, TimeSpan When, bool isPress) m_startInputInfo = ('\0', InputModifier.NONE, TimeSpan.Zero, false);

    public InputSystem(ConsoleSourceInfo provider) {
        m_backend = RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows) ? WindowsInputBackend.Init(source: provider.Windows) : null!;
        Console.InputEncoding = Encoding.UTF8;
    }

    /// <summary>
    /// Listen inputs from standard input.
    /// </summary>
    public void Run() {
        if (m_backend == IInputBackend.ERR) return;
        Thread.CurrentThread.Name = DEDICATED_THREAD_NAME;

        float deadZoneTime = DEAD_ZONE;
        InputAction lastAction = InputAction.PRESS;

        while (true) {
            DateTime now = DateTime.UtcNow;

            if (m_backend.ReadInput(out InputInfo info)) {
                /* 1. Check if the key same as before */
                if (m_startInputInfo.Key == info.Key && m_startInputInfo.Modifier == info.Modifiers) {
                    if ((now.TimeOfDay - m_startInputInfo.When).TotalMilliseconds >= HOLD_THRESHHOLD && lastAction != InputAction.HOLD) {

                        lastAction = InputAction.HOLD;
                        WorkerSystem.Current.AddInputMessage(message: new InputMessage(key: m_startInputInfo.Key, modifiers: m_startInputInfo.Modifier, action: InputAction.HOLD, isPress: m_startInputInfo.isPress));
                    }
                }
                /* 1.1 If not: do fast swap between the new & current keys */
                else if (m_startInputInfo.Key != info.Key || m_startInputInfo.Modifier != info.Modifiers) {
                    if (m_startInputInfo.When != TimeSpan.Zero) {

                        WorkerSystem.Current.AddInputMessage(message: new InputMessage(key: m_startInputInfo.Key, modifiers: m_startInputInfo.Modifier, action: InputAction.PRESS, isPress: m_startInputInfo.isPress));
                        deadZoneTime = DEAD_ZONE;
                    }

                    m_startInputInfo = (info.Key, info.Modifiers, now.TimeOfDay, info.IsPress);
                }

                Thread.Sleep(millisecondsTimeout: POOLING_TIME);
                continue;
            }

            /* This decrease CPU usage */
            Thread.Sleep(millisecondsTimeout: POOLING_TIME);

            /* 2. Send it after the DEAD_ZONE. (Only, if the action is not HOLD)*/
            if (deadZoneTime <= 0) {
                if (lastAction != InputAction.HOLD)
                    WorkerSystem.Current.AddInputMessage(message: new InputMessage(key: m_startInputInfo.Key, m_startInputInfo.Modifier, action: InputAction.PRESS, isPress: m_startInputInfo.isPress));

                m_startInputInfo = ('\0', InputModifier.NONE, TimeSpan.Zero, false);

                deadZoneTime = DEAD_ZONE;
                lastAction = InputAction.PRESS;
                continue;
            }

            if (m_startInputInfo.When != TimeSpan.Zero) {
                float inputTime = (float)(DateTime.UtcNow - now).TotalMilliseconds;
                deadZoneTime -= inputTime;
            }
        }

    }
}
