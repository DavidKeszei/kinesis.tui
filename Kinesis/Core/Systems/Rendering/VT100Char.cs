global using vtchar_t = Kinesis.Core.Rendering.VT100Char;

namespace Kinesis.Core.Rendering; 

/// <summary>
/// Represent a VT100 emulated character on the screen.
/// </summary>
public struct VT100Char: IEquatable<vtchar_t> {
    private RGB m_bg = RGB.Black;
    private RGB m_fg = RGB.White;

    private char m_character = ' ';
    private StyleFlag m_styles = StyleFlag.NONE;

    /// <summary>
    /// Character of the current buffer.
    /// </summary>
    public char Character { get => m_character; set => m_character = value; }

    /// <summary>
    /// Background color of the current buffer piece.
    /// </summary>
    public RGB Background { readonly get => m_bg; set => m_bg = value; }

    /// <summary>
    /// Foreground color of the current buffer piece.
    /// </summary>
    public RGB Foreground { readonly get => m_fg; set => m_fg = value; }

    /// <summary>
    /// Applied font styles to the character.
    /// </summary>
    public StyleFlag Styles { get => m_styles; set => m_styles = value; }

    public static bool operator ==(VT100Char l, VT100Char r) => l.Equals(r);

    public static bool operator !=(VT100Char l, VT100Char r) => !l.Equals(r);

    public VT100Char(char character, RGB color) {
        m_character = character;
        m_bg = color;
    }

    public readonly bool Equals(vtchar_t vt) 
        => vt.m_character == m_character && vt.Background.Equals(m_bg) && 
           vt.Foreground.Equals(rgb: m_fg) && 
           vt.m_styles == m_styles;

    public void Clear() {
        m_bg = RGB.Transparent;
        m_fg = RGB.White;

        m_styles = StyleFlag.NONE;
        m_character = ' ';
    }
}