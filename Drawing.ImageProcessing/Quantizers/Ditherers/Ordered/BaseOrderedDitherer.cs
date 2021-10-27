using System;
using System.Drawing;
using Codenet.Drawing.ImageProcessing.Quantizers.Helpers;

namespace Codenet.Drawing.ImageProcessing.Quantizers.Ditherers.Ordered
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public abstract class BaseOrderedDitherer : BaseColorDitherer
    {
        #region | Properties |

        /// <summary>
        /// Gets the width of the matrix.
        /// </summary>
        protected abstract Byte MatrixWidth { get; }

        /// <summary>
        /// Gets the height of the matrix.
        /// </summary>
        protected abstract Byte MatrixHeight { get; }

        #endregion

        #region << BaseColorDitherer >>

        /// <summary>
        /// See <see cref="BaseColorDitherer.OnProcessPixel"/> for more details.
        /// </summary>
        protected override Boolean OnProcessPixel(Pixel sourcePixel, Pixel targetPixel)
        {
            // reads the source pixel
            Color oldColor = SourceBuffer.GetColorFromPixel(sourcePixel);

            // converts alpha to solid color
            oldColor = QuantizationHelper.ConvertAlpha(oldColor);

            // retrieves matrix coordinates
            Int32 x = targetPixel.X % MatrixWidth;
            Int32 y = targetPixel.Y % MatrixHeight;

            // determines the threshold
            Int32 threshold = Convert.ToInt32(CachedMatrix[x, y]);

            // only process dithering if threshold is substantial
            if (threshold > 0)
            {
                Int32 red = GetClampedColorElement(oldColor.R + threshold);
                Int32 green = GetClampedColorElement(oldColor.G + threshold);
                Int32 blue = GetClampedColorElement(oldColor.B + threshold);

                Color newColor = Color.FromArgb(255, red, green, blue);

                if (TargetBuffer.IsIndexed)
                {
                    Byte newPixelIndex = (Byte) Quantizer.GetPaletteIndex(newColor, targetPixel.X, targetPixel.Y);
                    targetPixel.Index = newPixelIndex;
                }
                else
                {
                    targetPixel.Color = newColor;
                }
            }

            // writes the process pixel
            return true;
        }

        #endregion

        #region << IColorDitherer >>

        /// <summary>
        /// See <see cref="IColorDitherer.IsInplace"/> for more details.
        /// </summary>
        public override Boolean IsInplace
        {
            get { return true; }
        }

        #endregion
    }
}
