using System.Drawing;
using Codenet.Drawing.Util.GdiPlus;

namespace Codenet.Drawing.ImageProcessing;

public static class MarginsCropper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="image"></param>
    /// <param name="color"></param>
    /// <param name="colorDistanceAllowed"></param>
    /// <returns>Image after cropping, or null if there's no work to do.
    /// If result is zero width or zero height, returns null
    /// </returns>
    public static Image CropMargins(Bitmap image, Color color, double colorDistanceAllowed = 0.0)
    {
        return ImageMarginCropUtil.CropMargins(image, color, colorDistanceAllowed);
    }

    public static bool CropMarginsAndSave(
        string imgPath,
        string destPath, 
        Color colorCrop, 
        double colorDistanceAllowed,
        ProcessingHelper.EncodingOptions encodingOptions)
    {
        return ImageMarginCropUtil.CropMarginsAndSave(
            sourcePath: imgPath,
            destPath: destPath ?? imgPath,
            colorCrop: colorCrop, 
            colorDistanceAllowed: colorDistanceAllowed,
            encodingOptions: new ImageEncodingOptions
            {
                Quality = encodingOptions.JpegQuality,
                QuantizerSupplier = encodingOptions.QuantizerSupplier == null ? null : x => encodingOptions.QuantizerSupplier(x),
                UseLibJpeg = true,
                UseTempFile = true
            });
    }
}
