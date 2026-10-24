using Blueprint.Shared.Collections;
using Blueprint.Shared.Primitives;

namespace Tests.Shared.Collections;

public class ReactiveSetTests
{
    [Fact]
    public void Constructor_AddsInitialItemsAndRejectsNegativeLimit()
    {
        ReactiveSet<int> set = [1, 2];

        Assert.Equal([1, 2], set.ToArray());
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReactiveSet<int>(limit: -1));
        Assert.Throws<InvalidOperationException>(() => new ReactiveSet<int>([1, 1]));
    }

    [Fact]
    public void Add_AddsItemsAndFiresOnAdd()
    {
        ReactiveSet<int> set = new();
        List<int> added = [];
        set.OnAdd.Connect(added.Add);

        int[] result = set.Add(1, 2);

        Assert.Equal([1, 2], result);
        Assert.Equal([1, 2], set.ToArray());
        Assert.Equal([1, 2], added);
    }

    [Fact]
    public void Add_Duplicate_Throws()
    {
        ReactiveSet<int> set = [1];

        Assert.Throws<InvalidOperationException>(() => set.Add(1));
    }

    [Fact]
    public void Add_WhenAtLimit_RemovesOldestItem()
    {
        ReactiveSet<int> set = new([1, 2], limit: 2);
        List<int> removed = [];
        set.OnRemove.Connect(removed.Add);

        set.Add(3);

        Assert.Equal([2, 3], set.ToArray());
        Assert.Equal([1], removed);
    }

    [Fact]
    public void Remove_FiresOnRemoveAndRejectsMissingItems()
    {
        ReactiveSet<int> set = [1, 2];
        List<int> removed = [];
        set.OnRemove.Connect(removed.Add);

        set.Remove(1);

        Assert.Equal([2], set.ToArray());
        Assert.Equal([1], removed);
        Assert.Throws<InvalidOperationException>(() => set.Remove(1));
    }

    [Fact]
    public void Clear_FiresOnClearBeforeRemovingEachItem()
    {
        ReactiveSet<int> set = [1, 2];
        List<string> events = [];
        set.OnClear.Connect(_ => events.Add("clear"));
        set.OnRemove.Connect(item => events.Add($"remove:{item}"));

        set.Clear();

        Assert.Empty(set);
        Assert.Equal(3, events.Count);
        Assert.Equal("clear", events[0]);
        Assert.Contains("remove:1", events);
        Assert.Contains("remove:2", events);
    }

    [Fact]
    public void ReadOperations_ExposeItemsAndCopies()
    {
        ReactiveSet<int> set = [1, 2];
        List<int> iterated = [];
        List<int> visited = [];
        set.ForEach(visited.Add);
        foreach (int item in set)
        {
            iterated.Add(item);
        }

        int[] array = [.. set];
        List<int> list = [.. set];
        array[0] = 99;
        list.Clear();

        Assert.Equal(2, set.Count);
        Assert.True(set.Contains(1));
        Assert.False(set.Contains(3));
        Assert.Equal([1, 2], iterated);
        Assert.Equal([1, 2], visited);
        Assert.Equal([1, 2], set.ToArray());
    }

    [Fact]
    public void Destroy_ClearsItemsWithoutMutationEventsAndRejectsMutations()
    {
        ReactiveSet<int> set = [1];
        int events = 0;
        set.OnClear.Connect(_ => events++);
        set.OnRemove.Connect(_ => events++);

        set.Destroy();

        Assert.True(set.Destroyed);
        Assert.Empty(set);
        Assert.Equal(0, events);
        Assert.Throws<DestroyedObjectException>(() => set.Add(2));
        Assert.Throws<DestroyedObjectException>(() => set.Remove(1));
        Assert.Throws<DestroyedObjectException>(set.Clear);
    }
}
