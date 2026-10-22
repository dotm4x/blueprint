namespace Combine.Core.Events;

using Combine.Core.Primitives;

/// <summary>
/// Represents a dispatcher for specific events, allowing listeners
/// to be notified when the signal fires.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to signal listeners.</typeparam>
/// <example>
/// <code>
/// var onClick = new Signal&lt;ClickData&gt;();
///
/// onClick.Connect(data =>
/// {
///     Console.WriteLine($"Clicked {data.Id} {data.Count} times");
/// });
///
/// onClick.Fire(new ClickData("btn-submit", 3));
/// </code>
/// </example>
public class Signal<TPayload> : Event<TPayload>
{
    /// <summary>
    /// Dispatches the signal, invoking all connected listener callbacks
    /// with the provided payload.
    /// </summary>
    /// <param name="payload">The value passed to each event listener.</param>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the signal has already been destroyed.
    /// </exception>
    public void Fire(TPayload payload) => Dispatch(payload);
}

/// <summary>
/// Represents a dispatcher for events without a payload.
/// </summary>
/// <remarks>
/// Reuses the generic event implementation through the non-generic Event base class.
/// </remarks>
/// <example>
/// <code>
/// var onClick = new Signal();
///
/// onClick.Connect(() => Console.WriteLine("Clicked!"));
///
/// onClick.Fire();
/// </code>
/// </example>
public class Signal : Event
{
    /// <summary>
    /// Dispatches the signal, invoking all connected listener callbacks.
    /// </summary>
    /// <exception cref="DestroyedObjectException">
    /// Thrown when the signal has already been destroyed.
    /// </exception>
    public void Fire() => Dispatch();
}
