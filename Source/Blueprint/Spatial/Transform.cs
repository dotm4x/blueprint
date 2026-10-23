using Blueprint.Common.Events;
using Blueprint.Common.Primitives;

namespace Blueprint.Spatial;

/// <summary>
/// Represents the geometry properties of a element.
/// </summary>
/// <remarks>
/// The transform provides reactive position, rotation, size, and size
/// constraints. Each property is backed by a store that can be observed
/// and updated independently, except for the optional aspect ratio.
/// </remarks>
public class Transform : Destroyable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Transform"/> class.
    /// </summary>
    /// <remarks>
    /// The aspect ratio must be either null or a positive, finite value.
    /// </remarks>
    public Transform()
    {
        ValidateAspectRatio();
        AspectRatio.Connect(_ => ValidateAspectRatio(), true);
    }

    /// <summary>
    /// Gets the store containing the element's position.
    /// </summary>
    public Store<Position> Position { get; init; } = new(global::Blueprint.Spatial.Position.Zero);

    /// <summary>
    /// Gets the store containing the element's rotation.
    /// </summary>
    public Store<Rotation> Rotation { get; init; } = new(global::Blueprint.Spatial.Rotation.Zero);

    /// <summary>
    /// Gets the store containing the element's desired size.
    /// </summary>
    public Store<Size> Size { get; init; } = new(global::Blueprint.Spatial.Size.Zero);

    /// <summary>
    /// Gets the store containing the minimum allowed size.
    /// </summary>
    /// <remarks>
    /// A null value indicates that no minimum size is specified.
    /// </remarks>
    public Store<Size?> MinimumSize { get; init; } = new(null);

    /// <summary>
    /// Gets the store containing the maximum allowed size.
    /// </summary>
    /// <remarks>
    /// A null value indicates that no maximum size is specified.
    /// </remarks>
    public Store<Size?> MaximumSize { get; init; } = new(null);

    /// <summary>
    /// Gets the store containing the preferred width-to-height ratio.
    /// </summary>
    /// <remarks>
    /// A null value indicates that no aspect ratio is specified.
    /// When specified, the ratio must be positive and finite.
    /// The layout is responsible for applying this constraint when resolving
    /// the element's final size.
    /// </remarks>
    public Store<float?> AspectRatio { get; init; } = new(null);

    private void ValidateAspectRatio()
    {
        if (AspectRatio.Value is { } ratio && (!float.IsFinite(ratio) || ratio <= 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(AspectRatio),
                "Aspect ratio must be positive and finite."
            );
        }
    }

    /// <inheritdoc />
    protected override void OnDestroy()
    {
        Position.Destroy();
        Rotation.Destroy();
        Size.Destroy();
        MinimumSize.Destroy();
        MaximumSize.Destroy();
        AspectRatio.Destroy();
    }
}
