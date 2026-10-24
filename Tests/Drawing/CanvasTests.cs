using System.Numerics;
using Blueprint.Drawing;
using Blueprint.Drawing.Commands;
using Blueprint.Drawing.Effects;
using Blueprint.Geometry.Primitives;

namespace Tests.Drawing;

public class CanvasTests
{
    [Fact]
    public void SaveAndRestore_RestoresTransformationAndEffects()
    {
        TestCanvas canvas = new();
        TestEffect savedEffect = new();
        TestEffect laterEffect = new();
        canvas.Translate(Position.FromOffset(10, 20));
        canvas.ApplyEffect(savedEffect);
        List<DrawEffect> effectsBeforeSave = canvas.CurrentState.Effects;

        canvas.Save();
        canvas.Rotate(Rotation.FromDegrees(90));
        canvas.ApplyEffect(laterEffect);
        canvas.Restore();

        Assert.Equal(Matrix3x2.CreateTranslation(10, 20), canvas.CurrentState.Transform);
        Assert.Equal([savedEffect], canvas.RestoredState!.Effects);
        Assert.Same(canvas.CurrentState, canvas.RestoredState);
        Assert.NotSame(effectsBeforeSave, canvas.CurrentState.Effects);
        Assert.Equal(canvas.CurrentState.Transform, canvas.RestoredState.Transform);
    }

    [Fact]
    public void Restore_WithoutSavedStateThrows()
    {
        TestCanvas canvas = new();

        Assert.Throws<InvalidOperationException>(canvas.Restore);
    }

    [Fact]
    public void Restore_UsesMostRecentlySavedState()
    {
        TestCanvas canvas = new();
        canvas.Translate(Position.FromOffset(10, 0));
        canvas.Save();
        canvas.Translate(Position.FromOffset(20, 0));
        canvas.Save();
        canvas.Translate(Position.FromOffset(30, 0));

        canvas.Restore();
        Assert.Equal(Matrix3x2.CreateTranslation(30, 0), canvas.CurrentState.Transform);

        canvas.Restore();
        Assert.Equal(Matrix3x2.CreateTranslation(10, 0), canvas.CurrentState.Transform);
    }

    [Fact]
    public void Translate_ResolvesPositionAndNotifiesBackend()
    {
        TestCanvas canvas = new();

        canvas.Translate(new Position(new Dimension(0.5f, 12), Dimension.FromOffset(8)));

        Assert.Equal(Matrix3x2.CreateTranslation(112, 8), canvas.CurrentState.Transform);
        Assert.Equal(canvas.CurrentState.Transform, canvas.Transforms[^1]);
    }

    [Fact]
    public void Rotate_UpdatesTransformationAndNotifiesBackend()
    {
        TestCanvas canvas = new();

        canvas.Rotate(Rotation.FromDegrees(90));

        Assert.Equal(Matrix3x2.CreateRotation(MathF.PI / 2), canvas.CurrentState.Transform);
        Assert.Equal(canvas.CurrentState.Transform, canvas.Transforms[^1]);
    }

    [Fact]
    public void ApplyEffect_TracksEffectAndNotifiesBackend()
    {
        TestCanvas canvas = new();
        TestEffect effect = new();

        canvas.ApplyEffect(effect);

        Assert.Equal([effect], canvas.CurrentState.Effects);
        Assert.Equal([effect], canvas.AppliedEffects);
    }

    [Fact]
    public void ClearEffects_RemovesEffectsAndNotifiesBackend()
    {
        TestCanvas canvas = new();
        canvas.ApplyEffect(new TestEffect());

        canvas.ClearEffects();

        Assert.Empty(canvas.CurrentState.Effects);
        Assert.Equal(1, canvas.ClearEffectsCount);
    }

    [Fact]
    public void Draw_ForwardsCommandToBackend()
    {
        TestCanvas canvas = new();
        DrawCommand command = new DrawLineCommand(
            Position.Zero,
            Position.FromOffset(10, 20),
            new Blueprint.Drawing.Primitives.Pen(Blueprint.Drawing.Primitives.Color.Red)
        );

        canvas.Draw(command);

        Assert.Same(command, Assert.Single(canvas.DrawnCommands));
    }

    private sealed class TestEffect : DrawEffect;

    private sealed class TestCanvas : Canvas
    {
        public override Size Size => Size.FromOffset(200, 100);
        public CanvasState CurrentState => State;
        public CanvasState? RestoredState { get; private set; }
        public List<Matrix3x2> Transforms { get; } = [];
        public List<DrawEffect> AppliedEffects { get; } = [];
        public List<DrawCommand> DrawnCommands { get; } = [];
        public int ClearEffectsCount { get; private set; }

        public override void Draw(DrawCommand command) => DrawnCommands.Add(command);

        protected override void OnStateRestored(CanvasState state) => RestoredState = state;

        protected override void OnTransformChanged(Matrix3x2 transform) =>
            Transforms.Add(transform);

        protected override void OnEffectApplied(DrawEffect effect) => AppliedEffects.Add(effect);

        protected override void OnEffectsCleared() => ClearEffectsCount++;
    }
}
