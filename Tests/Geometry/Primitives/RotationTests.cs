using Blueprint.Geometry.Primitives;

namespace Tests.Geometry.Primitives;

public class RotationTests
{
    [Fact]
    public void Zero_HasNoAngularDisplacement() => Assert.Equal(0, Rotation.Zero.Radians);

    [Fact]
    public void Full_IsOneRevolution() => Assert.Equal(MathF.Tau, Rotation.Full.Radians);

    [Fact]
    public void Degrees_ConvertsRadiansToDegrees() =>
        Assert.Equal(90, Rotation.FromRadians(MathF.PI / 2).Degrees, 5);

    [Fact]
    public void FromDegrees_ConvertsDegreesToRadians() =>
        Assert.Equal(MathF.PI / 2, Rotation.FromDegrees(90).Radians, 5);

    [Fact]
    public void FromRadians_PreservesRadians() =>
        Assert.Equal(1.25f, Rotation.FromRadians(1.25f).Radians);

    [Theory]
    [InlineData(0, 0)]
    [InlineData(MathF.PI, MathF.PI)]
    [InlineData(-MathF.PI / 2, 3 * MathF.PI / 2)]
    [InlineData(5 * MathF.PI, MathF.PI)]
    public void Normalize_ReturnsAngleInSingleRevolution(float radians, float expectedRadians) =>
        Assert.Equal(expectedRadians, new Rotation(radians).Normalize().Radians, 5);

    [Fact]
    public void Addition_AddsRadians() =>
        Assert.Equal(new Rotation(3), new Rotation(1) + new Rotation(2));

    [Fact]
    public void Subtraction_SubtractsRadians() =>
        Assert.Equal(new Rotation(-1), new Rotation(1) - new Rotation(2));

    [Fact]
    public void Multiplication_SupportsBothOperandOrders()
    {
        Rotation rotation = new(1.5f);

        Assert.Equal(new Rotation(4.5f), rotation * 3);
        Assert.Equal(rotation * 3, 3 * rotation);
    }

    [Fact]
    public void Division_DividesRadians() =>
        Assert.Equal(new Rotation(0.75f), new Rotation(1.5f) / 2);

    [Fact]
    public void Division_ByZero_Throws() =>
        Assert.Throws<DivideByZeroException>(() => new Rotation(1) / 0);

    [Fact]
    public void UnaryOperators_PreserveOrNegateRadians()
    {
        Rotation rotation = new(1.5f);

        Assert.Equal(rotation, +rotation);
        Assert.Equal(new Rotation(-1.5f), -rotation);
    }

    [Fact]
    public void FloatConversions_ConvertDegreesInAndRadiansOut()
    {
        Rotation rotation = 180f;
        float radians = (float)rotation;

        Assert.Equal(MathF.PI, radians, 5);
    }
}
