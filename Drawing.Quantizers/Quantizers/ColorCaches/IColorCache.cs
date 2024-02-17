using System;
using System.Collections.Generic;
using Codenet.Drawing.Common;

namespace Codenet.Drawing.Quantizers.ColorCaches;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public interface IColorCache
{
    /// <summary>
    /// Prepares color cache for next use.
    /// </summary>
    void Prepare();

    /// <summary>
    /// Called when a palette is about to be cached, or precached.
    /// </summary>
    /// <param name="palette">The palette.</param>
    void CachePalette(IList<NeatColor> palette);

    /// <summary>
    /// Called when palette index is about to be retrieve for a given color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="paletteIndex">Index of the palette.</param>
    void GetColorPaletteIndex(NeatColor color, out Int32 paletteIndex);
}
