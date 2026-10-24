using Blueprint.Shared.Primitives;

namespace Tests.Shared.Primitives;

public class OnceTests
{
    [Fact]
    public void Execute_RunsOperationOnlyOnce()
    {
        Once once = new();
        int executions = 0;

        once.Execute(() => executions++);
        once.Execute(() => executions++);

        Assert.Equal(1, executions);
    }

    [Fact]
    public void Execute_WhenOperationThrows_RethrowsAndCannotRetry()
    {
        Once once = new();
        InvalidOperationException exception = new("failure");
        int executions = 0;

        InvalidOperationException actual = Assert.Throws<InvalidOperationException>(
            () => once.Execute(() =>
            {
                executions++;
                throw exception;
            }));

        Assert.Same(exception, actual);
        Assert.Equal(1, executions);
        Assert.Throws<InvalidOperationException>(() => once.Execute(() => executions++));
        Assert.Equal(1, executions);
    }

    [Fact]
    public void Execute_WhenCalledRecursively_ThrowsAndMarksOperationFailed()
    {
        Once once = new();

        Assert.Throws<InvalidOperationException>(() => once.Execute(
            () => once.Execute(() => { })));

        Assert.Throws<InvalidOperationException>(() => once.Execute(() => { }));
    }
}
