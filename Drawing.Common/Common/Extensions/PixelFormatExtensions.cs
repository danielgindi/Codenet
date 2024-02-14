using System;

namespace Codenet.Drawing.Common.Extensions
{
    public static class PixelFormatExtensions
    {
        /// <summary>
        /// Determines whether the specified pixel format is indexed.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>
        /// 	<c>true</c> if the specified pixel format is indexed; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsIndexed(this PixelFormat pixelFormat)
        {
            return pixelFormat == PixelFormat.Format1bppIndexed ||
                pixelFormat == PixelFormat.Format4bppIndexed ||
                pixelFormat == PixelFormat.Format8bppIndexed;
        }
        /// <summary>
        /// Determines whether the specified pixel format has alpha.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>
        /// 	<c>true</c> if the specified pixel format has alpha; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean HasAlpha(this PixelFormat pixelFormat)
        {
            return pixelFormat == PixelFormat.Format16bppArgb1555 ||
                pixelFormat == PixelFormat.Format32bppArgb ||
                pixelFormat == PixelFormat.Format32bppPArgb ||
                pixelFormat == PixelFormat.Format64bppArgb ||
                pixelFormat == PixelFormat.Format64bppPArgb;
        }

        /// <summary>
        /// Gets the bit count for a given pixel format.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>The bit count.</returns>
        public static Byte GetBitDepth(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 1;

                case PixelFormat.Format4bppIndexed:
                    return 4;

                case PixelFormat.Format8bppIndexed:
                    return 8;

                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 16;

                case PixelFormat.Format24bppRgb:
                    return 24;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 32;

                case PixelFormat.Format48bppRgb:
                    return 48;

                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;

                default:
                    throw new NotSupportedException($"A pixel format '{pixelFormat}' not supported!");
            }
        }

        /// <summary>
        /// Gets the available color count for a given pixel format.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>The available color count.</returns>
        public static UInt16 GetColorCount(this PixelFormat pixelFormat)
        {
            if (!pixelFormat.IsIndexed())
            {
                throw new NotSupportedException($"Cannot retrieve color count for a non-indexed format '{pixelFormat}'.");
            }

            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 2;

                case PixelFormat.Format4bppIndexed:
                    return 16;

                case PixelFormat.Format8bppIndexed:
                    return 256;

                default:
                    throw new NotSupportedException($"A pixel format '{pixelFormat}' not supported!");
            }
        }


        /// <summary>
        /// Determines whether [is deep color] [the specified pixel format].
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>
        /// 	<c>true</c> if [is deep color] [the specified pixel format]; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsDeepColor(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return true;

                default:
                    return false;
            }
        }
    }
}
