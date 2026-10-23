using Blueprint.Drawing.Brushes;

namespace Blueprint.Drawing.Commands;

using Blueprint.Geometry.Primitives;

/// <summary>
/// Represents a command that fills an ellipse.
/// </summary>
/// <param name="Position">The position of the ellipse.</param>
/// <param name="Size">The size of the ellipse.</param>
/// <param name="Brush">The brush used to fill the ellipse.</param>
public sealed record FillEllipseCommand(Position Position, Size Size, Brush Brush) : DrawCommand;
