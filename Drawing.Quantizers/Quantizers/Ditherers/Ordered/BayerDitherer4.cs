using System;

namespace Codenet.Drawing.Quantizers.Ditherers.Ordered;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public class BayerDitherer4 : BaseOrderedDitherer
{
    /// <summary>
    /// See <see cref="BaseColorDitherer.CreateCoeficientMatrix"/> for more details.
    /// </summary>
    protected override Byte[,] CreateCoeficientMatrix()
    {
        return new Byte[,] 
        {
    		{  1,  9,  3, 11 },
			    { 13,  5, 15,  7 },
			    {  4, 12,  2, 10 },
			    { 16,  8, 14,  6 }
        };
    }

    /// <summary>
    /// See <see cref="BaseOrderedDitherer.MatrixWidth"/> for more details.
    /// </summary>
    protected override Byte MatrixWidth
    {
        get { return 4; }
    }

    /// <summary>
    /// See <see cref="BaseOrderedDitherer.MatrixHeight"/> for more details.
    /// </summary>
    protected override Byte MatrixHeight
    {
        get { return 4; }
    }
}
