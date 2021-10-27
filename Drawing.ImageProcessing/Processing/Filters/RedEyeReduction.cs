using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class RedEyeReduction : IImageFilter
    {
        public class RedEyeRegion
        {
            private int _X = 0;
            private int _Y = 0;
            private int _CX = 20;
            private int _CY = 20;
            private float _IntensityBarrier = 1.5f;

	        public int X
	        {
                get { return _X; }
                set { _X = value; }
	        }

            public int Y
	        {
                get { return _Y; }
                set { _Y = value; }
	        }

            public int CX
	        {
                get { return _CX; }
                set
                {
                    _CX = value;
                    if (_CX < 0) _CX = 0;
                }
	        }

            public int CY
	        {
                get { return _CY; }
                set
                {
                    _CY = value;
                    if (_CY < 0) _CY = 0;
                }
	        }

            public float IntensityBarrier
            {
                get { return _IntensityBarrier; }
                set { _IntensityBarrier = value; }
            }

            public RedEyeRegion(int CenterX, int CenterY, int RegionCX, int RegionCY)
            {
                this.X = CenterX - RegionCX / 2;
                this.Y = CenterY - RegionCY / 2;
                this.CX = RegionCX;
                this.CY = RegionCY;
            }

            public RedEyeRegion(int CenterX, int CenterY, int RegionCX, int RegionCY, float IntensityBarrier)
            {
                this.X = CenterX - RegionCX / 2;
                this.Y = CenterY - RegionCY / 2;
                this.CX = RegionCX;
                this.CY = RegionCY;
                this.IntensityBarrier = IntensityBarrier;
            }

            public RedEyeRegion(Rectangle rect)
            {
                this.X = rect.Left;
                this.Y = rect.Top;
                this.CX = rect.Width;
                this.CY = rect.Height;
            }

            public RedEyeRegion(Rectangle rect, float IntensityBarrier)
            {
                this.X = rect.Left;
                this.Y = rect.Top;
                this.CX = rect.Width;
                this.CY = rect.Height;
                this.IntensityBarrier = IntensityBarrier;
            }
        }

        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            List<RedEyeRegion> regions = new List<RedEyeRegion>();
            foreach (object arg in args)
            {
                if (arg is RedEyeRegion)
                {
                    regions.Add((RedEyeRegion)arg);
                }
            }
            if (regions.Count == 0) return FilterError.OK;

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, regions);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, regions);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, regions);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, regions);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }
        public FilterError ProcessImage24rgb(
            DirectAccessBitmap bmp,
            List<RedEyeRegion> regions)
        {
            if (regions.Count == 0) return FilterError.OK;

            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int cx = bmp.Width;
            int cy = bmp.Height;
            int startX = bmp.StartX;
            int startY = bmp.StartY;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            int regionX, regionY, regionX2, regionY2, x, y;
            int pos;
            byte valR, valG, valB;
            float redIntensity;

            foreach (RedEyeRegion region in regions)
            {
                regionX = startX + region.X;
                regionY = startY + region.Y;
                regionX2 = regionX + region.CX;
                regionY2 = regionY + region.CY;
                if (regionX < startX) regionX = startX;
                else if (regionX > endX) regionX = endX;
                if (regionY < startY) regionY = startY;
                else if (regionY > endY) regionY = endY;
                if (regionX2 < startX) regionX2 = startX;
                else if (regionX2 > endX) regionX2 = endX;
                if (regionY2 < startY) regionY2 = startY;
                else if (regionY2 > endY) regionY2 = endY;

                for (y = regionY; y < regionY2; y++)
                {
                    pos = stride * y + bmp.StartX * 3;

                    for (x = regionX; x < regionX2; x++)
                    {
                        valR = data[pos + 2];
                        valG = data[pos + 1];
                        valB = data[pos];

                        redIntensity = ((float)valR / ((((float)valG) + ((float)valB)) / 2.0f));
                        if (redIntensity > region.IntensityBarrier)
                        {
                            data[pos + 2] = 90;
                        }

                        pos += 3;
                    }
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32rgb(
            DirectAccessBitmap bmp,
            List<RedEyeRegion> regions)
        {
            if (regions.Count == 0) return FilterError.OK;

            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int cx = bmp.Width;
            int cy = bmp.Height;
            int startX = bmp.StartX;
            int startY = bmp.StartY;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            int regionX, regionY, regionX2, regionY2, x, y;
            int pos;
            byte valR, valG, valB;
            float redIntensity;

            foreach (RedEyeRegion region in regions)
            {
                regionX = startX + region.X;
                regionY = startY + region.Y;
                regionX2 = regionX + region.CX;
                regionY2 = regionY + region.CY;
                if (regionX < startX) regionX = startX;
                else if (regionX > endX) regionX = endX;
                if (regionY < startY) regionY = startY;
                else if (regionY > endY) regionY = endY;
                if (regionX2 < startX) regionX2 = startX;
                else if (regionX2 > endX) regionX2 = endX;
                if (regionY2 < startY) regionY2 = startY;
                else if (regionY2 > endY) regionY2 = endY;

                for (y = regionY; y < regionY2; y++)
                {
                    pos = stride * y + bmp.StartX * 4;

                    for (x = regionX; x < regionX2; x++)
                    {
                        valR = data[pos + 2];
                        valG = data[pos + 1];
                        valB = data[pos];

                        redIntensity = ((float)valR / ((((float)valG) + ((float)valB)) / 2.0f));
                        if (redIntensity > region.IntensityBarrier)
                        {
                            data[pos + 2] = 90;
                        }

                        pos += 4;
                    }
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32rgba(
            DirectAccessBitmap bmp,
            List<RedEyeRegion> regions)
        {
            return ProcessImage32rgb(bmp, regions);
        }

        public FilterError ProcessImage32prgba(
            DirectAccessBitmap bmp,
            List<RedEyeRegion> regions)
        {
            if (regions.Count == 0) return FilterError.OK;

            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int cx = bmp.Width;
            int cy = bmp.Height;
            int startX = bmp.StartX;
            int startY = bmp.StartY;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            int regionX, regionY, regionX2, regionY2, x, y;
            int pos;
            byte valR, valG, valB;
            float redIntensity;
            float preAlpha;

            foreach (RedEyeRegion region in regions)
            {
                regionX = startX + region.X;
                regionY = startY + region.Y;
                regionX2 = regionX + region.CX;
                regionY2 = regionY + region.CY;
                if (regionX < startX) regionX = startX;
                else if (regionX > endX) regionX = endX;
                if (regionY < startY) regionY = startY;
                else if (regionY > endY) regionY = endY;
                if (regionX2 < startX) regionX2 = startX;
                else if (regionX2 > endX) regionX2 = endX;
                if (regionY2 < startY) regionY2 = startY;
                else if (regionY2 > endY) regionY2 = endY;

                for (y = regionY; y < regionY2; y++)
                {
                    pos = stride * y + bmp.StartX * 4;

                    for (x = regionX; x < regionX2; x++)
                    {
                        preAlpha = (float)data[pos + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;

                        valR = (byte)(int)(data[pos + 2] / preAlpha);
                        valG = (byte)(int)(data[pos + 1] / preAlpha);
                        valB = (byte)(int)(data[pos] / preAlpha);

                        redIntensity = ((float)valR / ((((float)valG) + ((float)valB)) / 2.0f));
                        if (redIntensity > region.IntensityBarrier)
                        {
                            valR = (byte)(int)(90 * preAlpha);
                        }

                        pos += 4;
                    }
                }
            }

            return FilterError.OK;
       }
    }
}
