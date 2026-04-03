using Kinesis.UI.Components;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI;

public ref struct ComponentIterator<T> where T: Component {
    private readonly IEnumerable<T> m_components = null!;
    private readonly int m_count = 0;

    private int m_current = -1;

    public readonly T Current { get => m_components.ElementAt(m_current); }

    public ComponentIterator(IEnumerable<T> components, uint count) {
        m_components = components;
        m_count = (int)count;
    }

    public bool MoveNext() {
        if (++m_current < m_count)
            return true;

        return false;
    }
}

public ref struct StyleEnumerator: IDisposable {
    private const int MAX_STYLE_LEN = 24;

    private readonly ComponentIterator<Style> m_styles = default!;
    private Style[] m_pooled = null!;

    public StyleEnumerator(Entity entity) {
        Style[] pooled = ArrayPool<Style>.Shared.Rent(minimumLength: MAX_STYLE_LEN);
        uint count = 0;

        foreach (Component component in entity) {

            if (component.TypeOf(Style.Name))
                pooled[count++] = (Style)component;
        }

        m_styles = new ComponentIterator<Style>(pooled, count);
        m_pooled = pooled;
    }

    public readonly ComponentIterator<Style> GetEnumerator() => m_styles;

    public void Dispose() {
        if (m_pooled != null) {
            ArrayPool<Style>.Shared.Return(m_pooled);
            m_pooled = null!;
        }
    }
}