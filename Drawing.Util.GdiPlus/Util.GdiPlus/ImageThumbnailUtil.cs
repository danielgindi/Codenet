using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using Codenet.Drawing.Common;

#nullable enable

namespace Codenet.Drawing.Util.GdiPlus;

public static class ImageThumbnailUtil
{
    public static bool MakeThumbnail(
        string sourcePath,
        string destPath,
        Int32 boundsX,
        Int32 boundsY,
        ImageFormat? destinationFormat = null,
        ImageEncodingOptions? encodingOptions = null,
        bool maintainAspectRatio = true,
        bool shrinkToFit = true,
        bool enlargeToFit = false,
        bool fitFromOutside = false,
        bool fixedFinalSize = false,
        double zoomFactor = 0.0d,
        Color? backgroundColor = null,
        CropAnchor? cropAnchor = null,
        Corner roundedCorners = Corner.None,
        int cornerRadius = 0,
        float borderWidth = 0f,
        Color? borderColor = null)
    {
        if (backgroundColor != null && backgroundColor.Value.A == 0)
            backgroundColor = null;

        if (borderColor != null && borderColor.Value.A == 0)
            borderColor = null;

        bool processBorderAndCorners =
            (borderWidth > 0 && borderColor != null)
            || (cornerRadius > 0 && roundedCorners != Corner.None);

        var dimensions = ImageDimensionsDecoder.GetImageRawSizeAndOrientation(sourcePath);

        if (dimensions == null)
        {
            using var image = Image.FromFile(sourcePath);
            dimensions = new ImageDimensionsDecoder.ImageSize { RawSize = new Size(image.Width, image.Height) };
        }

        var sourceSize = dimensions.Value.TransformedSize;

        if ((boundsX <= 0 || boundsY <= 0) && !fixedFinalSize)
        {
            if (boundsX <= 0) boundsX = sourceSize.Width;
            if (boundsY <= 0) boundsY = sourceSize.Height;
        }

        Size szResizedImage = Size.Empty;

        if (!maintainAspectRatio)
        {
            if (fixedFinalSize)
            {
                if (boundsX <= 0 || boundsY <= 0)
                {
                    szResizedImage = SizeHelper.CalculateBounds(
                        width: sourceSize.Width, 
                        height: sourceSize.Height,
                        boundsX: boundsX,
                        boundsY: boundsY, 
                        outsideBox: fitFromOutside, 
                        allowEnlarge: enlargeToFit, 
                        allowShrink: shrinkToFit
                    );

                    if (boundsX <= 0) 
                        boundsX = szResizedImage.Width;

                    if (boundsY <= 0)
                        boundsY = szResizedImage.Height;
                }
            }
            szResizedImage.Width = boundsX;
            szResizedImage.Height = boundsY;
        }
        else
        {
            szResizedImage = SizeHelper.CalculateBounds(
                width: sourceSize.Width, 
                height: sourceSize.Height,
                boundsX: boundsX, 
                boundsY: boundsY,
                outsideBox: fitFromOutside,
                allowEnlarge: enlargeToFit,
                allowShrink: shrinkToFit
            );

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

        encodingOptions ??= ImageSaveUtil.EncodingOptionsWithQualityBasedOnSize(
            dimensions.Value.TransformedSize.Width,
            dimensions.Value.TransformedSize.Height);

        return ImageFrameProcessingUtil.ProcessMultiframeImage(sourcePath, destPath, destinationFormat, encodingOptions, (frame, selectedFormat) =>
        {
            Image? imgProcessed = null;

            try
            {
                if (dimensions.Value.RequiredRotationToRevert != ImageDimensionsDecoder.RotateFlipType.None)
                {
                    try
                    {
                        frame.ApplyExifOrientationInPlace(true);
                    }
                    catch { }
                }

                backgroundColor = FixTransparentBgColor(backgroundColor ?? Color.Transparent, selectedFormat);

                imgProcessed = new Bitmap(finalSize.Width, finalSize.Height,
                    selectedFormat == ImageFormat.Png || selectedFormat == ImageFormat.Gif
                        ? System.Drawing.Imaging.PixelFormat.Format32bppArgb
                        : System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using var canvas = Graphics.FromImage(imgProcessed);

                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                canvas.SmoothingMode = SmoothingMode.HighQuality;
                canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                canvas.CompositingQuality = CompositingQuality.HighQuality;

                canvas.Clear(backgroundColor.Value);

                int halfBorderWidth = (int)(borderWidth / 2.0f + 0.5f);
                RenderWithRoundCornersAndBorder(
                    canvas: canvas,
                    image: frame,
                    borderRect: new Rectangle(
                        halfBorderWidth,
                        halfBorderWidth,
                        finalSize.Width - halfBorderWidth - halfBorderWidth,
                        finalSize.Height - halfBorderWidth - halfBorderWidth
                    ),
                    cornerRadiusTopLeft: (roundedCorners & Corner.TopLeft) != Corner.None ? cornerRadius : 0,
                    cornerRadiusTopRight: (roundedCorners & Corner.TopRight) != Corner.None ? cornerRadius : 0,
                    cornerRadiusBottomRight: (roundedCorners & Corner.BottomRight) != Corner.None ? cornerRadius : 0,
                    cornerRadiusBottomLeft: (roundedCorners & Corner.BottomLeft) != Corner.None ? cornerRadius : 0,
                    borderWidth: borderWidth,
                    borderColor: borderColor,
                    backgroundColor: null,
                    targetRect: new Rectangle(xDrawPos, yDrawPos, szResizedImage.Width, szResizedImage.Height)
                );
            }
            catch (Exception ex)
            {
                imgProcessed?.Dispose();
                imgProcessed = null;
                Trace.TraceError($"ImageThumbnailUtil.MakeThumbnail failed in ProcessMultiframeImage - {ex}");
            }

            return imgProcessed;
        });
    }

    /// <summary>
    /// Creates a rounded corner image from the source image and saves it to the destination file.
    /// </summary>
    /// <param name="sourcePath">Source image</param>
    /// <param name="destPath">Having the <paramref name="destPath"/> the same as <paramref name="sourcePath"/> may not be safe, especially for multi-frame sources where the source is streamed.</param>
    /// <param name="destinationFormat">Destination file format. null for original</param>
    /// <param name="encodingOptions"></param>
    /// <param name="roundedCorners"></param>
    /// <param name="cornerRadius"></param>
    /// <param name="backgroundColor"></param>
    /// <param name="borderWidth"></param>
    /// <param name="borderColor"></param>
    /// <returns></returns>
    public static bool SetImageRoundBorder(
        string sourcePath,
        string destPath,
        ImageFormat? destinationFormat,
        ImageEncodingOptions? encodingOptions,
        Corner roundedCorners,
        int cornerRadius,
        Color backgroundColor,
        float borderWidth,
        Color? borderColor = null)
    {
        var dimensions = ImageDimensionsDecoder.GetImageRawSizeAndOrientation(sourcePath);

        if (dimensions == null)
        {
            using var image = Image.FromFile(sourcePath);
            dimensions = new ImageDimensionsDecoder.ImageSize { RawSize = new Size(image.Width, image.Height) };
        }

        encodingOptions ??= ImageSaveUtil.EncodingOptionsWithQualityBasedOnSize(
            dimensions.Value.TransformedSize.Width,
            dimensions.Value.TransformedSize.Height);

        return ImageFrameProcessingUtil.ProcessMultiframeImage(sourcePath, destPath, destinationFormat, encodingOptions, (frame, selectedFormat) =>
        {
            Image? imgProcessed = null;

            try
            {
                if (dimensions.Value.RequiredRotationToRevert != ImageDimensionsDecoder.RotateFlipType.None)
                {
                    try
                    {
                        frame.ApplyExifOrientationInPlace(true);
                    }
                    catch { }
                }

                imgProcessed = new Bitmap(frame.Width, frame.Height,
                    selectedFormat == ImageFormat.Png
                        ? System.Drawing.Imaging.PixelFormat.Format32bppArgb
                        : System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using var canvas = Graphics.FromImage(imgProcessed);
                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                canvas.SmoothingMode = SmoothingMode.HighQuality;
                canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                canvas.CompositingQuality = CompositingQuality.HighQuality;

                canvas.Clear(FixTransparentBgColor(backgroundColor, selectedFormat));

                int halfBorderWidth = (int)(borderWidth / 2.0f + 0.5f);
                RenderWithRoundCornersAndBorder(
                    canvas: canvas,
                    image: frame,
                    borderRect: new Rectangle(
                        halfBorderWidth,
                        halfBorderWidth,
                        frame.Width - halfBorderWidth - halfBorderWidth,
                        frame.Height - halfBorderWidth - halfBorderWidth
                    ),
                    cornerRadiusTopLeft: (roundedCorners & Corner.TopLeft) != Corner.None ? cornerRadius : 0,
                    cornerRadiusTopRight: (roundedCorners & Corner.TopRight) != Corner.None ? cornerRadius : 0,
                    cornerRadiusBottomRight: (roundedCorners & Corner.BottomRight) != Corner.None ? cornerRadius : 0,
                    cornerRadiusBottomLeft: (roundedCorners & Corner.BottomLeft) != Corner.None ? cornerRadius : 0,
                    borderWidth: borderWidth,
                    borderColor: borderColor,
                    backgroundColor: null
                );
            }
            catch (Exception ex)
            {
                imgProcessed?.Dispose();
                imgProcessed = null;
                Trace.TraceError($"ImageThumbnailUtil.SetImageRoundBorder failed in ProcessMultiframeImage - {ex}");
            }

            return imgProcessed;
        });
    }

    private static void RenderWithRoundCornersAndBorder(
        Graphics canvas,
        Image image,
        Rectangle borderRect,
        int cornerRadiusTopLeft,
        int cornerRadiusTopRight,
        int cornerRadiusBottomRight,
        int cornerRadiusBottomLeft,
        float borderWidth,
        Color? borderColor,
        Color? backgroundColor,
        Rectangle? targetRect = null)
    {
        SmoothingMode mode = canvas.SmoothingMode;
        canvas.SmoothingMode = SmoothingMode.HighQuality;

        using var borderPen = new Pen(borderColor ?? Color.Transparent, borderWidth);

        if (cornerRadiusTopLeft <= 0 && cornerRadiusTopRight <= 0 &&
            cornerRadiusBottomRight <= 0 && cornerRadiusBottomLeft <= 0)
        {
            if (backgroundColor != null &&
                targetRect != null &&
                !targetRect!.Value.Contains(borderRect))
            {
                canvas.FillRectangle(new SolidBrush(backgroundColor!.Value), borderRect);
            }

            canvas.DrawImage(image, targetRect ?? borderRect);

            if (borderWidth > 0f && borderColor != null && borderColor.Value.A > 0)
            {
                canvas.DrawRectangle(borderPen, borderRect);
            }
        }
        else
        {
            GraphicsPath path = new GraphicsPath();

            int cornerW, cornerH;
            if (cornerRadiusTopLeft > 0)
            {
                cornerW = Math.Min(borderRect.Width - Math.Min(cornerRadiusTopRight, borderRect.Width), cornerRadiusTopLeft);
                cornerH = Math.Min(borderRect.Height - Math.Min(cornerRadiusBottomLeft, borderRect.Height), cornerRadiusTopLeft);
                path.AddArc(new Rectangle(borderRect.Location, new Size(cornerW * 2, cornerH * 2)), 180, 90);
            }
            else
            {
                path.AddLine(0, 0, 0, 0);
            }

            if (cornerRadiusTopRight > 0)
            {
                cornerW = Math.Min(borderRect.Width - Math.Min(cornerRadiusTopLeft, borderRect.Width), cornerRadiusTopRight);
                cornerH = Math.Min(borderRect.Height - Math.Min(cornerRadiusBottomRight, borderRect.Height), cornerRadiusTopRight);
                path.AddArc(new Rectangle(new Point(borderRect.X + borderRect.Width - cornerW * 2, borderRect.Y),
                    new Size(cornerW * 2, cornerH * 2)), 270, 90);
            }
            else
            {
                path.AddLine(borderRect.Right, 0, borderRect.Right, 0);
            }

            if (cornerRadiusBottomRight > 0)
            {
                cornerW = Math.Min(borderRect.Width - Math.Min(cornerRadiusBottomLeft, borderRect.Width), cornerRadiusBottomRight);
                cornerH = Math.Min(borderRect.Height - Math.Min(cornerRadiusTopRight, borderRect.Height), cornerRadiusBottomRight);
                path.AddArc(new Rectangle(new Point(borderRect.X + borderRect.Width - cornerW * 2, borderRect.Y + borderRect.Height - cornerH * 2),
                    new Size(cornerW * 2, cornerH * 2)), 0, 90);
            }
            else
            {
                path.AddLine(borderRect.Right, borderRect.Bottom, borderRect.Right, borderRect.Bottom);
            }

            if (cornerRadiusBottomLeft > 0)
            {
                cornerW = Math.Min(borderRect.Width - Math.Min(cornerRadiusBottomRight, borderRect.Width), cornerRadiusBottomLeft);
                cornerH = Math.Min(borderRect.Height - Math.Min(cornerRadiusTopLeft, borderRect.Height), cornerRadiusBottomLeft);
                path.AddArc(new Rectangle(new Point(borderRect.X, borderRect.Y + borderRect.Height - cornerH * 2),
                    new Size(cornerW * 2, cornerH * 2)), 90, 90);
            }
            else
            {
                path.AddLine(0, borderRect.Bottom, 0, borderRect.Bottom);
            }

            path.CloseFigure();

            if (backgroundColor != null &&
                targetRect != null &&
                !targetRect!.Value.Contains(borderRect))
            {
                canvas.FillPath(new SolidBrush(backgroundColor!.Value), path);
            }

            canvas.Save();
            canvas.SetClip(path);
            canvas.DrawImage(image, targetRect ?? borderRect);
            canvas.ResetClip();

            if (borderWidth > 0f && borderColor != null && borderColor.Value.A > 0)
            {
                canvas.DrawPath(borderPen, path);
            }
        }

        canvas.SmoothingMode = mode;
    }

    private static Color FixTransparentBgColor(Color color, ImageFormat destinationFormat)
    {
        if (destinationFormat.Equals(ImageFormat.Png) ||
            destinationFormat.Equals(ImageFormat.Icon)||
            destinationFormat.Equals(ImageFormat.Gif) && color == Color.Transparent)
            return color; // Supports transparency

        if (color.A == 0)
        {
            color = Color.White; // Avoid black backgrounds
        }

        return color;
    }
}