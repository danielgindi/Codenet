using System;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Quantizers.Helpers
{
    public static class PixelFormatUtility
    {
        /// <summary>
        /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
        /// 
        /// Gets the format by color count.
        /// </summary>
        public static PixelFormat GetFormatByColorCount(Int32 colorCount)
        {
            if (colorCount <= 0 || colorCount > 256)
            {
                String message = string.Format("A color count '{0}' not supported!", colorCount);
                throw new NotSupportedException(message);
            }

            PixelFormat result = PixelFormat.Format1bppIndexed;

            if (colorCount > 16)
            {
                result = PixelFormat.Format8bppIndexed;
            }
            else if (colorCount > 2)
            {
                result = PixelFormat.Format4bppIndexed;
            }

            return result;
        }
    }
}
