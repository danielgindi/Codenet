namespace Codenet.Drawing.ImageProcessing.Quantizers.Helpers
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public class PixelTransform
    {
        /// <summary>
        /// Gets the source pixel.
        /// </summary>
        public Pixel SourcePixel { get; private set; }

        /// <summary>
        /// Gets the target pixel.
        /// </summary>
        public Pixel TargetPixel { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelTransform"/> class.
        /// </summary>
        public PixelTransform(Pixel sourcePixel, Pixel targetPixel)
        {
            SourcePixel = sourcePixel;
            TargetPixel = targetPixel;
        }
    }
}
