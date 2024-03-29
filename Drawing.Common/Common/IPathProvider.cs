﻿using System.Collections.Generic;

namespace Codenet.Drawing.Common;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public interface IPathProvider
{
    /// <summary>
    /// Retrieves the path throughout the image to determine the order in which pixels will be scanned.
    /// </summary>
    IList<System.Drawing.Point> GetPointPath(int width, int height);
}
