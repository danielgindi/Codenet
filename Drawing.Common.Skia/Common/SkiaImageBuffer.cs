using System;
using System.IO;
using SkiaSharp;

#nullable enable

namespace Codenet.Drawing.Common;

public class SkiaImageBuffer : ImageBuffer
{
    private readonly SKBitmap _Bitmap;
    private bool _OwnsBitmap;

    private SkiaImageBuffer(
        SKBitmap bitmap,
        PixelFormat pixelFormat,
        IntPtr ptr,
        int size,
        bool canWrite,
        bool ownsBitmap
    ) : base(bitmap.Width, bitmap.Height, pixelFormat, true, canWrite && !bitmap.IsImmutable, ptr, false, size)
    {
        this._Bitmap = bitmap;
        this._OwnsBitmap = ownsBitmap;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBuffer"/> class.
    /// </summary>
    public static SkiaImageBuffer FromImage(SKImage image, bool canWrite = true)
    {
        var bitmap = SKBitmap.FromImage(image);
        if (bitmap == null)
            throw new InvalidDataException("Failed to decode image data.");
        return FromBitmap(bitmap, canWrite, true);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBuffer"/> class.
    /// </summary>
    public static SkiaImageBuffer FromBitmap(SKBitmap bitmap, bool canWrite = true, bool ownsBitmap = true)
    {
        var pixelFormat = bitmap.ColorType switch
        {
            SKColorType.Bgra8888 when bitmap.AlphaType != SKAlphaType.Premul => PixelFormat.Format32bppArgb,
            SKColorType.Bgra8888 when bitmap.AlphaType == SKAlphaType.Premul => PixelFormat.Format32bppPArgb,
            SKColorType.Rgb888x => PixelFormat.Format32bppRgb,
            SKColorType.Rgb565 => PixelFormat.Format16bppRgb565,
            _ => throw new NotSupportedException("Unsupported pixel format.")
        };


        var ptr = bitmap.GetPixels(out var length);
        var size = length.ToInt32();

        return new SkiaImageBuffer(bitmap, pixelFormat, ptr, size, canWrite, ownsBitmap);
    }

    #region IDisposable

    private bool _Disposed = false;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // dispose managed resources
        }

        if (!_Disposed)
        {
            // dispose unmanaged resources
            if (_OwnsBitmap)
            {
                _Bitmap.Dispose();
                _OwnsBitmap = false;
            }

            _Disposed = true;
        }
    }

    ~SkiaImageBuffer()
    {
        Dispose(false);
    }

    #endregion
}