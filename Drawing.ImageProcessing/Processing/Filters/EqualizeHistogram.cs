using System;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    /// <summary>
    /// This will equalize the histogram for an image.
    /// It will never work on the Alpha channel.
    /// 
    /// Optional arguments:
    /// Channel: sets the channels to be equalized. Default is RGB
    /// HistogramParam: passes ready histogram data, to prevent extra calculations
    /// GrayMultiplier: for gray channel only, type of luminosity multipliers
    /// </summary>
    public class EqualizeHistogram : IImageFilter
    {
        /************************************************************************/
        /*                                                                      */
        /* A little theory:                                                     */
        /* ----------------                                                     */
        /*                                                                      */
        /* Probability of a pixel value:                                        */
        /*   p(i) = n(i) / n = Histogram of i / Number of Pixels = [0..1]       */
        /*                                                                      */
        /* Cumulative distribution function:                                    */
        /*           i                                                          */
        /*  cdf(i) = Σ   p(j)                                                   */
        /*           j=0                                                        */
        /*                                                                      */
        /* Normalize to number of pixels:                                       */
        /*  cdf(i) = cdf(i) * number of pixels                                  */
        /*                                                                      */
        /* Compute cdf for all pixels in the image, and then normalize to       */
        /*  the number of color levels [0..255]:                                */
        /*                                                                      */
        /* h(v) = round( ( ( cdf(v) - cdfmin ) / ( (MxN) - cdfmin ) ) * (L-1) ) */
        /*                                                                      */
        /* h: resulting color level                                             */
        /* v: current color level value                                         */
        /* cdf: normalized table of cdf for all pixels in the image             */
        /* cdfmin: minimum value in the cdf table                               */
        /* M: width of image in pixels                                          */
        /* N: height of image in pixels                                         */
        /* L: number of color levels in the image (usually 256)                 */
        /*                                                                      */
        /************************************************************************/

        public class HistogramParam
        {
            public FilterColorChannel Channel;
            public int[] Histogram;

            public HistogramParam(FilterColorChannel channel, int[] histogram)
            {
                Channel = channel;
                Histogram = histogram;
            }
        }

        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            FilterColorChannel channels = FilterColorChannel.None;
            FilterGrayScaleWeight grayMultiplier = FilterGrayScaleWeight.None;
            int[] histogramR = null;
            int[] histogramG = null;
            int[] histogramB = null;
            int[] histogramGrey = null;
            foreach (object arg in args)
            {
                if (arg is HistogramParam)
                {
                    switch (((HistogramParam)arg).Channel)
                    {
                        case FilterColorChannel.Red:
                            histogramR = ((HistogramParam)arg).Histogram;
                            if (histogramR != null) channels |= FilterColorChannel.Red;
                            break;
                        case FilterColorChannel.Green:
                            histogramG = ((HistogramParam)arg).Histogram;
                            if (histogramG != null) channels |= FilterColorChannel.Green;
                            break;
                        case FilterColorChannel.Blue:
                            histogramB = ((HistogramParam)arg).Histogram;
                            if (histogramB != null) channels |= FilterColorChannel.Blue;
                            break;
                        case FilterColorChannel.Gray:
                            histogramGrey = ((HistogramParam)arg).Histogram;
                            if (histogramGrey != null) channels = FilterColorChannel.Gray;
                            break;
                    }
                }
                else if (arg is FilterColorChannel)
                {
                    channels |= (FilterColorChannel)arg;
                }
                else if (arg is FilterGrayScaleWeight)
                {
                    grayMultiplier |= (FilterGrayScaleWeight)arg;
                }
            }

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:

                    if (channels == FilterColorChannel.Gray)
                    {
                        return ProcessImageRgba(bmp, 3, histogramGrey, grayMultiplier);
                    }
                    else
                    {
                        return ProcessImageRgba(bmp, 3, channels, histogramR, histogramG, histogramB);
                    }

                case PixelFormat.Format32bppRgb:

                    if (channels == FilterColorChannel.Gray)
                    {
                        return ProcessImageRgba(bmp, 4, histogramGrey, grayMultiplier);
                    }
                    else
                    {
                        return ProcessImageRgba(bmp, 4, channels, histogramR, histogramG, histogramB);
                    }

                case PixelFormat.Format32bppArgb:

                    if (channels == FilterColorChannel.Gray)
                    {
                        return ProcessImageRgba(bmp, 4, histogramGrey, grayMultiplier);
                    }
                    else
                    {
                        return ProcessImageRgba(bmp, 4, channels, histogramR, histogramG, histogramB);
                    }

                case PixelFormat.Format32bppPArgb:

                    if (channels == FilterColorChannel.Gray)
                    {
                        return ProcessImage32prgba(bmp, histogramGrey, grayMultiplier);
                    }
                    else
                    {
                        return ProcessImage32prgba(bmp, channels, histogramR, histogramG, histogramB);
                    }

                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }

        private FilterError CalculateHistogramsIfNeeded(DirectAccessBitmap dab, FilterColorChannel channels, ref int[] R, ref int[] G, ref int [] B)
        {
            FilterColorChannel channelsToRetrieve = FilterColorChannel.None;
            if ((channels & FilterColorChannel.Red) == FilterColorChannel.Red && R == null)
                channelsToRetrieve |= FilterColorChannel.Red;
            if ((channels & FilterColorChannel.Green) == FilterColorChannel.Green && G == null)
                channelsToRetrieve |= FilterColorChannel.Green;
            if ((channels & FilterColorChannel.Blue) == FilterColorChannel.Blue && B == null)
                channelsToRetrieve |= FilterColorChannel.Blue;
            if (channelsToRetrieve != FilterColorChannel.None)
            {
                FilterError err;
                if (channelsToRetrieve == FilterColorChannel.Red)
                {
                    err = HistogramHelper.CalculateHistogram(dab, FilterColorChannel.Red, out R);
                }
                else if (channelsToRetrieve == FilterColorChannel.Green)
                {
                    err = HistogramHelper.CalculateHistogram(dab, FilterColorChannel.Green, out G);
                }
                else if (channelsToRetrieve == FilterColorChannel.Blue)
                {
                    err = HistogramHelper.CalculateHistogram(dab, FilterColorChannel.Blue, out B);
                }
                else
                {
                    err = HistogramHelper.CalculateHistogram(dab, out R, out G, out B);
                }
                if (err != FilterError.OK) return err;
            }
            return FilterError.OK;
        }

        private double[] CalculateCDF(int[] histogram, int numberOfPixels)
        {
            double[] cdf = new double[histogram.Length];
            double sigma = 0d;
            double numOfPx = (double)numberOfPixels;

            for (int i = 0, l = histogram.Length; i < l; i++)
            {
                if (histogram[i] > 0)
                {
                    sigma += histogram[i] / numOfPx;
                    cdf[i] = sigma;
                }
            }

            return cdf;
        }

        private int[] NormalizeCDF(double[] cdf, int numberOfPixels)
        {
            int[] ncdf = new int[cdf.Length];

            for (int i = 0, l = cdf.Length; i < l; i++)
            {
                ncdf[i] = (int)(cdf[i] * (double)numberOfPixels);
            }

            return ncdf;
        }

        private int FindLowestExcludingZero(int[] cdf)
        {
            int lowest = int.MaxValue;
            int cur;

            for (int i = 0, l = cdf.Length; i < l; i++)
            {
                cur = cdf[i];
                if (cur < lowest && cur != 0) lowest = cur;
            }

            if (lowest == int.MaxValue) lowest = 0;
            return lowest;
        }
        
        public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength, FilterColorChannel channels, int[] histR, int[] histG, int[] histB)
        {
            if (channels == FilterColorChannel.None) channels = FilterColorChannel.RGB;
            FilterError err = CalculateHistogramsIfNeeded(bmp, channels, ref histR, ref histG, ref histB);
            if (err != FilterError.OK) return err;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;

            int numberOfPixels = cx*cy;
            int[] cdfR = null, cdfG = null, cdfB = null;
            int cdfminR = 0, cdfminG = 0, cdfminB = 0;
            if (histR != null)
            {
                cdfR = NormalizeCDF(CalculateCDF(histR, numberOfPixels), numberOfPixels);
                cdfminR = FindLowestExcludingZero(cdfR);
                if (cdfminR == 0) cdfR = null; // Do not work on this channel
            }
            if (histG != null)
            {
                cdfG = NormalizeCDF(CalculateCDF(histG, numberOfPixels), numberOfPixels);
                cdfminG = FindLowestExcludingZero(cdfG);
                if (cdfminG == 0) cdfG = null; // Do not work on this channel
            }
            if (histB != null)
            {
                cdfB = NormalizeCDF(CalculateCDF(histB, numberOfPixels), numberOfPixels);
                cdfminB = FindLowestExcludingZero(cdfB);
                if (cdfminB == 0) cdfB = null; // Do not work on this channel
            }
            double numberOfPixelsR = numberOfPixels - cdfminR;
            double numberOfPixelsG = numberOfPixels - cdfminG;
            double numberOfPixelsB = numberOfPixels - cdfminB;
            int curPos;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos = stride * y + bmp.StartX * pixelLength;

                for (x = bmp.StartX; x < endX; x++)
                {
                    if (histR != null)
                    {
                        curPos = pos + 2;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfR[data[curPos]] - cdfminR) / numberOfPixelsR) * 255));
                    }
                    if (histG != null)
                    {
                        curPos = pos + 1;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfG[data[curPos]] - cdfminG) / numberOfPixelsG) * 255));
                    }
                    if (histB != null)
                    {
                        curPos = pos;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfB[data[curPos]] - cdfminB) / numberOfPixelsB) * 255));
                    }

                    pos += pixelLength;
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, FilterColorChannel channels, int[] histR, int[] histG, int[] histB)
        {
            if (channels == FilterColorChannel.None) channels = FilterColorChannel.RGB;
            FilterError err = CalculateHistogramsIfNeeded(bmp, channels, ref histR, ref histG, ref histB);
            if (err != FilterError.OK) return err;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;
            float preAlpha;

            int numberOfPixels = cx * cy;
            int[] cdfR = null, cdfG = null, cdfB = null;
            int cdfminR = 0, cdfminG = 0, cdfminB = 0;
            if (histR != null)
            {
                cdfR = NormalizeCDF(CalculateCDF(histR, numberOfPixels), numberOfPixels);
                cdfminR = FindLowestExcludingZero(cdfR);
            }
            if (histG != null)
            {
                cdfG = NormalizeCDF(CalculateCDF(histG, numberOfPixels), numberOfPixels);
                cdfminG = FindLowestExcludingZero(cdfG);
            }
            if (histB != null)
            {
                cdfB = NormalizeCDF(CalculateCDF(histB, numberOfPixels), numberOfPixels);
                cdfminB = FindLowestExcludingZero(cdfB);
            }
            double numberOfPixelsR = numberOfPixels - cdfminR;
            double numberOfPixelsG = numberOfPixels - cdfminG;
            double numberOfPixelsB = numberOfPixels - cdfminB;
            int curPos;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos = stride * y + bmp.StartX * 4;

                for (x = bmp.StartX; x < endX; x++)
                {
                    if (histR != null)
                    {
                        preAlpha = (float)data[pos + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        curPos = pos + 2;
                        data[curPos] =
                            (byte)(Math.Round(
                            (double)(((cdfR[(byte)(data[curPos] / preAlpha)] - cdfminR) / numberOfPixelsR) * 255))
                            * preAlpha);
                    }
                    if (histG != null)
                    {
                        preAlpha = (float)data[pos + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        curPos = pos + 1;
                        data[curPos] =
                            (byte)(Math.Round(
                            (double)(((cdfG[(byte)(data[curPos] / preAlpha)] - cdfminG) / numberOfPixelsG) * 255))
                            * preAlpha);
                    }
                    if (histB != null)
                    {
                        preAlpha = (float)data[pos + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        curPos = pos;
                        data[curPos] =
                            (byte)(Math.Round(
                            (double)(((cdfB[(byte)(data[curPos] / preAlpha)] - cdfminB) / numberOfPixelsB) * 255))
                            * preAlpha);
                    }

                    pos += 4;
                }
            }

            return FilterError.OK;
        }
        
        public FilterError ProcessImageRgba(DirectAccessBitmap bmp, int pixelLength, int[] histGrey, FilterGrayScaleWeight grayMultiplier)
        {
            if (histGrey == null)
            {
                FilterError err = HistogramHelper.CalculateHistogram(bmp, FilterColorChannel.Gray, out histGrey, grayMultiplier);
                if (err != FilterError.OK) return err;
            }

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;

            int numberOfPixels = cx * cy;
            int[] cdf;
            int cdfmin = 0;
            cdf = NormalizeCDF(CalculateCDF(histGrey, numberOfPixels), numberOfPixels);
            cdfmin = FindLowestExcludingZero(cdf);
            if (cdfmin == 0) return FilterError.OK; // No cdf...

            double numberOfPixels2 = numberOfPixels - cdfmin;
            int value;

            if (grayMultiplier == FilterGrayScaleWeight.None)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos = stride * y + bmp.StartX * pixelLength;

                    for (x = bmp.StartX; x < endX; x++)
                    {
                        value = data[pos + 2] + data[pos + 1] + data[pos];
                        value = (byte)(value / 3);
                        data[pos + 2] = data[pos + 1] = data[pos] =
                            (byte)Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255));

                        pos += pixelLength;
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
                else if (grayMultiplier == FilterGrayScaleWeight.Natural)
                {
                    lumR = GrayScaleMultiplier.NaturalRed;
                    lumG = GrayScaleMultiplier.NaturalGreen;
                    lumB = GrayScaleMultiplier.NaturalBlue;
                }
                else if (grayMultiplier == FilterGrayScaleWeight.Css)
                {
                    lumR = GrayScaleMultiplier.CssRed;
                    lumG = GrayScaleMultiplier.CssGreen;
                    lumB = GrayScaleMultiplier.CssBlue;
                }
                else
                {
                    lumR = GrayScaleMultiplier.SimpleRed;
                    lumG = GrayScaleMultiplier.SimpleGreen;
                    lumB = GrayScaleMultiplier.SimpleBlue;
                }

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos = stride * y + bmp.StartX * pixelLength;

                    for (x = bmp.StartX; x < endX; x++)
                    {
                        value = (byte)(data[pos + 2] * lumR) + (byte)(data[pos + 1] * lumG) + (byte)(data[pos] * lumB);
                        data[pos + 2] = data[pos + 1] = data[pos] =
                            (byte)Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255));

                        pos += pixelLength;
                    }
                }
            }

            return FilterError.OK;
        }
        
        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, int[] histGrey, FilterGrayScaleWeight grayMultiplier)
        {
            if (histGrey == null)
            {
                FilterError err = HistogramHelper.CalculateHistogram(bmp, FilterColorChannel.Gray, out histGrey, grayMultiplier);
                if (err != FilterError.OK) return err;
            }

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos;
            int x, y;
            float preAlpha;

            int numberOfPixels = cx * cy;
            int[] cdf;
            int cdfmin = 0;
            cdf = NormalizeCDF(CalculateCDF(histGrey, numberOfPixels), numberOfPixels);
            cdfmin = FindLowestExcludingZero(cdf);

            double numberOfPixels2 = numberOfPixels - cdfmin;
            int value;

            if (grayMultiplier == FilterGrayScaleWeight.None)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos = stride * y + bmp.StartX * 4;

                    for (x = bmp.StartX; x < endX; x++)
                    {
                        preAlpha = (float)data[pos + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        value = (byte)(data[pos + 2] / preAlpha);
                        value += (byte)(data[pos + 1] / preAlpha);
                        value += (byte)(data[pos] / preAlpha);
                        value = (byte)(value / 3);
                        data[pos + 2] = data[pos + 1] = data[pos] =
                            (byte)(Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255))
                            * preAlpha);

                        pos += 4;
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
                else if (grayMultiplier == FilterGrayScaleWeight.Natural)
                {
                    lumR = GrayScaleMultiplier.NaturalRed;
                    lumG = GrayScaleMultiplier.NaturalGreen;
                    lumB = GrayScaleMultiplier.NaturalBlue;
                }
                else if (grayMultiplier == FilterGrayScaleWeight.Css)
                {
                    lumR = GrayScaleMultiplier.CssRed;
                    lumG = GrayScaleMultiplier.CssGreen;
                    lumB = GrayScaleMultiplier.CssBlue;
                }
                else
                {
                    lumR = GrayScaleMultiplier.SimpleRed;
                    lumG = GrayScaleMultiplier.SimpleGreen;
                    lumB = GrayScaleMultiplier.SimpleBlue;
                }

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos = stride * y + bmp.StartX * 4;

                    for (x = bmp.StartX; x < endX; x++)
                    {
                        preAlpha = (float)data[pos + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;

                        value = (int)Math.Round((byte)(data[pos + 2] / preAlpha) * lumR + 
                            (byte)(data[pos + 1] / preAlpha) * lumG + 
                            (byte)(data[pos] / preAlpha) * lumB);

                        data[pos + 2] = data[pos + 1] = data[pos] =
                            (byte)(Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255))
                            * preAlpha);

                        pos += 4;
                    }
                }
            }

            return FilterError.OK;
        }
    }
}
