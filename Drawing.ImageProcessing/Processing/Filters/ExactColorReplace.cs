using System.Drawing.Imaging;
using System.Drawing;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class ExactColorReplace : IImageFilter
    {
        public class SourceColor : ColorArgument
        {
            public SourceColor(short A, short R, short G, short B, bool includeAlpha) :
                base(A, R, G, B, includeAlpha) { }
            public SourceColor(short A, short R, short G, short B)
                : base(A, R, G, B, false)
            {
            }
            public SourceColor(Color color, bool includeAlpha) :
                base(color, includeAlpha) { }
            public SourceColor(Color color) :
                base(color) { }
        }

        public class DestColor : ColorArgument
        {
            public DestColor(short A, short R, short G, short B, bool includeAlpha) :
                base(A, R, G, B, includeAlpha) { }
            public DestColor(short A, short R, short G, short B)
                : base(A, R, G, B, false)
            {
            }
            public DestColor(Color color, bool includeAlpha) :
                base(color, includeAlpha) { }
            public DestColor(Color color) :
                base(color) { }
        }

        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            SourceColor clrSource = null;
            DestColor clrDest = null;
            foreach (object arg in args)
            {
                if (arg is SourceColor) clrSource = arg as SourceColor;
                else if (arg is DestColor) clrDest = arg as DestColor;
            }
            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, clrSource, clrDest);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, clrSource, clrDest);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, clrSource, clrDest);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, clrSource, clrDest);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }

        public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, SourceColor clrSource, DestColor clrDest)
        {
            if (clrSource == null || clrDest == null)
                return FilterError.MissingArgument;
            if (clrSource.Is64Bit || clrDest.Is64Bit)
                return FilterError.InvalidArgument;

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
                pos = stride * y + bmp.StartX * 3;

                for (x = bmp.StartX; x < endX; x++)
                {
                    if (clrSource.R == data[pos + 2] &&
                        clrSource.G == data[pos + 1] &&
                        clrSource.B == data[pos])
                    {
                        data[pos + 2] = (byte)clrDest.R;
                        data[pos + 1] = (byte)clrDest.G;
                        data[pos] = (byte)clrDest.B;
                    }

                    pos += 3;
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32rgb(DirectAccessBitmap bmp, SourceColor clrSource, DestColor clrDest)
        {
            if (clrSource == null || clrDest == null)
                return FilterError.MissingArgument;
            if (clrSource.Is64Bit || clrDest.Is64Bit)
                return FilterError.InvalidArgument;

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
                pos = stride * y + bmp.StartX * 4;

                for (x = bmp.StartX; x < endX; x++)
                {
                    if (clrSource.R == data[pos + 2] &&
                        clrSource.G == data[pos + 1] &&
                        clrSource.B == data[pos])
                    {
                        data[pos + 2] = (byte)clrDest.R;
                        data[pos + 1] = (byte)clrDest.G;
                        data[pos] = (byte)clrDest.B;
                    }

                    pos += 4;
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, SourceColor clrSource, DestColor clrDest)
        {
            if (clrSource == null || clrDest == null)
                return FilterError.MissingArgument;
            if (clrSource.Is64Bit || clrDest.Is64Bit)
                return FilterError.InvalidArgument;

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
                pos = stride * y + bmp.StartX * 4;

                for (x = bmp.StartX; x < endX; x++)
                {
                    if ((!clrSource.IncludeAlpha || clrSource.A == data[pos + 3]) &&
                        clrSource.R == data[pos + 2] &&
                        clrSource.G == data[pos + 1] &&
                        clrSource.B == data[pos])
                    {
                        if (clrDest.IncludeAlpha) data[pos + 3] = (byte)clrDest.A;
                        data[pos + 2] = (byte)clrDest.R;
                        data[pos + 1] = (byte)clrDest.G;
                        data[pos] = (byte)clrDest.B;
                    }

                    pos += 4;
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, SourceColor clrSource, DestColor clrDest)
        {
            if (clrSource == null || clrDest == null)
                return FilterError.MissingArgument;
            if (clrSource.Is64Bit || clrDest.Is64Bit)
                return FilterError.InvalidArgument;

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
                    if ((!clrSource.IncludeAlpha || clrSource.A == data[pos + 3]) &&
                        clrSource.R == (data[pos + 2] / preAlpha) &&
                        clrSource.G == (data[pos + 1] / preAlpha) &&
                        clrSource.B == (data[pos] / preAlpha))
                    {
                        if (clrDest.IncludeAlpha) data[pos + 3] = (byte)clrDest.A;
                        data[pos + 2] = (byte)(clrDest.R * preAlpha);
                        data[pos + 1] = (byte)(clrDest.G * preAlpha);
                        data[pos] = (byte)(clrDest.B * preAlpha);
                    }

                    pos += 4;
                }
            }

            return FilterError.OK;
        }
    }
}
