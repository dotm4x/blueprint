using System.Numerics;
using Blueprint.Elements;
using Blueprint.Geometry.Primitives;
using Blueprint.Shared.Primitives;

namespace Tests.Elements;

public class ElementTests
{
    [Fact]
    public void Constructor_InitializesStoresCollectionsAndDefaults()
    {
        Element element = new();

        Assert.Null(element.Parent.Value);
        Assert.Empty(element.Elements);
        Assert.Empty(element.Tags);
        Assert.Equal(Vector2.Zero, element.Origin.Value);
        Assert.Equal(Position.Zero, element.Position.Value);
        Assert.Equal(Rotation.Zero, element.Rotation.Value);
        Assert.Equal(Size.Zero, element.Size.Value);
        Assert.Null(element.MinimumSize.Value);
        Assert.Null(element.MaximumSize.Value);
        Assert.Null(element.AspectRatio.Value);
        Assert.False(element.Clip.Value);
        Assert.Equal(1, element.Opacity.Value);
        Assert.False(element.Loaded);
        Assert.False(element.Destroyed);
        Assert.NotEmpty(element.Identifier);
        Assert.Equal(element.Identifier, element.Path);
    }

    [Fact]
    public void Constructor_WithIdentifier_UsesIdentifierInPathAndStringRepresentation()
    {
        Element element = new() { Identifier = "root" };

        Assert.Equal("root", element.Path);
        Assert.Equal("Element \"root\"", element.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void AspectRatio_InvalidValueThrowsAndPreservesPreviousValue(float ratio)
    {
        Element element = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => element.AspectRatio.Value = ratio);

        Assert.Null(element.AspectRatio.Value);
    }

    [Fact]
    public void AspectRatio_StoresPositiveFiniteValues()
    {
        Element element = new();

        element.AspectRatio.Value = 1.5f;

        Assert.Equal(1.5f, element.AspectRatio.Value);
    }

    [Fact]
    public void AddingAndRemovingChild_UpdatesParentRelationship()
    {
        Element parent = new();
        Element child = new();

        parent.Elements.Add(child);

        Assert.Same(parent, child.Parent.Value);
        Assert.Contains(child, parent.Elements);

        parent.Elements.Remove(child);

        Assert.Null(child.Parent.Value);
        Assert.DoesNotContain(child, parent.Elements);
    }

    [Fact]
    public void SettingParent_UpdatesBothParentCollections()
    {
        Element firstParent = new();
        Element secondParent = new();
        Element child = new();

        child.Parent.Value = firstParent;
        Assert.Contains(child, firstParent.Elements);

        child.Parent.Value = secondParent;

        Assert.DoesNotContain(child, firstParent.Elements);
        Assert.Contains(child, secondParent.Elements);
        Assert.Same(secondParent, child.Parent.Value);

        child.Parent.Value = null;
        Assert.DoesNotContain(child, secondParent.Elements);
    }

    [Fact]
    public void AddingChildAlreadyOwnedByAnotherParent_ReparentsChild()
    {
        Element firstParent = new();
        Element secondParent = new();
        Element child = new();
        firstParent.Elements.Add(child);

        secondParent.Elements.Add(child);

        Assert.DoesNotContain(child, firstParent.Elements);
        Assert.Contains(child, secondParent.Elements);
        Assert.Same(secondParent, child.Parent.Value);
    }

    [Fact]
    public void AddingDuplicateChild_ThrowsWithoutChangingRelationship()
    {
        Element parent = new();
        Element child = new();
        parent.Elements.Add(child);

        Assert.Throws<InvalidOperationException>(() => parent.Elements.Add(child));

        Assert.Single(parent.Elements);
        Assert.Same(parent, child.Parent.Value);
    }

    [Fact]
    public void SettingSelfAsParent_ThrowsAndPreservesHierarchy()
    {
        Element element = new();

        Assert.Throws<InvalidOperationException>(() => element.Parent.Value = element);

        Assert.Null(element.Parent.Value);
        Assert.Empty(element.Elements);
    }

    [Fact]
    public void SettingAncestorAsParent_ThrowsAndPreservesHierarchy()
    {
        Element root = new();
        Element child = new();
        root.Elements.Add(child);

        Assert.Throws<InvalidOperationException>(() => root.Parent.Value = child);

        Assert.Null(root.Parent.Value);
        Assert.Same(root, child.Parent.Value);
        Assert.Contains(child, root.Elements);
    }

    [Fact]
    public void SettingDestroyedElementAsParent_ThrowsAndPreservesPreviousParent()
    {
        Element previousParent = new();
        Element destroyedParent = new();
        Element child = new();
        previousParent.Elements.Add(child);
        destroyedParent.Destroy();

        Assert.Throws<DestroyedObjectException>(() => child.Parent.Value = destroyedParent);

        Assert.Same(previousParent, child.Parent.Value);
        Assert.Contains(child, previousParent.Elements);
        Assert.Empty(destroyedParent.Elements);
    }

    [Fact]
    public void AddingDestroyedChild_ThrowsAndDoesNotRetainChild()
    {
        Element parent = new();
        Element child = new();
        child.Destroy();

        Assert.Throws<DestroyedObjectException>(() => parent.Elements.Add(child));

        Assert.Empty(parent.Elements);
        Assert.Null(child.Parent.Value);
    }

    [Fact]
    public void Path_UsesParentPathAndIdentifier()
    {
        Element root = new() { Identifier = "root" };
        DerivedElement child = new() { Identifier = "child" };
        Element grandchild = new() { Identifier = "grandchild" };
        root.Elements.Add(child);
        child.Elements.Add(grandchild);

        Assert.Equal("root/child/grandchild", grandchild.Path);
        Assert.Equal("Element \"root/child/grandchild\"", grandchild.ToString());
    }

    [Fact]
    public void GetByIdentifier_FindsDirectAndRecursiveMatches()
    {
        Element root = new();
        DerivedElement child = new() { Identifier = "child" };
        Element grandchild = new() { Identifier = "grandchild" };
        root.Elements.Add(child);
        child.Elements.Add(grandchild);

        Assert.Same(child, root.Elements.GetByIdentifier("child"));
        Assert.Null(root.Elements.GetByIdentifier("grandchild"));
        Assert.Same(grandchild, root.Elements.GetByIdentifier("grandchild", recursive: true));
        Assert.Same(child, root.Elements.GetByIdentifier<DerivedElement>("child"));
        Assert.Null(root.Elements.GetByIdentifier<DerivedElement>("grandchild", recursive: true));
        Assert.Same(child, root.Elements.GetByIdentifier<Element>("child"));
        Assert.Null(root.Elements.GetByIdentifier("missing", recursive: true));
    }

    [Fact]
    public void GetByPath_ResolvesIdentifiersAndRelativeSegments()
    {
        Element root = new() { Identifier = "root" };
        DerivedElement child = new() { Identifier = "child" };
        Element grandchild = new() { Identifier = "grandchild" };
        root.Elements.Add(child);
        child.Elements.Add(grandchild);

        Assert.Same(root, root.Elements.GetByPath(""));
        Assert.Same(root, root.Elements.GetByPath("./"));
        Assert.Same(child, root.Elements.GetByPath("child"));
        Assert.Same(grandchild, root.Elements.GetByPath("child/grandchild"));
        Assert.Same(child, child.Elements.GetByPath("../child"));
        Assert.Same(root, child.Elements.GetByPath(".."));
        Assert.Same(grandchild, root.Elements.GetByPath("child//grandchild/"));
        Assert.Null(root.Elements.GetByPath("missing"));
        Assert.Same(child, root.Elements.GetByPath<Element>("child"));
        Assert.Null(root.Elements.GetByPath<DerivedElement>("grandchild"));
        Assert.Same(child, root.Elements.GetByPath<DerivedElement>("child"));
    }

    [Fact]
    public void GetByTag_FindsDirectAndRecursiveMatches()
    {
        Element root = new();
        DerivedElement child = new();
        Element grandchild = new();
        child.Tags.Add("active");
        grandchild.Tags.Add("active");
        root.Elements.Add(child);
        child.Elements.Add(grandchild);

        Assert.Equal([child], root.Elements.GetByTag("active"));
        Assert.Equal([child, grandchild], root.Elements.GetByTag("active", recursive: true));
        Assert.Equal([child], root.Elements.GetByTag<DerivedElement>("active", recursive: true));
        Assert.Empty(root.Elements.GetByTag("missing", recursive: true));
    }

    [Fact]
    public void Load_BuildsAndConfiguresOnceBeforeLoadingChildren()
    {
        List<string> events = [];
        ProbeElement initialChild = new("initial", events);
        ProbeElement builtChild = new("built", events);
        ProbeElement root = new("root", events, initialChild) { BuiltChildren = [builtChild] };
        root.ConfigureAction = element =>
        {
            events.Add("configure");
            Assert.False(element.Loaded);
            Assert.Equal([initialChild, builtChild], element.Elements.ToArray());
        };

        root.Load();
        root.Unload();
        root.Load();

        Assert.Equal(
            [
                "configure",
                "load:root",
                "load:initial",
                "load:built",
                "unload:initial",
                "unload:built",
                "unload:root",
                "load:root",
                "load:initial",
                "load:built",
            ],
            events
        );
        Assert.True(root.Loaded);
        Assert.True(initialChild.Loaded);
        Assert.True(builtChild.Loaded);
        Assert.Equal(1, root.BuildCount);
        Assert.Equal(1, root.ConfigureCount);
    }

    [Fact]
    public void Load_RequiresLoadedParent()
    {
        Element parent = new();
        Element child = new();
        parent.Elements.Add(child);

        Assert.Throws<InvalidOperationException>(child.Load);

        Assert.False(child.Loaded);
        Assert.False(parent.Loaded);
    }

    [Fact]
    public void Load_WhenAlreadyLoadedThrows()
    {
        Element element = new();
        element.Load();

        Assert.Throws<InvalidOperationException>(element.Load);

        Assert.True(element.Loaded);
    }

    [Fact]
    public void Unload_WhenNotLoadedThrows()
    {
        Element element = new();

        Assert.Throws<InvalidOperationException>(element.Unload);
    }

    [Fact]
    public void Load_InvokesParentBeforeChildrenAndUnloadInvokesChildrenBeforeParent()
    {
        List<string> events = [];
        ProbeElement parent = new("parent", events);
        ProbeElement firstChild = new("first", events);
        ProbeElement secondChild = new("second", events);
        parent.Elements.Add(firstChild);
        parent.Elements.Add(secondChild);

        parent.Load();
        parent.Unload();

        Assert.Equal(
            [
                "load:parent",
                "load:first",
                "load:second",
                "unload:first",
                "unload:second",
                "unload:parent",
            ],
            events
        );
        Assert.False(parent.Loaded);
        Assert.False(firstChild.Loaded);
        Assert.False(secondChild.Loaded);
    }

    [Fact]
    public void AddingChildToLoadedParent_LoadsChildAndRemovingItUnloadsChild()
    {
        ProbeElement parent = new("parent", []);
        ProbeElement child = new("child", []);
        parent.Load();

        parent.Elements.Add(child);

        Assert.True(child.Loaded);

        parent.Elements.Remove(child);

        Assert.False(child.Loaded);
        Assert.Null(child.Parent.Value);
    }

    [Fact]
    public void ReparentingBetweenLoadedParents_KeepsChildLoaded()
    {
        List<string> events = [];
        ProbeElement firstParent = new("first", events);
        ProbeElement secondParent = new("second", events);
        ProbeElement child = new("child", events);
        firstParent.Load();
        secondParent.Load();
        firstParent.Elements.Add(child);
        events.Clear();

        secondParent.Elements.Add(child);

        Assert.Empty(events);
        Assert.Same(secondParent, child.Parent.Value);
        Assert.DoesNotContain(child, firstParent.Elements);
        Assert.Contains(child, secondParent.Elements);
        Assert.True(child.Loaded);
    }

    [Fact]
    public void Unload_PreservesPersistentChildren()
    {
        ProbeElement parent = new("parent", []);
        ProbeElement persistentChild = new("child", []) { Persistent = true };
        parent.Elements.Add(persistentChild);
        parent.Load();

        parent.Unload();

        Assert.False(parent.Loaded);
        Assert.True(persistentChild.Loaded);
    }

    [Fact]
    public void RemovingPersistentChildFromUnloadedParent_LeavesChildLoaded()
    {
        Element parent = new();
        Element child = new() { Persistent = true };
        child.Load();
        parent.Elements.Add(child);

        parent.Elements.Remove(child);

        Assert.True(child.Loaded);
        Assert.Null(child.Parent.Value);
    }

    [Fact]
    public void Load_WhenOnLoadThrows_RestoresUnloadedStateWithoutCallingOnUnload()
    {
        ProbeElement element = new("element", []);
        InvalidOperationException failure = new("load failed");
        element.LoadAction = _ => throw failure;

        InvalidOperationException actual = Assert.Throws<InvalidOperationException>(element.Load);

        Assert.Same(failure, actual);
        Assert.False(element.Loaded);
        Assert.Equal(0, element.UnloadCount);
    }

    [Fact]
    public void Load_WhenChildFails_UnloadsNewlyLoadedChildrenInReverseAndRollsBackParent()
    {
        List<string> events = [];
        ProbeElement parent = new("parent", events);
        ProbeElement firstChild = new("first", events);
        ProbeElement failingChild = new("failing", events);
        parent.Elements.Add(firstChild);
        parent.Elements.Add(failingChild);
        failingChild.LoadAction = _ => throw new InvalidOperationException("child failed");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(parent.Load);

        Assert.Equal("child failed", exception.Message);
        Assert.Equal(
            ["load:parent", "load:first", "load:failing", "unload:first", "unload:parent"],
            events
        );
        Assert.False(parent.Loaded);
        Assert.False(firstChild.Loaded);
        Assert.False(failingChild.Loaded);
    }

    [Fact]
    public void Load_WhenChildFails_PreservesChildrenLoadedBeforeLoad()
    {
        ProbeElement parent = new("parent", []);
        ProbeElement persistentChild = new("persistent", []) { Persistent = true };
        ProbeElement failingChild = new("failing", []);
        persistentChild.Load();
        parent.Elements.Add(persistentChild);
        parent.Elements.Add(failingChild);
        failingChild.LoadAction = _ => throw new InvalidOperationException("child failed");

        Assert.Throws<InvalidOperationException>(parent.Load);

        Assert.False(parent.Loaded);
        Assert.True(persistentChild.Loaded);
        Assert.False(failingChild.Loaded);
    }

    [Fact]
    public void Load_WhenRollbackCleanupFails_LeavesParentLoadedForRetry()
    {
        List<string> events = [];
        ProbeElement parent = new("parent", events);
        ProbeElement cleanupChild = new("cleanup", events);
        ProbeElement failingChild = new("failing", events);
        parent.Elements.Add(cleanupChild);
        parent.Elements.Add(failingChild);
        failingChild.LoadAction = _ => throw new InvalidOperationException("load failed");
        cleanupChild.UnloadAction = _ =>
        {
            if (cleanupChild.UnloadCount == 1)
            {
                throw new InvalidOperationException("cleanup failed");
            }
        };

        AggregateException exception = Assert.Throws<AggregateException>(parent.Load);

        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Equal("load failed", exception.InnerExceptions[0].Message);
        Assert.Equal("cleanup failed", exception.InnerExceptions[1].Message);
        Assert.True(parent.Loaded);
        Assert.True(cleanupChild.Loaded);

        parent.Unload();

        Assert.False(parent.Loaded);
        Assert.False(cleanupChild.Loaded);
        Assert.False(failingChild.Loaded);
    }

    [Fact]
    public void Unload_WhenChildUnloadThrows_DoesNotUnloadParent()
    {
        ProbeElement parent = new("parent", []);
        ProbeElement child = new("child", []);
        parent.Elements.Add(child);
        parent.Load();
        child.UnloadAction = _ => throw new InvalidOperationException("cleanup failed");

        Assert.Throws<InvalidOperationException>(parent.Unload);

        Assert.True(parent.Loaded);
        Assert.True(child.Loaded);
        Assert.Equal(0, parent.UnloadCount);
    }

    [Fact]
    public void Destroy_UnloadsAndDestroysOwnedChildrenAndStores()
    {
        List<string> events = [];
        ProbeElement parent = new("parent", events);
        ProbeElement child = new("child", events);
        parent.Elements.Add(child);
        parent.Load();

        parent.Destroy();

        Assert.True(parent.Destroyed);
        Assert.True(child.Destroyed);
        Assert.False(parent.Loaded);
        Assert.False(child.Loaded);
        Assert.True(parent.Elements.Destroyed);
        Assert.True(parent.Tags.Destroyed);
        Assert.True(parent.Origin.Destroyed);
        Assert.True(parent.Position.Destroyed);
        Assert.True(parent.Rotation.Destroyed);
        Assert.True(parent.Size.Destroyed);
        Assert.True(parent.MinimumSize.Destroyed);
        Assert.True(parent.MaximumSize.Destroyed);
        Assert.True(parent.AspectRatio.Destroyed);
        Assert.True(parent.Clip.Destroyed);
        Assert.True(parent.Opacity.Destroyed);
        Assert.True(parent.Parent.Destroyed);
        Assert.Equal(
            [
                "load:parent",
                "load:child",
                "destroy:parent",
                "unload:child",
                "unload:parent",
                "destroy:child",
            ],
            events
        );
    }

    [Fact]
    public void Destroy_WhenAlreadyDestroyedThrows()
    {
        Element element = new();
        element.Destroy();

        Assert.Throws<DestroyedObjectException>(element.Destroy);
    }

    [Fact]
    public void Destroy_UnlinksChildFromParent()
    {
        Element parent = new();
        Element child = new();
        parent.Elements.Add(child);

        child.Destroy();

        Assert.DoesNotContain(child, parent.Elements);
        Assert.Null(child.Parent.Value);
        Assert.Empty(parent.Elements);
    }

    [Fact]
    public void ElementCollection_Destroy_DestroysCollectionButNotItsChildren()
    {
        Element parent = new();
        Element child = new();
        parent.Elements.Add(child);

        parent.Elements.Destroy();

        Assert.True(parent.Elements.Destroyed);
        Assert.Empty(parent.Elements);
        Assert.False(child.Destroyed);
        Assert.Same(parent, child.Parent.Value);
    }

    private sealed class DerivedElement : Element;

    private sealed class ProbeElement : Element
    {
        public Element[] BuiltChildren { get; init; } = [];
        public int BuildCount { get; private set; }
        public int ConfigureCount { get; private set; }
        public int UnloadCount { get; private set; }
        public Action<ProbeElement>? ConfigureAction { get; set; }
        public Action<ProbeElement>? LoadAction { get; set; }
        public Action<ProbeElement>? UnloadAction { get; set; }
        private readonly string _name;
        private readonly List<string> _events;

        protected override IEnumerable<Element> Build()
        {
            BuildCount++;
            return BuiltChildren;
        }

        protected override void Configure()
        {
            ConfigureCount++;
            ConfigureAction?.Invoke(this);
        }

        protected override void OnLoad()
        {
            _events.Add($"load:{_name}");
            LoadAction?.Invoke(this);
        }

        protected override void OnUnload()
        {
            UnloadCount++;
            _events.Add($"unload:{_name}");
            UnloadAction?.Invoke(this);
        }

        protected override void OnDestroy()
        {
            _events.Add($"destroy:{_name}");
            base.OnDestroy();
        }

        public ProbeElement(string name, List<string> events, params Element[] initialChildren)
        {
            _name = name;
            _events = events;

            foreach (Element child in initialChildren)
            {
                Elements.Add(child);
            }
        }
    }
}
