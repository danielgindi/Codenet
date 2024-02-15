using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Codenet.Drawing.Common;
using Codenet.Drawing.Quantizers.Quantizers;
using Codenet.Drawing.Encoders;
using Codenet.Drawing.Quantizers.XiaolinWu;
using Codenet.Drawing.Quantizers.Helpers;
using Codenet.IO;
using SkiaSharp;

#nullable enable

namespace Codenet.Drawing.Util.Skia;

public static class ImageFrameProcessingUtil
{
    public delegate SKBitmap? ProcessImageFrameDelegate(SKBitmap frame, SKEncodedImageFormat destinationFormat);

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
        SKEncodedImageFormat? destinationFormat,
        ImageEncodingOptions encodingOptions,
        ProcessImageFrameDelegate processor)
    {
        if (sourcePath == null) return false;
        if (destPath == null) return false;

        using var fileStream = File.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return ProcessMultiframeImage(
            sourceStream: fileStream,
            destPath: destPath,
            destinationFormat: destinationFormat,
            encodingOptions: encodingOptions,
            processor: processor);
    }

    /// <summary>
    /// Will trigger a function for processing each frame in image (will handle GIFs as well), and save the result to the destination file.
    /// </summary>
    /// <param name="sourceStream">Source stream</param>
    /// <param name="destPath">Having the <paramref name="destPath"/> the same as the source may not be safe, especially for multi-frame sources where the source is streamed.</param>
    /// <param name="destinationFormat">Destination file format. null for original</param>
    /// <param name="encodingOptions">Encoding options, including quality</param>
    /// <param name="processor">A processing function that will be executed for each frame.</param>
    /// <returns>true if successful, false if there was any failure.</returns>
    public static bool ProcessMultiframeImage(
        Stream sourceStream,
        string destPath,
        SKEncodedImageFormat? destinationFormat,
        ImageEncodingOptions encodingOptions,
        ProcessImageFrameDelegate processor)
    {
        if (sourceStream == null) return false;
        if (destPath == null) return false;

        if (encodingOptions.Quality <= 0.0f)
        {
            encodingOptions.Quality = 0.9f;
        }
        else if (encodingOptions.Quality > 1.0f)
        {
            encodingOptions.Quality = 1.0f;
        }

        using var codec = SKCodec.Create(sourceStream);

        if (codec == null)
            return false;

        var sourceFormat = codec.EncodedFormat;
        destinationFormat ??= sourceFormat;

        string? tempFilePath = encodingOptions.UseTempFile ? FileHelper.CreateEmptyTempFile() : null;

        try
        {
            if (sourceFormat == SKEncodedImageFormat.Gif &&
                destinationFormat == SKEncodedImageFormat.Gif &&
                !encodingOptions.ConvertToSingleFrame)
            {
                GifEncoder gifEncoder = new GifEncoder();
                try
                {
                    gifEncoder.Start(tempFilePath ?? destPath);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(@"ImageFrameProcessingUtil.ProcessMultiframeImage - Error: {0}", ex.ToString());
                    return false;
                }

                Action<SKBitmap, int> addFrame = (input, duration) =>
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

                    using var inputBuffer = SkiaImageBuffer.FromBitmap(input, false, false);
                    var targetPixelFormat = PixelFormatUtility.GetFormatByColorCount(255);
                    using var quantizedBuffer = ImageBuffer.Allocate(input.Width, input.Height, targetPixelFormat);
                    inputBuffer.Quantize(quantizedBuffer, quantizer, 255, 4);

                    gifEncoder.SetNextFrameDuration(duration);
                    gifEncoder.SetNextFrameTransparentColor(System.Drawing.Color.FromArgb(0x00FFFFFF));
                    gifEncoder.AddFrame(quantizedBuffer);
                };

                gifEncoder.SetSize(codec.Info.Width, codec.Info.Height);

                gifEncoder.SetRepeat(codec.RepetitionCount == -1 ? 0 : codec.RepetitionCount);

                int frameCount = codec.FrameCount;
                var frameInfos = codec.FrameInfo;

                var imageInfo = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

                var cache = new Dictionary<int, SKBitmap>();
                SKBitmap? reusableBmp = null;

                try
                {
                    var requiredSet = new HashSet<int>();
                    for (var i = 0; i < frameCount; i++)
                    {
                        if (frameInfos[i].RequiredFrame != -1)
                            requiredSet.Add(frameInfos[i].RequiredFrame);
                    }

                    for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
                    {
                        var frameInfo = frameInfos[frameIndex];
                        var decodeInfo = imageInfo;
                        if (frameIndex > 0)
                            decodeInfo = imageInfo.WithAlphaType(frameInfo.AlphaType);

                        if (reusableBmp != null && reusableBmp.ByteCount != decodeInfo.BytesSize)
                        {
                            reusableBmp.Dispose();
                            reusableBmp = null;
                        }

                        SKBitmap? bmp = reusableBmp;
                        bool success = false;

                        try
                        {
                            if (bmp == null)
                            {
                                bmp = new SKBitmap();
                                if (!bmp.TryAllocPixels(decodeInfo))
                                {
                                    bmp.Dispose();
                                    break;
                                }
                            }

                            var opts = new SKCodecOptions(frameIndex);

                            if (frameInfo.RequiredFrame > -1 && cache.TryGetValue(frameInfo.RequiredFrame, out var requiredBmp))
                            {
                                requiredBmp.CopyTo(bmp);
                                opts.PriorFrame = frameInfo.RequiredFrame;
                            }
                            else
                            {
                                bmp.Erase(SKColor.Empty);
                            }

                            var result = codec.GetPixels(decodeInfo, bmp.GetPixels(), opts);
                            if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
                            {
                                bmp.Dispose();
                                break;
                            }

                            if (requiredSet.Contains(frameIndex))
                                cache[frameIndex] = bmp;
                            else
                                reusableBmp = bmp;

                            if (processor != null)
                            {
                                var output = processor(bmp, destinationFormat.Value);
                                try
                                {

                                    if (output != null)
                                    {
                                        if (frameIndex == 0)
                                            gifEncoder.SetSize(output.Width, output.Height);

                                        addFrame(output, frameInfo.Duration);
                                    }
                                }
                                finally
                                {
                                    if (output != null && output != bmp)
                                        output.Dispose();
                                }
                            }
                            else
                            {
                                addFrame(bmp, frameInfo.Duration);
                            }

                            success = true;
                        }
                        finally
                        {
                            if (!success && bmp != null && bmp != reusableBmp)
                                bmp.Dispose();
                        }
                    }
                }
                finally
                {
                    foreach (var bmp in cache.Values)
                        bmp.Dispose();

                    reusableBmp?.Dispose();
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
                var bitmapInfo = codec.Info;

                if (bitmapInfo.AlphaType != SKAlphaType.Premul)
                    bitmapInfo.AlphaType = SKAlphaType.Premul;

                var sourceBitmap = new SKBitmap(bitmapInfo);
                var result = codec.GetPixels(bitmapInfo, sourceBitmap.GetPixels(out var length));
                if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
                {
                    sourceBitmap.Dispose();
                    sourceBitmap = null;
                    return false;
                }

                Action<SKBitmap> addFrame = (output) =>
                {
                    ImageSaveUtil.SaveImage(
                        imageData: output,
                        destPath: tempFilePath ?? destPath,
                        imageFormat: destinationFormat.Value,
                        encodingOptions: encodingOptions);
                };

                try
                {
                    if (processor != null)
                    {
                        var output = processor(sourceBitmap, destinationFormat.Value);
                        try
                        {
                            if (output != null)
                                addFrame(output);
                            else
                                return false;
                        }
                        finally
                        {
                            if (output != null && output != sourceBitmap)
                                output.Dispose();
                        }
                    }
                    else
                    {
                        addFrame(sourceBitmap);
                    }
                }
                finally
                {
                    sourceBitmap.Dispose(); 
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