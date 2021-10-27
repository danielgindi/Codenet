using System;
using System.Drawing;

namespace Codenet.Drawing.ImageProcessing
{
    public static class MarginsCropper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="color"></param>
        /// <param name="colorDistanceAllowed"></param>
        /// <returns>Image after cropping, or null if there's no work to do.
        /// If result is zero width or zero height, returns null
        /// </returns>
        public static Image CropMargins(Bitmap image, Color color, double colorDistanceAllowed = 0.0)
        {
            if (image == null) return null;
            if (color == null || color == Color.Empty) color = image.GetPixel(0, 0);

            colorDistanceAllowed = Math.Abs(colorDistanceAllowed);
            bool bCheckDistance = colorDistanceAllowed != 0;

            bool bGoOn;
            int iLinesTop = 0, iLinesBottom = 0;
            int iLinesLeft = 0, iLinesRight = 0;

            bGoOn = true;
            for (int y = 0, x; y < image.Height; y++)
            {
                for (x = 0; x < image.Width; x++)
                {
                    if (image.GetPixel(x, y) != color &&
                        (!bCheckDistance ||
                            GetColorDistance(image.GetPixel(x, y), color) > colorDistanceAllowed)
                        )
                    {
                        bGoOn = false; 
                        break;
                    }
                }
                if (bGoOn)
                {
                    iLinesTop++;
                }
                else break;
            }

            bGoOn = true;
            for (int y = image.Height - 1, x; y >= 0; y--)
            {
                for (x = 0; x < image.Width; x++)
                {
                    if (image.GetPixel(x, y) != color &&
                        (!bCheckDistance ||
                            GetColorDistance(image.GetPixel(x, y), color) > colorDistanceAllowed)
                        ) 
                    { 
                        bGoOn = false; 
                        break;
                    }
                }
                if (bGoOn) 
                {
                    iLinesBottom++;
                } 
                else break;
            }

            bGoOn = true;
            for (int x = 0, y; x < image.Width; x++)
            {
                for (y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y) != color &&
                        (!bCheckDistance ||
                            GetColorDistance(image.GetPixel(x, y), color) > colorDistanceAllowed)
                        )
                    {
                        bGoOn = false; 
                        break; 
                    }
                }
                if (bGoOn)
                {
                    iLinesLeft++;
                }
                else break;
            }

            bGoOn = true;
            for (int x = image.Width - 1, y; x >= 0; x--)
            {
                for (y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y) != color &&
                        (!bCheckDistance ||
                            GetColorDistance(image.GetPixel(x, y), color) > colorDistanceAllowed)
                        )
                    { 
                        bGoOn = false;
                        break;
                    }
                }
                if (bGoOn)
                {
                    iLinesRight++;
                }
                else break;
            }

            int iNewWidth = image.Width - iLinesLeft - iLinesRight;
            int iNewHeight = image.Height - iLinesTop - iLinesBottom;

            if (iNewWidth <= 0 || iNewHeight <= 0 || (iLinesLeft == 0 &&
                iLinesRight == 0 && iLinesTop == 0 && iLinesBottom == 0))
            {
                return null;
            }

            Image retImg = new Bitmap(iNewWidth, iNewHeight);
            using (var g = Graphics.FromImage(retImg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(image, -iLinesLeft, -iLinesTop, image.Width, image.Height);
            }

            return retImg;
        }

        public static Boolean CropMarginsAndSave(
            String imgPath,
            String destPath, 
            Color colorCrop, 
            double colorDistanceAllowed,
            ProcessingHelper.EncodingOptions encodingOptions)
        {
            if (imgPath == null) return false;
            if (destPath == null) destPath = imgPath;
            
            using (var imgSrc = Image.FromFile(imgPath))
            {
                ProcessingHelper.ProcessImageFramesToFile(imgSrc, destPath, null, encodingOptions, image =>
                {
                    if (image is Bitmap)
                    {
                        return CropMargins(image as Bitmap, colorCrop, colorDistanceAllowed);
                    }
                    else
                    {
                        using (var bmp = new Bitmap(imgSrc))
                        {
                            return CropMargins(bmp, colorCrop, colorDistanceAllowed);
                        }
                    }
                });
            }

            return true;
        }

        public static double GetColorDistance(Color c1, Color c2)
        {
            double a = Math.Pow(Convert.ToDouble(c1.A - c2.A), 2.0);
            double r = Math.Pow(Convert.ToDouble(c1.R - c2.R), 2.0);
            double g = Math.Pow(Convert.ToDouble(c1.G - c2.G), 2.0);
            double b = Math.Pow(Convert.ToDouble(c1.B - c2.B), 2.0);

            return Math.Sqrt(a + r + g + b);
        }
    }
}
