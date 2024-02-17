using SkiaSharp;

#nullable enable

namespace Codenet.Drawing.Util.Skia;

public static class SKBitmapExifExtensions
{
    public static SKBitmap? MakeWithAppliedExifOrientation(this SKBitmap bitmap, int exifOrientation)
    {
        if (exifOrientation < 2 || exifOrientation > 8)
            return bitmap;

        var rotatedBitmap = new SKBitmap(
            exifOrientation >= 5 ? bitmap.Height : bitmap.Width,
            exifOrientation >= 5 ? bitmap.Width : bitmap.Height,
            bitmap.ColorType,
            bitmap.AlphaType,
            bitmap.ColorSpace);

        try
        {
            using var canvas = new SKCanvas(rotatedBitmap);
            canvas.Clear(SKColors.Transparent);

            switch (exifOrientation)
            {
                case 2:
                    canvas.Scale(-1, 1);
                    canvas.Translate(-rotatedBitmap.Width, 0);
                    break;

                case 3:
                    canvas.Translate(rotatedBitmap.Width, rotatedBitmap.Height);
                    canvas.RotateDegrees(180);
                    break;

                case 4:
                    canvas.Translate(0, rotatedBitmap.Height);
                    canvas.Scale(1, -1);
                    break;

                case 5:
                    canvas.RotateDegrees(270);
                    canvas.Scale(-1, 1);
                    break;

                case 6:
                    canvas.Translate(rotatedBitmap.Width, 0);
                    canvas.RotateDegrees(90);
                    break;

                case 7:
                    canvas.Translate(rotatedBitmap.Width, rotatedBitmap.Height);
                    canvas.RotateDegrees(90);
                    canvas.Scale(-1, 1);
                    break;

                case 8:
                    canvas.Translate(0, rotatedBitmap.Height);
                    canvas.RotateDegrees(270);
                    break;
            }

            canvas.DrawBitmap(bitmap, 0, 0);

            return rotatedBitmap;
        }
        catch
        {
            rotatedBitmap.Dispose();
            throw;
        }
    }
}
