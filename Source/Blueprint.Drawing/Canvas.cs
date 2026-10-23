using System.Numerics;
using Blueprint.Drawing.Commands;
using Blueprint.Drawing.Effects;
using Blueprint.Geometry.Primitives;

namespace Blueprint.Drawing;

/// <summary>
/// Represents a surface on which graphical content can be drawn.
/// </summary>
public abstract class Canvas
{
    private readonly Stack<CanvasState> _states = new();

    /// <summary>
    /// Gets the size of the rendering surface.
    /// </summary>
    public abstract Size Size { get; }

    /// <summary>
    /// Gets the current drawing state.
    /// </summary>
    protected CanvasState State { get; private set; } = new();

    /// <summary>
    /// Saves the current drawing state.
    /// </summary>
    public void Save() => _states.Push(State.Clone());

    /// <summary>
    /// Restores the most recently saved drawing state.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No drawing state has been saved.
    /// </exception>
    public void Restore()
    {
        if (_states.Count == 0)
        {
            throw new InvalidOperationException("No drawing state has been saved.");
        }

        State = _states.Pop();
        OnStateRestored(State);
    }

    /// <summary>
    /// Translates the current coordinate space.
    /// </summary>
    /// <param name="position">The translation offset.</param>
    public void Translate(Position position)
    {
        Vector2 offset = position.Resolve(Size.Resolve(Vector2.Zero));

        State.Transform = Matrix3x2.CreateTranslation(offset) * State.Transform;

        OnTransformChanged(State.Transform);
    }

    /// <summary>
    /// Rotates the current coordinate space.
    /// </summary>
    /// <param name="rotation">The rotation to apply.</param>
    public void Rotate(Rotation rotation)
    {
        State.Transform = Matrix3x2.CreateRotation(rotation.Radians) * State.Transform;

        OnTransformChanged(State.Transform);
    }

    /// <summary>
    /// Draws the specified command.
    /// </summary>
    /// <param name="command">The drawing command to execute.</param>
    public abstract void Draw(DrawCommand command);

    /// <summary>
    /// Applies an effect to subsequent drawing operations.
    /// </summary>
    /// <param name="effect">The effect to apply.</param>
    public void ApplyEffect(DrawEffect effect)
    {
        State.Effects.Add(effect);
        OnEffectApplied(effect);
    }

    /// <summary>
    /// Removes all active drawing effects.
    /// </summary>
    public void ClearEffects()
    {
        State.Effects.Clear();
        OnEffectsCleared();
    }

    /// <summary>
    /// Restores the specified drawing state in the rendering backend.
    /// </summary>
    /// <param name="state">The state to restore.</param>
    protected abstract void OnStateRestored(CanvasState state);

    /// <summary>
    /// Notifies the backend that the current transformation has changed.
    /// </summary>
    /// <param name="transform">The current transformation matrix.</param>
    protected abstract void OnTransformChanged(Matrix3x2 transform);

    /// <summary>
    /// Notifies the backend that an effect has been applied.
    /// </summary>
    /// <param name="effect">The applied effect.</param>
    protected abstract void OnEffectApplied(DrawEffect effect);

    /// <summary>
    /// Notifies the backend that all active effects have been cleared.
    /// </summary>
    protected abstract void OnEffectsCleared();
}

/// <summary>
/// Represents the drawing state captured by a canvas.
/// </summary>
public class CanvasState
{
    /// <summary>
    /// Gets or sets the current transformation matrix.
    /// </summary>
    public Matrix3x2 Transform { get; set; } = Matrix3x2.Identity;

    /// <summary>
    /// Gets the effects active in this state.
    /// </summary>
    public List<DrawEffect> Effects { get; } = [];

    internal CanvasState Clone()
    {
        CanvasState state = new() { Transform = Transform };

        state.Effects.AddRange(Effects);

        return state;
    }
}
