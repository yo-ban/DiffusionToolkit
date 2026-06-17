using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Diffusion.Toolkit.Services;

public class ImagePayloadService
{
    public async Task<string> CreateDataUrl(string imagePath, bool downscale, int maxEdge, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            throw new FileNotFoundException("The selected image file could not be found.", imagePath);
        }

        var extension = Path.GetExtension(imagePath).ToLowerInvariant();

        if (downscale && extension is ".png" or ".jpg" or ".jpeg")
        {
            var bytes = await Task.Run(() => DownscaleToJpeg(imagePath, Math.Max(256, maxEdge)), cancellationToken);
            return $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}";
        }

        var mimeType = extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };

        var rawBytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
        return $"data:{mimeType};base64,{Convert.ToBase64String(rawBytes)}";
    }

    private static byte[] DownscaleToJpeg(string imagePath, int maxEdge)
    {
        using var input = File.OpenRead(imagePath);
        var decoder = BitmapDecoder.Create(input, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var source = decoder.Frames[0];

        var largestEdge = Math.Max(source.PixelWidth, source.PixelHeight);
        BitmapSource output = source;

        if (largestEdge > maxEdge)
        {
            var scale = (double)maxEdge / largestEdge;
            output = new TransformedBitmap(source, new ScaleTransform(scale, scale));
            output.Freeze();
        }

        var encoder = new JpegBitmapEncoder
        {
            QualityLevel = 88
        };
        encoder.Frames.Add(BitmapFrame.Create(output));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}
