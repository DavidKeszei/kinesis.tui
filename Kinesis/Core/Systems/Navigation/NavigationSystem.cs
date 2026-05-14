using System;
using System.Collections.Generic;
using System.Text;
using Kinesis.UI;

namespace Kinesis.Core;

/// <summary>
/// Represent a stack-based navigator in the library.
/// </summary>
public class NavigationSystem: INavigator {
    private readonly Dictionary<string, NavigationTarget> m_routes = null!;
    private readonly Stack<Island> m_navigationFrame = null!;

    private readonly ISystemProvider m_provider = null!;

    public SystemBehavior Behavior { get => SystemBehavior.STATIC; }

    /// <summary>
    /// Current page of the application.
    /// </summary>
    internal Island Current { 
        get {
            if (!m_navigationFrame.TryPeek(out Island? page))
                return null!;

            if (page.Tree.Count == 0)
                page.BuildTree(context: new BuildContext(current: page) { Root = page });

            return page;
        } 
    }

    internal NavigationSystem(ISystemProvider provider) {
        m_provider = provider;

        m_navigationFrame = new Stack<Island>();
        m_routes = new Dictionary<string, NavigationTarget>();
    }

    /// <summary>
    /// Register route to the <see cref="NavigationSystem"/> with name.
    /// </summary>
    /// <param name="route">Route identifier of the page.</param>
    /// <param name="creationMethod">Page of the route.</param>
    /// <returns>Return <see langword="true"/>, if the route is successfully added to the <see cref="NavigationSystem"/>.</returns>
    internal bool Register(string route, Func<ISystemProvider, Island> creationMethod) {
        bool success = m_routes.TryAdd(route, new NavigationTarget(creationMethod, null!));
        if (m_navigationFrame.Count == 0 && success) {

            m_routes[route] = new NavigationTarget(Creation: null!, Page: m_routes[route].Creation(m_provider));
            m_navigationFrame.Push(m_routes[route].Page!);

            m_routes[route].Page!.IsActive = true;
        }

        return success;
    }

    /// <summary>
    /// Navigate to the specified <paramref name="page"/>.
    /// </summary>
    /// <param name="page">Creation method for the page.</param>
    public void NavigateTo(Func<ISystemProvider, Island> page) {
        Island target = page(m_provider);
        target.IsActive = true;

        m_navigationFrame.Push(target);
    }

    public void NavigateTo(string route) {
        (Func<ISystemProvider, Island> creation, Island? island) = m_routes[route];
        island ??= creation(m_provider);

        if (island is INavigationHandler handler) 
            handler.OnNavigation(isBack: false);

        island.IsActive = true;
        m_navigationFrame.Peek().IsActive = false;

        m_navigationFrame.Push(island);
    }

    /// <summary>
    /// Navigate back to the previous <see cref="Island"/>.
    /// </summary>
    public void NavigateBack() {
        Island current = m_navigationFrame.Pop();
        current.IsActive = false;

        if (current is INavigationHandler handler)
            handler.OnNavigation(isBack: true);

        m_navigationFrame.Peek().IsActive = true;
    }
}