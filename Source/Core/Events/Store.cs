namespace Combine.Core.Events;

using Combine.Core.Primitives;

/// <summary>
/// Represents a change between two values.
/// </summary>
/// <typeparam name="TValue">The type of the values.</typeparam>
/// <param name="Previous">The value before the change.</param>
/// <param name="Current">The value after the change.</param>
public readonly record struct StoreChange<TValue>(TValue Previous, TValue Current);

/// <summary>
/// Provides read-only access to a store.
/// </summary>
/// <typeparam name="TValue">The type of the stored value.</typeparam>
public interface IReadOnlyStore<TValue> : IReadOnlyEvent<TValue>
{
    /// <summary>
    /// Gets an event that provides the previous and current values when the store changes.
    /// </summary>
    IReadOnlyEvent<StoreChange<TValue>> OnChange { get; }

    /// <summary>
    /// Gets the value held by the store before the most recent change.
    /// </summary>
    TValue Previous { get; }

    /// <summary>
    /// Gets the current value of the store.
    /// </summary>
    TValue Get();
}

/// <summary>
/// Provides full access to a store, including value mutation.
/// </summary>
/// <typeparam name="TValue">The type of the stored value.</typeparam>
public interface IStore<TValue> : IReadOnlyStore<TValue>, IEvent<TValue>
{
    /// <summary>
    /// Updates the value and notifies listeners only when it differs from the current value.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    void Set(TValue value);
}

/// <summary>
/// Initializes a new instance of the <see cref="Store{TValue}"/> class.
/// </summary>
/// <remarks>
/// Represents a reactive state container that holds a value and notifies listeners
/// when the value changes. The current and previous values are borrowed; changing or
/// destroying the store never destroys them.
/// </remarks>
/// <typeparam name="TValue">The type of value stored in the container.</typeparam>
/// <example>
/// <code>
/// var store = new Store&lt;int&gt;(0);
///
/// store.Connect(value =>
/// {
///     Console.WriteLine($"Current: {value}");
/// });
///
/// store.OnChange.Connect(change =>
/// {
///     Console.WriteLine($"Changed from {change.Previous} to {change.Current}.");
/// });
///
/// store.Set(5);
/// </code>
/// </example>
/// <param name="value">The initial value of the store.</param>
/// <param name="equals">
/// An optional equality function. A <see langword="true"/> result suppresses the update.
/// </param>
public class Store<TValue>(TValue value, Func<TValue, TValue, bool>? equals = null)
    : Event<TValue>,
        IStore<TValue>
{
    private readonly Signal<StoreChange<TValue>> _onChange = new();
    private TValue _value = value;

    /// <summary>
    /// Gets the current value of the store.
    /// </summary>
    public TValue Get() => _value;

    /// <summary>
    /// Gets an event that provides the previous and current values when the store changes.
    /// </summary>
    public IReadOnlyEvent<StoreChange<TValue>> OnChange => _onChange;

    /// <summary>
    /// Gets the value held before the most recent change.
    /// Initially, it equals the current value.
    /// </summary>
    public TValue Previous { get; private set; } = value;

    /// <summary>
    /// Updates the value and notifies listeners only when it differs from the current value.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the store has already been destroyed.
    /// </exception>
    public void Set(TValue value)
    {
        ThrowIfDestroyed();

        if (equals is not null)
        {
            if (equals(_value, value))
            {
                return;
            }
        }
        else if (EqualityComparer<TValue>.Default.Equals(_value, value))
        {
            return;
        }

        TValue previous = _value;

        Previous = previous;
        _value = value;

        _onChange.Fire(new StoreChange<TValue>(previous, value));
        Dispatch(value);
    }

    /// <summary>
    /// Destroys the change event and releases the store's connections.
    /// </summary>
    protected override void OnDestroy()
    {
        _onChange.Destroy();

        base.OnDestroy();
    }

    /// <summary>
    /// Converts a value into a new store containing that value.
    /// </summary>
    /// <param name="value">The initial value of the store.</param>
    public static implicit operator Store<TValue>(TValue value) => new(value);

    /// <summary>
    /// Returns the string representation of the current value.
    /// </summary>
    /// <returns>
    /// The string representation of the current value, or the literal <c>"null"</c>
    /// when the current value is <see langword="null"/>.
    /// </returns>
    public override string ToString() => _value?.ToString() ?? "null";
}
