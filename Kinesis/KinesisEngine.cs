using Kinesis.Input;
using Kinesis.Layout;
using Kinesis.Navigation;
using Kinesis.Processing;
using Kinesis.Rendering;
using Kinesis.UI;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis;

/// <summary>
/// Represent the heart of the library: This connects all systems to one class.
/// </summary>
public sealed class KinesisEngine: ISystemProvider {
    private readonly Renderer m_renderer = null!;
    private readonly InputSystem m_input = null!;

    private readonly WorkerSystem m_worker = null!;
    private readonly NavigationSystem m_navigator = null!;

    private readonly LayoutSystem m_layoutSystem = null!;
    private readonly List<SystemInvocationInfo> m_customSystems = null!;

    private readonly State<LayoutInfo> m_layoutInfo = null!;
    private readonly State<WorkerSystemState> m_workSyncState = null!;

    /// <summary>
    /// Create a new <see cref="KinesisEngine"/> instance.
    /// </summary>
    public KinesisEngine(string? title = null!, int x = -1, int y = -1) {
        Console.Out.Write(title == null ? $"\e]0;KinesisTUI\a" : $"\e]0;{title}\a");

        m_layoutInfo = new ValueState<LayoutInfo>();
        m_workSyncState = new ValueState<WorkerSystemState>(@default: WorkerSystemState.WAIT_FOR_RENDERER);

        m_layoutSystem = new LayoutSystem(m_layoutInfo, scale: new Vec2(x == -1 ? Console.BufferWidth : x, y == -1 ? Console.BufferHeight : y));
        m_input = new InputSystem();

        m_worker = WorkerSystem.Current;
        m_navigator = new NavigationSystem(provider: this);

        m_renderer = new Renderer(layout: m_layoutInfo, sync: m_workSyncState);
        m_customSystems = new List<SystemInvocationInfo>();

        m_customSystems.Add(new SystemInvocationInfo(null!, m_navigator, SystemInvocationTime.ON_CALL));
        RegisterBuiltInComponents();
    }

    public T? GetSystem<T>() where T: class, ISystem {
        for (int i = 0; i < m_customSystems.Count; ++i) {
            if (m_customSystems[i].When == SystemInvocationTime.ON_CALL && m_customSystems[i].System is T) {

                if (m_customSystems[i].System == null!)
                    m_customSystems[i] = m_customSystems[i] with { System = m_customSystems[i].Creation(this) };

                return m_customSystems[i].System as T;
            }
        }

        return default!;
    }

    public bool RegisterComponent<T>() where T: Component, IStaticType
        => ComponentRegistry.RegisterComponent<T>(name: T.Name);

    /// <summary>
    /// Add a system to the engine.
    /// </summary>
    /// <param name="action">The system itself.</param>
    /// <param name="when">Time of the invocation of the system.</param>
    public void AddSystem(SystemInvocationTime when, Func<ISystemProvider, ISystem> action)
        => m_customSystems.Add(item: new SystemInvocationInfo(action, null, when));

    /// <summary>
    /// Register a named route with a <paramref name="onCreate"/> method.
    /// </summary>
    /// <param name="name">Name of the route.</param>
    /// <param name="onCreate">Creation method of the <see cref="Island"/>.</param>
    /// <returns>Return <see langword="true"/>, if the route is successfully registered. Otherwise return <see langword="false"/>.</returns>
    public bool RegisterIsland<T>(string name, Func<ISystemProvider, T> onCreate) where T: Island
        => m_navigator.Register(name, onCreate);

    /// <summary>
    /// Start the <see cref="KinesisEngine"/> instance with the systems.
    /// </summary>
    public async Task Start(CancellationToken token = default) {
        bool firstRun = true;

        /* Run the starter systems. */
        await Run(invocation: SystemInvocationTime.ON_BEGIN);
        WorkerSystem.Current.AddSyncState(sync: m_workSyncState);

        /* Start main parts of the engine on different threads. (Input, Workers) */
        _ = Task.Run(action: () => m_worker.Run(), token);
        _ = Task.Run(action: () => m_input.Run(), token);

        _ = Task.Run(action: () => m_layoutSystem.Run(), token);
        while(!token.IsCancellationRequested) {

            /* Render the frame to the screen/terminal window. */
            await m_renderer.Render(entities: m_navigator.Current?.Tree ?? []);

            if (!firstRun) m_worker.AddRenderMessage(new RenderMessage(m_renderer.FrameTime, (int)m_renderer.FPS, m_layoutInfo.Value.Scale));
            else firstRun = false;
        }

        /* Run the shutdown systems. */
        await Run(invocation: SystemInvocationTime.ON_END);
    }

    private Task Run(SystemInvocationTime invocation) {
        ISystem system = null!;

        foreach (SystemInvocationInfo systemInfo in m_customSystems.Where(x => x.When == invocation)) {
            system = systemInfo.System ?? systemInfo.Creation(this);

            if (system.Behavior == SystemBehavior.DYNAMIC && systemInfo.System is IDynamicSystem dynamic)
                dynamic.Run();
        }

        return Task.CompletedTask;
    }

    private void RegisterBuiltInComponents() {
        this.RegisterComponent<RenderComponent>();

        this.RegisterComponent<Transform>();
        this.RegisterComponent<Hierarchy>();

        this.RegisterComponent<Style>();
        this.RegisterComponent<InteractionComponent>();

        this.RegisterComponent<RenderHierarchy>();
    }
}