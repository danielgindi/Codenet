using System;

namespace Codenet.Drawing.Quantizers.Ditherers.ErrorDiffusion;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public class StuckiDitherer : BaseErrorDistributionDitherer
{
    /// <summary>
    /// See <see cref="BaseColorDitherer.CreateCoeficientMatrix"/> for more details.
    /// </summary>
    protected override Byte[,] CreateCoeficientMatrix()
    {
        return new Byte[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 8, 4 },
            { 2, 4, 8, 4, 2 },
            { 1, 2, 4, 2, 1 }
        };
    }

    /// <summary>
    /// See <see cref="BaseErrorDistributionDitherer.MatrixSideWidth"/> for more details.
    /// </summary>
    protected override Int32 MatrixSideWidth
    {
        get { return 2; }
    }

    /// <summary>
    /// See <see cref="BaseErrorDistributionDitherer.MatrixSideHeight"/> for more details.
    /// </summary>
    protected override Int32 MatrixSideHeight
    {
        get { return 1; }
    }
}
