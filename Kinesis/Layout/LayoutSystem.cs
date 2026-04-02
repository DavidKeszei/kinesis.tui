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
    private const int POOLING_TIME = 8;
    #endregion

    private readonly State<LayoutInfo> m_info = null!;
    private readonly IConsoleSource<ConsoleScaleInfo> m_source = null!;

    /// <summary>
    /// Behavior of the <see cref="LayoutSystem"/>.
    /// </summary>
    public SystemBehavior Behavior { get => SystemBehavior.DYNAMIC; }

    /// <summary>
    /// Create a new <see cref="LayoutSystem"/> with <paramref name="scale"/>.
    /// </summary>
    /// <param name="scale">Start scale of the application. This is going be the pivot point of the observing.</param>
    public LayoutSystem(ConsoleSourceInfo provider, State<LayoutInfo> state, Vec2 scale) {
        m_info = state;
        m_source = provider.Windows;

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
            if (m_source.Read(out ConsoleScaleInfo info) && (m_info.Value.Scale.X != info.X || m_info.Value.Scale.Y != info.Y)) {

                m_info.Value = new LayoutInfo(new Vec2(x: info.X, y: info.Y), true);
                WorkerSystem.Current.AddLayoutMessage(message: new LayoutMessage(scale: m_info.Value.Scale));
            }
        }
    }
}

internal record struct LayoutInfo(Vec2 Scale, bool IsChanged);