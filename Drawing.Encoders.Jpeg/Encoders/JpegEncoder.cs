using System;
using System.IO;
using BitMiracle.LibJpeg.Classic;
using Codenet.Drawing.Common;
using Codenet.IO;

namespace Codenet.Drawing.Encoders;

public static class JpegEncoder
{
    public delegate void ProgressDelegate(int completedPasses, int totalPasses, int passCounter, int passLimit);

    // Handles warning silently
    private class LibJpegErrorHandler : jpeg_error_mgr
    {
        public bool IsFailed = false;

        public override void emit_message(int msg_level)
        {
        }

        public override void output_message()
        {
        }

        public override void error_exit()
        {
            IsFailed = true;
        }
    }

    /// <summary>
    /// Encodes the image using libjpeg.
    /// In any case of failure, will fallback to default encoder.
    /// </summary>
    /// <param name="imageBuffer">Source image</param>
    /// <param name="filePath">Target file path, will be overwritten</param>
    /// <param name="quality">Quality of output JPEG. 0.0 - 1.0.</param>
    /// <param name="smooth">Smooth input image? 0.0 - 1.0. Default 0.0;</param>
    /// <param name="progressive">Should the image be progressive? (Tends to result in smaller sizes for bigger resolutions)</param>
    /// <param name="optimize">Optimize Huffman table?</param>
    public static void EncodeImageWithLibjpeg(ImageBuffer imageBuffer, string filePath, float quality, float smooth = 0f, bool progressive = true, bool optimize = true)
    {
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            var nonAlphaBuffer = imageBuffer;

            if (imageBuffer.PixelFormat != PixelFormat.Format24bppRgb)
            {
                nonAlphaBuffer = ImageBuffer.Allocate(imageBuffer.Width, imageBuffer.Height, PixelFormat.Format24bppRgb);
                imageBuffer.ChangeFormat(nonAlphaBuffer, null);
            }

            try
            {
                jpeg_error_mgr errorManager = new LibJpegErrorHandler();
                jpeg_compress_struct jpegOut = new jpeg_compress_struct(errorManager);

                jpegOut.Image_width = imageBuffer.Width;
                jpegOut.Image_height = imageBuffer.Height;
                jpegOut.Input_components = 3;
                jpegOut.In_color_space = J_COLOR_SPACE.JCS_RGB;

                jpegOut.jpeg_set_defaults();

                jpegOut.jpeg_set_quality(Math.Min(100, Math.Max(0, (int)Math.Round(quality * 100f))), true);

                jpegOut.Smoothing_factor = Math.Min(100, Math.Max(0, (int)Math.Round(smooth * 100f)));

                if (progressive)
                {
                    jpegOut.jpeg_simple_progression();
                }

                if (optimize)
                {
                    jpegOut.Optimize_coding = true;
                }

                // Set destination
                jpegOut.jpeg_stdio_dest(fs);

                // Start compressor
                jpegOut.jpeg_start_compress(true);

                // Process data

                int lineWidth = nonAlphaBuffer.Width * 3,
                    stride = nonAlphaBuffer.Stride;

                byte swap;
                var scanLine = new byte[stride];
                var scanLines = new byte[][] { scanLine };

                for (int y = 0, height = nonAlphaBuffer.Height, x; y < height; y++)
                {
                    nonAlphaBuffer.CopyTo(y * stride, lineWidth, scanLine, 0);

                    for (x = 0; x < lineWidth; x += 3)
                    {
                        swap = scanLine[x];
                        scanLine[x] = scanLine[x + 2];
                        scanLine[x + 2] = swap;
                    }

                    jpegOut.jpeg_write_scanlines(scanLines, 1);
                }

                // Finish compression and release memory
                jpegOut.jpeg_finish_compress();
            }
            finally
            {
                if (nonAlphaBuffer != imageBuffer)
                {
                    nonAlphaBuffer.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Optimize a jpeg without losing quality
    /// </summary>
    /// <param name="inPath">Input file path</param>
    /// <param name="outPath">Output file path</param>
    /// <param name="progressive">Should the image be progressive? (Tends to result in smaller sizes for bigger resolutions)</param>
    /// <param name="optimize">Optimize Huffman table?</param>
    /// <param name="progress">Progress tracker</param>
    /// <returns>true if successful</returns>
    public static bool OptimizeJpeg(string inPath, string outPath, bool progressive = true, bool optimize = true, ProgressDelegate progress = null)
    {
        var errorManagerIn = new LibJpegErrorHandler();
        var errorManagerOut = new LibJpegErrorHandler();
        var jpegIn = new jpeg_decompress_struct(errorManagerIn);
        var jpegOut = new jpeg_compress_struct(errorManagerOut);

        if (progress != null)
        {
            var jpegProgress = new jpeg_progress_mgr();

            jpegProgress.OnProgress += (sender, e) =>
            {
                progress(jpegProgress.Completed_passes, jpegProgress.Total_passes, jpegProgress.Pass_counter, jpegProgress.Pass_limit);
            };

            jpegOut.Progress = jpegProgress;
        }

        string tmpPath = null;
        bool success = false;

        try
        {
            if (outPath == inPath)
            {
                tmpPath = FileHelper.CreateEmptyTempFile();
            }

            using var inStream = new FileStream(inPath, FileMode.Open, FileAccess.Read);
            using var outStream = new FileStream(tmpPath ?? outPath, FileMode.Create, FileAccess.Write);

            jpegIn.jpeg_stdio_src(inStream);

            jpegIn.jpeg_save_markers((int)JPEG_MARKER.COM, 0xFFFF);

            for (int m = 0; m < 16; m++)
            {
                jpegIn.jpeg_save_markers((int)JPEG_MARKER.APP0 + m, 0xFFFF);
            }

            jpegIn.jpeg_read_header(true);

            var coeff = jpegIn.jpeg_read_coefficients();

            jpegIn.jpeg_copy_critical_parameters(jpegOut);

            if (progressive)
            {
                jpegOut.jpeg_simple_progression();
            }

            if (optimize)
            {
                jpegOut.Optimize_coding = true;
            }

            jpegOut.jpeg_stdio_dest(outStream);

            jpegOut.jpeg_write_coefficients(coeff);

            foreach (var marker in jpegIn.Marker_list)
            {
                if (jpegOut.Write_JFIF_header &&
                    marker.Marker == (int)JPEG_MARKER.APP0 &&
                    marker.Data.Length >= 5 &&
                    marker.Data[0] == 0x4A &&
                    marker.Data[1] == 0x46 &&
                    marker.Data[2] == 0x49 &&
                    marker.Data[3] == 0x46 &&
                    marker.Data[4] == 0)
                {
                    continue;  // reject duplicate JFIF
                }

                if (jpegOut.Write_Adobe_marker &&
                    marker.Marker == (int)JPEG_MARKER.APP0 + 14 &&
                    marker.Data.Length >= 5 &&
                    marker.Data[0] == 0x41 &&
                    marker.Data[1] == 0x64 &&
                    marker.Data[2] == 0x6F &&
                    marker.Data[3] == 0x62 &&
                    marker.Data[4] == 0x65)
                {
                    continue;  // reject duplicate Adobe
                }

                jpegOut.jpeg_write_marker(marker.Marker, marker.Data);
            }

            jpegOut.jpeg_finish_compress();
            jpegIn.jpeg_finish_decompress();
            jpegOut.jpeg_destroy();
            jpegIn.jpeg_destroy();

            success = !errorManagerIn.IsFailed && !errorManagerOut.IsFailed;
        }
        finally
        {
            if (tmpPath != null)
            {
                if (success)
                {
                    File.Delete(outPath);
                    File.Move(tmpPath, outPath);
                }
                else
                {
                    File.Delete(tmpPath);
                }
            }
        }

        return success;
    }
}