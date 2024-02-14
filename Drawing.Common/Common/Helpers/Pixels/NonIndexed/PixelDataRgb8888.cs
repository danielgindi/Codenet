using System;
using System.Runtime.InteropServices;

namespace Codenet.Drawing.Common.Helpers.Pixels.NonIndexed;

/// <summary>
/// Name |         Unused        |        Red            |           Green       |         Blue          |
/// Bit  |31|30|29|28|27|26|25|24|23|22|21|20|19|18|17|16|15|14|13|12|11|10|09|08|07|06|05|04|03|02|01|00|
/// Byte |33333333333333333333333|22222222222222222222222|11111111111111111111111|00000000000000000000000|
/// Hex  0x00RRGGBB
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4)]
internal struct PixelDataRgb8888 : INonIndexedPixel
{
    // raw component values
    [FieldOffset(0)] private readonly Byte blue;    // 00 - 07
    [FieldOffset(1)] private readonly Byte green;   // 08 - 15
    [FieldOffset(2)] private readonly Byte red;     // 16 - 23
    [FieldOffset(3)] private readonly Byte unused;  // 24 - 31

    // raw high-level values
    [FieldOffset(0)] private UInt32 raw;             // 00 - 23

    // processed component values
    public Int32 Alpha { get { return 0xFF; } }
    public Int32 Red { get { return red; } }
    public Int32 Green { get { return green; } }
    public Int32 Blue { get { return blue; } }

    /// <summary>
    /// See <see cref="INonIndexedPixel.Argb"/> for more details.
    /// </summary>
    public UInt32 Argb
    {
        get { return PixelAccess.AlphaMask | raw; }
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
        raw = color.ARGB & PixelAccess.RedGreenBlueMask;
    }

    /// <summary>
    /// See <see cref="INonIndexedPixel.Value"/> for more details.
    /// </summary>
    public UInt64 Value
    {
        get { return (UInt32) raw; }
        set { raw = (UInt32)(value & 0xFFFFFF); }
    }
}
