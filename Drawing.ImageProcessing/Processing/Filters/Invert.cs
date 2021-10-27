using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class Invert : IImageFilter
    {
        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImageRgba(bmp, 3);
                case PixelFormat.Format32bppRgb:
                    return ProcessImageRgba(bmp, 4);
                case PixelFormat.Format32bppArgb:
                    return ProcessImageRgba(bmp, 4);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }

        public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos = stride * y + bmp.StartX * pixelLength;

                for (x = bmp.StartX; x < endX; x++)
                {
                    data[pos] = (byte)(255 - data[pos]);
                    data[pos + 1] = (byte)(255 - data[pos + 1]);
                    data[pos + 2] = (byte)(255 - data[pos + 2]);

                    pos += pixelLength;
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp)
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

            for (y = bmp.StartY; y < endY; y++)
            {
                pos = stride * y + bmp.StartX * 4;

                for (x = bmp.StartX; x < endX; x++)
                {
                    preAlpha = (float)data[pos + 3];
                    if (preAlpha > 0) preAlpha = preAlpha / 255f;

                    data[pos] = (byte)((255f - (data[pos] / preAlpha)) * preAlpha);
                    data[pos + 1] = (byte)((255f - (data[pos + 1] / preAlpha)) * preAlpha);
                    data[pos + 2] = (byte)((255f - (data[pos + 2] / preAlpha)) * preAlpha);

                    pos += 4;
                }
            }

            return FilterError.OK;
        }
    }
}
