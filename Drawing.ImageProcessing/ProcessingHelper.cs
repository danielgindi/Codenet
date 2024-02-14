using System;
using System.Drawing;
using System.Drawing.Imaging;
using Codenet.Drawing.Encoders;
using Codenet.Drawing.Common;
using Codenet.Drawing.Quantizers.XiaolinWu;
using Codenet.Drawing.Quantizers.Quantizers;
using Codenet.Drawing.Quantizers.Helpers;

namespace Codenet.Drawing.ImageProcessing
{
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
        /// <param name="sourceImageData">Source image</param>
        /// <param name="destinationPath">Destination path</param>
        /// <param name="destinationFormat">Destination file format. null for original</param>
        /// <param name="encodingOptions">Encoding options, including quality</param>
        /// <param name="processor">A processing function that will be executed for each frame.</param>
        /// <returns>true if successful, false if there was any failure.</returns>
        public static bool ProcessImageFramesToFile(
            Image sourceImageData,
            String destinationPath,
            ImageFormat destinationFormat,
            EncodingOptions encodingOptions,
            ProcessImageFrameDelegate processor)
        {
            if (destinationFormat == null)
            {
                destinationFormat = sourceImageData.RawFormat;
            }

            if (encodingOptions.JpegQuality <= 0.0f)
            {
                encodingOptions.JpegQuality = 0.9f;
            }
            else if (encodingOptions.JpegQuality > 1.0f)
            {
                encodingOptions.JpegQuality = 1.0f;
            }

            if (sourceImageData.RawFormat.Equals(ImageFormat.Gif) && destinationFormat.Equals(ImageFormat.Gif))
            {
                int frameCount = sourceImageData.GetFrameCount(FrameDimension.Time);
                byte[] durationBytes = sourceImageData.GetPropertyItem((int)ExifPropertyTag.PropertyTagFrameDelay).Value;
                Int16 loopCount = BitConverter.ToInt16(sourceImageData.GetPropertyItem((int)ExifPropertyTag.PropertyTagLoopCount).Value, 0);

                GifEncoder gifEncoder = new GifEncoder();
                try
                {
                    gifEncoder.Start(destinationPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"ImagingUtility.ProcessImageFramesToFile - Error: {0}", ex.ToString());
                    return false;
                }
                gifEncoder.SetSize(sourceImageData.Width, sourceImageData.Height);
                gifEncoder.SetRepeat(loopCount);

                byte[] pallette = sourceImageData.GetPropertyItem((int)ExifPropertyTag.PropertyTagGlobalPalette).Value;
                int transparentColorIndex = -1;
                Color transparentColor = Color.Empty;
                try
                {
                    transparentColorIndex = sourceImageData.GetPropertyItem((int)ExifPropertyTag.PropertyTagIndexTransparent).Value[0];
                    transparentColor = Color.FromArgb(pallette[transparentColorIndex * 3], pallette[transparentColorIndex * 3 + 1], pallette[transparentColorIndex * 3 + 2]);
                }
                catch { }

                Action<Image, int> addFrame = (input, duration) =>
                {
                    BaseColorQuantizer quantizer = null;
                    if (encodingOptions.QuantizerSupplier != null)
                    {
                        quantizer = encodingOptions.QuantizerSupplier(input);
                    }
                    if (quantizer == null)
                    {
                        quantizer = new WuColorQuantizer();
                    }

                    using var outputBuffer = GdiPlusImageBuffer.FromImage(input, ImageLockMode.ReadOnly);
                    var targetPixelFormat = PixelFormatUtility.GetFormatByColorCount(255);
                    using var quantizedBuffer = ImageBuffer.Allocate(input.Width, input.Height, targetPixelFormat);
                    outputBuffer.Quantize(quantizedBuffer, quantizer, 255, 4);

                    gifEncoder.SetNextFrameDuration(duration * 10);
                    gifEncoder.SetNextFrameTransparentColor(transparentColor);
                    gifEncoder.AddFrame(quantizedBuffer);
                };

                for (int frame = 0, duration; frame < frameCount; frame++)
                {
                    try
                    {
                        duration = BitConverter.ToInt32(durationBytes, 4 * frame); // In hundredth of a second
                        sourceImageData.SelectActiveFrame(FrameDimension.Time, frame);

                        if (processor != null)
                        {
                            using Image output = processor(sourceImageData);

                            if (output != null)
                            {
                                if (frame == 0)
                                    gifEncoder.SetSize(output.Width, output.Height);

                                addFrame(output, duration);
                            }
                        }
                        else
                        {
                            addFrame(sourceImageData, duration);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(@"ImagingUtility.ProcessImageFramesToFile - Error: {0}", ex.ToString());
                    }
                }

                try
                {
                    gifEncoder.Finish();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"ImagingUtility.ProcessImageFramesToFile - Error: {0}", ex.ToString());
                    return false;
                }
            }
            else
            {
                Action<Image> addFrame = (output) =>
                {
                    if (destinationFormat.Equals(ImageFormat.Jpeg))
                    {
                        using var imageBuffer = GdiPlusImageBuffer.FromImage(output, ImageLockMode.ReadOnly);
                        JpegEncoder.EncodeImageWithLibjpeg(imageBuffer, destinationPath, encodingOptions.JpegQuality);
                    }
                    else if (destinationFormat.Equals(ImageFormat.Gif))
                    {
                        BaseColorQuantizer quantizer = null;
                        if (encodingOptions.QuantizerSupplier != null)
                            quantizer = encodingOptions.QuantizerSupplier(output);
                        if (quantizer == null)
                            quantizer = new WuColorQuantizer();

                        using var quantized = GdiPlusImageBuffer.QuantizeImage(output, quantizer, 255, 4);
                        quantized.Save(destinationPath, ImageFormat.Gif);
                    }
                    else
                    {
                        output.Save(destinationPath, destinationFormat);
                    }
                };

                if (processor != null)
                {
                    using Image output = processor(sourceImageData);
                    if (output != null)
                        addFrame(output);
                    else
                        return false;
                }
                else
                {
                    addFrame(sourceImageData);
                }
            }
            return true;
        }
    }

}
