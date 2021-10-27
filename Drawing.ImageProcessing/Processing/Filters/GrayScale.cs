using System;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class GrayScale : IImageFilter
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

            if (amount.Value == 1f)
            {
                switch (bmp.Bitmap.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        return ProcessImageRgba(bmp, 3, mode);
                    case PixelFormat.Format32bppRgb:
                        return ProcessImageRgba(bmp, 4, mode);
                    case PixelFormat.Format32bppArgb:
                        return ProcessImageRgba(bmp, 4, mode);
                    case PixelFormat.Format32bppPArgb:
                        return ProcessImage32prgba(bmp, mode);
                    default:
                        return FilterError.IncompatiblePixelFormat;
                }
            }
            else
            {
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
        }

        /// <summary>
        /// This one would be inline by the JIT
        /// </summary>
        private static void ProcessPixel(
            float inR, float inG, float inB,
            out float outR, out float outG, out float outB,
            float factorR, float factorG, float factorB,
            float factorRInv, float factorGInv, float factorBInv,
            float adjust)
        {
            outR = (inR * (1f - (factorRInv * adjust))) + (inG * (factorG * adjust)) + (inB * (factorB * adjust));
            outG = (inR * (factorR * adjust)) + (inG * (1f - (factorGInv * adjust))) + (inB * (factorB * adjust));
            outB = (inR * (factorR * adjust)) + (inG * (factorG * adjust)) + (inB * (1f - (factorBInv * adjust)));
        }

        public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength, FilterGrayScaleWeight mode, Amount amount)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;

            float adjust = amount.Value;
            float
                rIn, gIn, bIn,
                rOut, gOut, bOut;

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

            float rLInv = 1f - rL,
                gLInv = 1f - gL,
                bLInv = 1f - bL;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos = stride * y + bmp.StartX * pixelLength;

                for (x = bmp.StartX; x < endX; x++)
                {
                    bIn = data[pos];
                    gIn = data[pos + 1];
                    rIn = data[pos + 2];

                    ProcessPixel(
                        rIn, gIn, bIn, 
                        out rOut, out gOut, out bOut, 
                        rL, gL, bL,
                        rLInv, gLInv, bLInv,
                        adjust);
                        
                    if (rOut > 255f) rOut = 255f;
                    else if (rOut < 0f) rOut = 0f;
                    if (gOut > 255f) gOut = 255f;
                    else if (gOut < 0f) gOut = 0f;
                    if (bOut > 255f) bOut = 255f;
                    else if (bOut < 0f) bOut = 0f;

                    data[pos] = (byte)rOut;
                    data[pos + 1] = (byte)gOut;
                    data[pos + 2] = (byte)bOut;

                    pos += pixelLength;
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, FilterGrayScaleWeight mode, Amount amount)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;
            float preAlpha;

            float adjust = amount.Value;
            float
                rIn, gIn, bIn,
                rOut, gOut, bOut;

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

            float rLInv = 1f - rL, 
                gLInv = 1f - gL,
                bLInv = 1f - bL;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos = stride * y + bmp.StartX * 4;

                for (x = bmp.StartX; x < endX; x++)
                {
                    preAlpha = (float)data[pos + 3];
                    if (preAlpha > 0) preAlpha = preAlpha / 255f;

                    bIn = data[pos] / preAlpha;
                    gIn = data[pos + 1] / preAlpha;
                    rIn = data[pos + 2] / preAlpha;

                    ProcessPixel(
                        rIn, gIn, bIn,
                        out rOut, out gOut, out bOut,
                        rL, gL, bL,
                        rLInv, gLInv, bLInv,
                        adjust);

                    if (rOut > 255f) rOut = 255f;
                    else if (rOut < 0f) rOut = 0f;
                    if (gOut > 255f) gOut = 255f;
                    else if (gOut < 0f) gOut = 0f;
                    if (bOut > 255f) bOut = 255f;
                    else if (bOut < 0f) bOut = 0f;

                    data[pos] = (byte)(rOut * preAlpha);
                    data[pos + 1] = (byte)(gOut * preAlpha);
                    data[pos + 2] = (byte)(bOut * preAlpha);

                    pos += 4;
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength, FilterGrayScaleWeight mode)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;

            if (mode == FilterGrayScaleWeight.Simple)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos = stride * y + bmp.StartX * pixelLength;

                    for (x = bmp.StartX; x < endX; x++)
                    {
                        data[pos] = data[pos + 1] = data[pos + 2] =
                            (byte)((data[pos] + data[pos + 1] + data[pos + 2]) / 3f);

                        pos += pixelLength;
                    }
                }
            }
            else
            {
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

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos = stride * y + bmp.StartX * pixelLength;

                    for (x = bmp.StartX; x < endX; x++)
                    {
                        data[pos] = data[pos + 1] = data[pos + 2] =
                            (byte)((data[pos] * bL +
                            data[pos + 1] * gL +
                            data[pos + 2] * rL));

                        pos += pixelLength;
                    }
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, FilterGrayScaleWeight mode)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;
            float preAlpha;

            if (mode == FilterGrayScaleWeight.Simple)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos = stride * y + bmp.StartX * 4;

                    for (x = bmp.StartX; x < endX; x++)
                    {
                        preAlpha = (float)data[pos + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        data[pos] = data[pos + 1] = data[pos + 2] =
                            (byte)(((
                            (data[pos] / preAlpha) +
                            (data[pos + 1] / preAlpha) +
                            (data[pos + 2] / preAlpha)
                            ) / 3f) * preAlpha);

                        pos += 4;
                    }
                }
            }
            else
            {
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

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos = stride * y + bmp.StartX * 4;

                    for (x = bmp.StartX; x < endX; x++)
                    {
                        preAlpha = (float)data[pos + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        data[pos] = data[pos + 1] = data[pos + 2] =
                            (byte)(((
                            (data[pos] / preAlpha) * bL +
                            (data[pos + 1] / preAlpha) * gL +
                            (data[pos + 2] / preAlpha) * rL
                            )) * preAlpha);

                        pos += 4;

                    }
                }
            }

            return FilterError.OK;
        }
    }
}
