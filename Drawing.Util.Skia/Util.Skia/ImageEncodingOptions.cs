using Codenet.Drawing.Quantizers.Quantizers;
using SkiaSharp;

#nullable enable

namespace Codenet.Drawing.Util.Skia;

public class ImageEncodingOptions
{
    public ImageEncodingOptions()
    {
        this.Quality = 1f;
    }

    /// <summary>
    /// Ranges from 0.0f to 1.0f
    /// </summary>
    public float Quality;

    /// <summary>
    /// Use libjpeg for encoding JPEGs, instead of built-in Skia encoder
    /// </summary>
    public bool UseLibJpeg = true;

    /// <summary>
    /// Convert to single frame if this is a multi-frame image
    /// </summary>
    public bool ConvertToSingleFrame = false;

    /// <summary>
    /// Use a temp file for saving the image, then replacing the destinatiom.
    /// This is useful for multi-frame images, where the destination file is the source file,
    /// or for any other case where atomicity is required.
    /// </summary>
    public bool UseTempFile = false;

    /// <summary>
    /// A supplier of the quantizer to use for GIFs
    /// </summary>
    public QuantizerRequestDelegate? QuantizerSupplier;

    public delegate BaseColorQuantizer? QuantizerRequestDelegate(SKBitmap frame);
}