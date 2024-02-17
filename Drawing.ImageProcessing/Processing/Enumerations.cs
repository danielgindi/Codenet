using System;

namespace Codenet.Drawing.ImageProcessing.Processing;

public enum FilterError
{
    None = 0,
    OK = 0,
    UnknownError = 1,
    IncompatiblePixelFormat = 2,
    InvalidArgument = 3,
    MissingArgument = 4
}

[Flags]
public enum FilterColorChannel
{
    None = 0,
    Alpha = 1,
    Red = 2,
    Green = 4,
    Blue = 8,
    Gray = 16,
    RGB = Red | Green | Blue,
    ARGB = Alpha | RGB
}

public enum FilterGrayScaleWeight
{
    None = 0,
    Simple = None,
    Natural = 1,
    NaturalNTSC = 2,
    Css = 3
}
