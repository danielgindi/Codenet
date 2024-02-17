using System;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters;

public class ReduceTransparency : IImageFilter
{
    public class TransparencyMultiplier
    {
        public float Value = 0;
        public TransparencyMultiplier(float amount)
        {
            this.Value = amount;
        }
    }

    public FilterError ProcessImage(
        DirectAccessBitmap bmp,
        params object[] args)
    {
        TransparencyMultiplier multiplier = null;

        foreach (object arg in args)
        {
            if (arg is TransparencyMultiplier)
            {
                multiplier = (TransparencyMultiplier)arg;
            }
            else if (arg is Single ||
                arg is Double)
            {
                multiplier = new TransparencyMultiplier(Convert.ToSingle(arg));
            }
        }

        if (multiplier == null)
        {
            multiplier = new TransparencyMultiplier(0.7f);
        }

        switch (bmp.Bitmap.PixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                return ProcessImage24rgb(bmp, multiplier);
            case PixelFormat.Format32bppRgb:
                return ProcessImage32rgb(bmp, multiplier);
            case PixelFormat.Format32bppArgb:
                return ProcessImage32rgba(bmp, multiplier);
            case PixelFormat.Format32bppPArgb:
                return ProcessImage32prgba(bmp, multiplier);
            default:
                return FilterError.IncompatiblePixelFormat;
        }
    }

    public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, TransparencyMultiplier transparencyMultiplier)
    {
        return FilterError.OK;
    }

    public FilterError ProcessImage32rgb(DirectAccessBitmap bmp, TransparencyMultiplier transparencyMultiplier)
    {
        return FilterError.OK;
    }

    public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, TransparencyMultiplier transparencyMultiplier)
    {
        int cx = bmp.Width;
        int cy = bmp.Height;
        int endX = cx + bmp.StartX;
        int endY = cy + bmp.StartY;
        byte[] data = bmp.Bits;
        int stride = bmp.Stride;
        int pos;
        int x, y;
        int alphaByte;
        float factor = transparencyMultiplier.Value;

        for (y = bmp.StartY; y < endY; y++)
        {
            pos = stride * y + bmp.StartX * 4;

            for (x = bmp.StartX; x < endX; x++)
            {
                alphaByte = data[pos + 3];
                if (alphaByte != 0 && alphaByte != 255)
                {
                    alphaByte = 255 - (int)Math.Round((255 - alphaByte) * factor);
                    if (alphaByte > 255)
                    {
                        alphaByte = 255;
                    }
                    else if (alphaByte < 0)
                    {
                        alphaByte = 0;
                    }
                    data[pos + 3] = (byte)alphaByte;
                }

                pos += 4;
            }
        }

        return FilterError.OK;
    }

    public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, TransparencyMultiplier transparencyMultiplier)
    {
        int cx = bmp.Width;
        int cy = bmp.Height;
        int endX = cx + bmp.StartX;
        int endY = cy + bmp.StartY;
        byte[] data = bmp.Bits;
        int stride = bmp.Stride;
        int pos;
        int x, y;
        float preAlpha, postAlpha;
        float factor = transparencyMultiplier.Value;

        for (y = bmp.StartY; y < endY; y++)
        {
            pos = stride * y + bmp.StartX * 4;

            for (x = bmp.StartX; x < endX; x++)
            {
                preAlpha = (float)data[pos + 3];
                if (preAlpha != 0.0f & preAlpha != 1.0f)
                {
                    if (preAlpha > 0)
                    {
                        preAlpha = preAlpha / 255f;
                    }

                    postAlpha = 1.0f - ((1.0f - preAlpha) * factor);
                    if (postAlpha > 1.0f)
                    {
                        postAlpha = 1.0f;
                    }
                    else if (postAlpha < 0.0f)
                    {
                        postAlpha = 0.0f;
                    }

                    data[pos] = (byte)((data[pos] / preAlpha) * postAlpha);
                    data[pos + 1] = (byte)((data[pos + 1] / preAlpha) * postAlpha);
                    data[pos + 2] = (byte)((data[pos + 2] / preAlpha) * postAlpha);
                    data[pos + 3] = (byte)(postAlpha * 255.0f);
                }

                pos += 4;
            }
        }

        return FilterError.OK;
    }
}
