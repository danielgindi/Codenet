using System;
using System.Collections.Generic;

namespace Codenet.Drawing.Common.Helpers;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public class ColorModelHelper
{
    #region Constants

    private const int X = 0;
    private const int Y = 1;
    private const int Z = 2;

    private const float Epsilon = 1E-05f;
    private const float OneThird = 1.0f / 3.0f;
    private const float TwoThirds = 2.0f * OneThird;
    public const Double HueFactor = 1.4117647058823529411764705882353;

    private static readonly float[] XYZWhite = new[] { 95.05f, 100.00f, 108.90f };

    #endregion

    #region -> RGB

    private static UInt32 GetColorComponent(Single v1, Single v2, Single hue)
    {
        Single preresult;

        if (hue < 0.0f) hue++;
        if (hue > 1.0f) hue--;

        if ((6.0f * hue) < 1.0f)
        {
            preresult = v1 + (((v2 - v1) * 6.0f) * hue);
        }
        else if ((2.0f * hue) < 1.0f)
        {
            preresult = v2;
        }
        else if ((3.0f * hue) < 2.0f)
        {
            preresult = v1 + (((v2 - v1) * (TwoThirds - hue)) * 6.0f);
        }
        else
        {
            preresult = v1;
        }

        return Convert.ToUInt32(255.0f * preresult);
    }

    public static NeatColor HSBtoRGB(Single hue, Single saturation, Single brightness)
    {
        // initializes the default black
        UInt32 red = 0;
        UInt32 green = 0;
        UInt32 blue = 0;

        // only if there is some brightness; otherwise leave it pitch black
        if (brightness > 0.0f)
        {
            // if there is no saturation; leave it gray based on the brightness only
            if (Math.Abs(saturation - 0.0f) < Epsilon)
            {
                red = green = blue = Convert.ToUInt32(255.0f * brightness);
            }
            else // the color is more complex
            {
                // converts HSL cylinder to one slice (its factors)
                Single factorHue = hue / 360.0f;
                Single factorA = brightness < 0.5f ? brightness * (1.0f + saturation) : (brightness + saturation) - (brightness * saturation);
                Single factorB = (2.0f * brightness) - factorA;

                // maps HSL slice to a RGB cube
                red = GetColorComponent(factorB, factorA, factorHue + OneThird);
                green = GetColorComponent(factorB, factorA, factorHue);
                blue = GetColorComponent(factorB, factorA, factorHue - OneThird);
            }
        }

        UInt32 argb = 255u << 24 | red << 16 | green << 8 | blue;
        return new NeatColor(argb);
    }

    #endregion

    #region RGB ->

    public static void RGBtoLab(Int32 red, Int32 green, Int32 blue, out Single l, out Single a, out Single b)
    {
        Single x, y, z;
        RGBtoXYZ(red, green, blue, out x, out y, out z);
        XYZtoLab(x, y, z, out l, out a, out b);
    }

    public static void RGBtoXYZ(Int32 red, Int32 green, Int32 blue, out Single x, out Single y, out Single z)
    {
        // normalize red, green, blue values
        Double redFactor = red / 255.0;
        Double greenFactor = green / 255.0;
        Double blueFactor = blue / 255.0;

        // convert to a sRGB form
        Double sRed = (redFactor > 0.04045) ? Math.Pow((redFactor + 0.055) / (1 + 0.055), 2.2) : (redFactor / 12.92);
        Double sGreen = (greenFactor > 0.04045) ? Math.Pow((greenFactor + 0.055) / (1 + 0.055), 2.2) : (greenFactor / 12.92);
        Double sBlue = (blueFactor > 0.04045) ? Math.Pow((blueFactor + 0.055) / (1 + 0.055), 2.2) : (blueFactor / 12.92);

        // converts
        x = Convert.ToSingle(sRed * 0.4124 + sGreen * 0.3576 + sBlue * 0.1805);
        y = Convert.ToSingle(sRed * 0.2126 + sGreen * 0.7152 + sBlue * 0.0722);
        z = Convert.ToSingle(sRed * 0.0193 + sGreen * 0.1192 + sBlue * 0.9505);
    }

    #endregion

    #region XYZ ->

    private static Single GetXYZValue(Single value)
    {
        return value > 0.008856f ? (Single)Math.Pow(value, OneThird) : (7.787f * value + 16.0f / 116.0f);
    }

    public static void XYZtoLab(Single x, Single y, Single z, out Single l, out Single a, out Single b)
    {
        l = 116.0f * GetXYZValue(y / XYZWhite[Y]) - 16.0f;
        a = 500.0f * (GetXYZValue(x / XYZWhite[X]) - GetXYZValue(y / XYZWhite[Y]));
        b = 200.0f * (GetXYZValue(y / XYZWhite[Y]) - GetXYZValue(z / XYZWhite[Z]));
    }

    #endregion

    #region Methods

    public static Int64 GetColorEuclideanDistance(ColorModel colorModel, NeatColor requestedColor, NeatColor realColor)
    {
        Single componentA, componentB, componentC;
        GetColorComponents(colorModel, requestedColor, realColor, out componentA, out componentB, out componentC);
        return (Int64) (componentA * componentA + componentB * componentB + componentC * componentC);
    }

    public static Int32 GetEuclideanDistance(NeatColor color, ColorModel colorModel, IList<NeatColor> palette)
    {
        // initializes the best difference, set it for worst possible, it can only get better
        Int64 leastDistance = Int64.MaxValue;
        Int32 result = 0;

        for (Int32 index = 0; index < palette.Count; index++)
        {
            NeatColor targetColor = palette[index];
            Int64 distance = GetColorEuclideanDistance(colorModel, color, targetColor);

            // if a difference is zero, we're good because it won't get better
            if (distance == 0)
            {
                result = index;
                break;
            }

            // if a difference is the best so far, stores it as our best candidate
            if (distance < leastDistance)
            {
                leastDistance = distance;
                result = index;
            }
        }

        return result;
    }

    public static Int32 GetComponentA(ColorModel colorModel, NeatColor color)
    {
        Int32 result = 0;

        switch (colorModel)
        {
            case ColorModel.RedGreenBlue:
                result = color.Red;
                break;

            case ColorModel.HueSaturationLuminance:
                {
                    color.ToHsv(out var h, out var s, out var v);
                    result = Convert.ToInt32(h / HueFactor);
                }
                break;

            case ColorModel.LabColorSpace:
                Single l, a, b;
                RGBtoLab(color.Red, color.Green, color.Blue, out l, out a, out b);
                result = Convert.ToInt32(l*255.0f);
                break;
        }

        return result;
    }

    public static Int32 GetComponentB(ColorModel colorModel, NeatColor color)
    {
        Int32 result = 0;

        switch (colorModel)
        {
            case ColorModel.RedGreenBlue:
                result = color.Green;
                break;

            case ColorModel.HueSaturationLuminance:
                {
                    color.ToHsv(out var h, out var s, out var v);
                    result = Convert.ToInt32(s * 255);
                }
                break;

            case ColorModel.LabColorSpace:
                Single l, a, b;
                RGBtoLab(color.Red, color.Green, color.Blue, out l, out a, out b);
                result = Convert.ToInt32(a*255.0f);
                break;
        }

        return result;
    }

    public static Int32 GetComponentC(ColorModel colorModel, NeatColor color)
    {
        Int32 result = 0;

        switch (colorModel)
        {
            case ColorModel.RedGreenBlue:
                result = color.Blue;
                break;

            case ColorModel.HueSaturationLuminance:
                {
                    color.ToHsv(out var h, out var s, out var v);
                    result = Convert.ToInt32(v * 255);
                }
                break;

            case ColorModel.LabColorSpace:
                Single l, a, b;
                RGBtoLab(color.Red, color.Green, color.Blue, out l, out a, out b);
                result = Convert.ToInt32(b*255.0f);
                break;
        }

        return result;
    }
    
    public static void GetColorComponents(ColorModel colorModel, NeatColor color, out Single componentA, out Single componentB, out Single componentC)
    {
        componentA = 0.0f;
        componentB = 0.0f;
        componentC = 0.0f;

        switch (colorModel)
        {
            case ColorModel.RedGreenBlue:
                componentA = color.Red;
                componentB = color.Green;
                componentC = color.Blue;
                break;

            case ColorModel.HueSaturationLuminance:
                {
                    color.ToHsv(out var h, out var s, out var v);
                    componentA = h;
                    componentB = s;
                    componentC = v;
                }
                break;

            case ColorModel.LabColorSpace:
                RGBtoLab(color.Red, color.Green, color.Blue, out componentA, out componentB, out componentC);
                break;

            case ColorModel.XYZ:
                RGBtoXYZ(color.Red, color.Green, color.Blue, out componentA, out componentB, out componentC);
                break;
        }
    }

    public static void GetColorComponents(ColorModel colorModel, NeatColor color, NeatColor targetColor, out Single componentA, out Single componentB, out Single componentC)
    {
        componentA = 0.0f;
        componentB = 0.0f;
        componentC = 0.0f;

        switch (colorModel)
        {
            case ColorModel.RedGreenBlue:
                componentA = color.Red - targetColor.Red;
                componentB = color.Green - targetColor.Green;
                componentC = color.Blue - targetColor.Blue;
                break;

            case ColorModel.HueSaturationLuminance:
                {
                    color.ToHsv(out var h, out var s, out var v);
                    targetColor.ToHsv(out var th, out var ts, out var tv);
                    componentA = h - th;
                    componentB = s - ts;
                    componentC = v - tv;
                }
                break;

            case ColorModel.LabColorSpace:

                Single sourceL, sourceA, sourceB;
                Single targetL, targetA, targetB;

                RGBtoLab(color.Red, color.Green, color.Blue, out sourceL, out sourceA, out sourceB);
                RGBtoLab(targetColor.Red, targetColor.Green, targetColor.Blue, out targetL, out targetA, out targetB);

                componentA = sourceL - targetL;
                componentB = sourceA - targetA;
                componentC = sourceB - targetB;

                break;

            case ColorModel.XYZ:

                Single sourceX, sourceY, sourceZ;
                Single targetX, targetY, targetZ;

                RGBtoXYZ(color.Red, color.Green, color.Blue, out sourceX, out sourceY, out sourceZ);
                RGBtoXYZ(targetColor.Red, targetColor.Green, targetColor.Blue, out targetX, out targetY, out targetZ);

                componentA = sourceX - targetX;
                componentB = sourceY - targetY;
                componentC = sourceZ - targetZ;

                break;
        }
    }

    public static void GetColorRGB(ColorModel colorModel, NeatColor color, out Int32 outR, out Int32 outG, out Int32 outB)
    {
        outR = 0;
        outG = 0;
        outB = 0;

        switch (colorModel)
        {
            case ColorModel.RedGreenBlue:
                outR = color.Red;
                outG = color.Green;
                outB = color.Blue;
                break;

            case ColorModel.HueSaturationLuminance:
                {
                    color.ToHsv(out var h, out var s, out var v);
                    outR = Convert.ToInt32(h / HueFactor);
                    outG = Convert.ToInt32(s * 255);
                    outB = Convert.ToInt32(v * 255);
                }
                break;

            case ColorModel.LabColorSpace:
                {
                    Single l, a, b;
                    RGBtoLab(color.Red, color.Green, color.Blue, out l, out a, out b);
                    outR = Convert.ToInt32(l * 255.0f);
                    outG = Convert.ToInt32(a * 255.0f);
                    outB = Convert.ToInt32(b * 255.0f);
                }
                break;

            case ColorModel.XYZ:
                {
                    Single x, y, z;
                    RGBtoXYZ(color.Red, color.Green, color.Blue, out x, out y, out z);
                    outR = Convert.ToInt32(x * 255.0f);
                    outG = Convert.ToInt32(y * 255.0f);
                    outB = Convert.ToInt32(z * 255.0f);
                }
                break;
        }
    }

    #endregion
}