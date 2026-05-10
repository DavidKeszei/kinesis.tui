using Kinesis.Utils;
using Kinesis.Native;
using Kinesis.UI;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Kinesis.Core;

namespace Kinesis.Core;

/// <summary>
/// Represent a unified source of the inputs.
/// </summary>
internal class InputSystem: IDynamicSystem {
    private const string DEDICATED_THREAD_NAME = "Input Thread";

    /// <summary>
    /// Indicates the wait time between two sampling. (3ms)
    /// </summary>
    private const int POOLING_TIME = 3;

    /// <summary>
    /// Minimum time, when we think no input was happened and we fire that. (5ms)
    /// </summary>
    private const int DEAD_ZONE = 5;

    /// <summary>
    /// Minimum time, when we think the press is long-press. (50ms)
    /// </summary>
    private const int HOLD_THRESHHOLD = 50;

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
                if (info.Key == '\0')
                    continue;

                /* 1. Check if the key same as before */
                if (m_startInputInfo.Key == info.Key && m_startInputInfo.Modifier == info.Modifiers) {
                    if ((now.TimeOfDay - m_startInputInfo.When).TotalMilliseconds >= HOLD_THRESHHOLD && lastAction != InputAction.HOLD) {

                        lastAction = InputAction.HOLD;
                        JobSystem.Current.AddInputMessage(message: new InputMessage(key: m_startInputInfo.Key, modifiers: m_startInputInfo.Modifier, action: InputAction.HOLD, isPress: m_startInputInfo.isPress));
                    }
                }
                /* 1.1 If not: do fast swap between the new & current keys */
                else if (m_startInputInfo.Key != info.Key || m_startInputInfo.Modifier != info.Modifiers) {
                    if (m_startInputInfo.When != TimeSpan.Zero) {

                        JobSystem.Current.AddInputMessage(message: new InputMessage(key: m_startInputInfo.Key, modifiers: m_startInputInfo.Modifier, action: InputAction.PRESS, isPress: m_startInputInfo.isPress));
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
                    JobSystem.Current.AddInputMessage(message: new InputMessage(key: m_startInputInfo.Key, m_startInputInfo.Modifier, action: InputAction.PRESS, isPress: m_startInputInfo.isPress));

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
