using System;
using System.Runtime.InteropServices;

namespace Codenet.Drawing.Common.Helpers.Pixels.NonIndexed
{
    /// <summary>
    /// Name |A |     Red      |    Green     |     Blue     |
    /// Bit  |15|14|13|12|11|10|09|08|07|06|05|04|03|02|01|00|
    /// Byte |11111111111111111111111|00000000000000000000000|
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    internal struct PixelDataArgb1555 : INonIndexedPixel
    {
        // raw component values
        [FieldOffset(0)] private Byte blue;     // 00 - 04
        [FieldOffset(0)] private UInt16 green;  // 05 - 09
        [FieldOffset(1)] private Byte red;      // 10 - 14
        [FieldOffset(1)] private Byte alpha;    // 15

        // raw high-level values
        [FieldOffset(0)] private UInt16 raw;    // 00 - 15

        // processed raw values
        public Int32 Alpha { get { return (alpha >> 7) & 0x1; } }
        public Int32 Red { get { return (red >> 2) & 0xF; } }
        public Int32 Green { get { return (green >> 5) & 0xF; } }
        public Int32 Blue { get { return blue & 0xF; } }

        /// <summary>
        /// See <see cref="INonIndexedPixel.Argb"/> for more details.
        /// </summary>
        public UInt32 Argb
        {
            get { return (Alpha == 0 ? 0 : PixelAccess.AlphaMask) | (UInt32)Red << PixelAccess.RedShift | (UInt32)Green << PixelAccess.GreenShift | (UInt32)Blue; }
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
            UInt32 argb = color.ARGB;
            alpha = (argb >> PixelAccess.AlphaShift) > PixelAccess.ByteMask ? PixelAccess.Zero : PixelAccess.One;
            red = (byte)(argb >> PixelAccess.RedShift);
            green = (byte)(argb >> PixelAccess.GreenShift);
            blue = (byte)(argb >> PixelAccess.BlueShift);
        }

        /// <summary>
        /// See <see cref="INonIndexedPixel.Value"/> for more details.
        /// </summary>
        public UInt64 Value
        {
            get { return raw; }
            set { raw = (UInt16)(value & 0xFFFF); }
        }
    }
}
