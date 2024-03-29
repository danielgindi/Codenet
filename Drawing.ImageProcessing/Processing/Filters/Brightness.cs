﻿using System;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters;

public class Brightness : IImageFilter
{
    public class Amount
    {
        public float Value = 0;
        public Amount(float amount)
        {
            this.Value = amount;
        }
    }

    public FilterError ProcessImage(
        DirectAccessBitmap bmp,
        params object[] args)
    {
        Amount amount = null;
        foreach (object arg in args)
        {
            if (arg is Amount)
            {
                amount = (Amount)arg;
            }
            else if (arg is Single ||
                arg is Double)
            {
                amount = new Amount(Convert.ToSingle(arg));
            }
        }

        if (amount != null && amount.Value == 0f)
        {
            return FilterError.OK;
        }

        if (amount == null)
        {
            amount = new Amount(1f);
        }

        switch (bmp.Bitmap.PixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                return ProcessImageRgba(bmp, 3, amount);
            case PixelFormat.Format32bppRgb:
                return ProcessImageRgba(bmp, 4, amount);
            case PixelFormat.Format32bppArgb:
                return ProcessImageRgba(bmp, 4, amount);
            case PixelFormat.Format32bppPArgb:
                return ProcessImage32prgba(bmp, amount);
            default:
                return FilterError.IncompatiblePixelFormat;
        }
    }

    public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength, Amount amount)
    {
        if (amount == null) return FilterError.MissingArgument;

        int cx = bmp.Width;
        int cy = bmp.Height;
        int endX = cx + bmp.StartX;
        int endY = cy + bmp.StartY;
        byte[] data = bmp.Bits;
        int stride = bmp.Stride;
        int pos;
        int x, y;
        float factor = amount.Value;
        if (factor < 0f) factor = 0f;
        int value;

        for (y = bmp.StartY; y < endY; y++)
        {
            pos = stride * y + bmp.StartX * pixelLength;

            for (x = bmp.StartX; x < endX; x++)
            {
                value = (int)(factor * data[pos]);
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos] = (byte)value;

                value = (int)(factor * data[pos + 1]);
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos + 1] = (byte)value;

                value = (int)(factor * data[pos + 2]);
                if (value > 255) value = 255;
                else if (value < 0) value = 0;
                data[pos + 2] = (byte)value;

                pos += pixelLength;
            }
        }

        return FilterError.OK;
    }
    
    public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, Amount amount)
    {
        if (amount == null) return FilterError.MissingArgument;

        int cx = bmp.Width;
        int cy = bmp.Height;
        int endX = cx + bmp.StartX;
        int endY = cy + bmp.StartY;
        byte[] data = bmp.Bits;
        int stride = bmp.Stride;
        int pos;
        int x, y;
        float preAlpha;
        float factor = amount.Value;
        if (factor < 0f) factor = 0f;
        int value;

        for (y = bmp.StartY; y < endY; y++)
        {
            pos = stride * y + bmp.StartX * 4;

            for (x = bmp.StartX; x < endX; x++)
            {
                preAlpha = (float)data[pos + 3];
                if (preAlpha > 0) preAlpha = preAlpha / 255f;

                value = (int)(factor * (data[pos] / preAlpha));
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos] = (byte)(value * preAlpha);

                value = (int)(factor * (data[pos + 1] / preAlpha));
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos + 1] = (byte)(value * preAlpha);

                value = (int)(factor * (data[pos + 2] / preAlpha));
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos + 2] = (byte)(value * preAlpha);

                pos += 4;
            }
        }

        return FilterError.OK;
    }
}
