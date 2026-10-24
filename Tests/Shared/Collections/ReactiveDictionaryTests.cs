using Blueprint.Shared.Collections;
using Blueprint.Shared.Primitives;

namespace Tests.Shared.Collections;

public class ReactiveDictionaryTests
{
    [Fact]
    public void Constructor_AddsInitialEntriesAndRejectsDuplicateKeys()
    {
        ReactiveDictionary<string, int> dictionary = new()
        {
            { "one", 1 },
            { "two", 2 },
        };

        Assert.Equal(2, dictionary.Count);
        Assert.Equal(1, dictionary["one"]);
        Assert.Throws<ArgumentException>(() =>
            new ReactiveDictionary<string, int>
            {
                { "one", 1 },
                { "one", 2 },
            }
        );
    }

    [Fact]
    public void Add_StoresEntryAndFiresOnAdd()
    {
        ReactiveDictionary<string, int> dictionary = new();
        List<KeyValuePair<string, int>> added = [];
        dictionary.OnAdd.Connect(added.Add);

        dictionary.Add("one", 1);

        Assert.Equal(1, dictionary["one"]);
        Assert.Equal([new KeyValuePair<string, int>("one", 1)], added);
        Assert.Throws<ArgumentException>(() => dictionary.Add("one", 2));
    }

    [Fact]
    public void Indexer_AddsMissingKeyAndUpdatesExistingKey()
    {
        ReactiveDictionary<string, int> dictionary = new();
        List<KeyValuePair<string, int>> added = [];
        List<(KeyValuePair<string, int> Previous, KeyValuePair<string, int> Current)> updates = [];
        dictionary.OnAdd.Connect(added.Add);
        dictionary.OnUpdate.Connect(updates.Add);

        dictionary["one"] = 1;
        dictionary["one"] = 2;

        Assert.Equal([new KeyValuePair<string, int>("one", 1)], added);
        Assert.Equal(
            [(new KeyValuePair<string, int>("one", 1), new KeyValuePair<string, int>("one", 2))],
            updates
        );
    }

    [Fact]
    public void Remove_ReturnsWhetherEntryWasRemovedAndFiresOnRemove()
    {
        ReactiveDictionary<string, int> dictionary = new() { { "one", 1 } };
        List<KeyValuePair<string, int>> removed = [];
        dictionary.OnRemove.Connect(removed.Add);

        bool didRemove = dictionary.Remove("one");
        bool missing = dictionary.Remove("one");

        Assert.True(didRemove);
        Assert.False(missing);
        Assert.Equal([new KeyValuePair<string, int>("one", 1)], removed);
        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void Clear_FiresOnClearBeforeRemovingEntries()
    {
        ReactiveDictionary<string, int> dictionary = new()
        {
            { "one", 1 },
            { "two", 2 },
        };
        List<string> events = [];
        dictionary.OnClear.Connect(_ => events.Add("clear"));
        dictionary.OnRemove.Connect(item => events.Add($"remove:{item.Key}"));

        dictionary.Clear();

        Assert.Empty(dictionary);
        Assert.Equal(3, events.Count);
        Assert.Equal("clear", events[0]);
        Assert.Contains("remove:one", events);
        Assert.Contains("remove:two", events);
    }

    [Fact]
    public void ReadOperations_ExposeEntriesAndValues()
    {
        ReactiveDictionary<string, int> dictionary = new()
        {
            { "one", 1 },
            { "two", 2 },
        };
        List<KeyValuePair<string, int>> visited = [];
        List<KeyValuePair<string, int>> iterated = [];
        dictionary.ForEach((key, entryValue) => visited.Add(new(key, entryValue)));
        foreach (KeyValuePair<string, int> item in dictionary)
        {
            iterated.Add(item);
        }

        KeyValuePair<string, int>[] array = [.. dictionary];
        var list = dictionary.ToList();
        array[0] = new("other", 3);
        list.Clear();

        Assert.True(dictionary.ContainsKey("one"));
        Assert.False(dictionary.ContainsKey("missing"));
        Assert.True(dictionary.ContainsValue(2));
        Assert.False(dictionary.ContainsValue(3));
        Assert.True(dictionary.TryGetValue("two", out int value));
        Assert.Equal(2, value);
        Assert.False(dictionary.TryGetValue("missing", out int missing));
        Assert.Equal(0, missing);
        Assert.Equal([1, 2], dictionary.Values.Order());
        Assert.Equal(2, visited.Count);
        Assert.Equal(2, iterated.Count);
        Assert.Equal(2, dictionary.ToArray().Length);
    }

    [Fact]
    public void Destroy_ClearsEntriesWithoutMutationEventsAndRejectsMutations()
    {
        ReactiveDictionary<string, int> dictionary = new() { { "one", 1 } };
        int events = 0;
        dictionary.OnClear.Connect(_ => events++);
        dictionary.OnRemove.Connect(_ => events++);

        dictionary.Destroy();

        Assert.True(dictionary.Destroyed);
        Assert.Empty(dictionary);
        Assert.Equal(0, events);
        Assert.Throws<DestroyedObjectException>(() => dictionary.Add("two", 2));
        Assert.Throws<DestroyedObjectException>(() => dictionary["two"] = 2);
        Assert.Throws<DestroyedObjectException>(() => dictionary.Remove("one"));
        Assert.Throws<DestroyedObjectException>(dictionary.Clear);
    }
}
