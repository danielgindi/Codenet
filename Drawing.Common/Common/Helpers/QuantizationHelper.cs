using System;

namespace Codenet.Drawing.Common.Helpers
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public class QuantizationHelper
    {
        private const UInt32 Alpha = 255u << 24;
        private static readonly NeatColor BackgroundColor;
        private static readonly Double[] Factors;

        static QuantizationHelper()
        {
            BackgroundColor = new NeatColor(0xffffffff);
            Factors = PrecalculateFactors();
        }

        /// <summary>
        /// Precalculates the alpha-fix values for all the possible alpha values (0-255).
        /// </summary>
        private static Double[] PrecalculateFactors()
        {
            Double[] result = new Double[256];

            for (Int32 value = 0; value < 256; value++)
            {
                result[value] = value / 255.0;
            }

            return result;
        }

        /// <summary>
        /// Converts the alpha blended color to a non-alpha blended color.
        /// </summary>
        /// <param name="color">The alpha blended color (ARGB).</param>
        /// <returns>The non-alpha blended color (RGB).</returns>
        public static NeatColor ConvertAlphaToSolid(NeatColor color)
        {
            return ConvertAlphaToSolid(color, out var _);
        }

        /// <summary>
        /// Converts the alpha blended color to a non-alpha blended color.
        /// </summary>
        public static NeatColor ConvertAlphaToSolid(NeatColor color, out Int32 rgb)
        {
            NeatColor result = color;

            if (color.Alpha < 255)
            {
                // performs a alpha blending (second color is BackgroundColor, by default a white color)
                double colorFactor = Factors[color.Alpha];
                double backgroundFactor = Factors[255 - color.Alpha];
                var red = (Int32)(color.Red * colorFactor + BackgroundColor.Red * backgroundFactor);
                var green = (Int32)(color.Green * colorFactor + BackgroundColor.Green * backgroundFactor);
                var blue = (Int32)(color.Blue * colorFactor + BackgroundColor.Blue * backgroundFactor);
                rgb = red << 16 | green << 8 | blue;
                result = new NeatColor(Alpha | (UInt32)rgb);
            }
            else
            {
                rgb = (Int32)color.Red << 16 | (Int32)color.Green << 8 | (Int32)color.Blue;
            }

            return result;
        }
    }
}