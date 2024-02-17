using System;
using System.Drawing;
using System.Drawing.Imaging;
using Codenet.Drawing.Quantizers.Quantizers;
using Codenet.Drawing.Util.GdiPlus;

namespace Codenet.Drawing.ImageProcessing;

public static partial class ProcessingHelper
{
    public delegate Image ProcessImageFrameDelegate(Image frame);
    public delegate BaseColorQuantizer QuantizerRequestDelegate(Image frame);
    
    public class EncodingOptions
    {
        public EncodingOptions()
        {
            this.JpegQuality = 1f;
        }

        public float JpegQuality;
        public QuantizerRequestDelegate QuantizerSupplier;
    }

    /// <summary>
    /// Will trigger a function for processing each frame in image (will handle GIFs as well), and save the result to the destination file.
    /// </summary>
    /// <param name="sourceImage">Source image</param>
    /// <param name="destinationPath">Destination path</param>
    /// <param name="destinationFormat">Destination file format. null for original</param>
    /// <param name="encodingOptions">Encoding options, including quality</param>
    /// <param name="processor">A processing function that will be executed for each frame.</param>
    /// <returns>true if successful, false if there was any failure.</returns>
    public static bool ProcessImageFramesToFile(
        Image sourceImage,
        String destinationPath,
        ImageFormat destinationFormat,
        EncodingOptions encodingOptions,
        ProcessImageFrameDelegate processor)
    {
        return ImageFrameProcessingUtil.ProcessMultiframeImage(
            sourceImage: sourceImage,
            destPath: destinationPath,
            destinationFormat: destinationFormat,
            encodingOptions: new ImageEncodingOptions
            {
                Quality = encodingOptions.JpegQuality,
                QuantizerSupplier = encodingOptions.QuantizerSupplier == null ? null : x => encodingOptions.QuantizerSupplier(x),
                UseLibJpeg = true,
                UseTempFile = true
            },
            processor: (frame, _) =>
            {
                return processor(frame);
            });
    }
}
