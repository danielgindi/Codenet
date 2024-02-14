using System;
using System.Collections.Generic;

namespace Codenet.Drawing.Common.PathProviders
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public class StandardPathProvider : IPathProvider
    {
        public IList<System.Drawing.Point> GetPointPath(Int32 width, Int32 height)
        {
            var result = new List<System.Drawing.Point>(width * height);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    result.Add(new System.Drawing.Point(x, y));
                }
            }

            return result;
        }
    }
}
