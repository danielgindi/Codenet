using System;
using System.Collections.Generic;
using Codenet.Drawing.Common;

namespace Codenet.Drawing.Quantizers.PathProviders
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public class SerpentinePathProvider : IPathProvider
    {
        public IList<System.Drawing.Point> GetPointPath(Int32 width, Int32 height)
        {
            bool leftToRight = true;
            var result = new List<System.Drawing.Point>(width * height);

            for (var y = 0; y < height; y++)
            {
                for (var x = leftToRight ? 0 : width - 1; leftToRight ? x < width : x >= 0; x += leftToRight ? 1 : -1)
                {
                    result.Add(new System.Drawing.Point(x, y));
                }

                leftToRight = !leftToRight;
            }

            return result;
        }
    }
}
