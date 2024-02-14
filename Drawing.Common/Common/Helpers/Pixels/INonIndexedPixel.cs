using System;

namespace Codenet.Drawing.Common.Helpers.Pixels
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    internal interface INonIndexedPixel
    {
        // components
        Int32 Alpha { get; }
        Int32 Red { get; }
        Int32 Green { get; }
        Int32 Blue { get; }

        // higher-level values
        UInt32 Argb { get; }
        UInt64 Value { get; set; }

        // color methods
        NeatColor GetColor();
        void SetColor(NeatColor color);
    }
}
