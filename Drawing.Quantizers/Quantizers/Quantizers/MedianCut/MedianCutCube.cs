using System;
using System.Collections.Generic;
using Codenet.Drawing.Common;
using Codenet.Drawing.Common.Helpers;

namespace Codenet.Drawing.Quantizers.MedianCut
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    internal class MedianCutCube
    {
        #region Fields

        // red bounds
        private Int32 redLowBound;
        private Int32 redHighBound;

        // green bounds
        private Int32 greenLowBound;
        private Int32 greenHighBound;

        // blue bounds
        private Int32 blueLowBound;
        private Int32 blueHighBound;

        private readonly ICollection<UInt32> colorList;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the color model.
        /// </summary>
        public ColorModel ColorModel { get; private set; }

        /// <summary>
        /// Gets or sets the index of the palette.
        /// </summary>
        /// <value>The index of the palette.</value>
        public Int32 PaletteIndex { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MedianCutCube"/> class.
        /// </summary>
        /// <param name="colors">The colors.</param>
        public MedianCutCube(ICollection<UInt32> colors)
        {
            ColorModel = ColorModel.RedGreenBlue;
            colorList = colors;
            Shrink();
        }

        #endregion

        #region Calculated properties

        /// <summary>
        /// Gets the size of the red side of this cube.
        /// </summary>
        /// <value>The size of the red side of this cube.</value>
        public Int32 RedSize
        {
            get { return redHighBound - redLowBound; }
        }

        /// <summary>
        /// Gets the size of the green side of this cube.
        /// </summary>
        /// <value>The size of the green side of this cube.</value>
        public Int32 GreenSize
        {
            get { return greenHighBound - greenLowBound; }
        }

        /// <summary>
        /// Gets the size of the blue side of this cube.
        /// </summary>
        /// <value>The size of the blue side of this cube.</value>
        public Int32 BlueSize
        {
            get { return blueHighBound - blueLowBound; }
        }

        /// <summary>
        /// Gets the average color from the colors contained in this cube.
        /// </summary>
        /// <value>The average color.</value>
        public NeatColor Color
        {
            get
            {
                Int32 red = 0, green = 0, blue = 0;

                foreach (UInt32 argb in colorList)
                {
                    NeatColor color = new NeatColor(argb);
                    ColorModelHelper.GetColorRGB(ColorModel, color, out Int32 cr, out Int32 cg, out Int32 cb);
                    red += cr;
                    green += cg;
                    blue += cb;
                }

                red = colorList.Count == 0 ? 0 : red / colorList.Count;
                green = colorList.Count == 0 ? 0 : green / colorList.Count;
                blue = colorList.Count == 0 ? 0 : blue / colorList.Count;

                // ColorModelHelper.HSBtoRGB(Convert.ToInt32(red/ColorModelHelper.HueFactor), green / 255.0f, blue / 255.0f);

                NeatColor result = NeatColor.FromARGB(255, (byte)red, (byte)green, (byte)blue);
                return result;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shrinks this cube to the least dimensions that covers all the colors in the RGB space.
        /// </summary>
        private void Shrink()
        {
            redLowBound = greenLowBound = blueLowBound = 255;
            redHighBound = greenHighBound = blueHighBound = 0;

            foreach (UInt32 argb in colorList)
            {
                NeatColor color = new NeatColor(argb);

                ColorModelHelper.GetColorRGB(ColorModel, color, out Int32 red, out Int32 green, out Int32 blue);

                if (red < redLowBound) redLowBound = red;
                if (red > redHighBound) redHighBound = red;
                if (green < greenLowBound) greenLowBound = green;
                if (green > greenHighBound) greenHighBound = green;
                if (blue < blueLowBound) blueLowBound = blue;
                if (blue > blueHighBound) blueHighBound = blue;
            }
        }

        /// <summary>
        /// Splits this cube's color list at median index, and returns two newly created cubes.
        /// </summary>
        /// <param name="componentIndex">Index of the component (red = 0, green = 1, blue = 2).</param>
        /// <param name="firstMedianCutCube">The first created cube.</param>
        /// <param name="secondMedianCutCube">The second created cube.</param>
        public void SplitAtMedian(Byte componentIndex, out MedianCutCube firstMedianCutCube, out MedianCutCube secondMedianCutCube)
        {
            List<UInt32> colors;

            switch (componentIndex)
            {
                // red colors
                case 0:
                    colors = new List<uint>(colorList);
                    colors.Sort((uint argb1, uint argb2) =>
                    {
                        return ColorModelHelper.GetComponentA(ColorModel, new NeatColor(argb1)).CompareTo(ColorModelHelper.GetComponentA(ColorModel, new NeatColor(argb2)));
                    });
                    break;

                // green colors
                case 1:
                    colors = new List<uint>(colorList);
                    colors.Sort((uint argb1, uint argb2) =>
                    {
                        return ColorModelHelper.GetComponentB(ColorModel, new NeatColor(argb1)).CompareTo(ColorModelHelper.GetComponentB(ColorModel, new NeatColor(argb2)));
                    });
                    break;

                // blue colors
                case 2:
                    colors = new List<uint>(colorList);
                    colors.Sort((uint argb1, uint argb2) =>
                    {
                        return ColorModelHelper.GetComponentC(ColorModel, new NeatColor(argb1)).CompareTo(ColorModelHelper.GetComponentC(ColorModel, new NeatColor(argb2)));
                    });
                    break;

                default:
                    throw new NotSupportedException("Only three color components are supported (R, G and B).");

            }

            // retrieves the median index (a half point)
            Int32 medianIndex = colorList.Count >> 1;

            // creates the two half-cubes
            firstMedianCutCube = new MedianCutCube(colors.GetRange(0, medianIndex));
            secondMedianCutCube = new MedianCutCube(colors.GetRange(medianIndex, colors.Count - medianIndex));
        }

        /// <summary>
        /// Assigns a palette index to this cube, to be later found by a GetPaletteIndex method.
        /// </summary>
        /// <param name="newPaletteIndex">The palette index to be assigned to this cube.</param>
        public void SetPaletteIndex(Int32 newPaletteIndex)
        {
            PaletteIndex = newPaletteIndex;
        }

        /// <summary>
        /// Determines whether the color is in the space of this cube.
        /// </summary>
        /// <param name="color">The color to be checked, if it's contained in this cube.</param>
        /// <returns>if true a color is in the space of this cube, otherwise returns false.</returns>
        public Boolean IsColorIn(NeatColor color)
        {
            ColorModelHelper.GetColorRGB(ColorModel, color, out Int32 red, out Int32 green, out Int32 blue);

            return (red >= redLowBound && red <= redHighBound) &&
                   (green >= greenLowBound && green <= greenHighBound) &&
                   (blue >= blueLowBound && blue <= blueHighBound);
        }

        #endregion
    }
}