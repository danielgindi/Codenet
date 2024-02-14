using System.Collections.Generic;

namespace Codenet.Drawing.Common
{
    /// <summary>
    /// This interface provides a color quantization capabilities.
    /// </summary>
    public interface IColorQuantizer : IPathProvider
    {
        /// <summary>
        /// Gets a value indicating whether to allow parallel processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to allow parallel processing; otherwise, <c>false</c>.
        /// </value>
        bool AllowParallel { get; }

        /// <summary>
        /// Prepares the quantizer for image processing.
        /// </summary>
        void Prepare(ImageBuffer image);

        /// <summary>
        /// Adds the color to quantizer.
        /// </summary>
        void AddColor(NeatColor color, int x, int y);

        /// <summary>
        /// Gets the palette with specified count of the colors.
        /// </summary>
        List<NeatColor> GetPalette(int colorCount);

        /// <summary>
        /// Gets the index of the palette for specific color.
        /// </summary>
        int GetPaletteIndex(NeatColor color, int x, int y);

        /// <summary>
        /// Gets the color count.
        /// </summary>
        int GetColorCount();

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Finish();
    }
}
