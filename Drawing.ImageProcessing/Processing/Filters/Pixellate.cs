using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class Pixellate : IImageFilter
    {
        public class BlockSize
        {
            private int _BlockCx = 10;
            private int _BlockCy = 10;
            public int BlockCx
            {
                set 
                {
                    if (value < 1) value = 1;
                    _BlockCx = value;
                }
                get { return _BlockCx; }
            }
            public int BlockCy
            {
                set 
                {
                    if (value < 1) value = 1;
                    _BlockCy = value;
                }
                get { return _BlockCy; }
            }

            public BlockSize(int blockCx, int blockCy)
            {
                this.BlockCx = blockCx;
                this.BlockCy = blockCy;
            }
            public BlockSize(int blockSize)
            {
                this.BlockCx = blockSize;
                this.BlockCy = blockSize;
            }
        }
        public enum Mode
        {
            TopLeftPixel = 0,
            CenterPixel = 1,
            Interpolate = 2
        }

        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            BlockSize blockSize = null;
            Mode mode = Mode.TopLeftPixel;
            foreach (object arg in args)
            {
                if (arg is BlockSize)
                {
                    blockSize = (BlockSize)arg;
                }
                else if (arg is Mode)
                {
                    mode = (Mode)arg;
                }
            }
            if (blockSize == null) return FilterError.MissingArgument;

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, blockSize, mode);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, blockSize, mode);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, blockSize, mode);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, blockSize, mode);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }

        public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, BlockSize blockSize, Mode mode)
        {
            if (blockSize == null) return FilterError.MissingArgument;
            if (blockSize.BlockCx == 1 && blockSize.BlockCy == 1) return FilterError.OK;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos;
            int sizeCx = blockSize.BlockCx;
            int sizeCy = blockSize.BlockCy;

            int x, y, xx, yy, endX2, endY2;
            int avgR, avgG, avgB;
            byte valR, valG, valB;
            int cntPxl = 0;

            for (y = bmp.StartY; y < endY; y += sizeCy)
            {
                for (x = bmp.StartX; x < endX; x += sizeCx)
                {
                    endX2 = x + sizeCx;
                    if (endX2 > endX) endX2 = endX;
                    endY2 = y + sizeCy;
                    if (endY2 > endY) endY2 = endY;
                    if (mode == Mode.Interpolate)
                    {
                        avgR = avgG = avgB = cntPxl = 0;
                        for (yy = y; yy < endY2; yy++)
                        {
                            pos1 = stride * yy;
                            for (xx = x; xx < endX2; xx++)
                            {
                                pos = pos1 + xx * 3;

                                avgR += data[pos + 2];
                                avgG += data[pos + 1];
                                avgB += data[pos];
                                cntPxl++;
                            }
                        }
                        valR = (byte)(avgR / cntPxl);
                        valG = (byte)(avgG / cntPxl);
                        valB = (byte)(avgB / cntPxl);
                    }
                    else if (mode == Mode.CenterPixel)
                    {
                        pos = stride * (y + (endY2 - y) / 2) + (x + (endX2 - x) / 2) * 3;
                        valR = data[pos + 2];
                        valG = data[pos + 1];
                        valB = data[pos];
                    }
                    else
                    {
                        pos = stride * y + x * 3;
                        valR = data[pos + 2];
                        valG = data[pos + 1];
                        valB = data[pos];
                    }

                    for (yy = y; yy < endY2; yy++)
                    {
                        pos1 = stride * yy;
                        for (xx = x; xx < endX2; xx++)
                        {
                            pos = pos1 + xx * 3;

                            data[pos + 2] = valR;
                            data[pos + 1] = valG;
                            data[pos] = valB;
                        }
                    }
                }
            }
            return FilterError.OK;
        }

        public FilterError ProcessImage32rgb(DirectAccessBitmap bmp, BlockSize blockSize, Mode mode)
        {
            return ProcessImage32rgba(bmp, blockSize, mode);
        }

        public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, BlockSize blockSize, Mode mode)
        {
            if (blockSize == null) return FilterError.MissingArgument;
            if (blockSize.BlockCx == 1 && blockSize.BlockCy == 1) return FilterError.OK;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;

            int sizeCx = blockSize.BlockCx;
            int sizeCy = blockSize.BlockCy;

            int x, y, xx, yy, endX2, endY2;
            int avgA, avgR, avgG, avgB;
            byte valA, valR, valG, valB;
            int cntPxl = 0;

            for (y = bmp.StartY; y < endY; y += sizeCy)
            {
                for (x = bmp.StartX; x < endX; x += sizeCx)
                {
                    endX2 = x + sizeCx;
                    if (endX2 > endX) endX2 = endX;
                    endY2 = y + sizeCy;
                    if (endY2 > endY) endY2 = endY;
                    if (mode == Mode.Interpolate)
                    {
                        avgA = avgR = avgG = avgB = cntPxl = 0;
                        for (yy = y; yy < endY2; yy++)
                        {
                            pos1 = stride * yy;
                            for (xx = x; xx < endX2; xx++)
                            {
                                pos2 = pos1 + xx * 4;

                                avgA += data[pos2 + 3];
                                avgR += data[pos2 + 2];
                                avgG += data[pos2 + 1];
                                avgB += data[pos2];
                                cntPxl++;
                            }
                        }
                        valA = (byte)(avgA / cntPxl);
                        valR = (byte)(avgR / cntPxl);
                        valG = (byte)(avgG / cntPxl);
                        valB = (byte)(avgB / cntPxl);
                    }
                    else if (mode == Mode.CenterPixel)
                    {
                        pos2 = stride * (y + (endY2 - y) / 2) + (x + (endX2 - x) / 2) * 4;
                        valA = data[pos2 + 3];
                        valR = data[pos2 + 2];
                        valG = data[pos2 + 1];
                        valB = data[pos2];
                    }
                    else
                    {
                        pos2 = stride * y + x * 4;
                        valA = data[pos2 + 3];
                        valR = data[pos2 + 2];
                        valG = data[pos2 + 1];
                        valB = data[pos2];
                    }

                    for (yy = y; yy < endY2; yy++)
                    {
                        pos1 = stride * yy;
                        for (xx = x; xx < endX2; xx++)
                        {
                            pos2 = pos1 + xx * 4;

                            data[pos2 + 3] = valA;
                            data[pos2 + 2] = valR;
                            data[pos2 + 1] = valG;
                            data[pos2] = valB;
                        }
                    }
                }
            }
            return FilterError.OK;
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, BlockSize blockSize, Mode mode)
        {
            if (blockSize == null) return FilterError.MissingArgument;
            if (blockSize.BlockCx == 1 && blockSize.BlockCy == 1) return FilterError.OK;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            float preAlpha;

            int sizeCx = blockSize.BlockCx;
            int sizeCy = blockSize.BlockCy;

            int x, y, xx, yy, endX2, endY2;
            int avgA, avgR, avgG, avgB;
            byte valA, valR, valG, valB;
            int cntPxl = 0;
            for (y = bmp.StartY; y < endY; y += sizeCy)
            {
                for (x = bmp.StartX; x < endX; x += sizeCx)
                {
                    endX2 = x + sizeCx;
                    if (endX2 > endX) endX2 = endX;
                    endY2 = y + sizeCy;
                    if (endY2 > endY) endY2 = endY;
                    if (mode == Mode.Interpolate)
                    {
                        avgA = avgR = avgG = avgB = cntPxl = 0;
                        for (yy = y; yy < endY2; yy++)
                        {
                            pos1 = stride * yy;
                            for (xx = x; xx < endX2; xx++)
                            {
                                pos2 = pos1 + xx * 4;

                                preAlpha = (float)data[pos2 + 3];
                                if (preAlpha > 0) preAlpha = preAlpha / 255f;

                                avgA += data[pos2 + 3];
                                avgR += (int)(data[pos2 + 2] / preAlpha);
                                avgG += (int)(data[pos2 + 1] / preAlpha);
                                avgB += (int)(data[pos2] / preAlpha);
                                cntPxl++;
                            }
                        }
                        valA = (byte)(avgA / cntPxl);

                        preAlpha = (float)valA;
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;

                        valR = (byte)(((byte)(avgR / cntPxl)) * preAlpha);
                        valG = (byte)(((byte)(avgG / cntPxl)) * preAlpha);
                        valB = (byte)(((byte)(avgB / cntPxl)) * preAlpha);
                    }
                    else if (mode == Mode.CenterPixel)
                    {
                        pos2 = stride * (y + (endY2 - y) / 2) + (x + (endX2 - x) / 2) * 4;
                        valA = data[pos2 + 3];
                        valR = data[pos2 + 2];
                        valG = data[pos2 + 1];
                        valB = data[pos2];
                    }
                    else
                    {
                        pos2 = stride * y + x * 4;
                        valA = data[pos2 + 3];
                        valR = data[pos2 + 2];
                        valG = data[pos2 + 1];
                        valB = data[pos2];
                    }

                    for (yy = y; yy < endY2; yy++)
                    {
                        pos1 = stride * yy;
                        for (xx = x; xx < endX2; xx++)
                        {
                            pos2 = pos1 + xx * 4;

                            data[pos2 + 3] = valA;
                            data[pos2 + 2] = valR;
                            data[pos2 + 1] = valG;
                            data[pos2] = valB;
                        }
                    }
                }
            }
            return FilterError.OK;
        }
    }
}
