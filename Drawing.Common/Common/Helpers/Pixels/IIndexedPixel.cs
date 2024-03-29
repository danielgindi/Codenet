﻿using System;

namespace Codenet.Drawing.Common.Helpers.Pixels;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
internal interface IIndexedPixel
{
    // index methods
    Byte GetIndex(Int32 offset);
    void SetIndex(Int32 offset, Byte value);
}
