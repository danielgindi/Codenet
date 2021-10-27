using System;
using System.Collections.Generic;
using System.Drawing;

namespace Codenet.Drawing.ImageProcessing.Quantizers.PathProviders
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public class ReversedPathProvider : IPathProvider
    {
        public IList<Point> GetPointPath(Int32 width, Int32 height)
        {
            List<Point> result = new List<Point>(width*height);

            for (Int32 y = height - 1; y >= 0; y--)
            for (Int32 x = width - 1; x >= 0; x--)
            {
                Point point = new Point(x, y);
                result.Add(point);
            }

            return result;
        }
    }
}
