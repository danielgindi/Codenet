using System;

namespace Codenet.Drawing.ImageProcessing
{
    [Flags]
    public enum Corner
    {
        None = 0,
        TopLeft = 0x01,
        TopRight = 0x02,
        BottomRight = 0x04,
        BottomLeft = 0x08,
        AllCorners = TopLeft | TopRight | BottomRight | BottomLeft
    }
}
