using Kinesis.Core.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core.Processing;

/// <summary>
/// Helper class for managing focuses between <see cref="InputJob"/> instances.
/// </summary>
internal sealed class FocusManager {
    private readonly IReadOnlyList<InputJob> m_jobs = null!;
    private int m_currentFocus = -1;

    /// <summary>
    /// Create a new <see cref="FocusManager"/> from a existing list of <see cref="InputJob"/> reference.
    /// </summary>
    /// <param name="reference"></param>
    public FocusManager(IReadOnlyList<InputJob> reference) => m_jobs = reference;

    /// <summary>
    /// Go to the next focusable job in the queue.
    /// </summary>
    public void Next() {
        int index = (m_currentFocus + 1) % m_jobs.Count;

        while (index != m_currentFocus && (m_jobs[index].IsFocused || m_jobs[index].IsGlobal)) {
            index = ++index % m_jobs.Count;
        }

        if(m_currentFocus >= 0)
            m_jobs[m_currentFocus].IsFocused = false;

        m_jobs[index].IsFocused = true;
        m_currentFocus = index;
    }
}
