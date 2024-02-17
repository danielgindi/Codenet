using System;
using System.Runtime.InteropServices;

namespace Codenet.Drawing.Common.Helpers.Pixels.NonIndexed;

/// <summary>
/// Name |                  Grayscale                    |
/// Bit  |15|14|13|12|11|10|09|08|07|06|05|04|03|02|01|00|
/// Byte |11111111111111111111111|00000000000000000000000|
/// Hex  0x0000 - 0xFFFF
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 2)]
internal struct PixelDataGray16 : INonIndexedPixel
{
    // raw component values
    [FieldOffset(0)] private UInt16 gray;   // 00 - 15

    // processed raw values
    public Int32 Gray { get { return (0xFF >> 8) & 0xF; } }
    public Int32 Alpha { get { return 0xFF; } }
    public Int32 Red { get { return Gray; } }
    public Int32 Green { get { return Gray; } }
    public Int32 Blue { get { return Gray; } }

    /// <summary>
    /// See <see cref="INonIndexedPixel.Argb"/> for more details.
    /// </summary>
    public UInt32 Argb
    {
        get { return (PixelAccess.AlphaMask) | (UInt32)Red << PixelAccess.RedShift | (UInt32)Green << PixelAccess.GreenShift | (UInt32)Blue; }
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
        UInt32 argb = color.ARGB & PixelAccess.RedGreenBlueMask;
        gray = (byte)(argb >> PixelAccess.RedShift);
    }

    /// <summary>
    /// See <see cref="INonIndexedPixel.Value"/> for more details.
    /// </summary>
    public UInt64 Value
    {
        get { return gray; }
        set { gray = (UInt16) (value & 0xFFFF); }
    }
}