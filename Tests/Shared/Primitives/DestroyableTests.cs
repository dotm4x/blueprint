using Blueprint.Shared.Primitives;

namespace Tests.Shared.Primitives;

public class DestroyableTests
{
    [Fact]
    public void Destroy_InvokesCleanupBeforeMarkingDestroyed()
    {
        TestDestroyable destroyable = new();

        destroyable.Destroy();

        Assert.False(destroyable.DestroyedDuringCleanup);
        Assert.True(destroyable.Destroyed);
        Assert.Equal(1, destroyable.CleanupCount);
    }

    [Fact]
    public void Destroy_WhenCalledMoreThanOnce_ThrowsWithTypeName()
    {
        TestDestroyable destroyable = new();
        destroyable.Destroy();

        DestroyedObjectException exception = Assert.Throws<DestroyedObjectException>(
            destroyable.Destroy);

        Assert.Contains(nameof(TestDestroyable), exception.Message);
        Assert.Equal(1, destroyable.CleanupCount);
    }

    [Fact]
    public void Destroy_WhenCleanupThrows_RemainsUndestroyed()
    {
        TestDestroyable destroyable = new() { CleanupException = new InvalidOperationException() };

        Assert.Throws<InvalidOperationException>(destroyable.Destroy);

        Assert.False(destroyable.Destroyed);
        Assert.Equal(1, destroyable.CleanupCount);
    }

    [Fact]
    public void ThrowIfDestroyed_WhenDestroyed_ThrowsWithTypeName()
    {
        TestDestroyable destroyable = new();
        destroyable.Destroy();

        DestroyedObjectException exception = Assert.Throws<DestroyedObjectException>(
            destroyable.CheckDestroyed);

        Assert.Contains(nameof(TestDestroyable), exception.Message);
    }

    private sealed class TestDestroyable : Destroyable
    {
        public int CleanupCount { get; private set; }

        public bool DestroyedDuringCleanup { get; private set; }

        public Exception? CleanupException { get; init; }

        public void CheckDestroyed() => ThrowIfDestroyed();

        protected override void OnDestroy()
        {
            CleanupCount++;
            DestroyedDuringCleanup = Destroyed;

            if (CleanupException is not null)
            {
                throw CleanupException;
            }
        }
    }
}
