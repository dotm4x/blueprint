using Blueprint.Drawing.Primitives;

namespace Blueprint.Drawing.Commands;

using Blueprint.Geometry.Primitives;

/// <summary>
/// Represents a command that draws a line between two positions.
/// </summary>
/// <param name="Start">The starting position of the line.</param>
/// <param name="End">The ending position of the line.</param>
/// <param name="Pen">The pen used to draw the line.</param>
public sealed record DrawLineCommand(Position Start, Position End, Pen Pen) : DrawCommand;
