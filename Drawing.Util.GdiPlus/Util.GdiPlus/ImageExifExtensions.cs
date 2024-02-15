using System.Drawing;
using System.Drawing.Imaging;
using Codenet.Drawing.Common;

#nullable enable

namespace Codenet.Drawing.Util.GdiPlus;

public static class ImageExifExtensions
{
    public static void ApplyExifOrientationInPlace(this Image image, bool removeExifOrientationTag)
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
}
