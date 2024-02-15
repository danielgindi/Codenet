using System;
using System.IO;
using SkiaSharp;

#nullable enable

namespace Codenet.Drawing.Util.Skia;

public static class ImageMarginCropUtil
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="color"></param>
    /// <param name="colorDistanceAllowed"></param>
    /// <returns>Image after cropping, or null if there's no work to do.
    /// If result is zero width or zero height, returns null
    /// </returns>
    public static SKImage? CropMargins(SKBitmap bitmap, SKColor? color = null, double colorDistanceAllowed = 0.0)
    {
        if (bitmap == null) return null;
        if (color == null || color == SKColor.Empty) 
            color = bitmap.GetPixel(0, 0);
        var unboxedColor = color.Value;

        colorDistanceAllowed = Math.Abs(colorDistanceAllowed);
        bool shouldCheckDistance = colorDistanceAllowed != 0;

        bool search;
        int iLinesTop = 0, iLinesBottom = 0;
        int iLinesLeft = 0, iLinesRight = 0;

        search = true;
        for (int y = 0, x; y < bitmap.Height; y++)
        {
            for (x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y) != color &&
                    (!shouldCheckDistance ||
                        GetColorDistance(bitmap.GetPixel(x, y), unboxedColor) > colorDistanceAllowed)
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
        for (int y = bitmap.Height - 1, x; y >= 0; y--)
        {
            for (x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y) != color &&
                    (!shouldCheckDistance ||
                        GetColorDistance(bitmap.GetPixel(x, y), unboxedColor) > colorDistanceAllowed)
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
        for (int x = 0, y; x < bitmap.Width; x++)
        {
            for (y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y) != color &&
                    (!shouldCheckDistance ||
                        GetColorDistance(bitmap.GetPixel(x, y), unboxedColor) > colorDistanceAllowed)
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
        for (int x = bitmap.Width - 1, y; x >= 0; x--)
        {
            for (y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y) != color &&
                    (!shouldCheckDistance ||
                        GetColorDistance(bitmap.GetPixel(x, y), unboxedColor) > colorDistanceAllowed)
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

        int newWidth = bitmap.Width - iLinesLeft - iLinesRight;
        int newHeight = bitmap.Height - iLinesTop - iLinesBottom;

        if (newWidth <= 0 || newHeight <= 0 || (iLinesLeft == 0 &&
            iLinesRight == 0 && iLinesTop == 0 && iLinesBottom == 0))
        {
            return null;
        }

        return SKImage.FromBitmap(bitmap).Subset(SKRectI.Create(new SKPointI(iLinesLeft, iLinesTop), new SKSizeI(newWidth, newHeight)));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceStream"></param>
    /// <param name="destPath">Having the <paramref name="destPath"/> the same as source file path may not be safe, especially for multi-frame sources where the source is streamed.</param>
    /// <param name="colorCrop"></param>
    /// <param name="colorDistanceAllowed"></param>
    /// <param name="encodingOptions"></param>
    /// <returns></returns>
    public static bool CropMarginsAndSave(
        Stream sourceStream,
        string destPath,
        SKColor? colorCrop = null,
        double colorDistanceAllowed = 0.0,
        ImageEncodingOptions? encodingOptions = null)
    {
        if (sourceStream == null) return false;
        if (destPath == null) return false;

        ImageFrameProcessingUtil.ProcessMultiframeImage(sourceStream, destPath, null, encodingOptions ?? new(), (image, _) =>
        {
            return SKBitmap.FromImage(CropMargins(image, colorCrop, colorDistanceAllowed));
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
        SKColor? colorCrop = null,
        double colorDistanceAllowed = 0.0,
        ImageEncodingOptions? encodingOptions = null)
    {
        if (sourcePath == null) return false;
        if (destPath == null) return false;

        using var fileStream = File.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return CropMarginsAndSave(
            sourceStream: fileStream,
            destPath: destPath,
            colorCrop: colorCrop,
            colorDistanceAllowed: colorDistanceAllowed,
            encodingOptions: encodingOptions);
    }

    private static double GetColorDistance(SKColor c1, SKColor c2)
    {
        double a = Math.Pow(Convert.ToDouble(c1.Alpha - c2.Alpha), 2.0);
        double r = Math.Pow(Convert.ToDouble(c1.Red - c2.Red), 2.0);
        double g = Math.Pow(Convert.ToDouble(c1.Green - c2.Green), 2.0);
        double b = Math.Pow(Convert.ToDouble(c1.Blue - c2.Blue), 2.0);

        return Math.Sqrt(a + r + g + b);
    }
}