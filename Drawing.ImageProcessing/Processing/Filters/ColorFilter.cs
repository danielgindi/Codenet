using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters;

public class ColorFilter : IImageFilter
{
    public class ColorFilterValue
    {
        private short _ValueR = 0;
        private short _ValueG = 0;
        private short _ValueB = 0;

        public short ValueR
        {
            set 
            {
                if (value < -255) value = -255;
                else if (value > 255) value = 255;
                _ValueR = value;
            }
            get { return _ValueR; }
        }

        public short ValueG
        {
            set 
            {
                if (value < -255) value = -255;
                else if (value > 255) value = 255;
                _ValueG = value;
            }
            get { return _ValueG; }
        }

        public short ValueB
        {
            set 
            {
                if (value < -255) value = -255;
                else if (value > 255) value = 255;
                _ValueB = value;
            }
            get { return _ValueB; }
        }

        public ColorFilterValue(short filterR, short filterG, short filterB)
        {
            this.ValueR = filterR;
            this.ValueG = filterG;
            this.ValueB = filterB;
        }
    }

    public FilterError ProcessImage(
        DirectAccessBitmap bmp,
        params object[] args)
    {
        ColorFilterValue filter = null;
        foreach (object arg in args)
        {
            if (arg is ColorFilterValue)
            {
                filter = (ColorFilterValue)arg;
            }
        }
        if (filter == null) return FilterError.MissingArgument;

        switch (bmp.Bitmap.PixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                return ProcessImageRgba(bmp, 3, filter);
            case PixelFormat.Format32bppRgb:
                return ProcessImageRgba(bmp, 4, filter);
            case PixelFormat.Format32bppArgb:
                return ProcessImageRgba(bmp, 4, filter);
            case PixelFormat.Format32bppPArgb:
                return ProcessImage32prgba(bmp, filter);
            default:
                return FilterError.IncompatiblePixelFormat;
        }
    }

    public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength, ColorFilterValue filter)
    {
        if (filter == null) return FilterError.MissingArgument;

        int cx = bmp.Width;
        int cy = bmp.Height;
        int endX = cx + bmp.StartX;
        int endY = cy + bmp.StartY;
        byte[] data = bmp.Bits;
        int stride = bmp.Stride;
        int pos;
        int x, y;
        int value;

        for (y = bmp.StartY; y < endY; y++)
        {
            pos = stride * y + bmp.StartX * pixelLength;

            for (x = bmp.StartX; x < endX; x++)
            {
                value = (int)(data[pos] + filter.ValueB);
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos] = (byte)value;

                value = (int)(data[pos + 1] + filter.ValueG);
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos + 1] = (byte)value;

                value = (int)(data[pos + 2] + filter.ValueR);
                if (value > 255) value = 255;
                else if (value < 0) value = 0;
                data[pos + 2] = (byte)value;

                pos += pixelLength;
            }
        }

        return FilterError.OK;
    }

    public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, ColorFilterValue filter)
    {
        if (filter == null) return FilterError.MissingArgument;

        int cx = bmp.Width;
        int cy = bmp.Height;
        int endX = cx + bmp.StartX;
        int endY = cy + bmp.StartY;
        byte[] data = bmp.Bits;
        int stride = bmp.Stride;
        int pos;
        int x, y;
        float preAlpha;
        int value;

        for (y = bmp.StartY; y < endY; y++)
        {
            pos = stride * y + bmp.StartX * 4;

            for (x = bmp.StartX; x < endX; x++)
            {
                preAlpha = (float)data[pos + 3];
                if (preAlpha > 0) preAlpha = preAlpha / 255f;

                value = (int)(data[pos] / preAlpha + filter.ValueB);
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos] = (byte)(value * preAlpha);

                value = (int)(data[pos + 1] / preAlpha + filter.ValueG);
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos + 1] = (byte)(value * preAlpha);

                value = (int)(data[pos + 2] / preAlpha + filter.ValueR);
                if (value > 255) value = 255; 
                else if (value < 0) value = 0;
                data[pos + 2] = (byte)(value * preAlpha);

                pos += 4;
            }
        }

        return FilterError.OK;
    }
}
