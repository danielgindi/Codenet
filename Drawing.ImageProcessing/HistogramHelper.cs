using System;
using Codenet.Drawing.ImageProcessing.Processing;

namespace Codenet.Drawing.ImageProcessing
{
    public static partial class HistogramHelper
    {
        public static FilterError CalculateHistogram(DirectAccessBitmap dab, FilterColorChannel channel, out int[] values)
        {
            return CalculateHistogram(dab, channel, out values, FilterGrayScaleWeight.None);
        }

        public static FilterError CalculateHistogram(DirectAccessBitmap dab, FilterColorChannel channel, out int[] values, FilterGrayScaleWeight grayMultiplier)
        {
            values = null;

            if (dab == null)
            {
                return FilterError.InvalidArgument;
            }

            if (dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppRgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                return FilterError.IncompatiblePixelFormat;
            }

            values = new int[256];

            if (channel == FilterColorChannel.Alpha &&
                (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ||
                dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                values[255] = dab.Width * dab.Height;
                return FilterError.OK;
            }

            int cx = dab.Width;
            int cy = dab.Height;
            int endX = cx + dab.StartX;
            int endY = cy + dab.StartY;
            int pixelBytes = dab.PixelFormatSize / 8;
            //int endXb = endX * pixelBytes;
            byte[] data = dab.Bits;
            int stride = dab.Stride;
            int pos1, pos2;
            int x, y;

            if (channel == FilterColorChannel.Gray)
            {
                if (grayMultiplier == FilterGrayScaleWeight.None)
                {
                    int value;
                    if (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                    {
                        float preAlpha;
                        for (y = dab.StartY; y < endY; y++)
                        {
                            pos1 = stride * y;
                            for (x = dab.StartX; x < endX; x++)
                            {
                                pos2 = pos1 + x * pixelBytes;
                                preAlpha = (float)data[pos2 + 3];
                                if (preAlpha > 0) preAlpha = preAlpha / 255f;
                                value = (byte)(data[pos2 + 2] / preAlpha);
                                value += (byte)(data[pos2 + 1] / preAlpha);
                                value += (byte)(data[pos2] / preAlpha);
                                values[(byte)Math.Round(value / 3d)]++;
                            }
                        }
                    }
                    else
                    {
                        for (y = dab.StartY; y < endY; y++)
                        {
                            pos1 = stride * y;
                            for (x = dab.StartX; x < endX; x++)
                            {
                                pos2 = pos1 + x * pixelBytes;
                                value = data[pos2 + 2];
                                value += data[pos2 + 1];
                                value += data[pos2];
                                values[(byte)Math.Round(value / 3d)]++;
                            }
                        }
                    }
                }
                else
                {
                    double lumR, lumG, lumB;

                    if (grayMultiplier == FilterGrayScaleWeight.NaturalNTSC)
                    {
                        lumR = GrayScaleMultiplier.NtscRed;
                        lumG = GrayScaleMultiplier.NtscGreen;
                        lumB = GrayScaleMultiplier.NtscBlue;
                    }
                    else
                    {
                        lumR = GrayScaleMultiplier.NaturalRed;
                        lumG = GrayScaleMultiplier.NaturalGreen;
                        lumB = GrayScaleMultiplier.NaturalBlue;
                    }

                    if (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                    {
                        float preAlpha;
                        for (y = dab.StartY; y < endY; y++)
                        {
                            pos1 = stride * y;
                            for (x = dab.StartX; x < endX; x++)
                            {
                                pos2 = pos1 + x * pixelBytes;
                                preAlpha = (float)data[pos2 + 3];
                                if (preAlpha > 0) preAlpha = preAlpha / 255f;
                                values[(byte)Math.Round((byte)(data[pos2 + 2] / preAlpha) * lumR + (byte)(data[pos2 + 1] / preAlpha) * lumG + (byte)(data[pos2] / preAlpha) * lumB)]++;
                            }
                        }
                    }
                    else
                    {
                        for (y = dab.StartY; y < endY; y++)
                        {
                            pos1 = stride * y;
                            for (x = dab.StartX; x < endX; x++)
                            {
                                pos2 = pos1 + x * pixelBytes;
                                values[(byte)Math.Round(data[pos2 + 2] * lumR + data[pos2 + 1] * lumG + data[pos2] * lumB)]++;
                            }
                        }
                    }
                }
            }
            else
            {
                int chanOffset;
                if (channel == FilterColorChannel.Alpha) chanOffset = 3;
                else if (channel == FilterColorChannel.Red) chanOffset = 2;
                else if (channel == FilterColorChannel.Green) chanOffset = 1;
                else if (channel == FilterColorChannel.Blue) chanOffset = 0;
                else return FilterError.InvalidArgument;

                if (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                {
                    float preAlpha;
                    for (y = dab.StartY; y < endY; y++)
                    {
                        pos1 = stride * y;
                        for (x = dab.StartX; x < endX; x++)
                        {
                            pos2 = pos1 + x * pixelBytes + chanOffset;
                            preAlpha = (float)data[pos2 + 3];
                            if (preAlpha > 0) preAlpha = preAlpha / 255f;
                            values[(byte)(data[pos2] / preAlpha)]++;
                        }
                    }
                }
                else
                {
                    for (y = dab.StartY; y < endY; y++)
                    {
                        pos1 = stride * y;
                        for (x = dab.StartX; x < endX; x++)
                        {
                            pos2 = pos1 + x * pixelBytes + chanOffset;
                            values[data[pos2]]++;
                        }
                    }
                }
            }
            return FilterError.OK;
        }

        public static FilterError CalculateHistogram(DirectAccessBitmap dab, out int[] valuesR, out int[] valuesG, out int[] valuesB)
        {
            valuesR = valuesG = valuesB = null;
            if (dab == null) return FilterError.InvalidArgument;
            if (dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppRgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                return FilterError.IncompatiblePixelFormat;
            }

            valuesR = new int[256];
            valuesG = new int[256];
            valuesB = new int[256];

            int cx = dab.Width;
            int cy = dab.Height;
            int endX = cx + dab.StartX;
            int endY = cy + dab.StartY;
            int pixelBytes = dab.PixelFormatSize / 8;
            //int endXb = endX * pixelBytes;
            byte[] data = dab.Bits;
            int stride = dab.Stride;
            int pos1, pos2;
            int x, y;

            if (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                float preAlpha;
                for (y = dab.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = dab.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * pixelBytes;
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        valuesR[(byte)(data[pos2 + 2] / preAlpha)]++;
                        valuesG[(byte)(data[pos2 + 1] / preAlpha)]++;
                        valuesB[(byte)(data[pos2] / preAlpha)]++;
                    }
                }
            }
            else
            {
                for (y = dab.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = dab.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * pixelBytes;
                        valuesR[data[pos2 + 2]]++;
                        valuesG[data[pos2 + 2]]++;
                        valuesB[data[pos2]]++;
                    }
                }
            }
            return FilterError.OK;
        }
    }

}
