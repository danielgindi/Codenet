namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class LaplaceEdgeDetect : ConvolutionMatrix
    {
        public new FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            Matrix5x5 kernel = new Matrix5x5(
                -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1,
                -1, -1, 24, -1, -1,
                -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1,
                1, 0, false);
            FilterColorChannel channels = FilterColorChannel.None;

            foreach (object arg in args)
            {
                if (arg is FilterColorChannel)
                {
                    channels |= (FilterColorChannel)arg;
                }
            }

            return base.ProcessImage(bmp, kernel, channels);
        }
    }
}
