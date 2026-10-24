using Blueprint.Drawing.Primitives;

namespace Tests.Drawing.Primitives;

public class ColorTests
{
    [Fact]
    public void Constructor_UsesNormalizedComponentsAndDefaultAlpha()
    {
        Color color = new(0.2f, 0.4f, 0.6f);

        AssertColor(new Color(0.2f, 0.4f, 0.6f, 1), color);
    }

    [Fact]
    public void Constructor_WithHexadecimal_UsesParsedValue()
    {
        Color color = new(1, 0, 0, 0, "#0F08");

        AssertColor(new Color(0, 1, 0, 0x88 / 255f), color);
    }

    [Theory]
    [InlineData("#123", 0x11, 0x22, 0x33, 0xFF)]
    [InlineData("#1234", 0x11, 0x22, 0x33, 0x44)]
    [InlineData("#123456", 0x12, 0x34, 0x56, 0xFF)]
    [InlineData("#12345678", 0x12, 0x34, 0x56, 0x78)]
    [InlineData("0x123456", 0x12, 0x34, 0x56, 0xFF)]
    [InlineData("  123456  ", 0x12, 0x34, 0x56, 0xFF)]
    public void Parse_SupportsHexFormatsAndPrefixes(
        string hexadecimal,
        byte red,
        byte green,
        byte blue,
        byte alpha
    )
    {
        Color expected = Color.FromBytes(red, green, blue, alpha);

        AssertColor(expected, Color.Parse(hexadecimal));
        AssertColor(expected, new Color(hexadecimal));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("#12")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    [InlineData("#GGG")]
    [InlineData("#123456789")]
    public void TryParse_InvalidValueReturnsFalseAndDefaultColor(string? value)
    {
        bool parsed = Color.TryParse(value, out Color color);

        Assert.False(parsed);
        Assert.Equal(default, color);
    }

    [Theory]
    [InlineData("#12")]
    [InlineData("#GGG")]
    [InlineData("#123456789")]
    public void Parse_InvalidValueThrowsFormatException(string value) =>
        Assert.Throws<FormatException>(() => Color.Parse(value));

    [Fact]
    public void FromBytes_ConvertsComponentsToNormalizedValues()
    {
        Color color = Color.FromBytes(0, 127, 255, 64);

        AssertColor(new Color(0, 127 / 255f, 1, 64 / 255f), color);
    }

    [Fact]
    public void Clamp_ConstrainsEveryComponentToSpecifiedRange()
    {
        Color color = Color.Clamp(new Color(-1, 0.3f, 2, 0.7f), 0.2f, 0.8f);

        AssertColor(new Color(0.2f, 0.3f, 0.8f, 0.7f), color);
        AssertColor(new Color(0, 0.3f, 1, 0.7f), new Color(-1, 0.3f, 2, 0.7f).Clamped());
    }

    [Fact]
    public void Clamp_MinimumGreaterThanMaximumThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Color.Clamp(Color.White, 1, 0));

    [Fact]
    public void WithAlpha_ReturnsCopyWithNewAlpha()
    {
        Color color = new(0.2f, 0.4f, 0.6f, 0.8f);

        AssertColor(new Color(0.2f, 0.4f, 0.6f, 0.3f), color.WithAlpha(0.3f));
    }

    [Fact]
    public void ToHex_ClampsAndFormatsComponentsWithOptionalAlpha()
    {
        Color color = new(1.2f, 0.5f, -0.2f, 0.25f);

        Assert.Equal("#FF800040", color.ToHex());
        Assert.Equal("#FF8000", color.ToHex(includeAlpha: false));
        Assert.Equal(color.ToHex(), color.ToString());
    }

    [Fact]
    public void Lerp_InterpolatesEveryComponent()
    {
        Color result = Color.Lerp(new Color(0, 0.2f, 0.4f, 0.6f), new Color(1, 0.8f, 0.6f, 0.2f), 0.25f);

        AssertColor(new Color(0.25f, 0.35f, 0.45f, 0.5f), result);
    }

    [Fact]
    public void Operators_OperateOnComponents()
    {
        Color left = new(0.8f, 0.6f, 0.4f, 0.2f);
        Color right = new(0.2f, 0.3f, 0.5f, 0.1f);

        AssertColor(new Color(1, 0.9f, 0.9f, 0.3f), left + right);
        AssertColor(new Color(0.6f, 0.3f, -0.1f, 0.1f), left - right);
        AssertColor(new Color(0.16f, 0.18f, 0.2f, 0.02f), left * right);
        AssertColor(new Color(0.4f, 0.3f, 0.2f, 0.1f), left / 2);
        AssertColor(new Color(0.5f, 0.5f, 0.5f, 0.5f), left / new Color(1.6f, 1.2f, 0.8f, 0.4f));
        AssertColor(new Color(-0.8f, -0.6f, -0.4f, -0.2f), -left);
        AssertColor(left * 2, 2 * left);
    }

    [Fact]
    public void NamedColors_ReturnExpectedValues()
    {
        AssertColor(new Color(0, 0, 0, 1), Color.Black);
        AssertColor(new Color(0, 0, 1, 1), Color.Blue);
        AssertColor(Color.FromBytes(0x64, 0x95, 0xED), Color.CornflowerBlue);
        AssertColor(new Color(0, 1, 1, 1), Color.Cyan);
        AssertColor(new Color(0.5f, 0.5f, 0.5f, 1), Color.Gray);
        AssertColor(new Color(0, 1, 0, 1), Color.Green);
        AssertColor(new Color(1, 0, 1, 1), Color.Magenta);
        AssertColor(new Color(1, 0, 0, 1), Color.Red);
        AssertColor(new Color(0, 0, 0, 0), Color.Transparent);
        AssertColor(new Color(1, 1, 1, 1), Color.White);
        AssertColor(new Color(1, 1, 0, 1), Color.Yellow);
    }

    [Fact]
    public void Random_ReturnsOpaqueColorWithNormalizedComponents()
    {
        Color color = Color.Random();

        Assert.InRange(color.R, 0, 1);
        Assert.InRange(color.G, 0, 1);
        Assert.InRange(color.B, 0, 1);
        Assert.Equal(1, color.A);
    }

    private static void AssertColor(Color expected, Color actual)
    {
        Assert.Equal(expected.R, actual.R, 5);
        Assert.Equal(expected.G, actual.G, 5);
        Assert.Equal(expected.B, actual.B, 5);
        Assert.Equal(expected.A, actual.A, 5);
    }
}
