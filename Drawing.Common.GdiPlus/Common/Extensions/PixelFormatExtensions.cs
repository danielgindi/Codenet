using System;

namespace Codenet.Drawing.Common.Extensions
{
    public static class PixelFormatGdiPlusExtensions
    {
        public static PixelFormat ToQuantizerPixelFormat(this System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            return pixelFormat switch
            {
                System.Drawing.Imaging.PixelFormat.Format16bppArgb1555 => PixelFormat.Format16bppArgb1555,
                System.Drawing.Imaging.PixelFormat.Format16bppGrayScale => PixelFormat.Format16bppGrayScale,
                System.Drawing.Imaging.PixelFormat.Format16bppRgb555 => PixelFormat.Format16bppRgb555,
                System.Drawing.Imaging.PixelFormat.Format16bppRgb565 => PixelFormat.Format16bppRgb565,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb => PixelFormat.Format24bppRgb,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb => PixelFormat.Format32bppRgb,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb => PixelFormat.Format32bppArgb,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb => PixelFormat.Format32bppPArgb,
                System.Drawing.Imaging.PixelFormat.Format48bppRgb => PixelFormat.Format48bppRgb,
                System.Drawing.Imaging.PixelFormat.Format64bppArgb => PixelFormat.Format64bppArgb,
                System.Drawing.Imaging.PixelFormat.Format64bppPArgb => PixelFormat.Format64bppPArgb,
                System.Drawing.Imaging.PixelFormat.Format1bppIndexed => PixelFormat.Format1bppIndexed,
                System.Drawing.Imaging.PixelFormat.Format4bppIndexed => PixelFormat.Format4bppIndexed,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed => PixelFormat.Format8bppIndexed,
                _ => throw new NotSupportedException("Unsupported pixel format.")
            };
        }

        public static System.Drawing.Imaging.PixelFormat ToGdiPlusPixelFormat(this PixelFormat pixelFormat)
        {
            return pixelFormat switch
            {
                PixelFormat.Format16bppArgb1555 => System.Drawing.Imaging.PixelFormat.Format16bppArgb1555,
                PixelFormat.Format16bppGrayScale => System.Drawing.Imaging.PixelFormat.Format16bppGrayScale,
                PixelFormat.Format16bppRgb555 => System.Drawing.Imaging.PixelFormat.Format16bppRgb555,
                PixelFormat.Format16bppRgb565 => System.Drawing.Imaging.PixelFormat.Format16bppRgb565,
                PixelFormat.Format24bppRgb => System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                PixelFormat.Format32bppRgb => System.Drawing.Imaging.PixelFormat.Format32bppRgb,
                PixelFormat.Format32bppArgb => System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                PixelFormat.Format32bppPArgb => System.Drawing.Imaging.PixelFormat.Format32bppPArgb,
                PixelFormat.Format48bppRgb => System.Drawing.Imaging.PixelFormat.Format48bppRgb,
                PixelFormat.Format64bppArgb => System.Drawing.Imaging.PixelFormat.Format64bppArgb,
                PixelFormat.Format64bppPArgb => System.Drawing.Imaging.PixelFormat.Format64bppPArgb,
                PixelFormat.Format1bppIndexed => System.Drawing.Imaging.PixelFormat.Format1bppIndexed,
                PixelFormat.Format4bppIndexed => System.Drawing.Imaging.PixelFormat.Format4bppIndexed,
                PixelFormat.Format8bppIndexed => System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                _ => throw new NotSupportedException("Unsupported pixel format.")
            };
        }
    }
}
