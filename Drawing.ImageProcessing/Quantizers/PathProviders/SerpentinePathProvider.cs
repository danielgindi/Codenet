using System;
using System.Collections.Generic;
using System.Drawing;

namespace Codenet.Drawing.ImageProcessing.Quantizers.PathProviders
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public class SerpentinePathProvider : IPathProvider
    {
        public IList<Point> GetPointPath(Int32 width, Int32 height)
        {
            Boolean leftToRight = true;
            List<Point> result = new List<Point>(width * height);

            for (Int32 y = 0; y < height; y++)
            {
                for (Int32 x = leftToRight ? 0 : width - 1; leftToRight ? x < width : x >= 0; x += leftToRight ? 1 : -1)
                {
                    Point point = new Point(x, y);
                    result.Add(point);
                }

                leftToRight = !leftToRight;
            }

            return result;
        }
    }
}
