using System;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class Sepia : IImageFilter
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

            if (amount.Value == 0f)
            {
                return FilterError.OK;
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

        /// <summary>
        /// This one would be inline by the JIT
        /// </summary>
        private static void ProcessPixel(
            float inR, float inG, float inB,
            out float outR, out float outG, out float outB,
            float adjust)
        {
            outR = (inR * (1f - (0.607f * adjust))) + (inG * (0.769f * adjust)) + (inB * (0.189f * adjust));
            outG = (inR * (0.349f * adjust)) + (inG * (1f - (0.314f * adjust))) + (inB * (0.168f * adjust));
            outB = (inR * (0.272f * adjust)) + (inG * (0.534f * adjust)) + (inB * (1f - (0.869f * adjust)));
        }

        public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength, Amount amount)
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

            for (y = bmp.StartY; y < endY; y++)
            {
                pos = stride * y + bmp.StartX * pixelLength;

                for (x = bmp.StartX; x < endX; x++)
                {
                    bIn = data[pos];
                    gIn = data[pos + 1];
                    rIn = data[pos + 2];

                    ProcessPixel(rIn, gIn, bIn, out rOut, out gOut, out bOut, adjust);

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
        
        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, Amount amount)
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

                    ProcessPixel(rIn, gIn, bIn, out rOut, out gOut, out bOut, adjust);

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
    }
}
