namespace Combine.Core.Primitives;

/// <summary>
/// Controls one-time composition.
/// </summary>
/// <remarks>
/// Composition is executed at most once. Recursive composition is rejected.
/// A failed composition cannot be retried. Callers must serialize access
/// to this object.
/// The composer does not own composed objects or manage their lifecycle.
/// </remarks>
public sealed class Composer
{
    /// <summary>
    /// Gets whether composition has completed successfully.
    /// </summary>
    public bool Composed { get; private set; }

    private bool _composing;
    private bool _failed;

    /// <summary>
    /// Ensures that composition completes once.
    /// </summary>
    /// <param name="compose">The operation that composes and registers contents.</param>
    /// <exception cref="InvalidOperationException">
    /// Composition is already active or has failed.
    /// </exception>
    public void Ensure(Action compose)
    {
        if (Composed)
        {
            return;
        }

        if (_composing || _failed)
        {
            throw new InvalidOperationException(
                "Composition has already started or failed.");
        }

        _composing = true;

        try
        {
            compose();
            Composed = true;
        }
        catch
        {
            _failed = true;
            throw;
        }
        finally
        {
            _composing = false;
        }
    }
}