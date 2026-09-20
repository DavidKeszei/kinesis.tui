using Kinesis.Core.Rendering;
using Kinesis.Utils;
using Kinesis.UI;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

using RenderSyncContext = Kinesis.Core.State<Kinesis.Core.WorkerSystemState>;
using Kinesis.Core.Processing;

namespace Kinesis.Core;

/// <summary>
/// Represent a bunch of workers for different tasks.
/// </summary>
internal sealed class JobSystem: IDynamicSystem {
    #region PREDEFINES
    private const string DEDICATED_THREAD_NAME = "kinesis.tui::job_thread";
    private const string ERR_SYNC_NOT_FOUND    = "The synchronization context/state wasn't found.";

    private const int MAX_MSG_RND_COUNT   = 1;
    private const int MAX_MSG_INPUT_COUNT = 32;

    private const int PRE_ALLOC_INTERACTION_COUNT = 2048;
    private const int POOLING_TIME = 8;
    #endregion

    private static JobSystem s_instance = null!;

    private readonly JobHandler<InputMessage, InputJob> m_inputHandler = null!;
    private readonly JobHandler<RenderMessage, RenderJob> m_renderHandler = null!;

    private readonly FocusManager m_focus = null!;
    private RenderSyncContext m_renderSync = null!;

    /// <summary>
    /// Indicates behavior of the <see cref="JobSystem"/>.
    /// </summary>
    public SystemBehavior Behavior { get => SystemBehavior.DYNAMIC; }

    /// <summary>
    /// Current instance of the <see cref="JobSystem"/>.
    /// </summary>
    public static JobSystem Current { get => s_instance ??= new JobSystem(); }

    private JobSystem() {
        m_renderHandler = new JobHandler<RenderMessage, RenderJob>(capacity: 1);
        m_inputHandler = new JobHandler<InputMessage, InputJob>(capacity: 32);

        m_focus = new FocusManager(reference: m_inputHandler.Targets);
    }

    /// <summary>
    /// Add synchronization context/state to the <see cref="JobSystem"/> from the <see cref="Renderer"/>.
    /// </summary>
    /// <param name="sync">Synchronization state of the <see cref="KinesisEngine"/>.</param>
    /// <remarks>Remarks: If the state wasn't set, then the <see cref="JobSystem.Run"/> throws <see cref="InvalidOperationException"/> in the first run.</remarks>
    public void AddRenderSync(RenderSyncContext sync) => m_renderSync ??= sync;

    /// <summary>
    /// Add processable message to the <see cref="JobSystem"/>.
    /// </summary>
    /// <typeparam name="T">Type of the message.</typeparam>
    /// <param name="message">Message itself.</param>
    public void AddMessage<T>(T message) where T: IJobMessage {
        if (T.Target == JobTag.INPUT) {

            ref InputMessage input = ref Unsafe.As<T, InputMessage>(ref message);
            if (MoveFocusIndex(input)) return;

            m_inputHandler.AddMessage(input);
            return;
        }

        m_renderHandler.AddMessage(Unsafe.As<T, RenderMessage>(ref message));
    }

    /// <summary>
    /// Add <paramref name="work"/> to the queue.
    /// </summary>
    /// <param name="work">Current work item.</param>
    /// <param name="island">Root <see cref="Island"/> instance of the work.</param>
    /// <param name="isFocusBased">Indicates the job requires some focus-based behavior.</param>
    /// <returns>Returns a <see cref="State{T}"/> instance, which helps request and track state of the job.</returns>
    public State<JobRequestIntent>? AddCallback<T>(Action<T> work, Island island, bool isFocusBased) where T: IJobMessage {
        if (work == null || island == null) return null!;

        State<JobRequestIntent> state = new ValueState<JobRequestIntent>(@default: JobRequestIntent.ACTIVE);
        if (T.Target == JobTag.INPUT) {

            InputJob input = new InputJob(island, Unsafe.As<Action<T>, Action<InputMessage>>(ref work), state, !isFocusBased);
            m_inputHandler.AddQueue.Enqueue(input);

            return state;
        }

        RenderJob render = new RenderJob(island, Unsafe.As<Action<T>, Action<RenderMessage>>(ref work), state);
        m_renderHandler.AddQueue.Enqueue(render);

        return state;
    }

    public void Run() {
        if (m_renderSync == null)
            throw new InvalidOperationException(message: ERR_SYNC_NOT_FOUND);

        Thread.CurrentThread.Name = DEDICATED_THREAD_NAME;
        bool firstRun = true;
        
        while(true) {
            if (m_renderSync.Value != WorkerSystemState.OPEN_FOR_PROCESSING) {
                Thread.Sleep(millisecondsTimeout: POOLING_TIME);
                continue;
            }

            m_inputHandler.Process();
            m_renderHandler.Process();

            // Search for the first focusable element from the UI
            if (firstRun) {
                m_focus.Next();
                firstRun = false;
            }

            m_renderSync.Value = WorkerSystemState.WAIT_FOR_RENDERER;
        }
    }

    private bool MoveFocusIndex(InputMessage input) {
        if (input.IsPressed && input.Key == '\t' && input.Modifiers == InputModifier.L_SHIFT) {
            m_focus.Next();
            return true;
        }

        return false;
    }
}

/// <summary>
/// Simple state representation between the <see cref="Renderer"/> and <see cref="JobSystem"/>.
/// </summary>
public enum WorkerSystemState: byte {
    /// <summary>
    /// Indicates the <see cref="JobSystem"/> can process one message from the queue.
    /// </summary>
    OPEN_FOR_PROCESSING,
    /// <summary>
    /// Indicates for the <see cref="JobSystem"/> wait to the <see cref="Renderer"/>.
    /// </summary>
    WAIT_FOR_RENDERER
}