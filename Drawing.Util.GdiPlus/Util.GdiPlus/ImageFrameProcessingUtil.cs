using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using Codenet.Drawing.Common;
using Codenet.Drawing.Quantizers.Quantizers;
using Codenet.Drawing.Encoders;
using Codenet.Drawing.Quantizers.XiaolinWu;
using Codenet.Drawing.Quantizers.Helpers;
using Codenet.IO;

#nullable enable

namespace Codenet.Drawing.Util.GdiPlus;

public static class ImageFrameProcessingUtil
{
    public delegate Image? ProcessImageFrameDelegate(Image frame, ImageFormat destinationFormat);

    /// <summary>
    /// Will trigger a function for processing each frame in image (will handle GIFs as well), and save the result to the destination file.
    /// </summary>
    /// <param name="sourcePath">Source image</param>
    /// <param name="destPath">Having the <paramref name="destPath"/> the same as <paramref name="sourcePath"/> may not be safe, especially for multi-frame sources where the source is streamed.</param>
    /// <param name="destinationFormat">Destination file format. null for original</param>
    /// <param name="encodingOptions">Encoding options, including quality</param>
    /// <param name="processor">A processing function that will be executed for each frame.</param>
    /// <returns>true if successful, false if there was any failure.</returns>
    public static bool ProcessMultiframeImage(
        string sourcePath,
        string destPath,
        ImageFormat? destinationFormat,
        ImageEncodingOptions encodingOptions,
        ProcessImageFrameDelegate processor)
    {
        if (sourcePath == null) return false;
        if (destPath == null) return false;

        using var image = Image.FromFile(sourcePath);

        return ProcessMultiframeImage(
            sourceImage: image,
            destPath: destPath,
            destinationFormat: destinationFormat,
            encodingOptions: encodingOptions,
            processor: processor);
    }

    /// <summary>
    /// Will trigger a function for processing each frame in image (will handle GIFs as well), and save the result to the destination file.
    /// </summary>
    /// <param name="sourceImage">Source image</param>
    /// <param name="destPath">Having the <paramref name="destPath"/> the same as the source may not be safe, especially for multi-frame sources where the source is streamed.</param>
    /// <param name="destinationFormat">Destination file format. null for original</param>
    /// <param name="encodingOptions">Encoding options, including quality</param>
    /// <param name="processor">A processing function that will be executed for each frame.</param>
    /// <returns>true if successful, false if there was any failure.</returns>
    public static bool ProcessMultiframeImage(
        Image sourceImage,
        string destPath,
        ImageFormat? destinationFormat,
        ImageEncodingOptions encodingOptions,
        ProcessImageFrameDelegate processor)
    {
        if (sourceImage == null) return false;
        if (destPath == null) return false;

        if (encodingOptions.Quality <= 0.0f)
        {
            encodingOptions.Quality = 0.9f;
        }
        else if (encodingOptions.Quality > 1.0f)
        {
            encodingOptions.Quality = 1.0f;
        }

        string? tempFilePath = encodingOptions.UseTempFile ? FileHelper.CreateEmptyTempFile() : null;

        var sourceFormat = sourceImage.RawFormat;
        destinationFormat ??= sourceFormat;

        try
        {
            if (sourceFormat.Equals(ImageFormat.Gif) &&
                destinationFormat.Equals(ImageFormat.Gif) &&
                 !encodingOptions.ConvertToSingleFrame)
            {
                int frameCount = sourceImage.GetFrameCount(FrameDimension.Time);
                byte[] durationBytes = sourceImage.GetPropertyItem((int)ExifPropertyTag.PropertyTagFrameDelay).Value;
                Int16 loopCount = BitConverter.ToInt16(sourceImage.GetPropertyItem((int)ExifPropertyTag.PropertyTagLoopCount).Value, 0);

                GifEncoder gifEncoder = new GifEncoder();
                try
                {
                    gifEncoder.Start(destPath);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(@"ImageFrameProcessingUtil.ProcessMultiframeImage - Error: {0}", ex.ToString());
                    return false;
                }
                gifEncoder.SetSize(sourceImage.Width, sourceImage.Height);
                gifEncoder.SetRepeat(loopCount);

                byte[] pallette = sourceImage.GetPropertyItem((int)ExifPropertyTag.PropertyTagGlobalPalette).Value;
                Color transparentColor = Color.Transparent;
                try
                {
                    var transparentColorIndex = sourceImage.GetPropertyItem((int)ExifPropertyTag.PropertyTagIndexTransparent).Value[0];
                    transparentColor = Color.FromArgb(pallette[transparentColorIndex * 3], pallette[transparentColorIndex * 3 + 1], pallette[transparentColorIndex * 3 + 2]);
                }
                catch { }

                Action<Image, int> addFrame = (input, duration) =>
                {
                    BaseColorQuantizer? quantizer = null;
                    if (encodingOptions.QuantizerSupplier != null)
                    {
                        quantizer = encodingOptions.QuantizerSupplier(input);
                    }
                    if (quantizer == null)
                    {
                        quantizer = new WuColorQuantizer();
                    }

                    using var inputBuffer = GdiPlusImageBuffer.FromImage(input, ImageLockMode.ReadOnly);
                    var targetPixelFormat = PixelFormatUtility.GetFormatByColorCount(255);
                    using var quantizedBuffer = ImageBuffer.Allocate(input.Width, input.Height, targetPixelFormat);
                    inputBuffer.Quantize(quantizedBuffer, quantizer, 255, 4);

                    var transparentIndex = quantizer.GetPaletteIndex(NeatColor.FromARGB(unchecked((UInt32)transparentColor.ToArgb())), 0, 0);
                    if (transparentIndex != -1)
                    {
                        transparentColor = Color.FromArgb(unchecked((int)quantizedBuffer.Palette[transparentIndex].ARGB));
                    }
                    
                    gifEncoder.SetNextFrameDuration(duration * 10);
                    gifEncoder.SetNextFrameTransparentColor(transparentColor);
                    gifEncoder.AddFrame(quantizedBuffer);
                };

                for (int frame = 0, duration; frame < frameCount; frame++)
                {
                    try
                    {
                        duration = BitConverter.ToInt32(durationBytes, 4 * frame); // In hundredth of a second
                        sourceImage.SelectActiveFrame(FrameDimension.Time, frame);

                        if (processor != null)
                        {
                            var output = processor(sourceImage, destinationFormat);

                            try
                            {
                                if (output != null)
                                {
                                    if (frame == 0)
                                        gifEncoder.SetSize(output.Width, output.Height);

                                    addFrame(output, duration);
                                }
                            }
                            finally
                            {
                                if (output != null && output != sourceImage)
                                    output.Dispose();
                            }
                        }
                        else
                        {
                            addFrame(sourceImage, duration);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(@"ImageFrameProcessingUtil.ProcessMultiframeImage - Error: {0}", ex.ToString());
                    }
                }

                try
                {
                    gifEncoder.Finish();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(@"ImageFrameProcessingUtil.ProcessMultiframeImage - Error: {0}", ex.ToString());
                    return false;
                }
            }
            else
            {
                Action<Image> addFrame = (output) =>
                {
                    ImageSaveUtil.SaveImage(
                        image: output,
                        destPath: tempFilePath ?? destPath,
                        imageFormat: destinationFormat,
                        encodingOptions: encodingOptions);
                };

                if (processor != null)
                {
                    var output = processor(sourceImage, destinationFormat);
                    try
                    {
                        if (output != null)
                            addFrame(output);
                        else
                            return false;
                    }
                    finally
                    {
                        if (output != null && output != sourceImage)
                            output.Dispose();
                    }
                }
                else
                {
                    addFrame(sourceImage);
                }
            }
        }
        catch
        {
            if (tempFilePath != null)
            {
                File.Delete(tempFilePath);
            }

            throw;
        }

        if (tempFilePath != null)
        {
            try
            {
                if (File.Exists(destPath))
                    File.Delete(destPath);
                File.Move(tempFilePath, destPath);
                FileHelper.ResetFilePermissionsToInherited(destPath);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        return true;
    }
}