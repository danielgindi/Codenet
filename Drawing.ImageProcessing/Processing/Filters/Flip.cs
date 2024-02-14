using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing.Filters;

public class Flip : IImageFilter
{
    public enum Direction
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = Horizontal | Vertical
    }

    public FilterError ProcessImage(
        DirectAccessBitmap bmp,
        params object[] args)
    {
        bool bHorz = false;
        bool bVert = false;
        foreach (object arg in args)
        {
            if (arg is Direction)
            {
                Direction dir = (Direction)arg;
                if ((dir & Direction.Horizontal) == Direction.Horizontal) bHorz = true;
                if ((dir & Direction.Vertical) == Direction.Vertical) bVert = true;
            }
        }
        if (!bHorz && !bVert) return FilterError.OK;

        switch (bmp.Bitmap.PixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                return ProcessImage24rgb(bmp, bHorz, bVert);
            case PixelFormat.Format32bppRgb:
                return ProcessImage32rgb(bmp, bHorz, bVert);
            case PixelFormat.Format32bppArgb:
                return ProcessImage32rgba(bmp, bHorz, bVert);
            case PixelFormat.Format32bppPArgb:
                return ProcessImage32prgba(bmp, bHorz, bVert);
            default:
                return FilterError.IncompatiblePixelFormat;
        }
    }

    public FilterError ProcessImage24rgb(
        DirectAccessBitmap bmp,
        bool horz, bool vert)
    {
        if (!horz && !vert) return FilterError.OK;

        int cx = bmp.Width;
        int cy = bmp.Height;
        int endX = cx + bmp.StartX;
        int endY = cy + bmp.StartY;
        int startX = bmp.StartX;
        int startY = bmp.StartY;
        int halfCx = cx / 2 + startX;
        int halfCy = cy / 2 + startY;
        byte[] data = bmp.Bits;
        int stride = bmp.Stride;
        int pos1, pos2;
        int x, y;
        int pos1flip, pos2flip;
        byte temp;
        for (y = bmp.StartY; y < endY; y++)
        {
            if (vert && y >= halfCy) break;

            pos1 = stride * y;
            pos1flip = stride * (vert ? (endY - y + startY - 1) : y);

            for (x = bmp.StartX; x < endX; x++)
            {
                if (!vert && x >= halfCx) break;

                pos2 = pos1 + x * 3;
                pos2flip = pos1flip + (horz ? (endX - x + startX - 1) : x) * 3;

                temp = data[pos2];
                data[pos2] = data[pos2flip];
                data[pos2flip] = temp;
                temp = data[pos2 + 1];
                data[pos2 + 1] = data[pos2flip + 1];
                data[pos2flip + 1] = temp;
                temp = data[pos2 + 2];
                data[pos2 + 2] = data[pos2flip + 2];
                data[pos2flip + 2] = temp;
            }
        }
        return FilterError.OK;
    }

    public FilterError ProcessImage32rgb(
        DirectAccessBitmap bmp,
        bool horz, bool vert)
    {
        if (!horz && !vert) return FilterError.OK;

        int cx = bmp.Width;
        int cy = bmp.Height;
        int endX = cx + bmp.StartX;
        int endY = cy + bmp.StartY;
        int startX = bmp.StartX;
        int startY = bmp.StartY;
        int halfCx = cx / 2 + startX;
        int halfCy = cy / 2 + startY;
        byte[] data = bmp.Bits;
        int stride = bmp.Stride;
        int pos1, pos2;
        int x, y;
        int pos1flip, pos2flip;
        byte temp;
        for (y = bmp.StartY; y < endY; y++)
        {
            if (vert && y >= halfCy) break;

            pos1 = stride * y;
            pos1flip = stride * (vert ? (endY - y + startY - 1) : y);
            for (x = bmp.StartX; x < endX; x++)
            {
                if (!vert && x >= halfCx) break;

                pos2 = pos1 + x * 4;
                pos2flip = pos1flip + (horz ? (endX - x + startX - 1) : x) * 4;

                temp = data[pos2];
                data[pos2] = data[pos2flip];
                data[pos2flip] = temp;
                temp = data[pos2 + 1];
                data[pos2 + 1] = data[pos2flip + 1];
                data[pos2flip + 1] = temp;
                temp = data[pos2 + 2];
                data[pos2 + 2] = data[pos2flip + 2];
                data[pos2flip + 2] = temp;
                temp = data[pos2 + 3];
                data[pos2 + 3] = data[pos2flip + 3];
                data[pos2flip + 3] = temp;
            }
        }
        return FilterError.OK;
    }

    public FilterError ProcessImage32rgba(
        DirectAccessBitmap bmp,
        bool horz, bool vert)
    {
        return ProcessImage32rgb(bmp, horz, vert);
    }

    public FilterError ProcessImage32prgba(
        DirectAccessBitmap bmp,
        bool horz, bool vert)
    {
        return ProcessImage32rgb(bmp, horz, vert);
   }
}
