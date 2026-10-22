using System.Numerics;

namespace Combine.Spatial;

/// <summary>
/// Initializes a new instance of the <see cref="Position"/> struct.
/// </summary>
/// <param name="X">The horizontal position.</param>
/// <param name="Y">The vertical position.</param>
/// <remarks>
/// Represents a two-dimensional position.
/// Each coordinate can combine a relative scale and an absolute offset.
/// </remarks>
/// <example>
/// <code>
/// Position position = new(
///     Dimension.FromScale(0.5f),
///     Dimension.FromOffset(24));
///
/// Vector2 resolved = position.Resolve(new Vector2(800, 600));
/// // (400, 24)
/// </code>
/// </example>
public readonly record struct Position(Dimension X, Dimension Y)
{
    /// <summary>
    /// Gets the origin position.
    /// </summary>
    public static Position Zero => default;

    /// <summary>
    /// Creates a position from absolute coordinates.
    /// </summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    /// <returns>A position with absolute coordinates.</returns>
    public static Position FromOffset(float x, float y) =>
        new(Dimension.FromOffset(x), Dimension.FromOffset(y));

    /// <summary>
    /// Creates a position from relative coordinates.
    /// </summary>
    /// <param name="x">The horizontal scale.</param>
    /// <param name="y">The vertical scale.</param>
    /// <returns>A position with relative coordinates.</returns>
    public static Position FromScale(float x, float y) =>
        new(Dimension.FromScale(x), Dimension.FromScale(y));

    /// <summary>
    /// Resolves the position against the available size.
    /// </summary>
    /// <param name="availableSize">The available size of the containing area.</param>
    /// <returns>The resulting absolute coordinates.</returns>
    public Vector2 Resolve(Vector2 availableSize) =>
        new(X.Resolve(availableSize.X), Y.Resolve(availableSize.Y));

    /// <summary>
    /// Adds two positions component-wise.
    /// </summary>
    public static Position operator +(Position left, Position right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <summary>
    /// Subtracts two positions component-wise.
    /// </summary>
    public static Position operator -(Position left, Position right) =>
        new(left.X - right.X, left.Y - right.Y);

    /// <summary>
    /// Multiplies both coordinates by a scalar.
    /// </summary>
    public static Position operator *(Position position, float scalar) =>
        new(position.X * scalar, position.Y * scalar);

    /// <summary>
    /// Multiplies both coordinates by a scalar.
    /// </summary>
    public static Position operator *(float scalar, Position position) => position * scalar;

    /// <summary>
    /// Divides both coordinates by a scalar.
    /// </summary>
    public static Position operator /(Position position, float scalar) =>
        new(position.X / scalar, position.Y / scalar);

    /// <summary>
    /// Negates both coordinates.
    /// </summary>
    public static Position operator -(Position position) => new(-position.X, -position.Y);

    /// <summary>
    /// Returns the position unchanged.
    /// </summary>
    public static Position operator +(Position position) => position;
}
