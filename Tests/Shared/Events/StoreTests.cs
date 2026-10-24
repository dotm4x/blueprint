using Blueprint.Shared.Events;
using Blueprint.Shared.Primitives;

namespace Tests.Shared.Events;

public class StoreTests
{
    [Fact]
    public void Constructor_InitializesCurrentAndPreviousValues()
    {
        Store<int> store = new(5);

        Assert.Equal(5, store.Value);
        Assert.Equal(5, store.Previous);
    }

    [Fact]
    public void Value_WhenChanged_UpdatesAndDispatchesCurrentAndChangeEvents()
    {
        Store<int> store = new(1);
        List<int> values = [];
        List<StoreChange<int>> changes = [];
        store.Connect(values.Add);
        store.OnChange.Connect(changes.Add);

        store.Value = 3;

        Assert.Equal(1, store.Previous);
        Assert.Equal(3, store.Value);
        Assert.Equal([3], values);
        Assert.Equal([new StoreChange<int>(1, 3)], changes);
    }

    [Fact]
    public void Value_WhenEqual_DoesNotDispatchOrChangePrevious()
    {
        Store<string> store = new("same");
        int notifications = 0;
        store.Connect(_ => notifications++);
        store.OnChange.Connect(_ => notifications++);

        store.Value = new string("same");

        Assert.Equal("same", store.Value);
        Assert.Equal("same", store.Previous);
        Assert.Equal(0, notifications);
    }

    [Fact]
    public void Value_UsesProvidedEqualityFunction()
    {
        Store<string> store = new("first", (left, right) =>
            string.Equals(left, right, StringComparison.OrdinalIgnoreCase));
        int notifications = 0;
        store.Connect(_ => notifications++);

        store.Value = "FIRST";

        Assert.Equal("first", store.Value);
        Assert.Equal(0, notifications);
    }

    [Fact]
    public void Value_AfterDestroy_Throws()
    {
        Store<int> store = new(1);
        store.Destroy();

        Assert.Throws<DestroyedObjectException>(() => store.Value = 2);
    }

    [Fact]
    public void Destroy_DisconnectsChangeListeners()
    {
        Store<int> store = new(1);
        int changes = 0;
        store.OnChange.Connect(_ => changes++);

        store.Destroy();

        Assert.True(store.Destroyed);
        Assert.Throws<DestroyedObjectException>(() => store.OnChange.Connect(_ => { }));
        Assert.Equal(0, changes);
    }

    [Fact]
    public void ImplicitConversion_CreatesStoreWithValue()
    {
        Store<int> store = 7;

        Assert.Equal(7, store.Value);
    }

    [Fact]
    public void ToString_ReturnsCurrentValueOrNullLiteral()
    {
        Store<int> number = new(42);
        Store<string?> nullStore = new(null);

        Assert.Equal("42", number.ToString());
        Assert.Equal("null", nullStore.ToString());
    }
}
