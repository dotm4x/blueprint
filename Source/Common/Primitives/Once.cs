namespace Combine.Common.Primitives;

/// <summary>
/// Ensures that an operation executes at most once.
/// </summary>
public class Once
{
    private bool _executed;
    private bool _executing;
    private bool _failed;

    /// <summary>
    /// Executes the specified operation once.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <exception cref="InvalidOperationException">
    /// The operation is already executing or has failed.
    /// </exception>
    public void Execute(Action operation)
    {
        if (_executed)
        {
            return;
        }

        if (_executing || _failed)
        {
            throw new InvalidOperationException("The operation has already started or failed.");
        }

        _executing = true;

        try
        {
            operation();
            _executed = true;
        }
        catch
        {
            _failed = true;
            throw;
        }
        finally
        {
            _executing = false;
        }
    }
}
