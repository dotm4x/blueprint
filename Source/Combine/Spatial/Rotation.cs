namespace Combine.Spatial;

/// <summary>
/// Represents a rotation angle in radians.
/// </summary>
/// <param name="Radians">The rotation angle in radians.</param>
/// <example>
/// <code>
/// Rotation rotation = Rotation.FromDegrees(90);
/// Rotation combined = rotation + Rotation.FromDegrees(45);
///
/// float radians = combined.Radians;
/// float degrees = combined.Degrees;
/// </code>
/// </example>
public readonly record struct Rotation(float Radians)
{
    /// <summary>
    /// Gets a rotation with no angular displacement.
    /// </summary>
    public static Rotation Zero => default;

    /// <summary>
    /// Gets a rotation representing a full revolution.
    /// </summary>
    public static Rotation Full => new(MathF.Tau);

    /// <summary>
    /// Gets the rotation angle in degrees.
    /// </summary>
    public float Degrees => Radians * (180f / MathF.PI);

    /// <summary>
    /// Creates a rotation from an angle in degrees.
    /// </summary>
    /// <param name="degrees">The rotation angle in degrees.</param>
    /// <returns>The equivalent rotation in radians.</returns>
    public static Rotation FromDegrees(float degrees) => new(degrees * (MathF.PI / 180f));

    /// <summary>
    /// Creates a rotation from an angle in radians.
    /// </summary>
    /// <param name="radians">The rotation angle in radians.</param>
    /// <returns>The corresponding rotation.</returns>
    public static Rotation FromRadians(float radians) => new(radians);

    /// <summary>
    /// Normalizes the rotation to the range from zero inclusive
    /// to one full revolution exclusive.
    /// </summary>
    /// <returns>The normalized rotation.</returns>
    public Rotation Normalize()
    {
        float radians = Radians % MathF.Tau;

        if (radians < 0)
        {
            radians += MathF.Tau;
        }

        return new(radians);
    }

    /// <summary>
    /// Adds two rotations.
    /// </summary>
    public static Rotation operator +(Rotation left, Rotation right) =>
        new(left.Radians + right.Radians);

    /// <summary>
    /// Subtracts one rotation from another.
    /// </summary>
    public static Rotation operator -(Rotation left, Rotation right) =>
        new(left.Radians - right.Radians);

    /// <summary>
    /// Multiplies a rotation by a scalar.
    /// </summary>
    public static Rotation operator *(Rotation rotation, float scalar) =>
        new(rotation.Radians * scalar);

    /// <summary>
    /// Multiplies a rotation by a scalar.
    /// </summary>
    public static Rotation operator *(float scalar, Rotation rotation) => rotation * scalar;

    /// <summary>
    /// Divides a rotation by a scalar.
    /// </summary>
    /// <param name="rotation">The rotation to divide.</param>
    /// <param name="scalar">The divisor.</param>
    /// <returns>The resulting rotation.</returns>
    /// <exception cref="DivideByZeroException">
    /// The scalar is zero.
    /// </exception>
    public static Rotation operator /(Rotation rotation, float scalar)
    {
        if (scalar == 0)
        {
            throw new DivideByZeroException();
        }

        return new(rotation.Radians / scalar);
    }

    /// <summary>
    /// Negates a rotation.
    /// </summary>
    public static Rotation operator -(Rotation rotation) => new(-rotation.Radians);

    /// <summary>
    /// Returns the rotation unchanged.
    /// </summary>
    public static Rotation operator +(Rotation rotation) => rotation;

    /// <summary>
    /// Converts an angle in degrees to a rotation.
    /// </summary>
    public static implicit operator Rotation(float degrees) => FromDegrees(degrees);

    /// <summary>
    /// Converts a rotation to its angle in radians.
    /// </summary>
    public static explicit operator float(Rotation rotation) => rotation.Radians;
}
