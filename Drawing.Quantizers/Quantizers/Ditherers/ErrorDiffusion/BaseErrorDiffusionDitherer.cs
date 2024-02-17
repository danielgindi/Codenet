using System;
using Codenet.Drawing.Common;
using Codenet.Drawing.Common.Helpers;

namespace Codenet.Drawing.Quantizers.Ditherers.ErrorDiffusion;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public abstract class BaseErrorDistributionDitherer : BaseColorDitherer
{
    #region Properties

    /// <summary>
    /// Gets the width of the matrix side.
    ///
    ///         center
    ///           v --------> side width = 2
    /// | 0 | 1 | 2 | 3 | 4 |
    /// </summary>
    protected abstract Int32 MatrixSideWidth { get; }

    /// <summary>
    /// Gets the height of the matrix side.
    /// 
    /// --- 
    ///  0  
    /// --- 
    ///  1  &lt;- center
    /// --- | 
    ///  2  | side height = 1
    /// --- v
    /// </summary>
    protected abstract Int32 MatrixSideHeight { get; }

    #endregion

    #region Methods

    private void ProcessNeighbor(PixelAccess targetPixel, Int32 x, Int32 y, Single factor, Int32 redError, Int32 greenError, Int32 blueError)
    {
        NeatColor oldColor = TargetBuffer.ReadColorUsingPixelFrom(targetPixel, x, y);
        oldColor = QuantizationHelper.ConvertAlphaToSolid(oldColor);
        byte red = GetClamped8bColorElementWithError(oldColor.Red, factor, redError);
        byte green = GetClamped8bColorElementWithError(oldColor.Green, factor, greenError);
        byte blue = GetClamped8bColorElementWithError(oldColor.Blue, factor, blueError);
        NeatColor newColor = NeatColor.FromARGB(255, red, green, blue);
        TargetBuffer.WriteColorUsingPixelAt(targetPixel, x, y, newColor, Quantizer);
    }

    #endregion

    #region BaseColorDitherer

    /// <summary>
    /// See <see cref="BaseColorDitherer.OnProcessPixel"/> for more details.
    /// </summary>
    protected override Boolean OnProcessPixel(PixelAccess sourcePixel, PixelAccess targetPixel)
    {
        // only process dithering when reducing from truecolor to indexed
        if (!TargetBuffer.IsIndexed) return false;

        // retrieves the colors
        NeatColor sourceColor = SourceBuffer.GetColorFromPixel(sourcePixel);
        NeatColor targetColor = TargetBuffer.GetColorFromPixel(targetPixel);

        // converts alpha to solid color
        sourceColor = QuantizationHelper.ConvertAlphaToSolid(sourceColor);

        // calculates the difference (error)
        Int32 redError = sourceColor.Red - targetColor.Red;
        Int32 greenError = sourceColor.Green - targetColor.Green;
        Int32 blueError = sourceColor.Blue - targetColor.Blue;

        // only propagate non-zero error
        if (redError != 0 || greenError != 0 || blueError != 0)
        {
            // processes the matrix
            for (Int32 shiftY = -MatrixSideHeight; shiftY <= MatrixSideHeight; shiftY++)
            for (Int32 shiftX = -MatrixSideWidth; shiftX <= MatrixSideWidth; shiftX++)
            {
                Int32 targetX = sourcePixel.X + shiftX;
                Int32 targetY = sourcePixel.Y + shiftY;
                Byte coeficient = CachedMatrix[shiftY + MatrixSideHeight, shiftX + MatrixSideWidth];
                Single coeficientSummed = CachedSummedMatrix[shiftY + MatrixSideHeight, shiftX + MatrixSideWidth];

                if (coeficient != 0 &&
                    targetX >= 0 && targetX < TargetBuffer.Width &&
                    targetY >= 0 && targetY < TargetBuffer.Height)
                {
                    ProcessNeighbor(targetPixel, targetX, targetY, coeficientSummed, redError, greenError, blueError);
                }
            }
        }

        // pixels are not processed, only neighbors are
        return false;
    }

    #endregion

    #region IColorDitherer

    /// <summary>
    /// See <see cref="IColorDitherer.IsInplace"/> for more details.
    /// </summary>
    public override Boolean IsInplace
    {
        get { return false; }
    }

    #endregion
}
