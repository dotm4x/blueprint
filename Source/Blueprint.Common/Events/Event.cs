using Blueprint.Common.Primitives;

namespace Blueprint.Common.Events;

/// <summary>
/// Provides read-only access to an event.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to event listeners.</typeparam>
public interface IReadOnlyEvent<TPayload> : IReadOnlyDestroyable
{
    /// <summary>
    /// Subscribes a callback function to the event.
    /// </summary>
    /// <param name="callback">The function to be called when the event occurs.</param>
    /// <param name="persistent">
    /// Whether the connection should survive standard clearing operations.
    /// </param>
    /// <returns>The event connection.</returns>
    EventConnection<TPayload> Connect(Action<TPayload> callback, bool persistent = false);
}

/// <summary>
/// Provides read-only access to an event without a payload.
/// </summary>
public interface IReadOnlyEvent : IReadOnlyDestroyable
{
    /// <summary>
    /// Subscribes a callback function to the event.
    /// </summary>
    /// <param name="callback">The function to be called when the event occurs.</param>
    /// <param name="persistent">
    /// Whether the connection should survive standard clearing operations.
    /// </param>
    /// <returns>The event connection.</returns>
    EventConnection<Unit> Connect(Action callback, bool persistent = false);
}

/// <summary>
/// Provides full access to an event, including connection management and destruction.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to event listeners.</typeparam>
public interface IEvent<TPayload> : IReadOnlyEvent<TPayload>, IDestroyable
{
    /// <inheritdoc cref="IReadOnlyDestroyable.Destroyed" />
    new bool Destroyed { get; }

    /// <summary>
    /// Clears event connections. By default, removes only non-persistent connections.
    /// </summary>
    /// <param name="force">
    /// If true, clears all connections, including persistent ones.
    /// </param>
    void Clear(bool force = false);
}

/// <summary>
/// Provides full access to an event without a payload.
/// </summary>
public interface IEvent : IReadOnlyEvent, IDestroyable
{
    /// <inheritdoc cref="IReadOnlyDestroyable.Destroyed" />
    new bool Destroyed { get; }

    /// <summary>
    /// Clears event connections. By default, removes only non-persistent connections.
    /// </summary>
    /// <param name="force">
    /// If true, clears all connections, including persistent ones.
    /// </param>
    void Clear(bool force = false);
}

/// <summary>
/// Base class for managing and dispatching events with type-safe payloads.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to event listeners.</typeparam>
public abstract class Event<TPayload> : Destroyable, IEvent<TPayload>
{
    /// <summary>
    /// Gets the active connections for the event.
    /// </summary>
    protected readonly HashSet<EventConnection<TPayload>> Connections = [];

    /// <summary>
    /// Clears event connections. By default, removes only non-persistent connections.
    /// </summary>
    /// <param name="force">
    /// If true, clears all connections, including persistent ones.
    /// </param>
    public void Clear(bool force = false)
    {
        ThrowIfDestroyed();

        foreach (EventConnection<TPayload> connection in Connections.ToArray())
        {
            if (force || !connection.Persistent)
            {
                connection.Disconnect();
            }
        }
    }

    /// <summary>
    /// Subscribes a callback function to the event.
    /// </summary>
    /// <param name="callback">The function to be called when the event occurs.</param>
    /// <param name="persistent">
    /// Whether the connection should survive standard clearing operations.
    /// </param>
    /// <returns>The event connection.</returns>
    public EventConnection<TPayload> Connect(Action<TPayload> callback, bool persistent = false)
    {
        ThrowIfDestroyed();

        var connection = new EventConnection<TPayload>(
            callback,
            persistent,
            connection => Connections.Remove(connection)
        );

        Connections.Add(connection);

        return connection;
    }

    /// <summary>
    /// Dispatches a payload to all currently connected listeners.
    /// </summary>
    /// <param name="payload">The value passed to each event listener.</param>
    /// <remarks>
    /// Uses a snapshot so listeners can safely connect or disconnect
    /// while the event is being dispatched.
    /// </remarks>
    protected void Dispatch(TPayload payload)
    {
        ThrowIfDestroyed();

        if (Connections.Count == 0)
        {
            return;
        }

        foreach (EventConnection<TPayload> connection in Connections.ToArray())
        {
            if (connection.Connected)
            {
                connection.Callback(payload);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnDestroy()
    {
        foreach (EventConnection<TPayload> connection in Connections)
        {
            connection.Detach();
        }

        Connections.Clear();

        base.OnDestroy();
    }
}

/// <summary>
/// Base class for managing and dispatching events without a payload.
/// </summary>
/// <remarks>
/// Reuses the generic event implementation internally.
/// </remarks>
public abstract class Event : Event<Unit>, IEvent
{
    /// <summary>
    /// Subscribes a callback function to the event.
    /// </summary>
    /// <param name="callback">The function to be called when the event occurs.</param>
    /// <param name="persistent">
    /// Whether the connection should survive standard clearing operations.
    /// </param>
    /// <returns>The event connection.</returns>
    public EventConnection<Unit> Connect(Action callback, bool persistent = false)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return base.Connect(_ => callback(), persistent);
    }

    /// <summary>
    /// Dispatches the event without a payload.
    /// </summary>
    protected void Dispatch() => base.Dispatch(Unit.Value);
}
