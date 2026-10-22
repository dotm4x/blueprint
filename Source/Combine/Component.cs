using Combine.Common;
using Combine.Common.Collections;
using Combine.Common.Events;
using Combine.Common.Primitives;
using Combine.Spatial;

namespace Combine;

/// <summary>
/// Represents a collection of components belonging to a parent component.
/// </summary>
/// <param name="owner">The component that owns this collection.</param>
public class ComponentCollection(Component owner) : ReactiveSet<Component>
{
    /// <summary>
    /// Gets a component by its identifier.
    /// </summary>
    public Component? GetByIdentifier(string identifier, bool recursive = false)
    {
        foreach (Component component in this)
        {
            if (component.Identifier == identifier)
            {
                return component;
            }

            if (!recursive)
            {
                continue;
            }

            Component? result = component.Components.GetByIdentifier(identifier, true);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a component of the specified type by its identifier.
    /// </summary>
    public TComponent? GetByIdentifier<TComponent>(string identifier, bool recursive = false)
        where TComponent : Component
    {
        return GetComponents<TComponent>(recursive)
            .FirstOrDefault(component => component.Identifier == identifier);
    }

    /// <summary>
    /// Gets a component using a path relative to the owner.
    /// </summary>
    /// <remarks>
    /// Path segments are component identifiers.
    /// The <c>.</c> segment refers to the current component, and <c>..</c>
    /// refers to its parent.
    /// </remarks>
    public Component? GetByPath(string path)
    {
        Component? current = owner;

        foreach (string segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            switch (segment)
            {
                case ".":
                {
                    break;
                }

                case "..":
                {
                    current = current.Parent.Value;
                    break;
                }

                default:
                {
                    current = current.Components.GetByIdentifier(segment);
                    break;
                }
            }

            if (current is null)
            {
                return null;
            }
        }

        return current;
    }

    /// <summary>
    /// Gets a component of the specified type by its path.
    /// </summary>
    public TComponent? GetByPath<TComponent>(string path)
        where TComponent : Component => GetByPath(path) as TComponent;

    /// <summary>
    /// Gets all components containing the specified tag.
    /// </summary>
    public IEnumerable<Component> GetByTag(string tag, bool recursive = false)
    {
        foreach (Component component in this)
        {
            if (component.Tags.Contains(tag))
            {
                yield return component;
            }

            if (!recursive)
            {
                continue;
            }

            foreach (Component descendant in component.Components.GetByTag(tag, true))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Gets all components of the specified type containing the specified tag.
    /// </summary>
    public IEnumerable<TComponent> GetByTag<TComponent>(string tag, bool recursive = false)
        where TComponent : Component
    {
        foreach (Component component in this)
        {
            if (component is TComponent typedComponent && component.Tags.Contains(tag))
            {
                yield return typedComponent;
            }

            if (!recursive)
            {
                continue;
            }

            foreach (TComponent descendant in component.Components.GetByTag<TComponent>(tag, true))
            {
                yield return descendant;
            }
        }
    }

    private IEnumerable<TComponent> GetComponents<TComponent>(bool recursive)
        where TComponent : Component
    {
        foreach (Component component in this)
        {
            if (component is TComponent typedComponent)
            {
                yield return typedComponent;
            }

            if (!recursive)
            {
                continue;
            }

            foreach (TComponent descendant in component.Components.GetComponents<TComponent>(true))
            {
                yield return descendant;
            }
        }
    }
}

/// <summary>
/// Represents a component in a hierarchical object graph.
/// </summary>
/// <remarks>
/// A component can contain child components and participate in the lifecycle
/// through composition, configuration, loading, unloading, and destruction.
///
/// Components are identified by a unique identifier. A component whose parent
/// is <see langword="null"/> is a root component.
///
/// Composition and configuration each run at most once, before the component's
/// first load. Configuration runs after composition.
/// </remarks>
public class Component : Destroyable, IIdentifiable<string>
{
    private readonly Once _composer = new();
    private readonly Once _configurator = new();

    private bool _restoringParent;
    private HashSet<Component>? _loadRollbackPreserved;
    private bool _loadRollbackInitialized;

    /// <summary>
    /// Gets the parent component.
    /// </summary>
    public Store<Component?> Parent { get; init; }

    /// <summary>
    /// Gets whether the component remains loaded when its parent is unloaded.
    /// </summary>
    public bool Persistent { get; init; }

    /// <summary>
    /// Gets the components owned by this component.
    /// </summary>
    public ComponentCollection Components { get; init; }

    /// <summary>
    /// Gets the component tags.
    /// </summary>
    public ReactiveSet<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the component transform store.
    /// </summary>
    public Store<Transform> Transform { get; init; }

    /// <summary>
    /// Gets whether the component clips its rendered content.
    /// </summary>
    public Store<bool> Clip { get; init; }

    /// <summary>
    /// Gets the unique component identifier.
    /// </summary>
    public string Identifier { get; init; } = $"component-{Random.Shared.NextInt64():x}";

    /// <summary>
    /// Gets whether the component is currently loaded.
    /// </summary>
    public bool Loaded { get; private set; }

    /// <summary>
    /// Gets the hierarchical path of the component.
    /// </summary>
    /// <remarks>
    /// Each path segment is a component identifier.
    /// Root components use their own identifier.
    /// </remarks>
    public string Path => Parent.Value is { } parent ? $"{parent.Path}/{Identifier}" : Identifier;

    /// <summary>
    /// Initializes a new component.
    /// </summary>
    public Component()
    {
        Parent = new Store<Component?>(null);
        Components = new ComponentCollection(this);
        Transform = new Store<Transform>(new Transform());
        Clip = new Store<bool>(false);

        Parent.Connect(OnParentChanged, true);
        Components.OnAdd.Connect(OnComponentAdded, true);
        Components.OnRemove.Connect(OnComponentRemoved, true);
    }

    /// <summary>
    /// Composes the child components owned by this component.
    /// </summary>
    /// <returns>The child components to register, in enumeration order.</returns>
    /// <remarks>
    /// Called at most once, before configuration and the first load.
    /// Components added through object initializers are registered before
    /// composed components.
    /// If composition fails, the failure is not retried.
    /// </remarks>
    protected virtual IEnumerable<Component> Compose()
    {
        yield break;
    }

    /// <summary>
    /// Configures relationships and behavior after composition, before loading.
    /// </summary>
    /// <remarks>
    /// Initial and composed children are available here.
    /// This method is called at most once, including across subsequent load cycles.
    /// If configuration fails, the failure is not retried.
    /// </remarks>
    protected virtual void Configure() { }

    private void ComposeComponents()
    {
        foreach (Component component in Compose())
        {
            try
            {
                Components.Add(component);
            }
            catch
            {
                if (component.Parent.Value is null && !component.Destroyed)
                {
                    component.Destroy();
                }

                throw;
            }
        }
    }

    private void ConfigureComponents() => Configure();

    private void OnParentChanged(Component? parent)
    {
        if (_restoringParent)
        {
            return;
        }

        Component? previous = Parent.Previous;

        try
        {
            ValidateParent(parent);
        }
        catch
        {
            _restoringParent = true;

            try
            {
                Parent.Value = previous;
            }
            finally
            {
                _restoringParent = false;
            }

            throw;
        }

        if (previous is not null && previous.Components.Contains(this))
        {
            previous.Components.Remove(this);
        }

        if (parent is not null && !parent.Components.Contains(this))
        {
            parent.Components.Add(this);
        }

        if (parent?.Loaded == true && !Loaded)
        {
            Load();
        }
        else if (parent?.Loaded != true && Loaded && !Persistent)
        {
            Unload();
        }
    }

    private void OnComponentAdded(Component component)
    {
        try
        {
            component.ThrowIfDestroyed();

            if (component.Parent.Value != this)
            {
                component.Parent.Value = this;
            }
        }
        catch
        {
            if (Components.Contains(component))
            {
                Components.Remove(component);
            }

            throw;
        }
    }

    private void OnComponentRemoved(Component component)
    {
        if (component.Parent.Value == this)
        {
            component.Parent.Value = null;
        }
    }

    private void ValidateParent(Component? parent)
    {
        if (parent is null)
        {
            return;
        }

        parent.ThrowIfDestroyed();

        if (ReferenceEquals(parent, this))
        {
            throw new InvalidOperationException($"{this} cannot be its own parent.");
        }

        HashSet<Component> visited = [];

        for (Component? ancestor = parent; ancestor is not null; ancestor = ancestor.Parent.Value)
        {
            if (ReferenceEquals(ancestor, this))
            {
                throw new InvalidOperationException(
                    $"Cannot set {parent} as the parent of {this} because it would create a cycle."
                );
            }

            if (!visited.Add(ancestor))
            {
                throw new InvalidOperationException(
                    $"Cannot assign {parent} because its hierarchy already contains a cycle."
                );
            }
        }
    }

    /// <summary>
    /// Called when the component is loaded.
    /// </summary>
    protected virtual void OnLoad() { }

    /// <summary>
    /// Called when the component is unloaded.
    /// </summary>
    protected virtual void OnUnload() { }

    /// <inheritdoc />
    protected override void OnDestroy()
    {
        if (Loaded)
        {
            Unload();
        }

        Parent.Value = null;

        foreach (Component component in Components.ToArray())
        {
            component.Destroy();
        }

        Components.Destroy();
        Tags.Destroy();
        Transform.Destroy();
        Clip.Destroy();
        Parent.Destroy();

        base.OnDestroy();
    }

    /// <summary>
    /// Loads the component and its children that are not already loaded.
    /// </summary>
    /// <remarks>
    /// The parent must be loaded before this component can be loaded.
    ///
    /// Composition and configuration occur before the component enters the
    /// loaded state. <see cref="OnLoad"/> is invoked after this component enters
    /// the loaded state and before its children are loaded.
    ///
    /// If loading a child fails, newly loaded children are unloaded in reverse
    /// order, including persistent children, and completed initialization is
    /// undone through <see cref="OnUnload"/>. Previously loaded children are
    /// preserved. If rollback fails, this component remains loaded so that
    /// <see cref="Unload"/> can retry cleanup. An incomplete <see cref="OnLoad"/>
    /// must clean up its own resources.
    /// </remarks>
    public void Load()
    {
        ThrowIfDestroyed();

        if (Loaded)
        {
            throw new InvalidOperationException($"{this} is already loaded.");
        }

        Component? parent = Parent.Value;

        if (parent is not null && !parent.Loaded)
        {
            throw new InvalidOperationException(
                $"{this} cannot be loaded because its parent is not loaded."
            );
        }

        _composer.Execute(ComposeComponents);
        _configurator.Execute(ConfigureComponents);

        Loaded = true;

        bool initialized = false;
        HashSet<Component> previouslyLoaded = [.. Components.Where(component => component.Loaded)];

        try
        {
            OnLoad();
            initialized = true;

            foreach (Component component in Components)
            {
                if (!component.Loaded)
                {
                    component.Load();
                }
            }
        }
        catch (Exception exception)
        {
            List<Exception> failures = [];

            foreach (Component component in Components.ToArray().Reverse())
            {
                if (!component.Loaded || previouslyLoaded.Contains(component))
                {
                    continue;
                }

                try
                {
                    component.Unload();
                }
                catch (Exception cleanupFailure)
                {
                    failures.Add(cleanupFailure);
                    break;
                }
            }

            if (failures.Count == 0)
            {
                try
                {
                    if (initialized)
                    {
                        OnUnload();
                    }

                    Loaded = false;
                }
                catch (Exception cleanupFailure)
                {
                    failures.Add(cleanupFailure);
                }
            }

            if (failures.Count != 0)
            {
                _loadRollbackPreserved = previouslyLoaded;
                _loadRollbackInitialized = initialized;

                throw new AggregateException(
                    "Component loading and rollback failed. Unload to retry cleanup.",
                    [exception, .. failures]
                );
            }

            throw;
        }
    }

    /// <summary>
    /// Unloads the component and its loaded, non-persistent children.
    /// </summary>
    /// <remarks>
    /// Persistent children retain their current loaded state during ordinary
    /// unloading. After a failed load rollback, this method also retries cleanup
    /// of newly loaded persistent children while preserving children that were
    /// already loaded before the attempt.
    /// </remarks>
    public void Unload()
    {
        ThrowIfDestroyed();

        if (!Loaded)
        {
            throw new InvalidOperationException($"{this} is not loaded.");
        }

        HashSet<Component>? preserved = _loadRollbackPreserved;
        bool rollingBack = preserved is not null;

        IEnumerable<Component> components = rollingBack
            ? Components.ToArray().Reverse()
            : Components;

        foreach (Component component in components)
        {
            if (component.Loaded && (!preserved?.Contains(component) ?? !component.Persistent))
            {
                component.Unload();
            }
        }

        if (!rollingBack || _loadRollbackInitialized)
        {
            OnUnload();
        }

        Loaded = false;
        _loadRollbackPreserved = null;
        _loadRollbackInitialized = false;
    }

    /// <inheritdoc />
    public override string ToString() => $"Component \"{Path}\"";
}
