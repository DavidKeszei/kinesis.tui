using Kinesis.Input.Windows;
using Kinesis.Processing;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Layout;

/// <summary>
/// This class observes changes in the current console windows dimension.
/// </summary>
internal partial class LayoutSystem: IDynamicSystem {
    #region CONSTS
    private const int POOLING_TIME = 1;
    #endregion

    #region NATIVE_IMPL_WIN32

    [LibraryImport(libraryName: "kernel32.dll", EntryPoint = "ReadConsoleInputW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(unmanagedType: UnmanagedType.Bool)]
    private static partial bool GetWindowScale(nint hnd, ref WindowsConsoleEventMsg buffer, uint length, out uint _);

    #endregion

    private readonly State<LayoutInfo> m_info = null!;
    private readonly nint m_stdHandle = nint.Zero;

    /// <summary>
    /// Behavior of the <see cref="LayoutSystem"/>.
    /// </summary>
    public SystemBehavior Behavior { get => SystemBehavior.DYNAMIC; }

    /// <summary>
    /// Create a new <see cref="LayoutSystem"/> with <paramref name="scale"/>.
    /// </summary>
    /// <param name="scale">Start scale of the application. This is going be the pivot point of the observing.</param>
    public LayoutSystem(nint handle, State<LayoutInfo> state, Vec2 scale) {
        m_info = state;
        m_stdHandle = handle;

        m_info.Value = new LayoutInfo(scale, IsChanged: false);
    }

    /// <summary>
    /// Start watching of changes of the console window.
    /// </summary>
    public void Run() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            RunOnWindows();
    }

    private void RunOnWindows() {
        while (true) {
            Thread.Sleep(millisecondsTimeout: POOLING_TIME);

            WindowsConsoleEventMsg msg = new WindowsConsoleEventMsg(tag: WindowsConsoleMsgTag.RESIZE);
            bool success = GetWindowScale(m_stdHandle, ref msg, (uint)Unsafe.SizeOf<WindowsConsoleEventMsg>(), out uint _) && msg.Tag == WindowsConsoleMsgTag.RESIZE;

            if (success && (m_info.Value.Scale.X != msg.ConsoleWindowScale.X || m_info.Value.Scale.Y != msg.ConsoleWindowScale.Y)) {

                m_info.Value = new LayoutInfo(new Vec2(x: msg.ConsoleWindowScale.X, y: msg.ConsoleWindowScale.Y), true);
                WorkerSystem.Current.AddLayoutMessage(message: new LayoutMessage(scale: m_info.Value.Scale));
            }
        }
    }
}

internal record struct LayoutInfo(Vec2 Scale, bool IsChanged);