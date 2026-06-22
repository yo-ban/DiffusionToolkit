using Diffusion.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Diffusion.Tests;

public class MetadataTests
{
    [Fact]
    public void GetImageSize_InvalidStream_Throws()
    {
        using var stream = new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });

        Assert.ThrowsAny<Exception>(() => Metadata.GetImageSize(stream));
    }

    [Fact]
    public void GetImageSize_EmptyStream_Throws()
    {
        using var stream = new MemoryStream();

        Assert.ThrowsAny<Exception>(() => Metadata.GetImageSize(stream));
    }

    [Fact]
    public void GetImageSize_ValidPng_ReturnsDimensions()
    {
        using var img = new Image<Rgb24>(4, 3);
        using var ms = new MemoryStream();
        img.SaveAsPng(ms);
        ms.Position = 0;

        var (width, height) = Metadata.GetImageSize(ms);

        Assert.Equal(4, width);
        Assert.Equal(3, height);
    }

    [Fact]
    public void GetImageSize_SquarePng_ReturnsEqualDimensions()
    {
        using var img = new Image<Rgb24>(8, 8);
        using var ms = new MemoryStream();
        img.SaveAsPng(ms);
        ms.Position = 0;

        var (width, height) = Metadata.GetImageSize(ms);

        Assert.Equal(8, width);
        Assert.Equal(8, height);
    }

    [Fact]
    public void GetImageSize_ValidJpeg_ReturnsDimensions()
    {
        using var img = new Image<Rgb24>(10, 20);
        using var ms = new MemoryStream();
        img.SaveAsJpeg(ms);
        ms.Position = 0;

        var (width, height) = Metadata.GetImageSize(ms);

        Assert.Equal(10, width);
        Assert.Equal(20, height);
    }
}
