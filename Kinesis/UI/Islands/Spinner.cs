using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kinesis.UI;

/// <summary>
/// Represents a simple loading indicator on the screen.
/// </summary>
public sealed class Spinner: Island, ICopyable<BuildContext> {
    #region PREDEFINES

    private readonly static char[][] s_spinnerFrames = [
        ['|', '/', '-', '\\'],
        ['⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏'],
        ['↖', '↑', '↗', '→', '↘', '↓', '↙', '←'],
        ['.', 'o', 'O', '0', 'O', 'o', '.'],
        ['_', '.', '-', '^', '-', '.', '_'],
        ['\\', '|', '/', '|', '\\', '|', '/', '|'],
        [' ', '_', ' ', '▄', ' ', '█', ' ', '▀'],
        [' ', '_', '-', '=', '≡', '*'],
        ['+', '-', '*', '%', '=', 'X'],
        ['.', '-', '\'', '|', '`', '-', '.', '|'],
        ['#', '%', 'X', 'x', '=', '+', '-', '.', ' '],
        ['0', '1', '5', '9', '8', '3', '2'],
        ['▏', '▎', '▍', '▌', '▋', '▊', '▉', '█']
    ];

    #endregion

    private readonly long m_changeTime = TimeSpan.FromSeconds(seconds: 1).Ticks;
    private readonly char[] m_spinnerStates = null!;

    private int m_index = 0;

    /// <summary>
    /// Foreground color of the <see cref="Spinner"/>.
    /// </summary>
    public RGB Foreground { get => Get<Style>()!.AsRGB; set => Get<Style>()!.AsRGB = value; }

    /// <summary>
    /// Frame states of the <see cref="Spinner"/>.
    /// </summary>
    public char[] States { init => m_spinnerStates = value; }

    /// <summary>
    /// Duration of one iteration.
    /// </summary>
    public TimeSpan Duration { init => m_changeTime = value.Ticks; }

    public Spinner(): base(count: 3) {
        _ = Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>(), isUnique: true);
        _ = Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static (x) => x.Value = Vec2.One), isUnique: true);

        _ = Attach<Style>(ComponentPool<Style>.Instance.Rent<Style>(static (x) => x.As<RGB?>(tag: StyleTag.FOREGROUND, value: null!)), isUnique: true);
    }

    /// <summary>
    /// Create a spinner based on the predefined constansts.
    /// </summary>
    /// <param name="preset">Enumeration value of the preset.</param>
    /// <param name="durationPerCycle">Duration of a cycle.</param>
    /// <param name="color">Foreground color of the <see cref="Spinner"/>.</param>
    /// <returns>Returns a new <see cref="Spinner"/> instance.</returns>
    public static Spinner Create(SpinnerPreset preset, TimeSpan durationPerCycle, RGB? color = null) {
        if (!Enum.IsDefined<SpinnerPreset>(preset))
            return null!;

        if (color != null) {
            return new Spinner {
                States = s_spinnerFrames[(int)preset],
                Duration = durationPerCycle,
                Foreground = color.Value
            };
        }

        return new Spinner {
            States = s_spinnerFrames[(int)preset],
            Duration = durationPerCycle,
        };
    }

    public void Copy(ref BuildContext from) {
        from.InheritStyle(this, @default: Style.CreateFromRGB(tag: StyleTag.FOREGROUND, color: RGB.White));

        from.SetPivot<Position>(this);
        from.SetPivot<Scale>(this);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        return new AnimatedArea<AnimatedNumber<int>, UIText>() {
            Selector = (_) => m_index,
            Applier = (text, value) => {
                m_index = value;
                text.Get<TextRenderer>()!.Write(text: [ m_spinnerStates[m_index % m_spinnerStates.Length] ]);
            },

            Duration = TimeSpan.FromTicks(value: m_changeTime),
            To = m_spinnerStates.Length,

            IsPeriodic = true,
            Content = new UIText {
                Name = $"__{nameof(Spinner)}__{Guid.CreateVersion7()}__",
                Text = $"{m_spinnerStates[m_index]}"
            }
        };
    }
}

public enum SpinnerPreset {
    SIMPLE,
    BRAILLE,
    ARROW,
    BUBBLE,
    GROWING_LINE,
    PENDULUM,
    BUILDING_BRICKS,
    BUILDING_BRICKS_ASCII,
    MATH,
    TIME_TRAVEL,
    INVERZ_LOAD,
    SCIFI,
    GROWIN_WIDTH
}