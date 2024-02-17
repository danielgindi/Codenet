namespace Codenet.Drawing.ImageProcessing.Processing.Filters;

public class GuassianBlur : ConvolutionMatrix
{
    public new FilterError ProcessImage(
        DirectAccessBitmap bmp,
        params object[] args)
    {
        Matrix3x3 kernel = new Matrix3x3(
            1, 2, 1,
            2, 4, 2,
            1, 2, 1,
            16, 0, false);
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
