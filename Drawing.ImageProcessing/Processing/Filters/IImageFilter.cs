namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public interface IImageFilter
    {
        FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args);
    }
}
