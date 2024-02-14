using System;
using System.Collections.Generic;
using Codenet.Drawing.Common;

namespace Codenet.Drawing.Quantizers.ColorCaches.Octree
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public class OctreeCacheNode
    {
        private static readonly Byte[] Mask = new Byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        private readonly OctreeCacheNode[] nodes;
        private readonly Dictionary<Int32, NeatColor> entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="OctreeCacheNode"/> class.
        /// </summary>
        public OctreeCacheNode()
        {
            nodes = new OctreeCacheNode[8];
            entries = new Dictionary<Int32, NeatColor>();
        }
        
        /// <summary>
        /// Adds the color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="paletteIndex">Index of the palette.</param>
        /// <param name="level">The level.</param>
        public void AddColor(NeatColor color, Int32 paletteIndex, Int32 level)
        {
            // if this node is a leaf, then increase a color amount, and pixel presence
            entries.Add(paletteIndex, color);
            
            if (level < 8) // otherwise goes one level deeper
            {
                // calculates an index for the next sub-branch
                Int32 index = GetColorIndexAtLevel(color, level);

                // if that branch doesn't exist, grows it
                if (nodes[index] == null)
                {
                    nodes[index] = new OctreeCacheNode();
                }

                // adds a color to that branch
                nodes[index].AddColor(color, paletteIndex, level + 1);
            }
        }

        /// <summary>
        /// Gets the index of the palette.
        /// </summary>
        public Dictionary<Int32, NeatColor> GetPaletteIndex(NeatColor color, Int32 level)
        {
            Dictionary<Int32, NeatColor> result = entries;
            
            if (level < 8)
            {
                Int32 index = GetColorIndexAtLevel(color, level);

                if (nodes[index] != null)
                {
                    result = nodes[index].GetPaletteIndex(color, level + 1);
                }
            }

            return result;
        }

        private static Int32 GetColorIndexAtLevel(NeatColor color, Int32 level)
        {
            return ((color.Red & Mask[level]) == Mask[level] ? 4 : 0) |
                   ((color.Green & Mask[level]) == Mask[level] ? 2 : 0) |
                   ((color.Blue & Mask[level]) == Mask[level] ? 1 : 0);
        }
    }
}
