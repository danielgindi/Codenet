using System;
using System.Drawing;
using System.Drawing.Imaging;
using Codenet.Drawing.Common;
using Codenet.Drawing.Util.GdiPlus;

namespace Codenet.Drawing.ImageProcessing;

public static partial class ImagingUtility
{
    public static void ApplyExifOrientation(Image image, bool removeExifOrientationTag)
    {
        if (image == null) return;

        image.ApplyExifOrientationInPlace(removeExifOrientationTag);
    }

    public static bool ProcessImage(
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
        return ImageThumbnailUtil.MakeThumbnail(
            sourcePath: sourcePath,
            destPath: destinationPath ?? sourcePath,
            boundsX: boundsX,
            boundsY: boundsY,
            destinationFormat: destinationFormat,
            encodingOptions: new ImageEncodingOptions
            {
                Quality = 1f,
                UseLibJpeg = true,
                UseTempFile = true
            },
            maintainAspectRatio: maintainAspectRatio,
            shrinkToFit: shrinkToFit,
            enlargeToFit: enlargeToFit,
            fitFromOutside: fitFromOutside,
            fixedFinalSize: fixedFinalSize,
            zoomFactor: zoomFactor,
            backgroundColor: backgroundColor,
            cropAnchor: cropAnchor,
            roundedCorners: roundedCorners,
            cornerRadius: cornerRadius,
            borderWidth: borderWidth,
            borderColor: borderColor
        );
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
    /// <param name="encodingOptions">Encoding options, including quality</param>
    public static void SaveImage(Image imageData, string imagePath, ImageFormat imageFormat, ProcessingHelper.EncodingOptions encodingOptions)
    {
        ImageSaveUtil.SaveImage(
            image: imageData,
            destPath: imagePath,
            imageFormat: imageFormat,
            encodingOptions: new ImageEncodingOptions
            {
                Quality = 1f,
                UseLibJpeg = true,
                UseTempFile = true
            });
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
        ImageSaveUtil.SaveImage(imageData, imagePath, imageFormat, null);
    }

    #endregion

    public static bool SetImageRoundBorder(
        string sourcePath,
        string destPath /* null for source */,
        ImageFormat destinationFormat /* null for original format */,
        int cornerRadius,
        float borderWidth,
        Color backgroundColor,
        Color borderColor)
    {
        return ImageThumbnailUtil.SetImageRoundBorder(
            sourcePath: sourcePath,
            destPath: destPath ?? sourcePath,
            destinationFormat: destinationFormat,
            encodingOptions: new ImageEncodingOptions
            {
                Quality = 1f,
                UseLibJpeg = true,
                UseTempFile = true
            }, 
            cornerRadius: cornerRadius,
            backgroundColor: backgroundColor,
            borderWidth: borderWidth,
            borderColor: borderColor
        );
    }
}