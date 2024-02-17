using System;
using System.Collections.Generic;
using Codenet.Drawing.Common;

namespace Codenet.Drawing.Quantizers.ColorCaches.LocalitySensitiveHash;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public class BucketInfo
{
    private readonly SortedDictionary<Int32, NeatColor> colors;

    /// <summary>
    /// Gets the colors.
    /// </summary>
    /// <value>The colors.</value>
    public IDictionary<Int32, NeatColor> Colors
    {
        get { return colors; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BucketInfo"/> class.
    /// </summary>
    public BucketInfo()
    {
        colors = new SortedDictionary<Int32, NeatColor>();
    }

    /// <summary>
    /// Adds the color to the bucket informations.
    /// </summary>
    /// <param name="paletteIndex">Index of the palette.</param>
    /// <param name="color">The color.</param>
    public void AddColor(Int32 paletteIndex, NeatColor color)
    {
        colors.Add(paletteIndex, color);
    }
}
