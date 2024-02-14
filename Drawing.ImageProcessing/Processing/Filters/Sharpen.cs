namespace Codenet.Drawing.ImageProcessing.Processing.Filters;

public class Sharpen : ConvolutionMatrix
{
    public new FilterError ProcessImage(
        DirectAccessBitmap bmp,
        params object[] args)
    {
        Matrix3x3 kernel = new Matrix3x3(
            0, -1, 0,
            -1, 5, -1,
            0, -1, 0,
            1, 0, true);
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
