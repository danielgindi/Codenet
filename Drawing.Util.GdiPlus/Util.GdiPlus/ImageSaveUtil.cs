using System.Drawing.Imaging;
using System.Drawing;
using Codenet.Drawing.Common;
using Codenet.Drawing.Encoders;
using Codenet.Drawing.Quantizers.Quantizers;
using Codenet.Drawing.Quantizers.XiaolinWu;

#nullable enable

namespace Codenet.Drawing.Util.GdiPlus;

public static class ImageSaveUtil
{
    /// <summary>
    /// Saves an Image object to the specified local path
    /// </summary>
    /// <param name="image"><see cref="Image"/> object to save</param>
    /// <param name="destPath">Destination local path</param>
    /// <param name="imageFormat">Image format to use</param>
    /// <param name="encodingOptions">Encoding options, including quality</param>
    public static void SaveImage(
        Image image,
        string destPath,
        ImageFormat imageFormat,
        ImageEncodingOptions? encodingOptions)
    {
        encodingOptions ??= EncodingOptionsWithQualityBasedOnSize(image.Width, image.Height);

        if (encodingOptions.Quality <= 0.0f)
        {
            encodingOptions.Quality = 0.9f;
        }
        else if (encodingOptions.Quality > 1.0f)
        {
            encodingOptions.Quality = 1.0f;
        }

        if (imageFormat.Equals(ImageFormat.Jpeg) && encodingOptions.UseLibJpeg)
        {
            using var imageBuffer = GdiPlusImageBuffer.FromImage(image, ImageLockMode.ReadOnly);
            JpegEncoder.EncodeImageWithLibjpeg(imageBuffer, destPath, encodingOptions.Quality);
        }
        else if (imageFormat.Equals(ImageFormat.Gif))
        {
            BaseColorQuantizer? quantizer = null;
            if (encodingOptions.QuantizerSupplier != null)
                quantizer = encodingOptions.QuantizerSupplier(image);
            if (quantizer == null)
                quantizer = new WuColorQuantizer();

            using var quantized = GdiPlusImageBuffer.QuantizeImage(image, quantizer, 255, 4);
            quantized.Save(destPath, ImageFormat.Gif);
        }
        else
        {
            image.Save(destPath, imageFormat);
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