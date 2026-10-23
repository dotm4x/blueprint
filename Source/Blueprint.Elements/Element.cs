using System.Numerics;
using Blueprint.Geometry.Primitives;
using Blueprint.Shared.Collections;
using Blueprint.Shared.Events;
using Blueprint.Shared.Interfaces;
using Blueprint.Shared.Primitives;

namespace Blueprint.Elements;

/// <summary>
/// Initializes a new instance of the <see cref="ElementCollection"/> class.
/// </summary>
/// <remarks>
/// Represents a collection of elements belonging to a parent element.
/// </remarks>
/// <param name="owner">The element that owns this collection.</param>
public class ElementCollection(Element owner) : ReactiveSet<Element>
{
    /// <summary>
    /// Gets an element by its identifier.
    /// </summary>
    public Element? GetByIdentifier(string identifier, bool recursive = false)
    {
        foreach (Element element in this)
        {
            if (element.Identifier == identifier)
            {
                return element;
            }

            if (!recursive)
            {
                continue;
            }

            Element? result = element.Elements.GetByIdentifier(identifier, true);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets an element of the specified type by its identifier.
    /// </summary>
    public TElement? GetByIdentifier<TElement>(string identifier, bool recursive = false)
        where TElement : Element
    {
        return GetElements<TElement>(recursive)
            .FirstOrDefault(element => element.Identifier == identifier);
    }

    /// <summary>
    /// Gets an element using a path relative to the owner.
    /// </summary>
    /// <remarks>
    /// Path segments are element identifiers.
    /// The <c>.</c> segment refers to the current element, and <c>..</c>
    /// refers to its parent.
    /// </remarks>
    public Element? GetByPath(string path)
    {
        Element? current = owner;

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
                    current = current.Elements.GetByIdentifier(segment);
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
    /// Gets an element of the specified type by its path.
    /// </summary>
    public TElement? GetByPath<TElement>(string path)
        where TElement : Element => GetByPath(path) as TElement;

    /// <summary>
    /// Gets all elements containing the specified tag.
    /// </summary>
    public IEnumerable<Element> GetByTag(string tag, bool recursive = false)
    {
        foreach (Element element in this)
        {
            if (element.Tags.Contains(tag))
            {
                yield return element;
            }

            if (!recursive)
            {
                continue;
            }

            foreach (Element descendant in element.Elements.GetByTag(tag, true))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Gets all elements of the specified type containing the specified tag.
    /// </summary>
    public IEnumerable<TElement> GetByTag<TElement>(string tag, bool recursive = false)
        where TElement : Element
    {
        foreach (Element element in this)
        {
            if (element is TElement typedElement && element.Tags.Contains(tag))
            {
                yield return typedElement;
            }

            if (!recursive)
            {
                continue;
            }

            foreach (TElement descendant in element.Elements.GetByTag<TElement>(tag, true))
            {
                yield return descendant;
            }
        }
    }

    private IEnumerable<TElement> GetElements<TElement>(bool recursive)
        where TElement : Element
    {
        foreach (Element element in this)
        {
            if (element is TElement typedElement)
            {
                yield return typedElement;
            }

            if (!recursive)
            {
                continue;
            }

            foreach (TElement descendant in element.Elements.GetElements<TElement>(true))
            {
                yield return descendant;
            }
        }
    }
}

/// <summary>
/// Represents a element in a hierarchical object graph.
/// </summary>
/// <remarks>
/// An element can contain child elements and participate in the lifecycle
/// through building, configuration, loading, unloading, and destruction.
///
/// Elements are identified by a unique identifier. An element whose parent
/// is <see langword="null"/> is a root element.
///
/// Build and configuration each run at most once, before the element's first
/// load. Configuration runs after building.
/// </remarks>
public class Element : Destroyable, IIdentifiable<string>
{
    private readonly Once _builder = new();
    private readonly Once _configurator = new();

    private bool _restoringParent;
    private HashSet<Element>? _loadRollbackPreserved;
    private bool _loadRollbackInitialized;

    /// <summary>
    /// Gets the parent element.
    /// </summary>
    public Store<Element?> Parent { get; init; }

    /// <summary>
    /// Gets whether the element remains loaded when its parent is unloaded.
    /// </summary>
    public bool Persistent { get; init; }

    /// <summary>
    /// Gets the elements owned by this element.
    /// </summary>
    public ElementCollection Elements { get; init; }

    /// <summary>
    /// Gets the element tags.
    /// </summary>
    public ReactiveSet<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the store containing the element's origin.
    /// </summary>
    /// <remarks>
    /// The origin defines the reference point of the element relative to
    /// its own size. It determines the point around which the element
    /// is positioned and transformed.
    ///
    /// Each coordinate is normalized, where (0, 0) represents the
    /// top-left corner and (1, 1) represents the bottom-right corner.
    /// </remarks>
    public Store<Vector2> Origin { get; init; } = new(Vector2.Zero);

    /// <summary>
    /// Gets the store containing the element's position.
    /// </summary>
    public Store<Position> Position { get; init; } =
        new(Blueprint.Geometry.Primitives.Position.Zero);

    /// <summary>
    /// Gets the store containing the element's rotation.
    /// </summary>
    public Store<Rotation> Rotation { get; init; } =
        new(Blueprint.Geometry.Primitives.Rotation.Zero);

    /// <summary>
    /// Gets the store containing the element's desired size.
    /// </summary>
    public Store<Size> Size { get; init; } = new(Blueprint.Geometry.Primitives.Size.Zero);

    /// <summary>
    /// Gets the store containing the minimum allowed size.
    /// </summary>
    public Store<Size?> MinimumSize { get; init; } = new(null);

    /// <summary>
    /// Gets the store containing the maximum allowed size.
    /// </summary>
    public Store<Size?> MaximumSize { get; init; } = new(null);

    /// <summary>
    /// Gets the store containing the preferred width-to-height ratio.
    /// </summary>
    public Store<float?> AspectRatio { get; init; } = new(null);

    /// <summary>
    /// Gets whether the element clips its rendered content.
    /// </summary>
    public Store<bool> Clip { get; init; }

    /// <summary>
    /// Gets the element's opacity.
    /// </summary>
    public Store<float> Opacity { get; init; } = new(1.0f);

    /// <summary>
    /// Gets the unique element identifier.
    /// </summary>
    public string Identifier { get; init; } = $"element-{Random.Shared.NextInt64():x}";

    /// <summary>
    /// Gets whether the element is currently loaded.
    /// </summary>
    public bool Loaded { get; private set; }

    /// <summary>
    /// Gets the hierarchical path of the element.
    /// </summary>
    /// <remarks>
    /// Each path segment is an element identifier.
    /// Root elements use their own identifier.
    /// </remarks>
    public string Path => Parent.Value is { } parent ? $"{parent.Path}/{Identifier}" : Identifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="Element"/> class.
    /// </summary>
    public Element()
    {
        Parent = new Store<Element?>(null);
        Elements = new ElementCollection(this);
        Clip = new Store<bool>(false);

        ValidateAspectRatio();
        AspectRatio.Connect(_ => ValidateAspectRatio(), true);
        Parent.Connect(OnParentChanged, true);
        Elements.OnAdd.Connect(OnElementAdded, true);
        Elements.OnRemove.Connect(OnElementRemoved, true);
    }

    /// <summary>
    /// Builds the child elements owned by this element.
    /// </summary>
    /// <returns>The child elements to register, in enumeration order.</returns>
    /// <remarks>
    /// Called at most once, before configuration and the first load.
    /// Elements added through object initializers are registered before
    /// built elements.
    /// If building fails, the failure is not retried.
    /// </remarks>
    protected virtual IEnumerable<Element> Build()
    {
        yield break;
    }

    /// <summary>
    /// Configures relationships and behavior after building, before loading.
    /// </summary>
    /// <remarks>
    /// Initial and built children are available here.
    /// This method is called at most once, including across subsequent load cycles.
    /// If configuration fails, the failure is not retried.
    /// </remarks>
    protected virtual void Configure() { }

    private void ValidateAspectRatio()
    {
        if (AspectRatio.Value is { } ratio && (!float.IsFinite(ratio) || ratio <= 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(AspectRatio),
                "Aspect ratio must be positive and finite."
            );
        }
    }

    private void BuildElements()
    {
        foreach (Element element in Build())
        {
            try
            {
                Elements.Add(element);
            }
            catch
            {
                if (element.Parent.Value is null && !element.Destroyed)
                {
                    element.Destroy();
                }

                throw;
            }
        }
    }

    private void ConfigureElements() => Configure();

    private void OnParentChanged(Element? parent)
    {
        if (_restoringParent)
        {
            return;
        }

        Element? previous = Parent.Previous;

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

        if (previous is not null && previous.Elements.Contains(this))
        {
            previous.Elements.Remove(this);
        }

        if (parent is not null && !parent.Elements.Contains(this))
        {
            parent.Elements.Add(this);
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

    private void OnElementAdded(Element element)
    {
        try
        {
            element.ThrowIfDestroyed();

            if (element.Parent.Value != this)
            {
                element.Parent.Value = this;
            }
        }
        catch
        {
            if (Elements.Contains(element))
            {
                Elements.Remove(element);
            }

            throw;
        }
    }

    private void OnElementRemoved(Element element)
    {
        if (element.Parent.Value == this)
        {
            element.Parent.Value = null;
        }
    }

    private void ValidateParent(Element? parent)
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

        HashSet<Element> visited = [];

        for (Element? ancestor = parent; ancestor is not null; ancestor = ancestor.Parent.Value)
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
    /// Called when the element is loaded.
    /// </summary>
    protected virtual void OnLoad() { }

    /// <summary>
    /// Called when the element is unloaded.
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

        foreach (Element element in Elements.ToArray())
        {
            element.Destroy();
        }

        Elements.Destroy();
        Tags.Destroy();
        Origin.Destroy();
        Position.Destroy();
        Rotation.Destroy();
        Size.Destroy();
        MinimumSize.Destroy();
        MaximumSize.Destroy();
        AspectRatio.Destroy();
        Clip.Destroy();
        Opacity.Destroy();
        Parent.Destroy();

        base.OnDestroy();
    }

    /// <summary>
    /// Loads the element and its children that are not already loaded.
    /// </summary>
    /// <remarks>
    /// The parent must be loaded before this element can be loaded.
    ///
    /// Building and configuration occur before the element enters the
    /// loaded state. <see cref="OnLoad"/> is invoked after this element enters
    /// the loaded state and before its children are loaded.
    ///
    /// If loading a child fails, newly loaded children are unloaded in reverse
    /// order, including persistent children, and completed initialization is
    /// undone through <see cref="OnUnload"/>. Previously loaded children are
    /// preserved. If rollback fails, this element remains loaded so that
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

        Element? parent = Parent.Value;

        if (parent is not null && !parent.Loaded)
        {
            throw new InvalidOperationException(
                $"{this} cannot be loaded because its parent is not loaded."
            );
        }

        _builder.Execute(BuildElements);
        _configurator.Execute(ConfigureElements);

        Loaded = true;

        bool initialized = false;
        HashSet<Element> previouslyLoaded = [.. Elements.Where(element => element.Loaded)];

        try
        {
            OnLoad();
            initialized = true;

            foreach (Element element in Elements)
            {
                if (!element.Loaded)
                {
                    element.Load();
                }
            }
        }
        catch (Exception exception)
        {
            List<Exception> failures = [];

            foreach (Element element in Elements.ToArray().Reverse())
            {
                if (!element.Loaded || previouslyLoaded.Contains(element))
                {
                    continue;
                }

                try
                {
                    element.Unload();
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

            if (failures.Count == 0)
            {
                throw;
            }

            _loadRollbackPreserved = previouslyLoaded;
            _loadRollbackInitialized = initialized;

            throw new AggregateException(
                "Element loading and rollback failed. Unload to retry cleanup.",
                [exception, .. failures]
            );
        }
    }

    /// <summary>
    /// Unloads the element and its loaded, non-persistent children.
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

        HashSet<Element>? preserved = _loadRollbackPreserved;
        bool rollingBack = preserved is not null;

        IEnumerable<Element> elements = rollingBack ? Elements.ToArray().Reverse() : Elements;

        foreach (Element element in elements)
        {
            if (element.Loaded && (!preserved?.Contains(element) ?? !element.Persistent))
            {
                element.Unload();
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
    public override string ToString() => $"Element \"{Path}\"";
}
