namespace Combine.Spatial;

/// <summary>
/// Initializes a new instance of the <see cref="Dimension"/> struct.
/// </summary>
/// <param name="Scale">The proportion of the available dimension.</param>
/// <param name="Offset">The absolute offset added to the scaled dimension.</param>
/// <remarks>
/// Represents a dimension composed of a relative scale and an absolute offset.
/// The resolved value is calculated by multiplying the available dimension
/// by <see cref="Scale"/> and adding <see cref="Offset"/>.
/// </remarks>
/// <example>
/// <code>
/// Dimension dimension = new(0.5f, 24);
/// float resolved = dimension.Resolve(800);
/// // 424
/// </code>
/// </example>
public readonly record struct Dimension(float Scale, float Offset)
{
    /// <summary>
    /// Gets a dimension with zero scale and zero offset.
    /// </summary>
    public static Dimension Zero => default;

    /// <summary>
    /// Creates a dimension using an absolute offset.
    /// </summary>
    /// <param name="offset">The absolute offset.</param>
    /// <returns>A dimension with zero scale and the specified offset.</returns>
    public static Dimension FromOffset(float offset) => new(0, offset);

    /// <summary>
    /// Creates a dimension using a relative scale.
    /// </summary>
    /// <param name="scale">The relative scale.</param>
    /// <returns>A dimension with the specified scale and zero offset.</returns>
    public static Dimension FromScale(float scale) => new(scale, 0);

    /// <summary>
    /// Resolves the dimension against an available size.
    /// </summary>
    /// <param name="availableSize">The available size of the containing area.</param>
    /// <returns>The resulting absolute dimension.</returns>
    public float Resolve(float availableSize) => availableSize * Scale + Offset;

    /// <summary>
    /// Adds two dimensions component-wise.
    /// </summary>
    public static Dimension operator +(Dimension left, Dimension right) =>
        new(left.Scale + right.Scale, left.Offset + right.Offset);

    /// <summary>
    /// Subtracts two dimensions component-wise.
    /// </summary>
    public static Dimension operator -(Dimension left, Dimension right) =>
        new(left.Scale - right.Scale, left.Offset - right.Offset);

    /// <summary>
    /// Multiplies both components by a scalar.
    /// </summary>
    public static Dimension operator *(Dimension dimension, float scalar) =>
        new(dimension.Scale * scalar, dimension.Offset * scalar);

    /// <summary>
    /// Multiplies both components by a scalar.
    /// </summary>
    public static Dimension operator *(float scalar, Dimension dimension) => dimension * scalar;

    /// <summary>
    /// Divides both components by a scalar.
    /// </summary>
    /// <exception cref="DivideByZeroException">
    /// The scalar is zero.
    /// </exception>
    public static Dimension operator /(Dimension dimension, float scalar)
    {
        if (scalar == 0)
        {
            throw new DivideByZeroException();
        }

        return new(dimension.Scale / scalar, dimension.Offset / scalar);
    }

    /// <summary>
    /// Negates both components.
    /// </summary>
    public static Dimension operator -(Dimension dimension) =>
        new(-dimension.Scale, -dimension.Offset);

    /// <summary>
    /// Returns the dimension unchanged.
    /// </summary>
    public static Dimension operator +(Dimension dimension) => dimension;
}
