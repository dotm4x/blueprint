using System.Numerics;
using Blueprint.Spatial;

namespace Blueprint.Layout;

/// <summary>
/// Defines how children are aligned along the cross axis of a layout.
/// </summary>
public enum Alignment
{
    /// <summary>
    /// Aligns children at the beginning of the cross axis.
    /// </summary>
    Start,

    /// <summary>
    /// Aligns children at the center of the cross axis.
    /// </summary>
    Center,

    /// <summary>
    /// Aligns children at the end of the cross axis.
    /// </summary>
    End,

    /// <summary>
    /// Stretches children to fill the available space along the cross axis,
    /// subject to their size constraints.
    /// </summary>
    Stretch,
}

/// <summary>
/// Defines how children are distributed along the main axis of a layout.
/// </summary>
public enum Distribution
{
    /// <summary>
    /// Places children at the beginning of the main axis.
    /// </summary>
    Start,

    /// <summary>
    /// Centers the group of children along the main axis.
    /// </summary>
    Center,

    /// <summary>
    /// Places children at the end of the main axis.
    /// </summary>
    End,

    /// <summary>
    /// Distributes the available space evenly between adjacent children.
    /// </summary>
    SpaceBetween,

    /// <summary>
    /// Distributes the available space around children, with half-sized
    /// spaces at the beginning and end.
    /// </summary>
    SpaceAround,

    /// <summary>
    /// Distributes the available space evenly between children and
    /// the beginning and end of the main axis.
    /// </summary>
    SpaceEvenly,
}

/// <summary>
/// Initializes a new instance of the <see cref="LayoutResult"/> record.
/// </summary>
/// <param name="Position">The calculated local position.</param>
/// <param name="Size">The calculated absolute size.</param>
/// <remarks>
/// Represents the calculated geometry of an element.
/// The geometry is expressed in the coordinate space of the parent element.
/// Position and size contain resolved values rather than relative dimensions.
/// The result does not modify the element's transform or perform rendering.
/// </remarks>
public sealed record LayoutResult(Vector2 Position, Vector2 Size);

/// <summary>
/// Initializes a new instance of the <see cref="LayoutArrangement"/> record.
/// </summary>
/// <param name="Element">The element being arranged.</param>
/// <param name="Result">The calculated geometry of the element.</param>
/// <remarks>
/// Associates an element with its calculated layout result.
/// </remarks>
public sealed record LayoutArrangement(Element Element, LayoutResult Result);

/// <summary>
/// Initializes a new instance of the <see cref="LayoutContext"/> record.
/// </summary>
/// <param name="Transform">The transform of the parent element.</param>
/// <param name="Elements">The elements to arrange.</param>
/// <remarks>
/// Provides the elements and geometry required by a layout.
/// A layout context provides the information required by a layout
/// implementation without exposing renderer internals.
/// </remarks>
public sealed record LayoutContext(Transform Transform, IReadOnlyList<Element> Elements);

/// <summary>
/// Defines the rules for measuring and arranging an element's children.
/// </summary>
/// <remarks>
/// A layout determines the desired size of its content and calculates the
/// final positions and sizes of its children. Concrete implementations define
/// their own measurement and arrangement algorithms.
///
/// Layouts are independent of rendering backends. The renderer is responsible
/// for consuming the calculated geometry and rendering the elements.
/// </remarks>
public abstract class Layout
{
    /// <summary>
    /// Gets or sets the outer spacing applied to the layout.
    /// </summary>
    /// <remarks>
    /// Each side is resolved against the corresponding available dimension.
    /// </remarks>
    public Thickness Margin { get; set; } = Thickness.Zero;

    /// <summary>
    /// Gets or sets the spacing between adjacent children.
    /// </summary>
    /// <remarks>
    /// The interpretation of spacing depends on the concrete layout.
    /// </remarks>
    public Dimension Spacing { get; set; } = Dimension.Zero;

    /// <summary>
    /// Gets or sets the alignment of children along the cross axis.
    /// </summary>
    /// <remarks>
    /// The cross axis is vertical in a horizontal layout and horizontal
    /// in a vertical layout.
    /// </remarks>
    public Alignment Alignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Gets or sets the distribution of children along the main axis.
    /// </summary>
    /// <remarks>
    /// The main axis is horizontal in a horizontal layout and vertical
    /// in a vertical layout. Distribution determines how free space is
    /// positioned around and between children.
    /// </remarks>
    public Distribution Distribution { get; set; } = Distribution.Start;

    /// <summary>
    /// Measures the desired size of the layout's content.
    /// </summary>
    /// <param name="context">The context containing the parent transform and children.</param>
    /// <param name="availableSize">The absolute size available for the content.</param>
    /// <returns>The desired absolute size of the layout's content.</returns>
    public abstract Vector2 Measure(LayoutContext context, Vector2 availableSize);

    /// <summary>
    /// Calculates the positions and sizes of the layout's children.
    /// </summary>
    /// <param name="context">The context containing the parent transform and children.</param>
    /// <param name="finalSize">The absolute size allocated to the content.</param>
    /// <returns>The calculated geometry for each child.</returns>
    public abstract IReadOnlyList<LayoutArrangement> Arrange(
        LayoutContext context,
        Vector2 finalSize
    );

    /// <summary>
    /// Resolves a child's size against the available size and its constraints.
    /// </summary>
    /// <param name="child">The child whose size is being resolved.</param>
    /// <param name="availableSize">The available size for the child.</param>
    /// <param name="preferredSize">
    /// An optional preferred size that overrides the child's configured size.
    /// </param>
    /// <returns>The resolved size of the child.</returns>
    protected Vector2 ResolveChildSize(
        Element child,
        Vector2 availableSize,
        Vector2? preferredSize = null
    )
    {
        Transform transform = child.Transform.Value;

        Vector2 size = preferredSize ?? transform.Size.Value.Resolve(availableSize);
        size = Vector2.Max(size, Vector2.Zero);

        Vector2? minimumSize = transform.MinimumSize.Value is { } minimum
            ? Vector2.Max(minimum.Resolve(availableSize), Vector2.Zero)
            : null;

        Vector2? maximumSize = transform.MaximumSize.Value is { } maximum
            ? Vector2.Max(maximum.Resolve(availableSize), Vector2.Zero)
            : null;

        if (minimumSize is { } min)
        {
            size = Vector2.Max(size, min);
        }

        if (maximumSize is { } max)
        {
            max = minimumSize is { } constrainedMinimum
                ? Vector2.Max(max, constrainedMinimum)
                : max;

            size = Vector2.Min(size, max);
        }

        if (transform.AspectRatio.Value is { } ratio && float.IsFinite(ratio) && ratio > 0)
        {
            size = ResolveAspectRatio(size, ratio, minimumSize, maximumSize);
        }

        return size;
    }

    /// <summary>
    /// Resolves the total margin along each axis.
    /// </summary>
    /// <param name="availableSize">The absolute size available to the layout.</param>
    /// <returns>The total horizontal and vertical margins.</returns>
    protected Vector2 ResolveMargin(Vector2 availableSize)
    {
        float horizontal =
            Margin.Left.Resolve(availableSize.X) + Margin.Right.Resolve(availableSize.X);

        float vertical =
            Margin.Top.Resolve(availableSize.Y) + Margin.Bottom.Resolve(availableSize.Y);

        return new Vector2(horizontal, vertical);
    }

    private static Vector2 ResolveAspectRatio(
        Vector2 size,
        float ratio,
        Vector2? minimumSize,
        Vector2? maximumSize
    )
    {
        Vector2 minimum = minimumSize is { } min ? Vector2.Max(min, Vector2.Zero) : Vector2.Zero;

        Vector2 maximum = maximumSize is { } max
            ? Vector2.Max(max, Vector2.Zero)
            : new Vector2(float.PositiveInfinity);

        maximum = Vector2.Max(maximum, minimum);

        float minimumHeight = MathF.Max(minimum.Y, minimum.X / ratio);
        float maximumHeight = MathF.Min(maximum.Y, maximum.X / ratio);

        if (minimumHeight > maximumHeight)
        {
            return size;
        }

        float height = (size.X * ratio + size.Y) / (ratio * ratio + 1);
        height = Math.Clamp(height, minimumHeight, maximumHeight);

        return new Vector2(height * ratio, height);
    }
}
