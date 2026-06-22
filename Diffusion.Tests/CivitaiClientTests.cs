using Diffusion.Civitai;
using Xunit;

namespace Diffusion.Tests;

public class CivitaiClientTests
{
    [Theory]
    [InlineData("Model", "model")]
    [InlineData("ModelHash", "modelHash")]
    [InlineData("Types", "types")]
    [InlineData("A", "a")]
    public void ToCamelCase_ConvertsFirstCharToLowerCase(string input, string expected)
    {
        Assert.Equal(expected, CivitaiClient.ToCamelCase(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ToCamelCase_EmptyOrNull_ReturnsInputUnchanged(string? input)
    {
        // Regression test for bug #40: an empty/null name must not throw
        // (Substring on an empty string previously threw ArgumentOutOfRangeException).
        Assert.Equal(input, CivitaiClient.ToCamelCase(input!));
    }
}
