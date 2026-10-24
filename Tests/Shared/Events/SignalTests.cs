using Blueprint.Shared.Events;
using Blueprint.Shared.Primitives;

namespace Tests.Shared.Events;

public class SignalTests
{
    [Fact]
    public void Fire_DispatchesPayloadToConnectedListeners()
    {
        Signal<int> signal = new();
        List<int> received = [];
        signal.Connect(received.Add);
        signal.Connect(value => received.Add(value * 2));

        signal.Fire(3);

        Assert.Equal(2, received.Count);
        Assert.Contains(3, received);
        Assert.Contains(6, received);
    }

    [Fact]
    public void Fire_AfterDestroy_Throws()
    {
        Signal<int> signal = new();
        signal.Destroy();

        Assert.Throws<DestroyedObjectException>(() => signal.Fire(1));
    }

    [Fact]
    public void Fire_WithoutPayload_InvokesConnectedListeners()
    {
        Signal signal = new();
        int calls = 0;
        signal.Connect(() => calls++);

        signal.Fire();

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Connect_WithoutPayload_NullCallback_Throws()
    {
        Signal signal = new();

        Assert.Throws<ArgumentNullException>(() => signal.Connect(null!));
    }
}
