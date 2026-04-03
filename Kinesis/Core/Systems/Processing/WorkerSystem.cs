using Kinesis.Core.Rendering;
using Kinesis.Utils;
using Kinesis.UI;

namespace Kinesis.Core;

/// <summary>
/// Represent smallest unit of work.
/// </summary>
/// <param name="Action">Callback of the work.</param>
/// <param name="Island">Container of the changed entities.</param>
internal record WorkTarget(Delegate Action, Island Island, WorkTag Tag);

/// <summary>
/// Represent a bunch of workers for different tasks.
/// </summary>
internal class WorkerSystem: IDynamicSystem {
    private const string DEDICATED_THREAD_NAME = "<Thread> Worker";
    private const string ERR_SYNC_NOT_FOUND = "The synchronization context/state wasn't found.";

    private const int MAX_MSG_COUNT = 128;
    private const int MAX_MSG_RND = 1;

    private const int MAX_INTERACTION_COUNT = 1024;
    private const int POOLING_TIME = 2;

    private static WorkerSystem m_instance = null!;
    private readonly RingBuffer<WorkTarget> m_targets = null!;

    private readonly RingBuffer<InputMessage> m_inputMessages = null!;
    private readonly RingBuffer<RenderMessage> m_renderMessages = null!;

    private readonly RingBuffer<LayoutMessage> m_layoutMessages = null!;
    private State<WorkerSystemState> m_workSync = null!;

    /// <summary>
    /// Indicates behavior of the <see cref="WorkerSystem"/>.
    /// </summary>
    public SystemBehavior Behavior { get => SystemBehavior.DYNAMIC; }

    /// <summary>
    /// Current instance of the <see cref="WorkerSystem"/>.
    /// </summary>
    public static WorkerSystem Current { get => m_instance ??= new WorkerSystem(); }

    public WorkerSystem() {
        m_targets = new RingBuffer<WorkTarget>(capacity: MAX_INTERACTION_COUNT);

        m_renderMessages = new RingBuffer<RenderMessage>(capacity: MAX_MSG_RND);
        m_inputMessages = new RingBuffer<InputMessage>(capacity: MAX_MSG_COUNT);

        m_layoutMessages = new RingBuffer<LayoutMessage>(capacity: MAX_MSG_COUNT);
    }

    /// <summary>
    /// Add synchronization context/state to the <see cref="WorkerSystem"/>.
    /// </summary>
    /// <param name="sync">Synchronization state of the <see cref="KinesisEngine"/>.</param>
    /// <remarks>Remarks: If the state wasn't set, then the <see cref="WorkerSystem.Run"/> throws <see cref="InvalidOperationException"/> in the first run.</remarks>
    public void AddSyncState(State<WorkerSystemState> sync) => m_workSync ??= sync;

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
        if(m_renderMessages.Count < m_renderMessages.Capacity)
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
        => m_targets.Write(new WorkTarget(work, island, T.Target));

    public void Run() {
        if (m_workSync == null)
            throw new InvalidOperationException(message: ERR_SYNC_NOT_FOUND);

        Thread.CurrentThread.Name = DEDICATED_THREAD_NAME;

        while(true) {
            if (m_workSync != WorkerSystemState.OPEN_FOR_PROCESSING) {
                Thread.Sleep(millisecondsTimeout: POOLING_TIME);
                continue;
            }

            Send<InputMessage>(messages: m_inputMessages);
            Send<LayoutMessage>(messages: m_layoutMessages);
            Send<RenderMessage>(messages: m_renderMessages);

            m_workSync.Value = WorkerSystemState.WAIT_FOR_RENDERER;
        }
    }

    private void Send<T>(RingBuffer<T> messages) where T: struct, IWorkMessage {
        if (!messages.Read(out T message)) return;

        foreach(WorkTarget target in m_targets) {
            if (!target.Island.IsActive)
                continue;

            if (target.Tag == T.Target && target.Action is Action<T> action)
                action(message);
        }
    }
}

/// <summary>
/// Simple state representation between the <see cref="Renderer"/> and <see cref="WorkerSystem"/>.
/// </summary>
public enum WorkerSystemState: byte {
    /// <summary>
    /// Indicates the <see cref="WorkerSystem"/> can process one message from the queue.
    /// </summary>
    OPEN_FOR_PROCESSING,
    /// <summary>
    /// Indicates for the <see cref="WorkerSystem"/> wait to the <see cref="Renderer"/>.
    /// </summary>
    WAIT_FOR_RENDERER
}