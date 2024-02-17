using System;
using Codenet.Drawing.Common;

namespace Codenet.Drawing.Quantizers.Helpers;

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
            throw new NotSupportedException($"A color count '{colorCount}' not supported!");
        }

        if (colorCount > 16) return PixelFormat.Format8bppIndexed;
        else if (colorCount > 2) return PixelFormat.Format4bppIndexed;
        else return PixelFormat.Format1bppIndexed;
    }
}
