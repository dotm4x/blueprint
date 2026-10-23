namespace Blueprint.Drawing.Primitives;

/// <summary>
/// Represents a pen used to outline a drawing area.
/// </summary>
/// <param name="Color">The color of the stroke.</param>
/// <param name="Thickness">The thickness of the stroke.</param>
public sealed record Pen(Color Color, float Thickness = 1.0f);
