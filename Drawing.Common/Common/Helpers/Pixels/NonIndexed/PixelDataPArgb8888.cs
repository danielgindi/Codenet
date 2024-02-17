using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Codenet.Drawing.Common.Helpers.Pixels.NonIndexed;

/// <summary>
/// Name |          Alpha        |        Red            |           Green       |         Blue          |
/// Bit  |31|30|29|28|27|26|25|24|23|22|21|20|19|18|17|16|15|14|13|12|11|10|09|08|07|06|05|04|03|02|01|00|
/// Byte |33333333333333333333333|22222222222222222222222|11111111111111111111111|00000000000000000000000|
/// Hex  0xAARRGGBB
/// Premultiplied with Alpha component
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4)]
internal struct PixelDataPArgb8888 : INonIndexedPixel
{
    // raw component values
    [FieldOffset(0)] private Byte blue;    // 00 - 07
    [FieldOffset(1)] private Byte green;   // 08 - 15
    [FieldOffset(2)] private Byte red;     // 16 - 23
    [FieldOffset(3)] private Byte alpha;   // 24 - 31

    // raw high-level values
    [FieldOffset(0)] private UInt32 raw;             // 00 - 31

    // processed component values
    public Int32 Alpha { get { return alpha; } }
    public Int32 Red { get { return UnPremul(red); } }
    public Int32 Green { get { return UnPremul(green); } }
    public Int32 Blue { get { return UnPremul(blue); } }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Premul(byte channel)
    {
        var alpha = this.alpha;
        if (alpha == 0)
            return channel;

        var result = channel * alpha / 255;
        return (byte)result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte UnPremul(byte channel)
    {
        var alpha = this.alpha;
        if (alpha == 0)
            return channel;

        var result = channel * 255 / alpha;
        if (result < 0)
            return 0;
        if (result > 255)
            result = 255;
        return (byte)result;
    }

    /// <summary>
    /// See <see cref="INonIndexedPixel.Argb"/> for more details.
    /// </summary>
    public UInt32 Argb
    {
        get { return unchecked((UInt32)Alpha) << PixelAccess.AlphaShift | unchecked((UInt32)Red) << PixelAccess.RedShift | unchecked((UInt32)Green) << PixelAccess.GreenShift | unchecked((UInt32)Blue); }
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
        SetColor(color.ARGB);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetColor(UInt32 argb)
    {
        alpha = (byte)(argb >> PixelAccess.AlphaShift);
        red = Premul((byte)(argb >> PixelAccess.RedShift));
        green = Premul((byte)(argb >> PixelAccess.GreenShift));
        blue = Premul((byte)(argb >> PixelAccess.BlueShift));
    }

    /// <summary>
    /// See <see cref="INonIndexedPixel.Value"/> for more details.
    /// </summary>
    public UInt64 Value
    {
        get { return Argb; }
        set { SetColor((UInt32)value); }
    }
}
