using System.Collections.Generic;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class ConvolutionMatrix : IImageFilter
    {
        public abstract class MatrixBase
        {
            public abstract int Size { get; }
            public virtual int HalfSize { get { return Size / 2; } }
            public abstract float[][] Matrix { get; }
            public abstract float Divisor { get; set; }
            public abstract float Offset { get; set; }
            public abstract bool Normalize { get; set; }

            public virtual void Reset()
            {
                int xyCenter = (Size - 1) / 2;
                for (int y = 0; y < Size; y++)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        if (x == xyCenter && y == xyCenter)
                            Matrix[y][x] = 1f;
                        else Matrix[y][x] = 0f;
                    }
                }
                Divisor = 1f;
                Offset = 0f;
            }
            public virtual float Sum()
            {
                float sum = 0f;
                for (int y = 0; y < Size; y++)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        sum += Matrix[y][x];
                    }
                }
                return sum;
            }
            public virtual void DoNormalize()
            {
                float sum = Sum();
                if (sum > 0)
                {
                    Divisor = sum;
                    Offset = 0;
                }
                else if (sum < 0)
                {
                    Divisor = -sum;
                    Offset = 255;
                }
                else
                {
                    Divisor = 1;
                    Offset = 128;
                }
            }
        }

        public class Matrix3x3 : MatrixBase
        {
            private float[][] _Matrix = new float[][]{
                  new float[]{0,0,0},
                  new float[]{0,1,0},
                  new float[]{0,0,0}
                      };
            private float _Divisor = 1f;
            private float _Offset;
            private bool _Normalize = false;

            public Matrix3x3(
                float m1_1, float m1_2, float m1_3,
                float m2_1, float m2_2, float m2_3,
                float m3_1, float m3_2, float m3_3,
                float divisor, float offset, bool normalize
                )
            {
                this._Matrix[0][0] = m1_1;
                this._Matrix[0][1] = m1_2;
                this._Matrix[0][2] = m1_3;
                this._Matrix[1][0] = m2_1;
                this._Matrix[1][1] = m2_2;
                this._Matrix[1][2] = m2_3;
                this._Matrix[2][0] = m3_1;
                this._Matrix[2][1] = m3_2;
                this._Matrix[2][2] = m3_3;
                this.Divisor = divisor;
                this.Offset = offset;
                this.Normalize = normalize;
            }

            public Matrix3x3()
            {
            }

            public override int Size 
            {
                get { return 3; }
            }

            public override float[][] Matrix
            {
                get { return _Matrix; }
            }

            public override float Divisor
            {
                get { return _Divisor; }
                set { _Divisor = value == 0f ? 1f : value; }
            }

            public override float Offset
            {
                get { return _Offset; }
                set { _Offset = value; }
            }

            public override bool Normalize
            {
                get { return _Normalize; }
                set { if (value == _Normalize) return; _Normalize = value; DoNormalize(); }
            }
        }

        public class Matrix5x5 : MatrixBase
        {
            private float[][] _Matrix = new float[][]{
                  new float[]{0,0,0,0,0},
                  new float[]{0,0,0,0,0},
                  new float[]{0,0,1,0,0},
                  new float[]{0,0,0,0,0},
                  new float[]{0,0,0,0,0}
                      };
            private float _Divisor = 1f;
            private float _Offset;
            private bool _Normalize = false;

            public Matrix5x5(
                float m1_1, float m1_2, float m1_3, float m1_4, float m1_5,
                float m2_1, float m2_2, float m2_3, float m2_4, float m2_5,
                float m3_1, float m3_2, float m3_3, float m3_4, float m3_5,
                float m4_1, float m4_2, float m4_3, float m4_4, float m4_5,
                float m5_1, float m5_2, float m5_3, float m5_4, float m5_5,
                float divisor, float offset, bool normalize
                )
            {
                this._Matrix[0][0] = m1_1;
                this._Matrix[0][1] = m1_2;
                this._Matrix[0][2] = m1_3;
                this._Matrix[0][3] = m1_4;
                this._Matrix[0][4] = m1_5;
                this._Matrix[1][0] = m2_1;
                this._Matrix[1][1] = m2_2;
                this._Matrix[1][2] = m2_3;
                this._Matrix[1][3] = m2_4;
                this._Matrix[1][4] = m2_5;
                this._Matrix[2][0] = m3_1;
                this._Matrix[2][1] = m3_2;
                this._Matrix[2][2] = m3_3;
                this._Matrix[2][3] = m3_4;
                this._Matrix[2][4] = m3_5;
                this._Matrix[3][0] = m4_1;
                this._Matrix[3][1] = m4_2;
                this._Matrix[3][2] = m4_3;
                this._Matrix[3][3] = m4_4;
                this._Matrix[3][4] = m4_5;
                this._Matrix[4][0] = m5_1;
                this._Matrix[4][1] = m5_2;
                this._Matrix[4][2] = m5_3;
                this._Matrix[4][3] = m5_4;
                this._Matrix[4][4] = m5_5;
                this.Divisor = divisor;
                this.Offset = offset;
                this.Normalize = normalize;
            }

            public Matrix5x5()
            {
            }

            public override int Size
            { 
                get { return 5; } 
            }
            public override float[][] Matrix
            { 
                get { return _Matrix; }
            }

            public override float Divisor
            {
                get { return _Divisor; }
                set { _Divisor = value == 0f ? 1f : value; }
            }

            public override float Offset
            {
                get { return _Offset; }
                set { _Offset = value; }
            }

            public override bool Normalize
            {
                get { return _Normalize; }
                set { if (value == _Normalize) return; _Normalize = value; DoNormalize(); }
            }
        }

        public class Matrix7x7 : MatrixBase
        {
            private float[][] _Matrix = new float[][]{
                  new float[]{0,0,0,0,0,0,0},
                  new float[]{0,0,0,0,0,0,0},
                  new float[]{0,0,0,0,0,0,0},
                  new float[]{0,0,0,1,0,0,0},
                  new float[]{0,0,0,0,0,0,0},
                  new float[]{0,0,0,0,0,0,0},
                  new float[]{0,0,0,0,0,0,0}
                      };
            private float _Divisor = 1f;
            private float _Offset;
            private bool _Normalize = false;

            public Matrix7x7(
                float m1_1, float m1_2, float m1_3, float m1_4, float m1_5, float m1_6, float m1_7,
                float m2_1, float m2_2, float m2_3, float m2_4, float m2_5, float m2_6, float m2_7,
                float m3_1, float m3_2, float m3_3, float m3_4, float m3_5, float m3_6, float m3_7,
                float m4_1, float m4_2, float m4_3, float m4_4, float m4_5, float m4_6, float m4_7,
                float m5_1, float m5_2, float m5_3, float m5_4, float m5_5, float m5_6, float m5_7,
                float m6_1, float m6_2, float m6_3, float m6_4, float m6_5, float m6_6, float m6_7,
                float m7_1, float m7_2, float m7_3, float m7_4, float m7_5, float m7_6, float m7_7,
                float divisor, float offset, bool normalize
                )
            {
                _Matrix[0][0] = m1_1;_Matrix[0][1] = m1_2;_Matrix[0][2] = m1_3;
                _Matrix[0][3] = m1_4;_Matrix[0][4] = m1_5;_Matrix[0][5] = m1_6;
                _Matrix[0][6] = m1_7;
                _Matrix[1][0] = m2_1; _Matrix[1][1] = m2_2; _Matrix[1][2] = m2_3;
                _Matrix[1][3] = m2_4; _Matrix[1][4] = m2_5; _Matrix[1][5] = m2_6;
                _Matrix[1][6] = m2_7;
                _Matrix[2][0] = m3_1; _Matrix[2][1] = m3_2; _Matrix[2][2] = m3_3;
                _Matrix[2][3] = m3_4; _Matrix[2][4] = m3_5; _Matrix[2][5] = m3_6;
                _Matrix[2][6] = m3_7;
                _Matrix[3][0] = m4_1; _Matrix[3][1] = m4_2; _Matrix[3][2] = m4_3;
                _Matrix[3][3] = m4_4; _Matrix[3][4] = m4_5; _Matrix[3][5] = m4_6;
                _Matrix[3][6] = m4_7;
                _Matrix[4][0] = m5_1; _Matrix[4][1] = m5_2; _Matrix[4][2] = m5_3;
                _Matrix[4][3] = m5_4; _Matrix[4][4] = m5_5; _Matrix[4][5] = m5_6;
                _Matrix[4][6] = m5_7;
                _Matrix[5][0] = m6_1; _Matrix[5][1] = m6_2; _Matrix[5][2] = m6_3;
                _Matrix[5][3] = m6_4; _Matrix[5][4] = m6_5; _Matrix[5][5] = m6_6;
                _Matrix[5][6] = m6_7;
                _Matrix[6][0] = m7_1; _Matrix[6][1] = m7_2; _Matrix[6][2] = m7_3;
                _Matrix[6][3] = m7_4; _Matrix[6][4] = m7_5; _Matrix[6][5] = m7_6;
                _Matrix[6][6] = m7_7;
                this.Divisor = divisor;
                this.Offset = offset;
                this.Normalize = normalize;
            }

            public Matrix7x7()
            {
            }

            public override int Size
            {
                get { return 7; } 
            }

            public override float[][] Matrix
            { 
                get { return _Matrix; } 
            }

            public override float Divisor
            {
                get { return _Divisor; }
                set { _Divisor = value == 0f ? 1f : value; }
            }

            public override float Offset
            {
                get { return _Offset; }
                set { _Offset = value; }
            }

            public override bool Normalize
            {
                get { return _Normalize; }
                set { if (value == _Normalize) return; _Normalize = value; DoNormalize(); }
            }
        }

        public enum Channel
        {
            None = 0,
            Red = 1,
            Green = 2,
            Blue = 4,
            Alpha = 8,
            RGB = Red | Green | Blue,
            ARGB = Alpha | RGB
        }

        public enum AlphaWeighting
        {
            None = 0,
            UseAlphaWeighting = 1,
        }

        /// <summary>
        /// Applies a convolution matrix to the bitmap.
        /// 
        /// Default channels are RGB (without alpha)
        /// </summary>
        /// <param name="bmp">Bitmap to process</param>
        /// <param name="args">Matrix3x3 or Matrix5x5, Channel.</param>
        /// <returns>ImageFilterError</returns>
        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            MatrixBase kernel = null;
            Channel channels = Channel.None;
            AlphaWeighting alphaWeighting = AlphaWeighting.None;

            foreach (object arg in args)
            {
                if (arg is MatrixBase)
                {
                    kernel = arg as MatrixBase;
                }
                else if (arg is Channel)
                {
                    channels |= (Channel)arg;
                }
                else if (arg is AlphaWeighting)
                {
                    alphaWeighting |= (AlphaWeighting)arg;
                }
            }
            if (kernel == null) return FilterError.MissingArgument;
            if (channels == Channel.None) channels = Channel.RGB;

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, kernel, (channels | Channel.Alpha) ^ Channel.Alpha, alphaWeighting);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgba(bmp, kernel, (channels | Channel.Alpha) ^ Channel.Alpha, alphaWeighting);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, kernel, channels, alphaWeighting);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, kernel, channels, alphaWeighting);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }

        private int[][] CloneChannelArrayOffset(int[][] channelArray, int offset)
        {
            int[][] channelClone = new int[channelArray.Length][];
            int x,y,len;
            for (y = 0; y < channelArray.Length; y++)
            {
                len = channelArray[y].Length;
                int[] lst = new int[len];
                for (x = 0; x < len; x++)
                {
                    lst[x] = channelArray[y][x] + offset;
                }
                channelClone[y] = lst;
            }
            return channelClone;
        }

        public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, MatrixBase kernel, Channel channels, AlphaWeighting alphaWeighting)
        {
            if (kernel == null) return FilterError.MissingArgument;
            if (channels == Channel.None) return FilterError.OK;

            if (kernel.Normalize) kernel.DoNormalize();

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            byte[] dataClone = (byte[])data.Clone();
            int stride = bmp.Stride;
            int bPos;
            int startX = bmp.StartX, startY = bmp.StartY;
            int x, y, mx, my, c;
            float fValue = 0f;

            int Size = kernel.Size;
            int HalfSize = kernel.HalfSize;
            int XYCenter = (Size - 1) / 2;

            int startXX = startX + HalfSize;
            int startYY = startX + HalfSize;
            int endXX = endX - HalfSize;
            int endYY = endY - HalfSize;

            int[][] channelBase = new int[Size][];
            for (y = 0; y < Size; y++)
            {
                int[] lst = new int[Size];
                for (x = 0; x < Size; x++)
                {
                    lst[x] = (y < XYCenter || y > XYCenter) ? (stride * (y - XYCenter)) : 0;
                    lst[x] += (x < XYCenter || x > XYCenter) ? (3 * (x - XYCenter)) : 0;
                }
                channelBase[y] = lst;
            }

            List<int[][]> lstChannelsOffsets = new List<int[][]>();
            List<Channel> lstChannelIds = new List<Channel>();
            if ((channels & Channel.Blue) == Channel.Blue)
            {
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 0));
                lstChannelIds.Add(Channel.Blue);
            }
            if ((channels & Channel.Green) == Channel.Green)
            {
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 1));
                lstChannelIds.Add(Channel.Green);
            }
            if ((channels & Channel.Red) == Channel.Red)
            {
                lstChannelIds.Add(Channel.Red);
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 2));
            }
            int[][][] channelsOffsets = lstChannelsOffsets.ToArray();
            int[][] curChannel;
            Channel[] channelIds = lstChannelIds.ToArray();
            int countChannels = channelsOffsets.Length;

            for (y = startYY; y < endYY; y++)
            {
                bPos = (stride * y) + (startXX * 3);
                for (x = startXX; x < endXX; x++)
                {
                    for (c = 0; c < countChannels; c++)
                    {
                        curChannel = channelsOffsets[c];
                        fValue = 0f;
                        for (my = 0; my < Size; my++)
                        {
                            for (mx = 0; mx < Size; mx++)
                            {
                                fValue += dataClone[bPos + curChannel[my][mx]] * kernel.Matrix[my][mx];
                            }
                        }
                        fValue /= kernel.Divisor;
                        fValue += kernel.Offset;
                        if (fValue < 0f) fValue = 0f;
                        else if (fValue > 255f) fValue = 255f;
                        data[bPos + curChannel[XYCenter][XYCenter]] = (byte)fValue;
                    }

                    bPos += 3;
                }
            }

            bool bYOutOfRange, bXOutOfRange;
            for (y = startY; y < endY; y++)
            {
                bYOutOfRange = y < startYY || y >= endYY;
                bPos = (stride * y) + (startX * 3);
                for (x = startX; x < endX; x++)
                {
                    bXOutOfRange = x < startXX || x >= endXX;
                    if (!bXOutOfRange && !bYOutOfRange)
                    {
                        x = endXX - 1;
                        bPos = (stride * y) + (endXX * 3);
                        continue;
                    }

                    for (c = 0; c < countChannels; c++)
                    {
                        curChannel = channelsOffsets[c];
                        fValue = 0f;
                        for (my = 0; my < Size; my++)
                        {
                            if (y + my - XYCenter < startY ||
                                y + my - XYCenter >= endY) continue;
                            for (mx = 0; mx < Size; mx++)
                            {
                                if (x + mx - XYCenter < startX ||
                                    x + mx - XYCenter >= endX) continue;
                                fValue += dataClone[bPos + curChannel[my][mx]] * kernel.Matrix[my][mx];
                            }
                        }
                        fValue /= kernel.Divisor;
                        fValue += kernel.Offset;
                        if (fValue < 0f) fValue = 0f;
                        else if (fValue > 255f) fValue = 255f;
                        data[bPos + curChannel[XYCenter][XYCenter]] = (byte)fValue;
                    }

                    bPos += 3;
                }
            }
            return FilterError.OK;
        }

        public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, MatrixBase kernel, Channel channels, AlphaWeighting alphaWeighting)
        {
            if (kernel == null) return FilterError.MissingArgument;
            if (channels == Channel.None) return FilterError.OK;
            if ((bmp.Bitmap.PixelFormat & PixelFormat.Alpha) == 0) alphaWeighting = AlphaWeighting.None;

            if (kernel.Normalize) kernel.DoNormalize();

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            byte[] dataClone = (byte[])data.Clone();
            int stride = bmp.Stride;
            int bPos;
            int startX = bmp.StartX, startY = bmp.StartY;
            int x, y, mx, my, c;
            float fValue = 0f, alphaSum = 0f;
            float temp;
            float matrixSum = kernel.Sum();

            int Size = kernel.Size;
            int HalfSize = kernel.HalfSize;
            int XYCenter = (Size - 1) / 2;

            int startXX = startX + HalfSize;
            int startYY = startX + HalfSize;
            int endXX = endX - HalfSize;
            int endYY = endY - HalfSize;

            int[][] channelBase = new int[Size][];
            for (y = 0; y < Size; y++)
            {
                int[] lst = new int[Size];
                for (x = 0; x < Size; x++)
                {
                    lst[x] = (y < XYCenter || y > XYCenter) ? (stride * (y - XYCenter)) : 0;
                    lst[x] += (x < XYCenter || x > XYCenter) ? (4 * (x - XYCenter)) : 0;
                }
                channelBase[y] = lst;
            }

            int[][] curChannel;
            int[][] alphaChannel = null;
            Channel curChannelId;

            List<int[][]> lstChannelsOffsets = new List<int[][]>();
            List<Channel> lstChannelIds = new List<Channel>();
            if ((channels & Channel.Blue) == Channel.Blue)
            {
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 0));
                lstChannelIds.Add(Channel.Blue);
            }
            if ((channels & Channel.Green) == Channel.Green)
            {
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 1));
                lstChannelIds.Add(Channel.Green);
            }
            if ((channels & Channel.Red) == Channel.Red)
            {
                lstChannelIds.Add(Channel.Red);
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 2));
            }
            if ((channels & Channel.Alpha) == Channel.Alpha
                || alphaWeighting == AlphaWeighting.UseAlphaWeighting)
            {
                alphaChannel = CloneChannelArrayOffset(channelBase, 3);
                if ((channels & Channel.Alpha) == Channel.Alpha)
                {
                    lstChannelsOffsets.Add(alphaChannel);
                    lstChannelIds.Add(Channel.Alpha);
                }
            }
            int[][][] channelsOffsets = lstChannelsOffsets.ToArray();
            Channel[] channelIds = lstChannelIds.ToArray();
            int countChannels = channelsOffsets.Length;

            for (y = startYY; y < endYY; y++)
            {
                bPos = (stride * y) + (startXX * 4);
                for (x = startXX; x < endXX; x++)
                {
                    alphaSum = 0f;
                    for (c = 0; c < countChannels; c++)
                    {
                        curChannel = channelsOffsets[c];
                        curChannelId = channelIds[c];
                        fValue = 0f;
                        for (my = 0; my < Size; my++)
                        {
                            for (mx = 0; mx < Size; mx++)
                            {
                                temp = kernel.Matrix[my][mx];

                                if (curChannelId != Channel.Alpha && alphaWeighting == AlphaWeighting.UseAlphaWeighting)
                                {
                                    temp *= dataClone[bPos + alphaChannel[my][mx]];
                                    alphaSum += temp < 0 ? -temp : temp;
                                }

                                fValue += dataClone[bPos + curChannel[my][mx]] * temp;
                            }
                        }
                        fValue /= kernel.Divisor;

                        if (curChannelId != Channel.Alpha && alphaWeighting == AlphaWeighting.UseAlphaWeighting)
                        {
                            if (alphaSum != 0)
                                fValue = fValue * matrixSum / alphaSum;
                            else
                                fValue = 0;
                        }

                        fValue += kernel.Offset;
                        if (fValue < 0f) fValue = 0f;
                        else if (fValue > 255f) fValue = 255f;
                        data[bPos + curChannel[XYCenter][XYCenter]] = (byte)fValue;
                    }

                    bPos += 4;
                }
            }

            bool bYOutOfRange, bXOutOfRange;
            for (y = startY; y < endY; y++)
            {
                bYOutOfRange = y < startYY || y >= endYY;
                bPos = (stride * y) + (startX * 4);
                for (x = startX; x < endX; x++)
                {
                    bXOutOfRange = x < startXX || x >= endXX;
                    if (!bXOutOfRange && !bYOutOfRange)
                    {
                        x = endXX - 1;
                        bPos = (stride * y) + (endXX * 4);
                        continue;
                    }

                    alphaSum = 0f;
                    for (c = 0; c < countChannels; c++)
                    {
                        curChannel = channelsOffsets[c];
                        curChannelId = channelIds[c];
                        fValue = 0f;
                        for (my = 0; my < Size; my++)
                        {
                            if (y + my - XYCenter < startY ||
                                y + my - XYCenter >= endY) continue;
                            for (mx = 0; mx < Size; mx++)
                            {
                                if (x + mx - XYCenter < startX ||
                                    x + mx - XYCenter >= endX) continue;

                                temp = kernel.Matrix[my][mx];

                                if (curChannelId != Channel.Alpha && alphaWeighting == AlphaWeighting.UseAlphaWeighting)
                                {
                                    temp *= dataClone[bPos + alphaChannel[my][mx]];
                                    alphaSum += temp < 0 ? -temp : temp;
                                }

                                fValue += dataClone[bPos + curChannel[my][mx]] * temp;
                            }
                        }
                        fValue /= kernel.Divisor;

                        if (curChannelId != Channel.Alpha && alphaWeighting == AlphaWeighting.UseAlphaWeighting)
                        {
                            if (alphaSum != 0)
                                fValue = fValue * matrixSum / alphaSum;
                            else
                                fValue = 0;
                        }

                        fValue += kernel.Offset;
                        if (fValue < 0f) fValue = 0f;
                        else if (fValue > 255f) fValue = 255f;
                        data[bPos + curChannel[XYCenter][XYCenter]] = (byte)fValue;
                    }

                    bPos += 4;
                }
            }
            return FilterError.OK;
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, MatrixBase kernel, Channel channels, AlphaWeighting alphaWeighting)
        {
            if (kernel == null) return FilterError.MissingArgument;
            if (channels == Channel.None) return FilterError.OK;

            if (kernel.Normalize) kernel.DoNormalize();

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            byte[] dataClone = (byte[])data.Clone();
            int stride = bmp.Stride;
            int bPos;
            int startX = bmp.StartX, startY = bmp.StartY;
            int x, y, mx, my, c;
            float preAlpha;
            float fValue = 0f, alphaSum = 0f;
            float temp;
            float matrixSum = kernel.Sum();

            int Size = kernel.Size;
            int HalfSize = kernel.HalfSize;
            int XYCenter = (Size - 1) / 2;

            int startXX = startX + HalfSize;
            int startYY = startX + HalfSize;
            int endXX = endX - HalfSize;
            int endYY = endY - HalfSize;

            int[][] channelBase = new int[Size][];
            for (y = 0; y < Size; y++)
            {
                int[] lst = new int[Size];
                for (x = 0; x < Size; x++)
                {
                    lst[x] = (y < XYCenter || y > XYCenter) ? (stride * (y - XYCenter)) : 0;
                    lst[x] += (x < XYCenter || x > XYCenter) ? (4 * (x - XYCenter)) : 0;
                }
                channelBase[y] = lst;
            }

            int[][] curChannel;
            int[][] alphaChannel = null;
            Channel curChannelId;

            List<int[][]> lstChannelsOffsets = new List<int[][]>();
            List<Channel> lstChannelIds = new List<Channel>();
            if ((channels & Channel.Blue) == Channel.Blue)
            {
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 0));
                lstChannelIds.Add(Channel.Blue);
            }
            if ((channels & Channel.Green) == Channel.Green)
            {
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 1));
                lstChannelIds.Add(Channel.Green);
            }
            if ((channels & Channel.Red) == Channel.Red)
            {
                lstChannelIds.Add(Channel.Red);
                lstChannelsOffsets.Add(CloneChannelArrayOffset(channelBase, 2));
            }
            if ((channels & Channel.Alpha) == Channel.Alpha
                || alphaWeighting == AlphaWeighting.UseAlphaWeighting)
            {
                alphaChannel = CloneChannelArrayOffset(channelBase, 3);
                if ((channels & Channel.Alpha) == Channel.Alpha)
                {
                    lstChannelsOffsets.Add(alphaChannel);
                    lstChannelIds.Add(Channel.Alpha);
                }
            }
            int[][][] channelsOffsets = lstChannelsOffsets.ToArray();
            Channel[] channelIds = lstChannelIds.ToArray();
            int countChannels = channelsOffsets.Length;

            for (y = startYY; y < endYY; y++)
            {
                bPos = (stride * y) + (startXX * 4);
                for (x = startXX; x < endXX; x++)
                {
                    alphaSum = 0f;
                    for (c = 0; c < countChannels; c++)
                    {
                        curChannel = channelsOffsets[c];
                        curChannelId = channelIds[c];
                        fValue = 0f;
                        for (my = 0; my < Size; my++)
                        {
                            for (mx = 0; mx < Size; mx++)
                            {
                                temp = kernel.Matrix[my][mx];

                                if (curChannelId != Channel.Alpha && alphaWeighting == AlphaWeighting.UseAlphaWeighting)
                                {
                                    temp *= dataClone[bPos + alphaChannel[my][mx]];
                                    alphaSum += temp < 0 ? -temp : temp;
                                }

                                preAlpha = (float)dataClone[bPos + alphaChannel[my][mx]];
                                if (preAlpha > 0) preAlpha = preAlpha / 255f;

                                fValue += (dataClone[bPos + curChannel[my][mx]] / preAlpha) * temp;
                            }
                        }
                        fValue /= kernel.Divisor;

                        if (curChannelId != Channel.Alpha && alphaWeighting == AlphaWeighting.UseAlphaWeighting)
                        {
                            if (alphaSum != 0)
                                fValue = fValue * matrixSum / alphaSum;
                            else
                                fValue = 0;
                        }

                        fValue += kernel.Offset;
                        if (fValue < 0f) fValue = 0f;
                        else if (fValue > 255f) fValue = 255f;
                        preAlpha = (float)data[bPos + alphaChannel[XYCenter][XYCenter]];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        data[bPos + curChannel[XYCenter][XYCenter]] = (byte)(fValue * preAlpha);
                    }

                    bPos += 4;
                }
            }

            bool bYOutOfRange, bXOutOfRange;
            for (y = startY; y < endY; y++)
            {
                bYOutOfRange = y < startYY || y >= endYY;
                bPos = (stride * y) + (startX * 4);
                for (x = startX; x < endX; x++)
                {
                    bXOutOfRange = x < startXX || x >= endXX;
                    if (!bXOutOfRange && !bYOutOfRange)
                    {
                        x = endXX - 1;
                        bPos = (stride * y) + (endXX * 4);
                        continue;
                    }

                    alphaSum = 0f;
                    for (c = 0; c < countChannels; c++)
                    {
                        curChannel = channelsOffsets[c];
                        curChannelId = channelIds[c];
                        fValue = 0f;
                        for (my = 0; my < Size; my++)
                        {
                            if (y + my - XYCenter < startY ||
                                y + my - XYCenter >= endY) continue;
                            for (mx = 0; mx < Size; mx++)
                            {
                                if (x + mx - XYCenter < startX ||
                                    x + mx - XYCenter >= endX) continue;

                                temp = kernel.Matrix[my][mx];

                                if (curChannelId != Channel.Alpha && alphaWeighting == AlphaWeighting.UseAlphaWeighting)
                                {
                                    temp *= dataClone[bPos + alphaChannel[my][mx]];
                                    alphaSum += temp < 0 ? -temp : temp;
                                }

                                preAlpha = (float)dataClone[bPos + alphaChannel[my][mx]];
                                if (preAlpha > 0) preAlpha = preAlpha / 255f;

                                fValue += (dataClone[bPos + curChannel[my][mx]] / preAlpha) * temp;
                            }
                        }
                        fValue /= kernel.Divisor;

                        if (curChannelId != Channel.Alpha && alphaWeighting == AlphaWeighting.UseAlphaWeighting)
                        {
                            if (alphaSum != 0)
                                fValue = fValue * matrixSum / alphaSum;
                            else
                                fValue = 0;
                        }

                        fValue += kernel.Offset;
                        if (fValue < 0f) fValue = 0f;
                        else if (fValue > 255f) fValue = 255f;
                        preAlpha = (float)data[bPos + alphaChannel[XYCenter][XYCenter]];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        data[bPos + curChannel[XYCenter][XYCenter]] = (byte)(fValue * preAlpha);
                    }

                    bPos += 4;
                }
            }
            return FilterError.OK;
        }
    }
}
