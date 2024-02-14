using System;

namespace Codenet.Drawing.Common;

public readonly struct NeatColor : IEquatable<NeatColor>
{
    /// <summary>
    /// Zeroed out color
    /// </summary>
    public static readonly NeatColor Empty;


    private readonly UInt32 _value;

    /// <summary>
    /// Initializes from a raw ARGB value
    /// </summary>
    /// <param name="argb">Sample structure: 0xAARRGGBB</param>
    public NeatColor(UInt32 argb)
    {
        _value = argb;
    }

    public NeatColor(byte alpha, byte red, byte green, byte blue)
    {
        _value = (UInt32)(alpha << 24 | red << 16 | green << 8 | blue);
    }

    public NeatColor(byte red, byte green, byte blue)
    {
        _value = 0xFF000000u | (UInt32)(red << 16) | (UInt32)(green << 8) | blue;
    }

    public static NeatColor FromARGB(byte alpha, byte red, byte green, byte blue)
    {
        return new NeatColor(alpha, red, green, blue);
    }

    /// <summary>
    /// Creates a new <see cref="NeatColor"/> from a raw ARGB value
    /// </summary>
    /// <param name="argb">Sample structure: 0xAARRGGBB</param>
    public static NeatColor FromARGB(UInt32 argb)
    {
        return new NeatColor(argb);
    }

    /// <summary>
    /// ARGB (raw) value of the color
    /// </summary>
    public UInt32 ARGB => _value;

    /// <summary>
    /// Alpha component of the color
    /// </summary>
    public byte Alpha => (byte)(_value >> 24 & 0xFFu);

    /// <summary>
    /// Red component of the color
    /// </summary>
    public byte Red => (byte)(_value >> 16 & 0xFFu);

    /// <summary>
    /// Green component of the color
    /// </summary>
    public byte Green => (byte)(_value >> 8 & 0xFFu);

    /// <summary>
    /// Blue component of the color
    /// </summary>
    public byte Blue => (byte)(_value & 0xFFu);

    /// <summary>
    /// Returns a copy of this color with the alpha component changed
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public NeatColor WithAlpha(byte alpha)
    {
        return new NeatColor(alpha, Red, Green, Blue);
    }

    public void ToHsv(out float h, out float s, out float v)
    {
        float fR = Red / 255f;
        float fG = Green / 255f;
        float fB = Blue / 255f;

        float min = Math.Min(Math.Min(fR, fG), fB);
        float max = Math.Max(Math.Max(fR, fG), fB);
        float delta = max - min;

        h = 0f;
        s = 0f;
        v = max;

        if (Math.Abs(delta) > 0.001f)
        {
            s = delta / max;
            float num1 = ((max - fR) / 6f + delta / 2f) / delta;
            float num2 = ((max - fG) / 6f + delta / 2f) / delta;
            float num3 = ((max - fB) / 6f + delta / 2f) / delta;

            if (Math.Abs(fR - max) < 0.001f)
                h = num3 - num2;
            else if (Math.Abs(fG - max) < 0.001f)
                h = 0.333333343f + num1 - num3;
            else
                h = 2f / 3f + num2 - num1;

            if (h < 0f)
            {
                h += 1f;
            }

            if (h > 1f)
            {
                h -= 1f;
            }
        }

        h *= 360f;
        s *= 100f;
        v *= 100f;
    }

    /// <summary>
    /// Returns a #AARRGGBB string representation of this color
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";
    }

    public bool Equals(NeatColor obj)
    {
        return obj._value == _value;
    }

    public override bool Equals(object other)
    {
        if (other is NeatColor)
        {
            NeatColor obj = (NeatColor)other;
            return Equals(obj);
        }

        return false;
    }

    public static bool operator ==(NeatColor left, NeatColor right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(NeatColor left, NeatColor right)
    {
        return !left.Equals(right);
    }
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}
