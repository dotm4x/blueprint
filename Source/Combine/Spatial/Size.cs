using System.Numerics;

namespace Combine.Spatial;

/// <summary>
/// Initializes a new instance of the <see cref="Size"/> struct.
/// </summary>
/// <param name="Width">The horizontal dimension.</param>
/// <param name="Height">The vertical dimension.</param>
/// <remarks>
/// Represents a two-dimensional size.
/// Each dimension can combine a relative scale and an absolute offset.
/// </remarks>
/// <example>
/// <code>
/// Size size = new(
///     Dimension.FromScale(1),
///     Dimension.FromOffset(240));
///
/// Vector2 resolved = size.Resolve(new Vector2(800, 600));
/// // (800, 240)
/// </code>
/// </example>
public readonly record struct Size(Dimension Width, Dimension Height)
{
    /// <summary>
    /// Gets a size with zero width and height.
    /// </summary>
    public static Size Zero => default;

    /// <summary>
    /// Creates a size from absolute dimensions.
    /// </summary>
    /// <param name="width">The absolute width.</param>
    /// <param name="height">The absolute height.</param>
    /// <returns>A size with absolute dimensions.</returns>
    public static Size FromOffset(float width, float height) =>
        new(Dimension.FromOffset(width), Dimension.FromOffset(height));

    /// <summary>
    /// Creates a size from relative dimensions.
    /// </summary>
    /// <param name="width">The relative width scale.</param>
    /// <param name="height">The relative height scale.</param>
    /// <returns>A size with relative dimensions.</returns>
    public static Size FromScale(float width, float height) =>
        new(Dimension.FromScale(width), Dimension.FromScale(height));

    /// <summary>
    /// Resolves the size against the available dimensions.
    /// </summary>
    /// <param name="availableSize">The available size of the containing area.</param>
    /// <returns>The resulting absolute size.</returns>
    public Vector2 Resolve(Vector2 availableSize) =>
        new(Width.Resolve(availableSize.X), Height.Resolve(availableSize.Y));

    /// <summary>
    /// Adds two sizes component-wise.
    /// </summary>
    public static Size operator +(Size left, Size right) =>
        new(left.Width + right.Width, left.Height + right.Height);

    /// <summary>
    /// Subtracts two sizes component-wise.
    /// </summary>
    public static Size operator -(Size left, Size right) =>
        new(left.Width - right.Width, left.Height - right.Height);

    /// <summary>
    /// Multiplies both dimensions by a scalar.
    /// </summary>
    public static Size operator *(Size size, float scalar) =>
        new(size.Width * scalar, size.Height * scalar);

    /// <summary>
    /// Multiplies both dimensions by a scalar.
    /// </summary>
    public static Size operator *(float scalar, Size size) => size * scalar;

    /// <summary>
    /// Divides both dimensions by a scalar.
    /// </summary>
    public static Size operator /(Size size, float scalar) =>
        new(size.Width / scalar, size.Height / scalar);

    /// <summary>
    /// Negates both dimensions.
    /// </summary>
    public static Size operator -(Size size) => new(-size.Width, -size.Height);

    /// <summary>
    /// Returns the size unchanged.
    /// </summary>
    public static Size operator +(Size size) => size;
}
