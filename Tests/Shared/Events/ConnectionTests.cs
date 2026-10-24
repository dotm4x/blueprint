using Blueprint.Shared.Events;
using Blueprint.Shared.Primitives;

namespace Tests.Shared.Events;

public class ConnectionTests
{
    [Fact]
    public void Add_RegistersConnectionOnlyOnce()
    {
        Signal signal = new();
        EventConnection<Unit> connection = signal.Connect(() => { });
        EventConnectionGroup group = new();

        Assert.True(group.Add(connection));
        Assert.False(group.Add(connection));
        Assert.Equal(1, group.Count);
    }

    [Fact]
    public void Remove_UnregistersWithoutDisconnectingConnection()
    {
        Signal signal = new();
        EventConnection<Unit> connection = signal.Connect(() => { });
        EventConnectionGroup group = new();
        group.Add(connection);

        bool removed = group.Remove(connection);
        signal.Fire();

        Assert.True(removed);
        Assert.True(connection.Connected);
        Assert.Equal(0, group.Count);
    }

    [Fact]
    public void Disconnect_DisconnectsAllConnectionsAndAllowsReuse()
    {
        Signal signal = new();
        int calls = 0;
        EventConnection<Unit> connection = signal.Connect(() => calls++);
        EventConnectionGroup group = new();
        group.Add(connection);

        group.Disconnect();
        group.Disconnect();
        signal.Fire();

        Assert.False(connection.Connected);
        Assert.Equal(0, group.Count);
        Assert.Equal(0, calls);

        EventConnection<Unit> nextConnection = signal.Connect(() => calls++);
        Assert.True(group.Add(nextConnection));
        Assert.Equal(1, group.Count);
    }

    [Fact]
    public void Destroy_DisconnectsRegisteredConnections()
    {
        Signal signal = new();
        EventConnection<Unit> connection = signal.Connect(() => { });
        EventConnectionGroup group = new();
        group.Add(connection);

        group.Destroy();

        Assert.False(connection.Connected);
        Assert.Equal(0, group.Count);
        Assert.Throws<DestroyedObjectException>(() => group.Add(connection));
        Assert.Throws<DestroyedObjectException>(() => group.Disconnect());
        Assert.Throws<DestroyedObjectException>(() => group.Remove(connection));
    }
}
