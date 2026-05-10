using Kinesis.Core.Rendering;
using Kinesis.Utils;
using Kinesis.UI;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kinesis.Core;

/// <summary>
/// Represent smallest unit of work.
/// </summary>
/// <param name="Action">Callback of the work.</param>
/// <param name="Island">Container of the changed entities.</param>
internal record JobTarget(Delegate Action, Island Island, WorkTag Tag);

/// <summary>
/// Represent a bunch of workers for different tasks.
/// </summary>
internal class JobSystem: IDynamicSystem {

    #region PREDEFINES
    private const string DEDICATED_THREAD_NAME = "JobSystem::Worker";
    private const string ERR_SYNC_NOT_FOUND = "The synchronization context/state wasn't found.";

    private const int MAX_MSG_COUNT = 128;
    private const int MAX_MSG_RND = 1;

    private const int MAX_INTERACTION_COUNT = 1024;
    private const int POOLING_TIME = 8;
    #endregion

    private static JobSystem s_instance = null!;
    private readonly RingBuffer<JobTarget> m_targets = null!;

    private readonly RingBuffer<InputMessage> m_inputMessages = null!;
    private readonly RingBuffer<RenderMessage> m_renderMessages = null!;

    private readonly RingBuffer<LayoutMessage> m_layoutMessages = null!;
    private State<JobSystemStateInfo> m_workState = null!;

    /// <summary>
    /// Indicates behavior of the <see cref="JobSystem"/>.
    /// </summary>
    public SystemBehavior Behavior { get => SystemBehavior.DYNAMIC; }

    /// <summary>
    /// Current instance of the <see cref="JobSystem"/>.
    /// </summary>
    public static JobSystem Current { get => s_instance ??= new JobSystem(); }

    public JobSystem() {
        m_targets = new RingBuffer<JobTarget>(capacity: MAX_INTERACTION_COUNT);

        m_renderMessages = new RingBuffer<RenderMessage>(capacity: MAX_MSG_RND);
        m_inputMessages = new RingBuffer<InputMessage>(capacity: MAX_MSG_COUNT);

        m_layoutMessages = new RingBuffer<LayoutMessage>(capacity: MAX_MSG_COUNT);
    }

    /// <summary>
    /// Add synchronization context/state to the <see cref="JobSystem"/> from the <see cref="ImmediateRenderer"/>.
    /// </summary>
    /// <param name="sync">Synchronization state of the <see cref="KinesisEngine"/>.</param>
    /// <remarks>Remarks: If the state wasn't set, then the <see cref="JobSystem.Run"/> throws <see cref="InvalidOperationException"/> in the first run.</remarks>
    public void AddRenderSync(State<JobSystemStateInfo> sync) => m_workState ??= sync;

    /// <summary>
    /// Add new <see cref="InputMessage"/> to the workers.
    /// </summary>
    /// <param name="message">The message itself.</param>
    public void AddInputMessage(InputMessage message) {
        if(m_inputMessages.Count < m_inputMessages.Capacity)
            m_inputMessages.Write(message);
    }

    /// <summary>
    /// Add new <see cref="RenderMessage"/> to the workers.
    /// </summary>
    /// <param name="message">The message itself.</param>
    public void AddRenderMessage(RenderMessage message) {
        m_renderMessages.Write(message);
    }

    /// <summary>
    /// Add new <see cref="LayoutMessage"/> to the workers.
    /// </summary>
    /// <param name="message">The message itself.</param>
    public void AddLayoutMessage(LayoutMessage message) {
        if(m_layoutMessages.Count < m_layoutMessages.Capacity)
            m_layoutMessages.Write(message);
    }

    /// <summary>
    /// Add <paramref name="work"/> to the queue.
    /// </summary>
    /// <param name="work">Current work item.</param>
    public void AddCallback<T>(Action<T> work, Island island) where T: IWorkMessage
        => m_targets.Write(new JobTarget(work, island, T.Target));

    public void Run() {
        if (m_workState == null)
            throw new InvalidOperationException(message: ERR_SYNC_NOT_FOUND);

        Thread.CurrentThread.Name = DEDICATED_THREAD_NAME;

        while(true) {
            if (m_workState.Value.State != WorkerSystemState.OPEN_FOR_PROCESSING) {
                Thread.Sleep(millisecondsTimeout: POOLING_TIME);
                Debug.WriteLine($"[JobSystem] Current job-calls: {m_targets.Count}");
                continue;
            }

            Send<InputMessage>(messages: m_inputMessages);
            Send<LayoutMessage>(messages: m_layoutMessages);
            Send<RenderMessage>(messages: m_renderMessages);

            m_workState.Value.State = WorkerSystemState.WAIT_FOR_RENDERER;
        }
    }

    private void Send<T>(RingBuffer<T> messages) where T: struct, IWorkMessage {
        if (!messages.Read(out T message)) return;

        foreach(JobTarget target in m_targets) {
            if (!target.Island.IsActive)
                continue;

            Delegate _ref = target.Action;
            if (target.Tag == T.Target)
                Unsafe.As<Delegate, Action<T>>(ref _ref)(message);
        }
    }
}

/// <summary>
/// Simple state representation between the <see cref="ImmediateRenderer"/> and <see cref="JobSystem"/>.
/// </summary>
public enum WorkerSystemState: byte {
    /// <summary>
    /// Indicates the <see cref="JobSystem"/> can process one message from the queue.
    /// </summary>
    OPEN_FOR_PROCESSING,
    /// <summary>
    /// Indicates for the <see cref="JobSystem"/> wait to the <see cref="ImmediateRenderer"/>.
    /// </summary>
    WAIT_FOR_RENDERER
}

internal class JobSystemStateInfo {
    private WorkerSystemState m_state = WorkerSystemState.WAIT_FOR_RENDERER;

    public WorkerSystemState State { get => m_state; set => m_state = value; }
}