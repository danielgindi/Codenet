using System;
using System.Runtime.InteropServices;

namespace Codenet.Drawing.Common.Helpers.Pixels.NonIndexed;

/// <summary>
/// Name |                     Alpha                     |                      Red                      |                     Green                     |                     Blue                      |
/// Bit  |63|62|61|60|59|58|57|56|55|54|53|52|51|50|49|48|47|46|45|44|43|42|41|40|39|38|37|36|35|34|33|32|31|30|29|28|27|26|25|24|23|22|21|20|19|18|17|16|15|14|13|12|11|10|09|08|07|06|05|04|03|02|01|00|
/// Byte |77777777777777777777777|66666666666666666666666|55555555555555555555555|44444444444444444444444|33333333333333333333333|22222222222222222222222|11111111111111111111111|00000000000000000000000|
/// Hex  0xAAAARRRRGGGGBBBB
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct PixelDataArgb64 : INonIndexedPixel
{
    // raw component values
    [FieldOffset(0)] private UInt16 blue;   // 00 - 15
    [FieldOffset(2)] private UInt16 green;  // 16 - 31
    [FieldOffset(4)] private UInt16 red;    // 32 - 47
    [FieldOffset(6)] private UInt16 alpha;  // 48 - 63

    // raw high-level values
    [FieldOffset(0)] private UInt64 raw;    // 00 - 63

    // processed component values
    public Int32 Alpha { get { return alpha >> 8; } }
    public Int32 Red { get { return red >> 8; } }
    public Int32 Green { get { return green >> 8; } }
    public Int32 Blue { get { return blue >> 8; } }

    /// <summary>
    /// See <see cref="INonIndexedPixel.Argb"/> for more details.
    /// </summary>
    public UInt32 Argb
    {
        get { return (UInt32)Alpha << 48 | (UInt32)Red << 32 | (UInt32)Green << 16 | (UInt32)Blue; }
    }

    /// <summary>
    /// See <see cref="INonIndexedPixel.GetColor"/> for more details.
    /// </summary>
    public NeatColor GetColor()
    {
        return new NeatColor(Argb);
    }

    /// <summary>
    /// See <see cref="INonIndexedPixel.SetColor"/> for more details.
    /// </summary>
    public void SetColor(NeatColor color)
    {
        alpha = (UInt16) (color.Alpha << 8);
        red = (UInt16) (color.Red << 8);
        green = (UInt16) (color.Green << 8);
        blue = (UInt16) (color.Blue << 8);
    }

    /// <summary>
    /// See <see cref="INonIndexedPixel.Value"/> for more details.
    /// </summary>
    public UInt64 Value
    {
        get { return raw; }
        set { raw = value; }
    }
}
