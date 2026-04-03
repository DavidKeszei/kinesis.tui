using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

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
