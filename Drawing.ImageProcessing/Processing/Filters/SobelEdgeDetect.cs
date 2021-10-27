namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class SobelEdgeDetect : ConvolutionMatrix
    {
        public new FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            Matrix3x3 kernel = new Matrix3x3(
                -2, -1, 0,
                -1, 1, 1,
                0, 1, 2,
                1, -255, false);
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
