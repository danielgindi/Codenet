using System;

namespace Codenet.Drawing.Quantizers.Uniform;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
internal struct UniformColorSlot
{
    private Int32 value;
    private Int32 pixelCount;

    /// <summary>
    /// Adds the value to the slot.
    /// </summary>
    /// <param name="component">The color component value.</param>
    public void AddValue(Int32 component)
    {
        value += component;
        pixelCount++;
    }

    /// <summary>
    /// Gets the average, just simple value divided by pixel presence.
    /// </summary>
    /// <returns>The average color component value.</returns>
    public Int32 GetAverage()
    {
        Int32 result = 0;

        if (pixelCount > 0)
        {
            result = pixelCount == 1 ? value : value / pixelCount;
        }

        return result;
    }
}
