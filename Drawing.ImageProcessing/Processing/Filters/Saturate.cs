using System;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters;

public class Saturate : IImageFilter
{
    public class Amount
    {
        private float _Value = 1f;
        public Amount(float amount)
        {
            Value = amount;
        }
        public float Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
    }

    public FilterError ProcessImage(
        DirectAccessBitmap bmp,
        params object[] args)
    {
        FilterGrayScaleWeight mode = FilterGrayScaleWeight.Natural;
        Amount amount = null;

        foreach (object arg in args)
        {
            if (arg is FilterGrayScaleWeight)
            {
                mode = (FilterGrayScaleWeight)arg;
            }
            else if (arg is Amount)
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
                return ProcessImageRgba(bmp, 3, mode, amount);
            case PixelFormat.Format32bppRgb:
                return ProcessImageRgba(bmp, 4, mode, amount);
            case PixelFormat.Format32bppArgb:
                return ProcessImageRgba(bmp, 4, mode, amount);
            case PixelFormat.Format32bppPArgb:
                return ProcessImage32prgba(bmp, mode, amount);
            default:
                return FilterError.IncompatiblePixelFormat;
        }
    }
    
    public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength, FilterGrayScaleWeight mode, Amount amount)
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

        float rL = 0, gL = 0, bL = 0;

        if (mode == FilterGrayScaleWeight.NaturalNTSC)
        {
            rL = GrayScaleMultiplier.NtscRed;
            gL = GrayScaleMultiplier.NtscGreen;
            bL = GrayScaleMultiplier.NtscBlue;
        }
        else if (mode == FilterGrayScaleWeight.Natural)
        {
            rL = GrayScaleMultiplier.NaturalRed;
            gL = GrayScaleMultiplier.NaturalGreen;
            bL = GrayScaleMultiplier.NaturalBlue;
        }
        else if (mode == FilterGrayScaleWeight.Css)
        {
            rL = GrayScaleMultiplier.CssRed;
            gL = GrayScaleMultiplier.CssGreen;
            bL = GrayScaleMultiplier.CssBlue;
        }
        else
        {
            rL = GrayScaleMultiplier.SimpleRed;
            gL = GrayScaleMultiplier.SimpleGreen;
            bL = GrayScaleMultiplier.SimpleBlue;
        }

        float M1_1, M1_2, M1_3, M2_1, M2_2, M2_3, M3_1, M3_2, M3_3;
        float v;
        M1_1 = (1.0f - amount.Value) * rL + amount.Value;
        M1_2 = (1.0f - amount.Value) * rL;
        M1_3 = (1.0f - amount.Value) * rL;
        M2_1 = (1.0f - amount.Value) * gL;
        M2_2 = (1.0f - amount.Value) * gL + amount.Value;
        M2_3 = (1.0f - amount.Value) * gL;
        M3_1 = (1.0f - amount.Value) * bL;
        M3_2 = (1.0f - amount.Value) * bL;
        M3_3 = (1.0f - amount.Value) * bL + amount.Value;

        float r, g, b;
        for (y = bmp.StartY; y < endY; y++)
        {
            pos = stride * y + bmp.StartX * pixelLength;

            for (x = bmp.StartX; x < endX; x++)
            {
                r = data[pos + 2];
                g = data[pos + 1];
                b = data[pos];

                v = (M1_1 * r + M2_1 * g + M3_1 * b);
                if (v > 255) v = 255; 
                else if (v < 0) v = 0;
                data[pos + 2] = (byte)v;

                v = (M1_2 * r + M2_2 * g + M3_2 * b);
                if (v > 255) v = 255; 
                else if (v < 0) v = 0;
                data[pos + 1] = (byte)v;

                v = (M1_3 * r + M2_3 * g + M3_3 * b);
                if (v > 255) v = 255; 
                else if (v < 0) v = 0;
                data[pos] = (byte)v;

                pos += pixelLength;
            }
        }

        return FilterError.OK;
    }

    public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, FilterGrayScaleWeight mode, Amount amount)
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

        float rL = 0, gL = 0, bL = 0;

        if (mode == FilterGrayScaleWeight.NaturalNTSC)
        {
            rL = GrayScaleMultiplier.NtscRed;
            gL = GrayScaleMultiplier.NtscGreen;
            bL = GrayScaleMultiplier.NtscBlue;
        }
        else if (mode == FilterGrayScaleWeight.Natural)
        {
            rL = GrayScaleMultiplier.NaturalRed;
            gL = GrayScaleMultiplier.NaturalGreen;
            bL = GrayScaleMultiplier.NaturalBlue;
        }
        else if (mode == FilterGrayScaleWeight.Css)
        {
            rL = GrayScaleMultiplier.CssRed;
            gL = GrayScaleMultiplier.CssGreen;
            bL = GrayScaleMultiplier.CssBlue;
        }
        else
        {
            rL = GrayScaleMultiplier.SimpleRed;
            gL = GrayScaleMultiplier.SimpleGreen;
            bL = GrayScaleMultiplier.SimpleBlue;
        }

        float M1_1, M1_2, M1_3, M2_1, M2_2, M2_3, M3_1, M3_2, M3_3;
        float v;
        M1_1 = (1.0f - amount.Value) * rL + amount.Value;
        M1_2 = (1.0f - amount.Value) * rL;
        M1_3 = (1.0f - amount.Value) * rL;
        M2_1 = (1.0f - amount.Value) * gL;
        M2_2 = (1.0f - amount.Value) * gL + amount.Value;
        M2_3 = (1.0f - amount.Value) * gL;
        M3_1 = (1.0f - amount.Value) * bL;
        M3_2 = (1.0f - amount.Value) * bL;
        M3_3 = (1.0f - amount.Value) * bL + amount.Value;

        float r, g, b;
        for (y = bmp.StartY; y < endY; y++)
        {
            pos = stride * y + bmp.StartX * 4;

            for (x = bmp.StartX; x < endX; x++)
            {
                preAlpha = (float)data[pos + 3];
                if (preAlpha > 0) preAlpha = preAlpha / 255f;
                
                r = data[pos + 2] / preAlpha;
                g = data[pos + 1] / preAlpha;
                b = data[pos] / preAlpha;

                v = (M1_1 * r + M2_1 * g + M3_1 * b);
                if (v > 255) v = 255; 
                else if (v < 0) v = 0;
                data[pos + 2] = (byte)(v * preAlpha);

                v = (M1_2 * r + M2_2 * g + M3_2 * b);
                if (v > 255) v = 255; 
                else if (v < 0) v = 0;
                data[pos + 1] = (byte)(v * preAlpha);

                v = (M1_3 * r + M2_3 * g + M3_3 * b);
                if (v > 255) v = 255;
                else if (v < 0) v = 0;
                data[pos] = (byte)(v * preAlpha);

                pos += 4;
            }
        }

        return FilterError.OK;
    }
}
