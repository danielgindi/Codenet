using System;
using Codenet.Drawing.Common;

namespace Codenet.Drawing.Quantizers.DistinctSelection;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// 
/// Stores all the informations about single color only once, to be used later.
/// </summary>
public class DistinctColorInfo
{
    private const Int32 Factor = 5000000;

    /// <summary>
    /// The original color.
    /// </summary>
    public UInt32 Color { get; private set; }

    /// <summary>
    /// The pixel presence count in the image.
    /// </summary>
    public Int32 Count { get; private set; }

    /// <summary>
    /// A hue component of the color.
    /// </summary>
    public Int32 Hue { get; private set; }

    /// <summary>
    /// A saturation component of the color.
    /// </summary>
    public Int32 Saturation { get; private set; }

    /// <summary>
    /// A brightness component of the color.
    /// </summary>
    public Int32 Brightness { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctColorInfo"/> struct.
    /// </summary>
    public DistinctColorInfo(NeatColor color)
    {
        Color = color.ARGB;
        Count = 1;

        color.ToHsv(out var h, out var s, out var v);

        Hue = Convert.ToInt32(h * Factor);
        Saturation = Convert.ToInt32(s * Factor);
        Brightness = Convert.ToInt32(v * Factor);
    }

    /// <summary>
    /// Increases the count of pixels of this color.
    /// </summary>
    public DistinctColorInfo IncreaseCount()
    {
        Count++;
        return this;
    }
}
