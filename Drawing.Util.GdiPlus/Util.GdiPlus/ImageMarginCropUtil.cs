using System;
using System.Drawing;

#nullable enable

namespace Codenet.Drawing.Util.GdiPlus;

public static class ImageMarginCropUtil
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
    /// 
    public static Image? CropMargins(Bitmap image, Color? color = null, double colorDistanceAllowed = 0.0)
    {
        if (image == null) return null;
        if (color == null || color == Color.Empty)
            color = image.GetPixel(0, 0);
        var unboxedColor = color.Value;

        colorDistanceAllowed = Math.Abs(colorDistanceAllowed);
        bool shouldCheckDistance = colorDistanceAllowed != 0;

        bool search;
        int iLinesTop = 0, iLinesBottom = 0;
        int iLinesLeft = 0, iLinesRight = 0;

        search = true;
        for (int y = 0, x; y < image.Height; y++)
        {
            for (x = 0; x < image.Width; x++)
            {
                if (image.GetPixel(x, y) != color &&
                    (!shouldCheckDistance ||
                        GetColorDistance(image.GetPixel(x, y), unboxedColor) > colorDistanceAllowed)
                    )
                {
                    search = false;
                    break;
                }
            }
            if (search)
            {
                iLinesTop++;
            }
            else break;
        }

        search = true;
        for (int y = image.Height - 1, x; y >= 0; y--)
        {
            for (x = 0; x < image.Width; x++)
            {
                if (image.GetPixel(x, y) != color &&
                    (!shouldCheckDistance ||
                        GetColorDistance(image.GetPixel(x, y), unboxedColor) > colorDistanceAllowed)
                    )
                {
                    search = false;
                    break;
                }
            }
            if (search)
            {
                iLinesBottom++;
            }
            else break;
        }

        search = true;
        for (int x = 0, y; x < image.Width; x++)
        {
            for (y = 0; y < image.Height; y++)
            {
                if (image.GetPixel(x, y) != color &&
                    (!shouldCheckDistance ||
                        GetColorDistance(image.GetPixel(x, y), unboxedColor) > colorDistanceAllowed)
                    )
                {
                    search = false;
                    break;
                }
            }
            if (search)
            {
                iLinesLeft++;
            }
            else break;
        }

        search = true;
        for (int x = image.Width - 1, y; x >= 0; x--)
        {
            for (y = 0; y < image.Height; y++)
            {
                if (image.GetPixel(x, y) != color &&
                    (!shouldCheckDistance ||
                        GetColorDistance(image.GetPixel(x, y), unboxedColor) > colorDistanceAllowed)
                    )
                {
                    search = false;
                    break;
                }
            }
            if (search)
            {
                iLinesRight++;
            }
            else break;
        }

        int newWidth = image.Width - iLinesLeft - iLinesRight;
        int newHeight = image.Height - iLinesTop - iLinesBottom;

        if (newWidth <= 0 || newHeight <= 0 || (iLinesLeft == 0 &&
            iLinesRight == 0 && iLinesTop == 0 && iLinesBottom == 0))
        {
            return null;
        }

        Image retImg = new Bitmap(newWidth, newHeight);
        using (var g = Graphics.FromImage(retImg))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.DrawImage(image, -iLinesLeft, -iLinesTop, image.Width, image.Height);
        }

        return retImg;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceImage"></param>
    /// <param name="destPath">Having the <paramref name="destPath"/> the same as the source file path may not be safe, especially for multi-frame sources where the source is streamed.</param>
    /// <param name="colorCrop"></param>
    /// <param name="colorDistanceAllowed"></param>
    /// <param name="encodingOptions"></param>
    /// <returns></returns>
    public static bool CropMarginsAndSave(
        Image sourceImage,
        string destPath,
        Color? colorCrop = null,
        double colorDistanceAllowed = 0.0,
        ImageEncodingOptions? encodingOptions = null)
    {
        if (sourceImage == null) return false;
        if (destPath == null) return false;

        ImageFrameProcessingUtil.ProcessMultiframeImage(sourceImage, destPath, null, encodingOptions ?? new(), (image, _) =>
        {
            if (image is Bitmap)
            {
                return CropMargins((Bitmap)image, colorCrop, colorDistanceAllowed);
            }
            else
            {
                using var bmp = new Bitmap(image);
                return CropMargins(bmp, colorCrop, colorDistanceAllowed);
            }
        });

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destPath">Having the <paramref name="destPath"/> the same as <paramref name="sourcePath"/> may not be safe, especially for multi-frame sources where the source is streamed.</param>
    /// <param name="colorCrop"></param>
    /// <param name="colorDistanceAllowed"></param>
    /// <param name="encodingOptions"></param>
    /// <returns></returns>
    public static bool CropMarginsAndSave(
        string sourcePath,
        string destPath,
        Color? colorCrop = null,
        double colorDistanceAllowed = 0.0,
        ImageEncodingOptions? encodingOptions = null)
    {
        if (sourcePath == null) return false;
        if (destPath == null) return false;

        using var image = Image.FromFile(sourcePath);
        return CropMarginsAndSave(
            sourceImage: image,
            destPath: destPath,
            colorCrop: colorCrop,
            colorDistanceAllowed: colorDistanceAllowed,
            encodingOptions: encodingOptions);
    }

    private static double GetColorDistance(Color c1, Color c2)
    {
        double a = Math.Pow(Convert.ToDouble(c1.A - c2.A), 2.0);
        double r = Math.Pow(Convert.ToDouble(c1.R - c2.R), 2.0);
        double g = Math.Pow(Convert.ToDouble(c1.G - c2.G), 2.0);
        double b = Math.Pow(Convert.ToDouble(c1.B - c2.B), 2.0);

        return Math.Sqrt(a + r + g + b);
    }
}