using Diffusion.IO;
using Xunit;

namespace Diffusion.Tests;

public class HashFunctionsTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    private string CreateTempFile(long size, byte fill = 0)
    {
        var path = Path.GetTempFileName();
        _tempFiles.Add(path);
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        var buffer = new byte[Math.Min(size, 64 * 1024)];
        Array.Fill(buffer, fill);
        long remaining = size;
        while (remaining > 0)
        {
            int toWrite = (int)Math.Min(remaining, buffer.Length);
            fs.Write(buffer, 0, toWrite);
            remaining -= toWrite;
        }
        return path;
    }

    [Fact]
    public void CalculateHash_SmallFiles_DifferentContent_DifferentHashes()
    {
        var file1 = CreateTempFile(100, 0xAA);
        var file2 = CreateTempFile(100, 0xBB);

        var hash1 = HashFunctions.CalculateHash(file1);
        var hash2 = HashFunctions.CalculateHash(file2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void CalculateHash_SameFile_SameHash()
    {
        var file = CreateTempFile(100, 0xCC);

        var hash1 = HashFunctions.CalculateHash(file);
        var hash2 = HashFunctions.CalculateHash(file);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CalculateHash_LargeFile_ReturnsNonEmptyHash()
    {
        var file = CreateTempFile(2 * 1024 * 1024, 0xDD);

        var hash = HashFunctions.CalculateHash(file);

        Assert.Equal(8, hash.Length);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void CalculateHash_SmallFile_ReturnsValidHexHash()
    {
        var file = CreateTempFile(50, 0xEE);

        var hash = HashFunctions.CalculateHash(file);

        Assert.Equal(8, hash.Length);
        Assert.All(hash, c => Assert.True(char.IsDigit(c) || (c >= 'a' && c <= 'f')));
    }

    [Fact]
    public void CalculateSHA256_SameFile_SameHash()
    {
        var file = CreateTempFile(1024, 0x42);

        var hash1 = HashFunctions.CalculateSHA256(file);
        var hash2 = HashFunctions.CalculateSHA256(file);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CalculateSHA256_DifferentContent_DifferentHashes()
    {
        var file1 = CreateTempFile(1024, 0x01);
        var file2 = CreateTempFile(1024, 0x02);

        var hash1 = HashFunctions.CalculateSHA256(file1);
        var hash2 = HashFunctions.CalculateSHA256(file2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void CalculateSHA256_Returns64CharHexHash()
    {
        var file = CreateTempFile(512, 0x33);

        var hash = HashFunctions.CalculateSHA256(file);

        Assert.Equal(64, hash.Length);
        Assert.All(hash, c => Assert.True(char.IsDigit(c) || (c >= 'a' && c <= 'f')));
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try { File.Delete(file); } catch { }
        }
    }
}
