using System;
using System.Runtime.InteropServices;

namespace Codenet.Drawing.Common.Helpers.Pixels.NonIndexed
{
    /// <summary>
    /// Name |          Red          |        Green          |           Blue        | 
    /// Bit  |23|22|21|20|19|18|17|16|15|14|13|12|11|10|09|08|07|06|05|04|03|02|01|00|
    /// Byte |22222222222222222222222|11111111111111111111111|00000000000000000000000|
    /// Hex  0xRRGGBB
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    internal struct PixelDataRgb888 : INonIndexedPixel
    {
        // raw component values
        [FieldOffset(0)] private Byte blue;    // 00 - 07
        [FieldOffset(1)] private Byte green;   // 08 - 15
        [FieldOffset(2)] private Byte red;     // 16 - 23

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
            get { return PixelAccess.AlphaMask | (UInt32)Red << PixelAccess.RedShift | (UInt32)Green << PixelAccess.GreenShift | (UInt32)Blue; }
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
            red = color.Red;
            green = color.Green;
            blue = color.Blue;
        }

        /// <summary>
        /// See <see cref="INonIndexedPixel.Value"/> for more details.
        /// </summary>
        public UInt64 Value
        {
            get { return (UInt32) Argb; }
            set
            {
                red = (Byte) ((value >> PixelAccess.RedShift) & 0xFF);
                green = (Byte) ((value >> PixelAccess.GreenShift) & 0xFF);
                blue = (Byte) ((value >> PixelAccess.BlueShift) & 0xFF);
            }
        }
    }
}
