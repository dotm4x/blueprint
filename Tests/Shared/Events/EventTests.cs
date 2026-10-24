using Blueprint.Shared.Events;
using Blueprint.Shared.Primitives;

namespace Tests.Shared.Events;

public class EventTests
{
    [Fact]
    public void Connect_AndDispatch_InvokesConnectedCallback()
    {
        Signal<int> signal = new();
        List<int> received = [];
        EventConnection<int> connection = signal.Connect(received.Add);

        signal.Fire(42);

        Assert.True(connection.Connected);
        Assert.Equal([42], received);
    }

    [Fact]
    public void Disconnect_PreventsFurtherDispatch_AndIsIdempotent()
    {
        Signal<int> signal = new();
        int calls = 0;
        EventConnection<int> connection = signal.Connect(_ => calls++);
        connection.Disconnect();
        connection.Disconnect();

        signal.Fire(1);

        Assert.False(connection.Connected);
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Clear_PreservesPersistentConnections()
    {
        Signal<int> signal = new();
        int regularCalls = 0;
        int persistentCalls = 0;
        EventConnection<int> regular = signal.Connect(_ => regularCalls++);
        EventConnection<int> persistent = signal.Connect(_ => persistentCalls++, persistent: true);

        signal.Clear();
        signal.Fire(1);

        Assert.False(regular.Connected);
        Assert.True(persistent.Connected);
        Assert.Equal(0, regularCalls);
        Assert.Equal(1, persistentCalls);
    }

    [Fact]
    public void Clear_Force_DisconnectsPersistentConnections()
    {
        Signal<int> signal = new();
        EventConnection<int> connection = signal.Connect(_ => { }, persistent: true);

        signal.Clear(force: true);

        Assert.False(connection.Connected);
    }

    [Fact]
    public void Dispatch_UsesSnapshotWhenListenersConnectDuringDispatch()
    {
        Signal<int> signal = new();
        int lateListenerCalls = 0;
        EventConnection<int>? lateListener = null;
        signal.Connect(_ => lateListener ??= signal.Connect(_ => lateListenerCalls++));

        signal.Fire(1);

        Assert.Equal(0, lateListenerCalls);

        signal.Fire(2);

        Assert.Equal(1, lateListenerCalls);
        Assert.True(lateListener!.Connected);
    }

    [Fact]
    public void Destroy_DetachesConnectionsAndRejectsFurtherOperations()
    {
        Signal<int> signal = new();
        EventConnection<int> connection = signal.Connect(_ => { });

        signal.Destroy();

        Assert.True(signal.Destroyed);
        Assert.False(connection.Connected);
        Assert.Throws<DestroyedObjectException>(() => signal.Connect(_ => { }));
        Assert.Throws<DestroyedObjectException>(() => signal.Clear());
    }
}
