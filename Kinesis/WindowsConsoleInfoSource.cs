using Kinesis.Input.Windows;
using Kinesis.Layout;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis;

/// <summary>
/// Represents a Windows specific console information source.
/// </summary>
internal sealed partial class WindowsConsoleInfoProvider: IConsoleSource<ConsoleScaleInfo>, IConsoleSource<InputKeyEventInfo> {
    #region DEFINES

    private const int QUEUE_COUNT = 16;
    private const int WAIT = 5;

    private const uint READ_COUNT = 1;

    #endregion
    #region P/INVOKE

    [LibraryImport(libraryName: "kernel32.dll", EntryPoint = "ReadConsoleInputW")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(unmanagedType: UnmanagedType.Bool)]
    private static partial bool Read(nint handle, ref WindowsConsoleEventMsg message, uint count, out uint _);

    [LibraryImport(libraryName: "kernel32.dll", EntryPoint = "PeekConsoleInputW")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(unmanagedType: UnmanagedType.Bool)]
    private static partial bool Peek(nint handle, ref WindowsConsoleEventMsg message, uint count, out uint _);

    #endregion

    private readonly Queue<InputKeyEventInfo> m_inputs = null!;
    private readonly Queue<ConsoleScaleInfo> m_layouts = null!;

    private readonly Task m_watchman = null!;
    private readonly WindowsConsoleMsgTag[] m_tags = null!;

    private bool m_isLocked = false;

    public WindowsConsoleInfoProvider() {
        m_inputs = new Queue<InputKeyEventInfo>(capacity: QUEUE_COUNT);
        m_layouts = new Queue<ConsoleScaleInfo>(capacity: QUEUE_COUNT);

        m_tags = Enum.GetValues<WindowsConsoleMsgTag>();
        m_watchman = Task.Run(async() => await Watch());
    }

    public bool Read(out ConsoleScaleInfo result) {
        result = default;

        if (Interlocked.CompareExchange<bool>(ref m_isLocked, true, false) != false) {
            return false;
        }

        bool success = m_layouts.TryDequeue(out result);
        Interlocked.Exchange<bool>(ref m_isLocked, false);

        return success;
    }

    public bool Read(out InputKeyEventInfo result) {
        result = default;

        if (Interlocked.CompareExchange<bool>(ref m_isLocked, true, false) != false) {
            return false;
        }

        bool success = m_inputs.TryDequeue(out result);
        Interlocked.Exchange<bool>(ref m_isLocked, false);

        return success;
    }

    private async Task Watch() {
        WindowsConsoleEventMsg message = default!;

        while (true) {
            bool success = Peek(handle: StdHandle.Input, ref message, count: READ_COUNT, out uint _) && CheckIfSupported(tag: message.Tag);

            if (success && Read(handle: StdHandle.Input, ref message, count: READ_COUNT, out _)) {
                while (Interlocked.CompareExchange<bool>(ref m_isLocked, true, false) != false)
                    await Task.Delay(millisecondsDelay: WAIT);

                switch(message.Tag) {
                    case WindowsConsoleMsgTag.INPUT:
                        m_inputs.Enqueue(message.KeyInfo);
                        break;

                    case WindowsConsoleMsgTag.LAYOUT:
                        m_layouts.Enqueue(message.ConsoleWindowScale);
                        break;
                }

                Interlocked.Exchange<bool>(ref m_isLocked, false);
                continue;
            }

            /* Reads out the unsupported messages */
            _ = Read(handle: StdHandle.Input, ref message, count: READ_COUNT, out _);
        }
    }

    private bool CheckIfSupported(WindowsConsoleMsgTag tag) {
        for (int i = 0; i < m_tags.Length; ++i)
            if (m_tags[i] == tag)
                return true;

        return false;
    }
}
