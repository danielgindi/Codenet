using System;
using System.Drawing;
using BitMiracle.LibJpeg.Classic;
using System.IO;
using System.Drawing.Imaging;
using Codenet.IO;

namespace Codenet.Drawing.ImageProcessing.Encoders
{
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
        /// <param name="image">Source image</param>
        /// <param name="filePath">Target file path, will be overwritten</param>
        /// <param name="quality">Quality of output JPEG. 0.0 - 1.0.</param>
        /// <param name="smooth">Smooth input image? 0.0 - 1.0. Default 0.0;</param>
        /// <param name="progressive">Should the image be progressive? (Tends to result in smaller sizes for bigger resolutions)</param>
        /// <param name="optimize">Optimize Huffman table?</param>
        public static void EncodeImageWithLibjpeg(Image image, string filePath, float quality, float smooth = 0f, bool progressive = true, bool optimize = true)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    var bmp = NormalizedBitmapForImage(image);

                    try
                    {
                        jpeg_error_mgr errorManager = new LibJpegErrorHandler();
                        jpeg_compress_struct jpegOut = new jpeg_compress_struct(errorManager);

                        jpegOut.Image_width = image.Width;
                        jpegOut.Image_height = image.Height;
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

                        var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                        try
                        {

                            // Set destination
                            jpegOut.jpeg_stdio_dest(fs);

                            // Start compressor
                            jpegOut.jpeg_start_compress(true);

                            // Process data

                            int lineWidth = data.Width * 3,
                                stride = data.Stride;

                            byte swap;
                            var scanLine = new byte[stride];
                            var scanLines = new byte[][] { scanLine };

                            for (int y = 0, height = bmp.Height, x; y < height; y++)
                            {
                                System.Runtime.InteropServices.Marshal.Copy(data.Scan0 + y * stride, scanLine, 0, lineWidth);

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
                            bmp.UnlockBits(data);
                        }
                    }
                    finally
                    {
                        if (bmp != image)
                        {
                            bmp.Dispose();
                        }
                    }
                }
            }
            catch
            {
                // Fallback
                EncodeImageWithDefault(image, filePath, quality);
            }
        }

        /// <summary>
        /// Encodes the image using default framework encoder
        /// </summary>
        /// <param name="image">Source image</param>
        /// <param name="filePath">Target file path, will be overwritten</param>
        /// <param name="quality">Quality of output JPEG. 0.0 - 1.0.</param>
        public static void EncodeImageWithDefault(Image image, string filePath, float quality)
        {
            ImageCodecInfo encoder = null;
            var encoders = ImageCodecInfo.GetImageEncoders();
            using (var encoderParameters = new EncoderParameters(1))
            {
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, Math.Max(Math.Min((long)Math.Round(quality * 100), 100L), 0L));
                foreach (var item in encoders)
                {
                    if (item.MimeType == @"image/jpeg")
                    {
                        encoder = item;
                        break;
                    }
                }

                if (encoder != null)
                {
                    image.Save(filePath, encoder, encoderParameters);
                }
                else
                {
                    image.Save(filePath, ImageFormat.Jpeg);
                }
            }
        }

        private static Bitmap NormalizedBitmapForImage(Image image)
        {
            if (image.PixelFormat == PixelFormat.Format24bppRgb)
            {
                return image as Bitmap;
            }

            Bitmap bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bmp))
            {
                if ((image.PixelFormat & PixelFormat.Alpha) == PixelFormat.Alpha ||
                    (image.PixelFormat & PixelFormat.PAlpha) == PixelFormat.PAlpha ||
                    (image.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
                {
                    // Chance of transparency
                    g.FillRectangle(new SolidBrush(Color.White), 0, 0, image.Width, image.Height);
                }
                g.DrawImage(image, 0, 0);
            }

            return bmp;
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

                using (var inStream = new FileStream(inPath, FileMode.Open, FileAccess.Read))
                {
                    using (var outStream = new FileStream(tmpPath ?? outPath, FileMode.Create, FileAccess.Write))
                    {
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
                }
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
}
