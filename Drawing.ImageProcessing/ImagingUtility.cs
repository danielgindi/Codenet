using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Codenet.Drawing.Common;
using Codenet.Drawing.Encoders;
using Codenet.Drawing.Quantizers.XiaolinWu;
using Codenet.Drawing.Quantizers.Quantizers;
using Codenet.IO;
using System.Diagnostics;

namespace Codenet.Drawing.ImageProcessing
{
    public static partial class ImagingUtility
    {
        public static void ApplyExifOrientation(Image image, bool removeExifOrientationTag)
        {
            if (image == null) return;

            PropertyItem item = image.GetPropertyItem((int)ExifPropertyTag.PropertyTagOrientation);
            if (item != null)
            {
                switch (item.Value[0])
                {
                    default:
                    case 1:
                        break;
                    case 2:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        break;
                    case 5:
                        image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 6:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 8:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }

                if (removeExifOrientationTag)
                {
                    image.RemovePropertyItem(item.Id);
                }
            }
        }

        private static Color FixTransparentBgColor(Color color, ImageFormat destinationFormat)
        {
            if (destinationFormat.Equals(ImageFormat.Png) ||
                destinationFormat.Equals(ImageFormat.Gif) ||
                destinationFormat.Equals(ImageFormat.Icon))
                return color; // Supports transparency

            if (color == Color.Transparent || color == Color.Empty)
            {
                color = Color.White; // Avoid black backgrounds
            }

            return color;
        }

        public static void FillRoundRectangle(Graphics destinationGraphics, Rectangle destinationRect,
            int cornerRadiusTopLeft,
            int cornerRadiusTopRight,
            int cornerRadiusBottomRight,
            int cornerRadiusBottomLeft,
            Brush fillBrush, Pen borderPen)
        {
            SmoothingMode mode = destinationGraphics.SmoothingMode;
            destinationGraphics.SmoothingMode = SmoothingMode.HighQuality;

            if (cornerRadiusTopLeft <= 0 && cornerRadiusTopRight <= 0 &&
                cornerRadiusBottomRight <= 0 && cornerRadiusBottomLeft <= 0)
            {
                // No corners.. Just fill it
                if (borderPen.Width > 0 && (borderPen.Color != Color.Transparent && borderPen.Color != Color.Empty))
                {
                    destinationGraphics.DrawRectangle(borderPen, destinationRect); // Draw the line first, this way we get better quality
                }
                destinationGraphics.FillRectangle(fillBrush, destinationRect);
            }
            else
            {
                GraphicsPath path = new GraphicsPath();

                //int maxWRadius = destinationRect.Width / 2;
                //int maxHRadius = destinationRect.Height / 2;

                int cornerW, cornerH;
                if (cornerRadiusTopLeft > 0)
                {
                    cornerW = Math.Min(destinationRect.Width - Math.Min(cornerRadiusTopRight, destinationRect.Width), cornerRadiusTopLeft);
                    cornerH = Math.Min(destinationRect.Height - Math.Min(cornerRadiusBottomLeft, destinationRect.Height), cornerRadiusTopLeft);
                    path.AddArc(new Rectangle(destinationRect.Location, new Size(cornerW * 2, cornerH * 2)), 180, 90);
                }
                else
                {
                    path.AddLine(0, 0, 0, 1);
                    path.AddLine(0, 0, 1, 0);
                }
                if (cornerRadiusTopRight > 0)
                {
                    cornerW = Math.Min(destinationRect.Width - Math.Min(cornerRadiusTopLeft, destinationRect.Width), cornerRadiusTopRight);
                    cornerH = Math.Min(destinationRect.Height - Math.Min(cornerRadiusBottomRight, destinationRect.Height), cornerRadiusTopRight);
                    path.AddArc(new Rectangle(new Point(destinationRect.X + destinationRect.Width - cornerW * 2, destinationRect.Y),
                        new Size(cornerW * 2, cornerH * 2)), 270, 90);
                }
                else
                {
                    path.AddLine(destinationRect.Right - 1, 0, destinationRect.Right, 0);
                    path.AddLine(destinationRect.Right, 0, destinationRect.Right, 1);
                }
                if (cornerRadiusBottomRight > 0)
                {
                    cornerW = Math.Min(destinationRect.Width - Math.Min(cornerRadiusBottomLeft, destinationRect.Width), cornerRadiusBottomRight);
                    cornerH = Math.Min(destinationRect.Height - Math.Min(cornerRadiusTopRight, destinationRect.Height), cornerRadiusBottomRight);
                    path.AddArc(new Rectangle(new Point(destinationRect.X + destinationRect.Width - cornerW * 2, destinationRect.Y + destinationRect.Height - cornerH * 2),
                        new Size(cornerW * 2, cornerH * 2)), 0, 90);
                }
                else
                {
                    path.AddLine(destinationRect.Right - 1, destinationRect.Bottom, destinationRect.Right, destinationRect.Bottom);
                    path.AddLine(destinationRect.Right, destinationRect.Bottom - 1, destinationRect.Right, destinationRect.Bottom);
                }
                if (cornerRadiusBottomLeft > 0)
                {
                    cornerW = Math.Min(destinationRect.Width - Math.Min(cornerRadiusBottomRight, destinationRect.Width), cornerRadiusBottomLeft);
                    cornerH = Math.Min(destinationRect.Height - Math.Min(cornerRadiusTopLeft, destinationRect.Height), cornerRadiusBottomLeft);
                    path.AddArc(new Rectangle(new Point(destinationRect.X, destinationRect.Y + destinationRect.Height - cornerH * 2),
                        new Size(cornerW * 2, cornerH * 2)), 90, 90);
                }
                else
                {
                    path.AddLine(0, destinationRect.Bottom, 1, destinationRect.Bottom);
                    path.AddLine(0, destinationRect.Bottom - 1, 0, destinationRect.Bottom);
                }
                path.CloseFigure();

                if (borderPen.Width > 0 && (borderPen.Color != Color.Transparent && borderPen.Color != Color.Empty))
                {
                    destinationGraphics.DrawPath(borderPen, path); // Draw the line first, this way we get better quality
                }
                destinationGraphics.FillPath(fillBrush, path);
            }

            destinationGraphics.SmoothingMode = mode;
        }

        public static Boolean ProcessImage(
            String sourcePath,
            String destinationPath/*=null*/,
            ImageFormat destinationFormat/*=null*/,
            Color backgroundColor,
            Int32 boundsX,
            Int32 boundsY,
            Boolean maintainAspectRatio/*=true*/,
            Boolean shrinkToFit/*=true*/,
            Boolean enlargeToFit/*=false*/,
            Boolean fitFromOutside/*=false*/,
            Boolean fixedFinalSize/*=false*/,
            Double zoomFactor/*0.0d*/,
            CropAnchor cropAnchor/*=CropAnchor.None*/,
            Corner roundedCorners,
            Int32 cornerRadius,
            Color borderColor,
            float borderWidth)
        {
            bool retValue = false;

            if (destinationPath == null) destinationPath = sourcePath;
            if (backgroundColor == null || backgroundColor == Color.Empty) backgroundColor = Color.Transparent;
            if (borderColor == null || borderColor == Color.Empty) borderColor = Color.Transparent;

            bool processBorderAndCorners =
                (borderWidth > 0 && borderColor != Color.Transparent)
                || (cornerRadius > 0 && roundedCorners != Corner.None);

            if ((boundsX <= 0 || boundsY <= 0) && !fixedFinalSize)
            {
                Size? sz = ImageDimensionsDecoder.GetImageSize(sourcePath);
                if (sz == null)
                {
                    using (var image = Image.FromFile(sourcePath))
                        sz = new Size(image.Width, image.Height);
                }
                if (boundsX <= 0) boundsX = sz.Value.Width;
                if (boundsY <= 0) boundsY = sz.Value.Height;
            }

            using (var imgOriginal = Image.FromFile(sourcePath))
            {
                try
                {
                    ApplyExifOrientation(imgOriginal, true);
                }
                catch { }

                if (destinationFormat == null) destinationFormat = imgOriginal.RawFormat;

                backgroundColor = FixTransparentBgColor(backgroundColor, destinationFormat);

                Size szResizedImage = Size.Empty;

                if (!maintainAspectRatio)
                {
                    if (fixedFinalSize)
                    {
                        if (boundsX <= 0 || boundsY <= 0)
                        {
                            szResizedImage = SizeHelper.CalculateBounds(
                                imgOriginal.Width, imgOriginal.Height,
                                boundsX, boundsY, fitFromOutside, enlargeToFit, shrinkToFit);
                            if (boundsX <= 0) boundsX = szResizedImage.Width;
                            if (boundsY <= 0) boundsY = szResizedImage.Height;
                        }
                    }
                    szResizedImage.Width = boundsX;
                    szResizedImage.Height = boundsY;
                }
                else
                {
                    szResizedImage = SizeHelper.CalculateBounds(
                        imgOriginal.Width, imgOriginal.Height,
                        boundsX, boundsY, fitFromOutside, enlargeToFit, shrinkToFit);
                    if (fixedFinalSize)
                    {
                        if (boundsX <= 0) boundsX = szResizedImage.Width;
                        if (boundsY <= 0) boundsY = szResizedImage.Height;
                    }
                }
                if (zoomFactor != 0.0d && zoomFactor != 1.0d)
                {
                    szResizedImage.Width = (int)Math.Round((double)szResizedImage.Width * zoomFactor);
                    szResizedImage.Height = (int)Math.Round((double)szResizedImage.Height * zoomFactor);
                }

                Size finalSize = Size.Empty;
                if (fixedFinalSize)
                {
                    finalSize.Width = boundsX;
                    finalSize.Height = boundsY;
                }
                else
                {
                    finalSize.Width = szResizedImage.Width > boundsX ? boundsX : szResizedImage.Width;
                    finalSize.Height = szResizedImage.Height > boundsY ? boundsY : szResizedImage.Height;
                }

                int xDrawPos = 0,
                    yDrawPos = 0;

                if (cropAnchor != null)
                {
                    xDrawPos = (int)-((float)(szResizedImage.Width - finalSize.Width) * cropAnchor.X);
                    yDrawPos = (int)-((float)(szResizedImage.Height - finalSize.Height) * cropAnchor.Y);

                    xDrawPos = Math.Max(Math.Min(xDrawPos, 0), -(szResizedImage.Width - finalSize.Width));
                    yDrawPos = Math.Max(Math.Min(yDrawPos, 0), -(szResizedImage.Height - finalSize.Height));
                }

                string tempFilePath = FileHelper.CreateEmptyTempFile();

                ProcessingHelper.EncodingOptions encodingOptions = EncodingOptionsWithQualityBasedOnSize(imgOriginal.Width, imgOriginal.Height);

                retValue = ProcessingHelper.ProcessImageFramesToFile(imgOriginal, tempFilePath ?? destinationPath, destinationFormat, encodingOptions, delegate (Image frame)
                {
                    Image imgProcessed = null, imgProcessBorder = null;
                    try
                    {
                        imgProcessed = new Bitmap(finalSize.Width, finalSize.Height);
                        using (var gProcessed = Graphics.FromImage(imgProcessed))
                        {
                            gProcessed.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gProcessed.SmoothingMode = SmoothingMode.HighQuality;
                            gProcessed.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gProcessed.CompositingQuality = CompositingQuality.HighQuality;

                                // Fill background if we are not intending to do that later on
                                if (!processBorderAndCorners) gProcessed.Clear(backgroundColor);
                            else gProcessed.Clear(FixTransparentBgColor(Color.Transparent, destinationFormat));

                                // Draw the final image in the correct position inside the final box
                                gProcessed.DrawImage(frame, xDrawPos, yDrawPos, szResizedImage.Width, szResizedImage.Height);

                            if (processBorderAndCorners)
                            {
                                imgProcessBorder = new Bitmap(finalSize.Width, finalSize.Height);
                                using (var gProcessBorder = Graphics.FromImage(imgProcessBorder))
                                {
                                    gProcessBorder.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    gProcessBorder.SmoothingMode = SmoothingMode.HighQuality;
                                    gProcessBorder.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    gProcessBorder.CompositingQuality = CompositingQuality.HighQuality;
                                    gProcessBorder.Clear(backgroundColor);
                                    using (var brush = new TextureBrush(imgProcessed))
                                    using (var pen = new Pen(borderColor, borderWidth))
                                    {
                                        Int32 halfBorderWidth = (Int32)(borderWidth / 2.0f + 0.5f);
                                        FillRoundRectangle(gProcessBorder,
                                            new Rectangle(
                                                halfBorderWidth,
                                                halfBorderWidth,
                                                finalSize.Width - halfBorderWidth - halfBorderWidth,
                                                finalSize.Height - halfBorderWidth - halfBorderWidth),
                                                (roundedCorners & Corner.TopLeft) == 0 ? 0 : cornerRadius,
                                                (roundedCorners & Corner.TopRight) == 0 ? 0 : cornerRadius,
                                                (roundedCorners & Corner.BottomRight) == 0 ? 0 : cornerRadius,
                                                (roundedCorners & Corner.BottomLeft) == 0 ? 0 : cornerRadius,
                                                brush, pen);
                                    }
                                }
                                if (imgProcessed != null) imgProcessed.Dispose();
                                imgProcessed = imgProcessBorder;
                                imgProcessBorder = null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (imgProcessed != null) imgProcessed.Dispose();
                        if (imgProcessBorder != null) imgProcessBorder.Dispose();
                        imgProcessed = imgProcessBorder = null;
                        Trace.TraceError($"ImagingUtility.ProcessImage failed in ProcessImageFramesToFile - {ex}");
                    }
                    return imgProcessed;
                });

                // So we can overwrite
                imgOriginal.Dispose();

                if (retValue && tempFilePath != null)
                {
                    using (var temporaryFileDeleter = new TemporaryFileDeleter(tempFilePath))
                    {
                        if (System.IO.File.Exists(destinationPath)) System.IO.File.Delete(destinationPath);
                        System.IO.File.Move(tempFilePath, destinationPath);
                        FileHelper.ResetFilePermissionsToInherited(destinationPath);
                        temporaryFileDeleter.DoNotDelete();
                    }
                }
            }

            return retValue;
        }

        #region Saving

        /// <summary>
        /// Saves an Image object to the specified local path
        /// 
        /// This may throw any Exception that an Image.Save(...) may throw
        /// </summary>
        /// <param name="imageData">Image object to save</param>
        /// <param name="imagePath">Destination local path</param>
        /// <param name="imageFormat">Image format to use</param>
        /// <param name="jpegQuality">Quality to use in case of a jpeg format (0.0 - 1.0)</param>
        public static void SaveImage(Image imageData, string imagePath, ImageFormat imageFormat, ProcessingHelper.EncodingOptions encodingOptions)
        {
            if (encodingOptions.JpegQuality <= 0.0f)
            {
                encodingOptions.JpegQuality = 0.9f;
            }
            else if (encodingOptions.JpegQuality > 1.0f)
            {
                encodingOptions.JpegQuality = 1.0f;
            }

            if (imageFormat.Equals(ImageFormat.Jpeg))
            {
                using var imageBuffer = GdiPlusImageBuffer.FromImage(imageData, ImageLockMode.ReadOnly);
                JpegEncoder.EncodeImageWithLibjpeg(imageBuffer, imagePath, encodingOptions.JpegQuality);
            }
            else if (imageFormat.Equals(ImageFormat.Gif))
            {
                BaseColorQuantizer quantizer = null;
                if (encodingOptions.QuantizerSupplier != null)
                {
                    quantizer = encodingOptions.QuantizerSupplier(imageData);
                }
                if (quantizer == null)
                {
                    quantizer = new WuColorQuantizer();
                }

                using (var quantized = GdiPlusImageBuffer.QuantizeImage(imageData, quantizer, 255, 4))
                {
                    quantized.Save(imagePath, ImageFormat.Gif);
                }
            }
            else
            {
                imageData.Save(imagePath, imageFormat);
            }
        }

        /// <summary>
        /// Saves an Image object to the specified local path
        /// In case of a GIF format chosen, it will use a default high quality Quantizer 
        /// In case of a JPEG format, it will default to 0.9 (90%) quality
        /// 
        /// This may throw any Exception that an Image.Save(...) may throw
        /// </summary>
        /// <param name="imageData">Image object to save</param>
        /// <param name="imagePath">Destination local path</param>
        /// <param name="imageFormat">Image format to use</param>
        public static void SaveImage(Image imageData, string imagePath, ImageFormat imageFormat)
        {
            ProcessingHelper.EncodingOptions encodingOptions = EncodingOptionsWithQualityBasedOnSize(imageData.Width, imageData.Height);
            SaveImage(imageData, imagePath, imageFormat, encodingOptions);
        }

        #endregion

        public static bool SetImageRoundBorder(
            String sourcePath,
            String destinationPath /* null for source */,
            ImageFormat destinationFormat /* null for original format */,
            int cornerRadius,
            float borderWidth,
            Color backgroundColor,
            Color borderColor)
        {
            if (destinationPath == null) destinationPath = sourcePath;
            using (var imgOriginal = Image.FromFile(sourcePath))
            {
                try
                {
                    ApplyExifOrientation(imgOriginal, true);
                }
                catch { }

                string tempFilePath = FileHelper.CreateEmptyTempFile();

                ProcessingHelper.EncodingOptions encodingOptions = EncodingOptionsWithQualityBasedOnSize(imgOriginal.Width, imgOriginal.Height);

                bool retValue = ProcessingHelper.ProcessImageFramesToFile(imgOriginal, destinationPath, destinationFormat, encodingOptions, delegate (Image frame)
                {
                    Image imgProcessed = null;
                    try
                    {
                        imgProcessed = new Bitmap(frame.Width, frame.Height);
                        using (var gTemp = Graphics.FromImage(imgProcessed))
                        {
                            gTemp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gTemp.SmoothingMode = SmoothingMode.HighQuality;
                            gTemp.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gTemp.CompositingQuality = CompositingQuality.HighQuality;
                            gTemp.Clear(backgroundColor);
                            using (var brush = new TextureBrush(frame))
                            using (var pen = new Pen(borderColor, borderWidth))
                            {
                                Int32 halfBorderWidth = (Int32)(borderWidth / 2.0f + 0.5f);
                                FillRoundRectangle(gTemp,
                                    new Rectangle(
                                        halfBorderWidth,
                                        halfBorderWidth,
                                        frame.Width - halfBorderWidth - halfBorderWidth,
                                        frame.Height - halfBorderWidth - halfBorderWidth),
                                        cornerRadius,
                                        cornerRadius,
                                        cornerRadius,
                                        cornerRadius,
                                        brush, pen);

                                return imgProcessed;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (imgProcessed != null) imgProcessed.Dispose();
                        imgProcessed = null;
                        Trace.TraceError($"ImagingUtility.SetImageRoundBorder failed in ProcessImageFramesToFile - {ex}");
                    }
                    return imgProcessed;
                });

                // So we can overwrite
                imgOriginal.Dispose();

                if (retValue && tempFilePath != null)
                {
                    using (TemporaryFileDeleter temporaryFileDeleter = new TemporaryFileDeleter(tempFilePath))
                    {
                        if (System.IO.File.Exists(destinationPath)) System.IO.File.Delete(destinationPath);
                        System.IO.File.Move(tempFilePath, destinationPath);
                        FileHelper.ResetFilePermissionsToInherited(destinationPath);
                        temporaryFileDeleter.DoNotDelete();
                    }
                }

                return retValue;
            }
        }

        public static string MimeTypeForFormat(ImageFormat imageFormat)
        {
            if (imageFormat.Equals(ImageFormat.Jpeg))
            {
                return "image/jpeg";
            }
            else if (imageFormat.Equals(ImageFormat.Png))
            {
                return "image/png";
            }
            else if (imageFormat.Equals(ImageFormat.Gif))
            {
                return "image/gif";
            }
            else if (imageFormat.Equals(ImageFormat.Bmp))
            {
                return "image/bmp";
            }
            else if (imageFormat.Equals(ImageFormat.Emf))
            {
                return "image/x-emf";
            }
            else if (imageFormat.Equals(ImageFormat.Exif))
            {
                return "image/x-exif";
            }
            else if (imageFormat.Equals(ImageFormat.Icon))
            {
                return "image/x-icon";
            }
            else if (imageFormat.Equals(ImageFormat.Tiff))
            {
                return "image/tiff";
            }
            else if (imageFormat.Equals(ImageFormat.Wmf))
            {
                return "image/x-wmf";
            }

            return "image/x-unknown";
        }

        private static ProcessingHelper.EncodingOptions EncodingOptionsWithQualityBasedOnSize(int width, int height)
        {
            var encodingOptions = new ProcessingHelper.EncodingOptions();

            if (width * height > 2000000)
            {
                encodingOptions.JpegQuality = 0.9f;
            }
            else
            {
                encodingOptions.JpegQuality = 0.85f;
            }

            return encodingOptions;
        }
    }

}
