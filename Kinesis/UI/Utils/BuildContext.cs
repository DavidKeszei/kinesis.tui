using Kinesis.Core;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represent a state in the UI building process.
/// </summary>
public ref struct BuildContext {
    #region PREDEFINES
    public const int STYLE_STACK_FRAME_LEN = 6;

    //private const int POSITION = 0;
    //private const int SCALE = 1;

    //private const int BACKGROUND = 2;
    //private const int FOREGROUND = 3;

    //private const int FONT_STYLE = 4;
    //private const int PADDING = 5;
    #endregion

    private readonly Island m_root = null!;
    private Island m_currentIsland = null!;

    private readonly Entity m_current = null!;
    private readonly Stack<Component>[] m_inheritanceTargets = null!;

    private readonly int m_levelId = 0;
    private byte m_flags = 0;

    private readonly bool m_isTop = true;

    /// <summary>
    /// Current target entity of the building.
    /// </summary>
    public readonly Entity Current { get => m_current; internal init => m_current = value; }

    /// <summary>
    /// Current island of the segment.
    /// </summary>
    public Island CurrentIsland { readonly get => m_currentIsland; internal set => m_currentIsland = value; }

    /// <summary>
    /// Root <see cref="Island"/> of the building.
    /// </summary>
    public readonly Island Root { get => m_root; internal init => m_root = value; }

    /// <summary>
    /// Flags, which indicates what kind of components setted. (Must be init.-d with 0 value every level of building)
    /// </summary>
    internal byte ChangeStyleFlag { init => m_flags = value; }

    /// <summary>
    /// Indicates the current is the root level. 
    /// </summary>
    internal readonly bool IsTop { get => m_isTop; init => m_isTop = value; }

    internal readonly int LevelId { get => m_levelId; init => m_levelId = value; }

    internal BuildContext(Entity current) {
        m_current = current;
        m_inheritanceTargets = new Stack<Component>[(int)BuildSnapshotComponents.__COUNT__]; /* Padding is largest index -> Count of inheritable components */

        for (int i = 0; i < m_inheritanceTargets.Length; ++i)
            m_inheritanceTargets[i] = new Stack<Component>(capacity: 128);
    }

    /// <summary>
    /// Inherit from an most-closes component on the build-tree.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="target">Target of the <see cref="InheritStyle"/>.</param>
    /// <param name="default">If store not have any inheritable component, then this value was written. This can't be <see langword="null"/>.</param>
    /// <param name="index">Index of the component.</param>
    public void InheritStyle(Entity target, Style @default, int index = 0) {
        /* TODO(2026-06-30T23:44:28): Change Inhetit<T> to InheritStyle for copy/inherit styles. (Status: Done✅)*/ 	
        if (target == null || @default == null) return;

        Style? component = target.Get<Style>(index);
        if (component == null) return;

        int type = MatchId(@default);
        if (type == -1) return;

        if (Style.IsDefault(component)) {
            Style copy = m_inheritanceTargets[type].TryPeek(out Component? _) ? ((Style)m_inheritanceTargets[type].Peek()) : @default;
            component.Copy(ref copy);
        }

        if ((m_flags & (1 << type)) != (1 << type)) {
            m_inheritanceTargets[type].Push(component);
            m_flags |= (byte)(1 << type);
        }
    }

    /// <summary>
    /// Set pivot of a(n) <typeparamref name="T"/> component. 
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="target">Holder of the component.</param>
    public void SetPivot<T>(Entity target) where T: Component, IStaticType {
        T component = target.Get<T>() ?? null!;

        if (component == null) return;
        int type = MatchId(component);

        if (component is Scale scale && m_inheritanceTargets[type].TryPeek(out Component? peek))
            scale.Maximum = ((Scale)peek);

        if (component is Position position && m_inheritanceTargets[type].TryPeek(out Component? pos))
            position.Origin = ((Position)pos);

        if ((m_flags & (1 << type)) != (1 << type)) {
            m_inheritanceTargets[type].Push(component);
            m_flags |= (byte)(1 << type);
        }
    }

    /// <summary>
    /// Create a new <see cref="BuildStackSnapshot"/> instance from the current state.
    /// </summary>
    /// <returns>Returns a <see cref="BuildStackSnapshot"/> instance.</returns>
    internal readonly BuildStackSnapshot CreateBuildSnapshot() {
        return new BuildStackSnapshot(
            Scale: m_inheritanceTargets[(int)BuildSnapshotComponents.SCALE].TryPeek(out Component? scale) ? (Scale)scale : null!,
            Position: m_inheritanceTargets[(int)BuildSnapshotComponents.POSITION].TryPeek(out Component? position) ? (Position)position : null!,
            Styles: [
                m_inheritanceTargets[(int)BuildSnapshotComponents.BACKGROUND].TryPeek(out Component? bg) ? (Style)bg : null!,
                m_inheritanceTargets[(int)BuildSnapshotComponents.FOREGROUND].TryPeek(out Component? fg) ? (Style)fg : null!,
                m_inheritanceTargets[(int)BuildSnapshotComponents.FONT_STYLE].TryPeek(out Component? fontStyle) ? (Style)fontStyle : null!,
            ]
        );
    }

    internal readonly void LoadSnapshot(BuildStackSnapshot snapshot) {
        if (snapshot == null!) return;

        if(snapshot.Scale != null)  m_inheritanceTargets[(int)BuildSnapshotComponents.SCALE].Push(item: snapshot.Scale);
        if(snapshot.Position != null!) m_inheritanceTargets[(int)BuildSnapshotComponents.POSITION].Push(item: snapshot.Position);

        for (int i = (int)BuildSnapshotComponents.BACKGROUND; i < (int)BuildSnapshotComponents.__COUNT__; ++i) {
            if (snapshot.Styles[i - (int)BuildSnapshotComponents.BACKGROUND] == null) continue;

            m_inheritanceTargets[i].Push(snapshot.Styles[i - (int)BuildSnapshotComponents.BACKGROUND]);
        }
    }

    internal readonly void DropCurrentLevelStyles() {
        for (int i = 0; i < m_inheritanceTargets.Length; ++i) {
            if((m_flags & (1 << i)) == (1 << i))
                _ = m_inheritanceTargets[i].TryPop(out _);
        }
    }

    private readonly int MatchId(Component component) {
        return component switch {
            Position => (int)BuildSnapshotComponents.POSITION,
            Scale    => (int)BuildSnapshotComponents.SCALE,
            Style    => Unsafe.As<Component, Style>(ref component).Tag switch {
                StyleTag.BACKGROUND => (int)BuildSnapshotComponents.BACKGROUND,
                StyleTag.FOREGROUND => (int)BuildSnapshotComponents.FOREGROUND,
                StyleTag.FONT_ATTR  => (int)BuildSnapshotComponents.FONT_STYLE,
                StyleTag.PADDING    => (int)BuildSnapshotComponents.PADDING,
                _                   => -1
            },
            _        => -1,
        };
    }
}


file enum BuildSnapshotComponents: byte {
    POSITION,
    SCALE,
    BACKGROUND,
    FOREGROUND,
    FONT_STYLE,
    PADDING,
    __COUNT__
}