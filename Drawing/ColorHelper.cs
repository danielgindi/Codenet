using System;
using System.Drawing;
using System.Globalization;

namespace Codenet.Drawing;

public static class ColorHelper
{
    private static CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    public static string GetCss(Color color, bool hash = true, bool allowAlpha = true, bool forceAlpha = false)
    {
        if (color == Color.Transparent) return "transparent";
        if (color == Color.Empty) return "";
        if ((allowAlpha && color.A < 255) || forceAlpha)
        {
            if (hash) return $"#{color.R:x2}{color.G:x2}{color.B:x2}{color.A:x2}";
            else return $"{color.R:x2}{color.G:x2}{color.B:x2}{color.A:x2}";
        }
        else
        {
            if (hash) return $"#{color.R:x2}{color.G:x2}{color.B:x2}";
            else return $"{color.R:x2}{color.G:x2}{color.B:x2}";
        }
    }

    public static string GetCssRgb(Color color, bool allowAlpha = true, bool forceAlpha = false)
    {
        if (color == Color.Transparent) return "transparent";
        if (color == Color.Empty) return "";
        if ((allowAlpha && color.A < 255) || forceAlpha)
        {
            return $@"rgba({color.R},{color.G},{color.B},{color.A / 255.0:0.##})";
        }
        else
        {
            return $"rgb({color.R},{color.G},{color.B})";
        }
    }

    public static Color FromCss(string css)
    {
        css = css.Trim();

        if (css.StartsWith("rgba("))
        {
            css = css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
            string[] rgba = css.Split(',');
            if (rgba.Length == 4)
            {
                try
                {
                    return Color.FromArgb(
                        (int)(Convert.ToDecimal(rgba[3].Trim(), InvariantCulture) * 255),
                        Convert.ToInt32(rgba[0].Trim(), InvariantCulture),
                        Convert.ToInt32(rgba[1].Trim(), InvariantCulture),
                        Convert.ToInt32(rgba[2].Trim(), InvariantCulture)
                        );
                }
                catch { }
            }
            return Color.Empty;
        }
        else if (css.StartsWith("argb("))
        {
            css = css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
            string[] rgba = css.Split(',');
            if (rgba.Length == 4)
            {
                try
                {
                    return Color.FromArgb(
                        (int)(Convert.ToDecimal(rgba[0].Trim(), InvariantCulture) * 255),
                        Convert.ToInt32(rgba[1].Trim(), InvariantCulture),
                        Convert.ToInt32(rgba[2].Trim(), InvariantCulture),
                        Convert.ToInt32(rgba[3].Trim(), InvariantCulture)
                        );
                }
                catch { }
            }
            return Color.Empty;
        }
        else if (css.StartsWith("rgb("))
        {
            css = css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
            string[] rgba = css.Split(',');
            if (rgba.Length == 3)
            {
                try
                {
                    return Color.FromArgb(
                          Convert.ToInt32(rgba[0].Trim(), InvariantCulture),
                          Convert.ToInt32(rgba[1].Trim(), InvariantCulture),
                          Convert.ToInt32(rgba[2].Trim(), InvariantCulture)
                          );
                }
                catch { }
            }
            return Color.Empty;
        }
        else if (css.Equals("transparent"))
        {
            return Color.Transparent;
        }
        else
        {
            css = css.Trim('#', ' ', ';');
            if (css.Length == 3) css = "" + (char)css[0] + (char)css[0] + (char)css[1] + (char)css[1] + (char)css[2] + (char)css[2];
            if (css.Length == 6)
            {
                try
                {
                    return Color.FromArgb(
                        Int32.Parse(css.Substring(0, 2), NumberStyles.HexNumber, InvariantCulture),
                        Int32.Parse(css.Substring(2, 2), NumberStyles.HexNumber, InvariantCulture),
                        Int32.Parse(css.Substring(4, 2), NumberStyles.HexNumber, InvariantCulture)
                        );
                }
                catch
                {
                    return Color.Empty;
                }
            }
            else if (css.Length == 8)
            {
                try
                {
                    return Color.FromArgb(
                        Int32.Parse(css.Substring(6, 2), NumberStyles.HexNumber, InvariantCulture),
                        Int32.Parse(css.Substring(0, 2), NumberStyles.HexNumber, InvariantCulture),
                        Int32.Parse(css.Substring(2, 2), NumberStyles.HexNumber, InvariantCulture),
                        Int32.Parse(css.Substring(4, 2), NumberStyles.HexNumber, InvariantCulture)
                        );
                }
                catch
                {
                    return Color.Empty;
                }
            }

            return Color.Empty;
        }
    }
}
