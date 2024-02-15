using System.IO;
using Codenet.Drawing.Common;
using Codenet.Drawing.Encoders;
using Codenet.Drawing.Quantizers.Helpers;
using Codenet.Drawing.Quantizers.Quantizers;
using Codenet.Drawing.Quantizers.XiaolinWu;
using SkiaSharp;

#nullable enable

namespace Codenet.Drawing.Util.Skia;

public static class ImageSaveUtil
{
    /// <summary>
    /// Saves an Image object to the specified local path
    /// </summary>
    /// <param name="image"><see cref="SKImage"/> object to save</param>
    /// <param name="destPath">Destination local path</param>
    /// <param name="imageFormat">Image format to use</param>
    /// <param name="encodingOptions">Encoding options, including quality</param>
    public static void SaveImage(
        SKImage image,
        string destPath,
        SKEncodedImageFormat imageFormat,
        ImageEncodingOptions? encodingOptions)
    {
        using var bitmap = SKBitmap.FromImage(image);
        if (bitmap == null)
            throw new InvalidDataException("Failed to decode image data.");
        SaveImage(bitmap, destPath, imageFormat, encodingOptions);
    }

    /// <summary>
    /// Saves an Image object to the specified local path
    /// </summary>
    /// <param name="imageData"><see cref="SKImage"/> object to save</param>
    /// <param name="destPath">Destination local path</param>
    /// <param name="imageFormat">Image format to use</param>
    /// <param name="encodingOptions">Encoding options, including quality</param>
    public static void SaveImage(
        SKBitmap imageData,
        string destPath, 
        SKEncodedImageFormat imageFormat,
        ImageEncodingOptions? encodingOptions)
    {
        encodingOptions ??= EncodingOptionsWithQualityBasedOnSize(imageData.Width, imageData.Height);

        if (encodingOptions.Quality <= 0.0f)
        {
            encodingOptions.Quality = 0.9f;
        }
        else if (encodingOptions.Quality > 1.0f)
        {
            encodingOptions.Quality = 1.0f;
        }

        if (imageFormat == SKEncodedImageFormat.Jpeg && encodingOptions.UseLibJpeg)
        {
            using var imageBuffer = SkiaImageBuffer.FromBitmap(imageData, false, false);
            JpegEncoder.EncodeImageWithLibjpeg(imageBuffer, destPath, encodingOptions.Quality);
        }
        else if (imageFormat == SKEncodedImageFormat.Gif)
        {
            using var imageBuffer = SkiaImageBuffer.FromBitmap(imageData, false, false);

            BaseColorQuantizer? quantizer = null;
            if (encodingOptions.QuantizerSupplier != null)
            {
                quantizer = encodingOptions.QuantizerSupplier(imageData);
            }
            if (quantizer == null)
            {
                quantizer = new WuColorQuantizer();
            }

            var targetPixelFormat = PixelFormatUtility.GetFormatByColorCount(255);
            using var quantizedBuffer = ImageBuffer.Allocate(imageData.Width, imageData.Height, targetPixelFormat);
            imageBuffer.Quantize(quantizedBuffer, quantizer, 255, 4);

            using var gifEncoder = new GifEncoder();
            gifEncoder.Start(destPath);
            gifEncoder.SetSize(imageBuffer.Width, imageBuffer.Height);
            gifEncoder.SetRepeat(1);
            gifEncoder.SetNextFrameDuration(100);
            gifEncoder.SetNextFrameTransparentColor(System.Drawing.Color.FromArgb(0x00FFFFFF));
            gifEncoder.AddFrame(quantizedBuffer);
            gifEncoder.Finish();
        }
        else
        {
            using var stream = File.Create(destPath);
            imageData.Encode(imageFormat, (int)(100 * encodingOptions.Quality)).SaveTo(stream);
        }
    }

    internal static ImageEncodingOptions EncodingOptionsWithQualityBasedOnSize(int width, int height)
    {
        var encodingOptions = new ImageEncodingOptions();

        if (width * height > 2000000)
        {
            encodingOptions.Quality = 0.9f;
        }
        else
        {
            encodingOptions.Quality = 0.85f;
        }

        return encodingOptions;
    }
}