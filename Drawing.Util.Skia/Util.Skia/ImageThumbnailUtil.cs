using System;
using System.Drawing;
using System.Diagnostics;
using SkiaSharp;
using Codenet.Drawing.Common;

#nullable enable

namespace Codenet.Drawing.Util.Skia;

public static class ImageThumbnailUtil
{
    public static bool MakeThumbnail(
        string sourcePath,
        string destPath,
        Int32 boundsX,
        Int32 boundsY,
        SKEncodedImageFormat? destinationFormat = null,
        ImageEncodingOptions? encodingOptions = null,
        bool maintainAspectRatio = true,
        bool shrinkToFit = true,
        bool enlargeToFit = false,
        bool fitFromOutside = false,
        bool fixedFinalSize = false,
        double zoomFactor = 0.0d,
        SKColor? backgroundColor = null,
        CropAnchor? cropAnchor = null,
        Corner roundedCorners = Corner.None,
        int cornerRadius = 0,
        float borderWidth = 0f,
        SKColor? borderColor = null)
    {
        if (backgroundColor != null && backgroundColor.Value.Alpha == 0)
            backgroundColor = null;

        if (borderColor != null && borderColor.Value.Alpha == 0)
            borderColor = null;

        bool processBorderAndCorners =
            (borderWidth > 0 && borderColor != null)
            || (cornerRadius > 0 && roundedCorners != Corner.None);

        var dimensions = ImageDimensionsDecoder.GetImageRawSizeAndOrientation(sourcePath);

        if (dimensions == null)
        {
            var bounds = SKBitmap.DecodeBounds(sourcePath);
            dimensions = new ImageDimensionsDecoder.ImageSize { RawSize = new Size(bounds.Width, bounds.Height) };
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

        var retValue = ImageFrameProcessingUtil.ProcessMultiframeImage(sourcePath, destPath, destinationFormat, encodingOptions, (frame, selectedFormat) =>
        {
            SKBitmap? bitmap = null;
            var transformedFrame = frame;

            try
            {
                if (dimensions.Value.RequiredRotationToRevert != ImageDimensionsDecoder.RotateFlipType.None)
                {
                    try
                    {
                        transformedFrame = transformedFrame.MakeWithAppliedExifOrientation(dimensions.Value.ExifOrientation) ?? transformedFrame;
                    }
                    catch { }
                }

                backgroundColor = FixTransparentBgColor(backgroundColor ?? SKColor.Empty, selectedFormat, transformedFrame.AlphaType);

                bitmap = new SKBitmap(
                    finalSize.Width, finalSize.Height,
                    transformedFrame!.AlphaType == SKAlphaType.Opaque && backgroundColor.Value.Alpha == 255);

                using var canvas = new SKCanvas(bitmap);

                canvas.Clear(backgroundColor.Value);

                int halfBorderWidth = borderWidth > 0 ? (int)(borderWidth / 2.0f + 0.5f) : 0;
                RenderWithRoundCornersAndBorder(
                    canvas: canvas,
                    bitmap: transformedFrame,
                    borderRect: new SKRectI(
                        halfBorderWidth,
                        halfBorderWidth,
                        finalSize.Width - halfBorderWidth,
                        finalSize.Height - halfBorderWidth
                    ),
                    cornerRadius: cornerRadius,
                    borderWidth: borderWidth,
                    borderColor: borderColor,
                    backgroundColor: null,
                    targetRect: new SKRectI(xDrawPos, yDrawPos, xDrawPos + szResizedImage.Width, yDrawPos + szResizedImage.Height)
                );
            }
            catch (Exception ex)
            {
                bitmap?.Dispose();
                bitmap = null;
                Trace.TraceError($"ImageThumbnailUtil.MakeThumbnail failed in ProcessMultiframeImage - {ex}");
            }
            finally
            {
                if (transformedFrame != frame)
                    transformedFrame.Dispose();
            }

            return bitmap;
        });

        return retValue;
    }

    /// <summary>
    /// Creates a rounded corner image from the source image and saves it to the destination file.
    /// </summary>
    /// <param name="sourcePath">Source image</param>
    /// <param name="destPath">Having the <paramref name="destPath"/> the same as <paramref name="sourcePath"/> may not be safe, especially for multi-frame sources where the source is streamed.</param>
    /// <param name="destinationFormat">Destination file format. null for original</param>
    /// <param name="encodingOptions"></param>
    /// <param name="cornerRadius"></param>
    /// <param name="backgroundColor"></param>
    /// <param name="borderWidth"></param>
    /// <param name="borderColor"></param>
    /// <returns></returns>
    public static bool SetImageRoundBorder(
        string sourcePath,
        string destPath,
        SKEncodedImageFormat? destinationFormat,
        ImageEncodingOptions? encodingOptions,
        int cornerRadius,
        SKColor backgroundColor,
        float borderWidth,
        SKColor? borderColor = null)
    {
        var dimensions = ImageDimensionsDecoder.GetImageRawSizeAndOrientation(sourcePath);

        if (dimensions == null)
        {
            var bounds = SKBitmap.DecodeBounds(sourcePath);
            dimensions = new ImageDimensionsDecoder.ImageSize { RawSize = new Size(bounds.Width, bounds.Height) };
        }

        encodingOptions ??= ImageSaveUtil.EncodingOptionsWithQualityBasedOnSize(
            dimensions.Value.TransformedSize.Width,
            dimensions.Value.TransformedSize.Height);

        return ImageFrameProcessingUtil.ProcessMultiframeImage(sourcePath, destPath, destinationFormat, encodingOptions, (frame, selectedFormat) =>
        {
            SKBitmap? bitmap = null;
            var transformedFrame = frame;

            try
            {
                if (dimensions.Value.RequiredRotationToRevert != ImageDimensionsDecoder.RotateFlipType.None)
                {
                    try
                    {
                        transformedFrame = transformedFrame.MakeWithAppliedExifOrientation(dimensions.Value.ExifOrientation) ?? transformedFrame;
                    }
                    catch { }
                }

                bitmap = new SKBitmap(
                    transformedFrame!.Width, transformedFrame.Height,
                    transformedFrame.AlphaType == SKAlphaType.Opaque && backgroundColor.Alpha == 255);

                using var canvas = new SKCanvas(bitmap);

                canvas.Clear(FixTransparentBgColor(backgroundColor, selectedFormat, bitmap.AlphaType));

                int halfBorderWidth = (int)(borderWidth / 2.0f + 0.5f);
                RenderWithRoundCornersAndBorder(
                    canvas: canvas,
                    bitmap: transformedFrame,
                    borderRect: new SKRectI(
                        halfBorderWidth,
                        halfBorderWidth,
                        frame.Width - halfBorderWidth,
                        frame.Height - halfBorderWidth
                    ),
                    cornerRadius: cornerRadius,
                    borderWidth: borderWidth,
                    borderColor: borderColor,
                    backgroundColor: null);
            }
            catch (Exception ex)
            {
                bitmap?.Dispose();
                bitmap = null;
                Trace.TraceError($"ImageThumbnailUtil.SetImageRoundBorder failed in ProcessMultiframeImage - {ex}");
            }
            finally
            {
                if (transformedFrame != frame)
                    transformedFrame.Dispose();
            }

            return bitmap;
        });
    }

    private static void RenderWithRoundCornersAndBorder(
        SKCanvas canvas,
        SKBitmap bitmap,
        SKRectI borderRect,
        int cornerRadius,
        float borderWidth,
        SKColor? borderColor,
        SKColor? backgroundColor,
        SKRectI? targetRect = null)
    {
        if (targetRect != null && targetRect.Value.Left == 0 && targetRect.Value.Top == 0 &&
                       targetRect.Value.Right == bitmap.Width && targetRect.Value.Bottom == bitmap.Height)
        {
            targetRect = null;
        }

        using var borderPath = new SKPath();

        if (cornerRadius > 0)
            borderPath.AddRoundRect(borderRect, cornerRadius, cornerRadius);
        else borderPath.AddRect(borderRect);

        if (backgroundColor != null &&
            targetRect != null &&
            !targetRect!.Value.Contains(borderRect))
        {
            canvas.DrawPath(borderPath, new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = backgroundColor!.Value,
            });
        }

        if (cornerRadius > 0 || targetRect != null)
        {
            canvas.Save();
            canvas.ClipPath(borderPath);
        }

        var destSize = (targetRect ?? borderRect).Size;
        var scaledBitmap = destSize.Width != bitmap.Width || destSize.Height != bitmap.Height
            ? bitmap.Resize(destSize, SKFilterQuality.High)
            : bitmap;

        canvas.DrawBitmap(scaledBitmap, targetRect ?? borderRect);

        if (cornerRadius > 0 || targetRect != null)
        {
            canvas.Restore();
        }

        if (borderWidth > 0f && borderColor != null && borderColor.Value.Alpha > 0)
        {
            canvas.DrawPath(borderPath, new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = borderColor.Value,
                StrokeWidth = borderWidth,
                IsAntialias = true
            });
        }
    }

    private static SKColor FixTransparentBgColor(SKColor color, SKEncodedImageFormat destinationFormat, SKAlphaType alphaType)
    {
        if (alphaType == SKAlphaType.Opaque ||
            destinationFormat == SKEncodedImageFormat.Jpeg)
        {
            if (color.Alpha == 0)
            {
                color = new SKColor(255, 255, 255, 255); // Avoid black backgrounds
            }
        }

        return color;
    }
}