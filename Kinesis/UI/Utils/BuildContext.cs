using Kinesis.Core;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a state in the Ui building process.
/// </summary>
public ref struct BuildContext {
    #region PREDEFINES
    private const int POSITION = 0;
    private const int SCALE = 1;

    private const int BACKGROUND = 2;
    private const int FOREGROUND = 3;

    private const int FONT_STYLE = 4;
    private const int PADDING = 5;
    #endregion

    private readonly Island m_root = null!;
    private readonly Entity m_current = null!;

    private readonly Stack<Component>[] m_inheritanceTargets = null!;
    private byte m_flags = 0;

    /// <summary>
    /// Current target entity of the building.
    /// </summary>
    public readonly Entity Current { get => m_current; internal init => m_current = value; }

    /// <summary>
    /// Root <see cref="Island"/> of the building.
    /// </summary>
    public readonly Island Root { get => m_root; internal init => m_root = value; }

    internal byte ChangeStyleFlag { init => m_flags = value; }

    internal BuildContext(Entity current) {
        m_current = current;
        m_inheritanceTargets = new Stack<Component>[PADDING]; /* Padding is largest index -> Count of inheritable components */

        for (int i = 0; i < m_inheritanceTargets.Length; ++i)
            m_inheritanceTargets[i] = new Stack<Component>(capacity: 16);
    }

    /// <summary>
    /// Set a inheritable component, if that equals with <see cref="IEmpty{TSelf}.Empty"/>.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="target">Target of the Set{T}().</param>
    /// <param name="default">If store not have any inheritable component, then this value was written. This can't be <see langword="null"/>.</param>
    /// <param name="index">Index of the component.</param>
    public void Set<T>(Entity target, T @default, int index = 0) where T : Component, IStaticType, IDefault<T>, ICopyable<T> {
        if (target == null || @default == null) return;

        T? component = target.GetComponent<T>(index);
        if (component == null) return;

        int type = MatchId(@default);
        if (type == -1) return;

        if (T.IsDefault(component)) {
            Component copy = m_inheritanceTargets[type].TryPeek(out Component? result) ? ((T)m_inheritanceTargets[type].Peek()) : @default;
            component.Copy(ref Unsafe.As<Component, T>(ref copy));

            if (component is Position position && result != null)
                position.Origin = ((Position)result);
        }

        if (component is Scale scale && m_inheritanceTargets[type].TryPeek(out Component? peek))
            scale.Maximum = ((Scale)peek);

        m_inheritanceTargets[type].Push(component);
        m_flags |= (byte)(1 << type);
    }

    internal readonly void DropCurrentLevelStyles() {
        for (int i = 0; i < m_inheritanceTargets.Length; ++i) {
            if((m_flags & (1 << i)) == (1 << i))
                _ = m_inheritanceTargets[i].TryPop(out _);
        }
    }

    private readonly int MatchId(Component component) {
        return component switch {
            Position => POSITION,
            Scale => SCALE,
            Style => Unsafe.As<Component, Style>(ref component).Tag switch {
                StyleTag.BACKGROUND => BACKGROUND,
                StyleTag.FOREGROUND => FOREGROUND,
                StyleTag.FONT_ATTR => FONT_STYLE,
                StyleTag.PADDING => PADDING,
                _ => -1
            },
            _ => -1,
        };
    }
}
