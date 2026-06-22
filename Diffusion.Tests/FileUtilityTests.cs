using Diffusion.Common;
using Xunit;

namespace Diffusion.Tests;

public class FileUtilityTests
{
    [Fact]
    public void IsValidFilename_Null_ReturnsFalse()
    {
        Assert.False(FileUtility.IsValidFilename(null!));
    }

    [Fact]
    public void IsValidFilename_EmptyString_ReturnsFalse()
    {
        Assert.False(FileUtility.IsValidFilename(""));
    }

    [Fact]
    public void IsValidFilename_Whitespace_ReturnsFalse()
    {
        Assert.False(FileUtility.IsValidFilename("   "));
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("NUL")]
    [InlineData("COM1")]
    [InlineData("LPT1")]
    [InlineData("con")]
    [InlineData("aux")]
    public void IsValidFilename_ReservedName_ReturnsFalse(string name)
    {
        Assert.False(FileUtility.IsValidFilename(name));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public void IsValidFileName_DotNames_ReturnsFalse(string name)
    {
        Assert.False(FileUtility.IsValidFilename(name));
    }

    [Fact]
    public void IsValidFilename_ValidName_ReturnsTrue()
    {
        Assert.True(FileUtility.IsValidFilename("image.png"));
    }

    [Fact]
    public void IsValidFilename_NameWithLeadingSpace_ReturnsFalse()
    {
        Assert.False(FileUtility.IsValidFilename(" image.png"));
    }

    [Fact]
    public void IsValidFilename_NameWithTrailingSpace_ReturnsFalse()
    {
        Assert.False(FileUtility.IsValidFilename("image.png "));
    }

    [Fact]
    public void IsValidFilename_NameWithInvalidChar_ReturnsFalse()
    {
        var invalid = new string(Path.GetInvalidFileNameChars().First(), 1);
        Assert.False(FileUtility.IsValidFilename($"file{invalid}name.png"));
    }

    [Theory]
    [InlineData("image_001.png")]
    [InlineData("model.safetensors")]
    [InlineData("a")]
    [InlineData("12345")]
    public void IsValidFilename_ValidNames_ReturnTrue(string name)
    {
        Assert.True(FileUtility.IsValidFilename(name));
    }
}
