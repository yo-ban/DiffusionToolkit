using Diffusion.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Diffusion.Tests;

public class MetadataTests
{
    [Fact]
    public void GetDirectoryTextFileCache_RemovesDeletedFiles()
    {
        // Regression test for bug #52: deleted .txt files must not persist in the cache.
        var dir = Path.Combine(Path.GetTempPath(), "DiffusionTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var fileA = Path.Combine(dir, "alpha.txt");
            var fileB = Path.Combine(dir, "beta.txt");
            File.WriteAllText(fileA, "a");
            File.WriteAllText(fileB, "b");

            var first = Metadata.GetDirectoryTextFileCache(dir);
            Assert.Contains(fileA, first);
            Assert.Contains(fileB, first);

            // Act: delete one file and refresh the cache.
            File.Delete(fileB);
            var second = Metadata.GetDirectoryTextFileCache(dir);

            // Assert: the remaining file is still present and the deleted one is gone.
            Assert.Contains(fileA, second);
            Assert.DoesNotContain(fileB, second);
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { }
        }
    }

    [Fact]
    public void GetDirectoryTextFileCache_IncludesNewlyAddedFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), "DiffusionTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var fileA = Path.Combine(dir, "alpha.txt");
            File.WriteAllText(fileA, "a");

            var first = Metadata.GetDirectoryTextFileCache(dir);
            Assert.Contains(fileA, first);

            // Act: add a new file and refresh the cache.
            var fileB = Path.Combine(dir, "beta.txt");
            File.WriteAllText(fileB, "b");
            var second = Metadata.GetDirectoryTextFileCache(dir);

            Assert.Contains(fileA, second);
            Assert.Contains(fileB, second);
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { }
        }
    }

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
