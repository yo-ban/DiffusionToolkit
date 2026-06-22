using Diffusion.Common;
using Xunit;

namespace Diffusion.Tests;

public class SemanticVersionTests
{
    [Theory]
    [InlineData("v1.0", 1, 0, 0)]
    [InlineData("v2.5", 2, 5, 0)]
    [InlineData("v10.20.30", 10, 20, 30)]
    [InlineData("v1.0.1", 1, 0, 1)]
    public void Parse_ValidVersion_SetsComponents(string text, int major, int minor, int build)
    {
        var version = SemanticVersion.Parse(text);

        Assert.Equal(major, version.Major);
        Assert.Equal(minor, version.Minor);
        Assert.Equal(build, version.Build);
    }

    [Theory]
    [InlineData("not a version")]
    [InlineData("1.0")]
    [InlineData("")]
    [InlineData("v")]
    public void Parse_InvalidVersion_ThrowsArgumentException(string text)
    {
        Assert.Throws<ArgumentException>(() => SemanticVersion.Parse(text));
    }

    [Theory]
    [InlineData("v1.0", true, 1, 0)]
    [InlineData("v2.5.3", true, 2, 5)]
    [InlineData("nope", false, 0, 0)]
    [InlineData("", false, 0, 0)]
    public void TryParse_ReturnsCorrectResult(string text, bool expectedSuccess, int major, int minor)
    {
        var success = SemanticVersion.TryParse(text, out var version);

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(major, version.Major);
        Assert.Equal(minor, version.Minor);
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse()
    {
        var success = SemanticVersion.TryParse(null, out var version);

        Assert.False(success);
        Assert.Equal(0, version.Major);
    }

    [Fact]
    public void CompareTo_HigherMajor_IsGreater()
    {
        var v1 = new SemanticVersion { Major = 1, Minor = 0 };
        var v2 = new SemanticVersion { Major = 2, Minor = 0 };

        Assert.True(v2 > v1);
        Assert.True(v1 < v2);
    }

    [Fact]
    public void CompareTo_HigherMinor_IsGreater()
    {
        var v1 = new SemanticVersion { Major = 1, Minor = 0 };
        var v2 = new SemanticVersion { Major = 1, Minor = 5 };

        Assert.True(v2 > v1);
    }

    [Fact]
    public void CompareTo_HigherBuild_IsGreater()
    {
        var v1 = new SemanticVersion { Major = 1, Minor = 0, Build = 1 };
        var v2 = new SemanticVersion { Major = 1, Minor = 0, Build = 2 };

        Assert.True(v2 > v1);
    }

    [Fact]
    public void CompareTo_EqualVersions_AreNotGreaterOrLess()
    {
        var v1 = new SemanticVersion { Major = 1, Minor = 0, Build = 0 };
        var v2 = new SemanticVersion { Major = 1, Minor = 0, Build = 0 };

        Assert.False(v2 > v1);
        Assert.False(v1 > v2);
        Assert.Equal(0, v1.CompareTo(v2));
    }

    [Fact]
    public void Operator_LeftNull_DoesNotThrow()
    {
        SemanticVersion? v1 = null;
        var v2 = new SemanticVersion { Major = 1, Minor = 0 };

        Assert.False(v1 > v2);
        Assert.True(v1 < v2);
    }

    [Fact]
    public void Operator_BothNull_AreEqual()
    {
        SemanticVersion? v1 = null;
        SemanticVersion? v2 = null;

        Assert.False(v1 > v2);
        Assert.False(v1 < v2);
    }

    [Theory]
    [InlineData("v1.0", "v1.0")]
    [InlineData("v2.5.3", "v2.5.3")]
    public void ToString_Roundtrips(string input, string expected)
    {
        var version = SemanticVersion.Parse(input);

        Assert.Equal(expected, version.ToString());
    }

    [Theory]
    [InlineData("v1.0", true)]
    [InlineData("v1.0.0", true)]
    [InlineData("v1", false)]
    [InlineData("1.0", false)]
    public void IsSemanticVersion_DetectsCorrectly(string text, bool expected)
    {
        Assert.Equal(expected, SemanticVersion.IsSemanticVersion(text));
    }
}
