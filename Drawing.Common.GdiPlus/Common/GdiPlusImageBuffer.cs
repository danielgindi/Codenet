using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Codenet.Drawing.Common.Extensions;
using Codenet.Drawing.Quantizers.Helpers;

namespace Codenet.Drawing.Common;

public class GdiPlusImageBuffer : ImageBuffer
{
    private readonly Bitmap bitmap;
    private readonly BitmapData bitmapData;

    private GdiPlusImageBuffer(
        Bitmap bitmap,
        BitmapData bitmapData,
        Common.PixelFormat pixelFormat,
        int size,
        ImageLockMode lockMode
    ) : base(bitmap.Width,
            bitmap.Height,
            pixelFormat, 
            lockMode == ImageLockMode.ReadOnly || lockMode == ImageLockMode.ReadWrite,
            lockMode == ImageLockMode.WriteOnly || lockMode == ImageLockMode.ReadWrite,
            bitmapData.Scan0, false, size)
    {
        this.bitmap = bitmap;
        this.bitmapData = bitmapData;

        this.OnPaletteChange += (sender, args) =>
        {
            var palette = this.Palette;

            if (palette == null)
                throw new ArgumentNullException("Cannot assign a null palette.");

            if (bitmap == null)
                throw new ArgumentNullException("Cannot assign a palette to a null image.");

            if (!IsIndexed)
                throw new InvalidOperationException($"Cannot store a palette to a non-indexed image with pixel format '{(object)bitmap.PixelFormat}'.");

            // retrieves a target image palette
            ColorPalette imagePalette = bitmap.Palette;

            // checks if the palette can fit into the image palette
            if (palette.Count > imagePalette.Entries.Length)
            {
                throw new ArgumentOutOfRangeException($"Cannot store a palette with '{palette.Count}' colors intto an image palette where only '{imagePalette.Entries.Length}' colors are allowed.");
            }

            // copies all color entries
            for (Int32 index = 0; index < palette.Count; index++)
            {
                imagePalette.Entries[index] = Color.FromArgb(unchecked((int)palette[index].ARGB));
            }

            // assigns the palette to the target image
            bitmap.Palette = imagePalette;
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBuffer"/> class.
    /// </summary>
    public static GdiPlusImageBuffer FromImage(Image image, ImageLockMode lockMode)
    {
        return FromBitmap((Bitmap)image, lockMode);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBuffer"/> class.
    /// </summary>
    public static GdiPlusImageBuffer FromBitmap(Bitmap bitmap, ImageLockMode lockMode)
    {
        var pixelFormat = bitmap.PixelFormat.ToQuantizerPixelFormat();

        // determines the bounds of an image, and locks the data in a specified mode
        Rectangle bounds = Rectangle.FromLTRB(0, 0, bitmap.Width, bitmap.Height);

        // locks the bitmap data
        BitmapData bitmapData = null;
        lock (bitmap) bitmapData = bitmap.LockBits(bounds, lockMode, bitmap.PixelFormat);

        var stride = bitmapData.Stride < 0 ? -bitmapData.Stride : bitmapData.Stride;
        var size = stride * bitmap.Height;

        try
        {
            return new GdiPlusImageBuffer(bitmap, bitmapData, pixelFormat, size, lockMode);
        }
        catch
        {
            bitmap.UnlockBits(bitmapData);
            throw;
        }
    }

    #region | Scan colors methods |

    public static void ScanImageColors(Image sourceImage, IColorQuantizer quantizer, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        {
            source.ScanColors(quantizer, parallelTaskCount);
        }
    }

    #endregion

    #region | Synthetize palette methods |

    public static List<NeatColor> SynthetizeImagePalette(Image sourceImage, IColorQuantizer quantizer, Int32 colorCount, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        {
            return source.SynthetizePalette(quantizer, colorCount, parallelTaskCount);
        }
    }

    #endregion

    #region | Quantize methods |

    public static Image QuantizeImage(ImageBuffer source, IColorQuantizer quantizer, Int32 colorCount, Int32 parallelTaskCount = 4)
    {
        // performs the pure quantization wihout dithering
        return QuantizeImage(source, quantizer, null, colorCount, parallelTaskCount);
    }

    public static Image QuantizeImage(ImageBuffer source, IColorQuantizer quantizer, IColorDitherer ditherer, Int32 colorCount, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(source, "source");

        // creates a target bitmap in an appropriate format
        var targetPixelFormat = PixelFormatUtility.GetFormatByColorCount(colorCount).ToGdiPlusPixelFormat();
        Image result = new Bitmap(source.Width, source.Height, targetPixelFormat);

        // lock mode
        ImageLockMode lockMode = ditherer == null ? ImageLockMode.WriteOnly : ImageLockMode.ReadWrite;

        // wraps target image to a buffer
        using (var target = FromImage(result, lockMode))
        {
            source.Quantize(target, quantizer, ditherer, colorCount, parallelTaskCount);
            return result;
        }
    }

    public static Image QuantizeImage(Image sourceImage, IColorQuantizer quantizer, Int32 colorCount, Int32 parallelTaskCount = 4)
    {
        // performs the pure quantization wihout dithering
        return QuantizeImage(sourceImage, quantizer, null, colorCount, parallelTaskCount);
    }

    public static Image QuantizeImage(Image sourceImage, IColorQuantizer quantizer, IColorDitherer ditherer, Int32 colorCount, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");

        // lock mode
        ImageLockMode lockMode = ditherer == null ? ImageLockMode.ReadOnly : ImageLockMode.ReadWrite;

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, lockMode))
        {
            return QuantizeImage(source, quantizer, ditherer, colorCount, parallelTaskCount);
        }
    }

    #endregion

    #region | Calculate mean error methods |

    public static Double CalculateImageMeanError(ImageBuffer source, Image targetImage, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(source, "source");
        CheckNull(targetImage, "targetImage");

        // wraps source image to a buffer
        using (var target = FromImage(targetImage, ImageLockMode.ReadOnly))
        {
            // use other override to calculate error
            return source.CalculateMeanError(target, parallelTaskCount);
        }
    }

    public static Double CalculateImageMeanError(Image sourceImage, ImageBuffer target, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        {
            // use other override to calculate error
            return source.CalculateMeanError(target, parallelTaskCount);
        }
    }

    public static Double CalculateImageMeanError(Image sourceImage, Image targetImage, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");
        CheckNull(targetImage, "targetImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        using (var target = FromImage(targetImage, ImageLockMode.ReadOnly))
        {
            // use other override to calculate error
            return source.CalculateMeanError(target, parallelTaskCount);
        }
    }

    #endregion

    #region | Calculate normalized mean error methods |

    public static Double CalculateImageNormalizedMeanError(ImageBuffer source, Image targetImage, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(source, "source");
        CheckNull(targetImage, "targetImage");

        // wraps source image to a buffer
        using (var target = FromImage(targetImage, ImageLockMode.ReadOnly))
        {
            // use other override to calculate error
            return source.CalculateNormalizedMeanError(target, parallelTaskCount);
        }
    }

    public static Double CalculateImageNormalizedMeanError(Image sourceImage, ImageBuffer target, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        {
            // use other override to calculate error
            return source.CalculateNormalizedMeanError(target, parallelTaskCount);
        }
    }

    public static Double CalculateImageNormalizedMeanError(Image sourceImage, Image targetImage, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");
        CheckNull(targetImage, "targetImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        using (var target = FromImage(targetImage, ImageLockMode.ReadOnly))
        {
            // use other override to calculate error
            return source.CalculateNormalizedMeanError(target, parallelTaskCount);
        }
    }

    #endregion

    #region | Change pixel format methods |

    public static void ChangeFormat(
        ImageBuffer source,
        System.Drawing.Imaging.PixelFormat targetFormat,
        IColorQuantizer quantizer, 
        out Image targetImage,
        Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(source, "source");

        // creates a target bitmap in an appropriate format
        targetImage = new Bitmap(source.Width, source.Height, targetFormat);

        // wraps target image to a buffer
        using (var target = FromImage(targetImage, ImageLockMode.WriteOnly))
        {
            source.ChangeFormat(target, quantizer, parallelTaskCount);
        }
    }

    public static void ChangeFormat(Image sourceImage,
        System.Drawing.Imaging.PixelFormat targetFormat,
        IColorQuantizer quantizer,
        out Image targetImage, 
        Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        {
            ChangeFormat(source, targetFormat, quantizer, out targetImage, parallelTaskCount);
        }
    }

    #endregion

    #region | Dithering methods |

    public static void DitherImage(ImageBuffer source, Image targetImage, IColorDitherer ditherer, IColorQuantizer quantizer, Int32 colorCount, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(source, "source");
        CheckNull(targetImage, "targetImage");

        // wraps source image to a buffer
        using (var target = FromImage(targetImage, ImageLockMode.ReadWrite))
        {
            // use other override to calculate error
            source.Dither(target, ditherer, quantizer, colorCount, parallelTaskCount);
        }
    }

    public static void DitherImage(Image sourceImage, ImageBuffer target, IColorDitherer ditherer, IColorQuantizer quantizer, Int32 colorCount, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        {
            // use other override to calculate error
            source.Dither(target, ditherer, quantizer, colorCount, parallelTaskCount);
        }
    }

    public static void DitherImage(Image sourceImage, Image targetImage, IColorDitherer ditherer, IColorQuantizer quantizer, Int32 colorCount, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");
        CheckNull(targetImage, "targetImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        using (var target = FromImage(targetImage, ImageLockMode.ReadWrite))
        {
            // use other override to calculate error
            source.Dither(target, ditherer, quantizer, colorCount, parallelTaskCount);
        }
    }

    #endregion

    #region | Gamma correction |

    public static void CorrectImageGamma(Image sourceImage, Single gamma, IColorQuantizer quantizer, Int32 parallelTaskCount = 4)
    {
        // checks parameters
        CheckNull(sourceImage, "sourceImage");

        // wraps source image to a buffer
        using (var source = FromImage(sourceImage, ImageLockMode.ReadOnly))
        {
            source.CorrectGamma(gamma, quantizer, parallelTaskCount);
        }
    }

    #endregion

    #region Generic methods

    private static void CheckNull(object argument, string argumentName)
    {
        if (argument == null)
        {
            throw new ArgumentNullException($"Cannot use '{argumentName}' when it is null!");
        }
    }

    #endregion

    #region IDisposable

    private bool _Disposed = false;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_Disposed)
            {
                lock (bitmap)
                {
                    if (!_Disposed)
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                }

                _Disposed = true;
            }
        }

        base.Dispose(disposing);
    }

    #endregion
}
