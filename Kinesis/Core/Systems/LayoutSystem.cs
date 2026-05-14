using Kinesis.Core;
using Kinesis.Core.Utils;
using Kinesis.Native;
using Kinesis.UI.Components;
using Kinesis.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinesis.Core;

/// <summary>
/// This class observes changes in the current console windows dimension.
/// </summary>
internal partial class LayoutSystem: IDynamicSystem {
    #region PREDEFINES
    private const int POOLING_TIME = 1;
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

        m_info.Value = new LayoutInfo(scale, IsChanged: true);
    }

    /// <summary>
    /// Start watching of changes of the console window.
    /// </summary>
    public void Run() {
        JobSystem.Current.AddLayoutMessage(message: new LayoutMessage(scale: m_info.Value.Scale));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            RunOnWindows();
    }

    private void RunOnWindows() {
        bool isFirst = true;
        ConsoleScaleInfo info = default;

        while (true) {
            if (m_source.Read(out ConsoleScaleInfo current) && (m_info.Value.Scale.X != info.X || m_info.Value.Scale.Y != info.Y)) {
                info = current;
                continue;
            }

            Thread.Sleep(millisecondsTimeout: POOLING_TIME);

            if (!info.Equals(default)) {
                if (!isFirst) {
                    m_info.Value = new LayoutInfo(new Vec2(x: info.X, y: info.Y), IsChanged: true);
                    JobSystem.Current.AddLayoutMessage(message: new LayoutMessage(scale: m_info.Value.Scale with { X = info.X - 1, Y = info.Y - 1}));
                }

                isFirst = false;
                info = default;
            }
        }
    }
}

internal record struct LayoutInfo(Vec2 Scale, bool IsChanged);