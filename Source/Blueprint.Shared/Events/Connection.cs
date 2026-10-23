using Blueprint.Shared.Primitives;

namespace Blueprint.Shared.Events;

/// <summary>
/// Manages a group of event connections.
/// </summary>
/// <remarks>
/// Allows connections with different payload types to be managed together.
/// Disconnecting or destroying the group disconnects its registered
/// connections without destroying their associated events.
/// </remarks>
public sealed class EventConnectionGroup : Destroyable
{
    private readonly HashSet<IEventConnection> _connections = [];

    /// <summary>
    /// Gets the number of connections registered in the group.
    /// </summary>
    public int Count => _connections.Count;

    /// <inheritdoc />
    protected override void OnDestroy()
    {
        Disconnect();

        base.OnDestroy();
    }

    /// <summary>
    /// Adds a connection to the group.
    /// </summary>
    /// <param name="connection">The connection to register.</param>
    /// <returns>
    /// True if the connection was added; otherwise, false.
    /// </returns>
    public bool Add(IEventConnection connection)
    {
        ThrowIfDestroyed();

        return _connections.Add(connection);
    }

    /// <summary>
    /// Disconnects all registered connections and clears the group.
    /// </summary>
    /// <remarks>
    /// Calling this method multiple times has no additional effect.
    /// The group remains available for new connections afterward.
    /// </remarks>
    public void Disconnect()
    {
        ThrowIfDestroyed();

        foreach (IEventConnection connection in _connections.ToArray())
        {
            connection.Disconnect();
        }

        _connections.Clear();
    }

    /// <summary>
    /// Removes a connection from the group without disconnecting it.
    /// </summary>
    /// <param name="connection">The connection to remove.</param>
    /// <returns>
    /// True if the connection was removed; otherwise, false.
    /// </returns>
    public bool Remove(IEventConnection connection)
    {
        ThrowIfDestroyed();

        return _connections.Remove(connection);
    }
}

/// <summary>
/// Represents a connection between an event and a callback.
/// </summary>
public interface IEventConnection
{
    /// <summary>
    /// Gets a value indicating whether the connection is currently active.
    /// </summary>
    bool Connected { get; }

    /// <summary>
    /// Gets a value indicating whether the connection persists after being cleared.
    /// </summary>
    bool Persistent { get; }

    /// <summary>
    /// Disconnects the callback from its event.
    /// </summary>
    void Disconnect();
}

/// <summary>
/// Represents an active connection between an event and a callback.
/// </summary>
/// <typeparam name="TPayload">The type of the value passed to the callback.</typeparam>
public class EventConnection<TPayload> : IEventConnection
{
    private Action<EventConnection<TPayload>>? _disconnect;

    internal EventConnection(
        Action<TPayload> callback,
        bool persistent,
        Action<EventConnection<TPayload>> disconnect
    )
    {
        Callback = callback;
        Persistent = persistent;
        _disconnect = disconnect;
    }

    /// <summary>
    /// Gets the callback function executed when the event is dispatched.
    /// </summary>
    public Action<TPayload> Callback { get; }

    /// <summary>
    /// Gets a value indicating whether the connection is currently active.
    /// </summary>
    public bool Connected => _disconnect is not null;

    /// <summary>
    /// Disconnects the callback from its event.
    /// </summary>
    /// <remarks>
    /// Calling this method more than once has no effect.
    /// </remarks>
    public void Disconnect()
    {
        Action<EventConnection<TPayload>>? disconnect = _disconnect;

        if (disconnect is null)
        {
            return;
        }

        _disconnect = null;
        disconnect(this);
    }

    /// <summary>
    /// Gets a value indicating whether the connection persists after being cleared.
    /// </summary>
    public bool Persistent { get; }

    /// <summary>
    /// Marks the connection as disconnected without invoking its disconnect callback.
    /// </summary>
    internal void Detach() => _disconnect = null;
}
