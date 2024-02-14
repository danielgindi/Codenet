using System;
using System.Runtime.InteropServices;

namespace Codenet.Drawing.Common.Helpers.Pixels.NonIndexed;

/// <summary>
/// Name |                      Red                      |                     Green                     |                     Blue                      |
/// Bit  |47|46|45|44|43|42|41|40|39|38|37|36|35|34|33|32|31|30|29|28|27|26|25|24|23|22|21|20|19|18|17|16|15|14|13|12|11|10|09|08|07|06|05|04|03|02|01|00|
/// Byte |55555555555555555555555|44444444444444444444444|33333333333333333333333|22222222222222222222222|11111111111111111111111|00000000000000000000000|
/// Hex  0xRRRRGGGGBBBB
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 6)]
internal struct PixelDataRgb48 : INonIndexedPixel
{
    // raw component values
    [FieldOffset(0)] private UInt16 blue;   // 00 - 15
    [FieldOffset(2)] private UInt16 green;  // 16 - 31
    [FieldOffset(4)] private UInt16 red;    // 32 - 47

    // raw high-level values
    [FieldOffset(0)] private UInt64 raw;    // 00 - 47

    // processed component values
    public Int32 Alpha { get { return 0xFF; } }
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
        set { raw = value & 0xFFFFFFFFFFFF; }
    }
}
