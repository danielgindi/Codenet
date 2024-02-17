using System;
using System.Drawing;

namespace Codenet.Drawing;

public static class SizeHelper
{
    public static Size CalculateBounds(
        Int32 width, Int32 height,
        Int32 boundsX, Int32 boundsY,
        Boolean outsideBox = false,
        Boolean allowEnlarge = false,
        Boolean allowShrink = true)
    {
        Int32 newwidth, newheight;
        if ((width == boundsX && height == boundsY) ||
            ((width < boundsX && height < boundsY) && !allowEnlarge) ||
            ((width > boundsX && height > boundsY) && !allowShrink) ||
            ((width > boundsX || height > boundsY) && !outsideBox && !allowShrink))
        {
            newwidth = width;
            newheight = height;
        }
        else
        {
            Decimal aspectOriginal = (height == 0) ? 1m : ((Decimal)width / (Decimal)height);
            Decimal aspectNew = (boundsY == 0) ? 1m : ((Decimal)boundsX / (Decimal)boundsY);

            if ((aspectNew > aspectOriginal && !outsideBox) ||
                    (aspectNew < aspectOriginal && outsideBox))
            {
                newheight = boundsY;
                newwidth = Decimal.ToInt32(Decimal.Floor(((Decimal)boundsY) * aspectOriginal));
            }
            else if ((aspectNew > aspectOriginal && outsideBox) ||
                             (aspectNew < aspectOriginal && !outsideBox))
            {
                newwidth = boundsX;
                newheight = Decimal.ToInt32(Decimal.Floor(((Decimal)boundsX) / aspectOriginal));
            }
            else // aspectNew==aspectOriginal
            {
                newwidth = boundsX;
                newheight = boundsY;
            }
        }

        return new Size(newwidth, newheight);
    }
}
