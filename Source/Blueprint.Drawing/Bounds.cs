using Blueprint.Geometry.Primitives;

namespace Blueprint.Drawing;

/// <summary>
/// Represents the resolved geometry of an element.
/// </summary>
/// <param name="Position">The final position of the element.</param>
/// <param name="Size">The final size of the element.</param>
/// <param name="Rotation">The final rotation of the element.</param>
public sealed record DrawBounds(Position Position, Size Size, Rotation Rotation);
