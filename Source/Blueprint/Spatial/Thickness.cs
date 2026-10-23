namespace Blueprint.Spatial;

/// <summary>
/// Initializes a new instance of the <see cref="Thickness"/> struct.
/// </summary>
/// <param name="Top">The top dimension.</param>
/// <param name="Right">The right dimension.</param>
/// <param name="Bottom">The bottom dimension.</param>
/// <param name="Left">The left dimension.</param>
/// <remarks>
/// Represents the dimensions of the four sides of a rectangular area.
/// </remarks>
/// <example>
/// <code>
/// Thickness margin = new(Dimension.FromOffset(12));
///
/// Thickness custom = new(
///     Dimension.FromOffset(8),
///     Dimension.FromOffset(16),
///     Dimension.FromOffset(8),
///     Dimension.FromOffset(16));
/// </code>
/// </example>
public readonly record struct Thickness(
    Dimension Top,
    Dimension Right,
    Dimension Bottom,
    Dimension Left
)
{
    /// <summary>
    /// Gets a thickness with zero dimensions.
    /// </summary>
    public static Thickness Zero => default;

    /// <summary>
    /// Initializes a new instance of the <see cref="Thickness"/> struct
    /// with a uniform dimension.
    /// </summary>
    /// <param name="uniform">The dimension applied to every side.</param>
    public Thickness(Dimension uniform)
        : this(uniform, uniform, uniform, uniform) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Thickness"/> struct
    /// with separate horizontal and vertical dimensions.
    /// </summary>
    /// <param name="horizontal">The dimension applied to the left and right sides.</param>
    /// <param name="vertical">The dimension applied to the top and bottom sides.</param>
    public Thickness(Dimension horizontal, Dimension vertical)
        : this(vertical, horizontal, vertical, horizontal) { }
}
