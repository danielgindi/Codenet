﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Codenet.Drawing.ImageProcessing.Quantizers.Helpers;

namespace Codenet.Drawing.ImageProcessing.Quantizers.Octree
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// 
    /// The idea here is to build a tree structure containing always a maximum of K different 
    /// colors. If a further color is to be added to the tree structure, its color value has 
    /// to be merged with the most likely one that is already in the tree. The both values are 
    /// substituted by their mean. 
    ///
    /// The most important data structure are the nodes of the octree. Each inner node of the 
    /// octree contain a maximum of eight successors, the leave nodes keep information for the 
    /// color value (colorvalue), the color index (colorindex), and a counter (colorcount) for 
    /// the pixel that are already mapped to a particular leave. Because each of the red, green 
    /// and blue value is between 0 and 255 the maximum depth of the tree is eight. In level i 
    /// Bit i of RGB is used as selector for the successors. 
    ///
    /// The octree is constructed during reading the image that is to be quantized. Only that 
    /// parts of the octree are created that are really needed. Initially the first K values 
    /// are represented exactly (in level eight). When the number of leaves nodes (currentK) 
    /// exceeds K, the tree has to reduced. That would mean that leaves at the largest depth 
    /// are substituted by their predecessor.
    /// </summary>
    public class OctreeQuantizer : BaseColorQuantizer
    {
        #region | Fields |

        private OctreeNode root;
        private Int32 lastColorCount;
        private List<OctreeNode>[] levels;

        #endregion

        #region | Calculated properties |

        /// <summary>
        /// Gets the leaf nodes only (recursively).
        /// </summary>
        /// <value>All the tree leaves.</value>
        internal IEnumerable<OctreeNode> Leaves
        {
            get 
            {
                List<OctreeNode> leaves = new List<OctreeNode>();
                foreach (OctreeNode node in root.ActiveNodes)
                {
                    if (node.IsLeaf) leaves.Add(node);
                }
                return leaves;
            }
        }

        #endregion

        #region | Methods |

        /// <summary>
        /// Adds the node to a level node list.
        /// </summary>
        /// <param name="level">The depth level.</param>
        /// <param name="octreeNode">The octree node to be added.</param>
        internal void AddLevelNode(Int32 level, OctreeNode octreeNode)
        {
            levels[level].Add(octreeNode);
        }

        #endregion

        #region << BaseColorQuantizer >>

        /// <summary>
        /// See <see cref="BaseColorQuantizer.OnPrepare"/> for more details.
        /// </summary>
        protected override void OnPrepare(ImageBuffer image)
        {
            base.OnPrepare(image);

            OnFinish();
        }

        /// <summary>
        /// See <see cref="BaseColorQuantizer.OnAddColor"/> for more details.
        /// </summary>
        protected override void OnAddColor(Color color, Int32 key, Int32 x, Int32 y)
        {
            root.AddColor(color, 0, this);
        }

        /// <summary>
        /// See <see cref="BaseColorQuantizer.OnGetPalette"/> for more details.
        /// </summary>
        protected override List<Color> OnGetPalette(Int32 colorCount)
        {
            // use optimized palette, if any
            List<Color> optimizedPalette = base.OnGetPalette(colorCount);
            if (optimizedPalette != null) return optimizedPalette;

            // otherwise let's get to build one
            List<Color> result = new List<Color>();

            IEnumerator<OctreeNode> leavesEnumerator = Leaves.GetEnumerator();
            Int32 leafCount = 0;
            while (leavesEnumerator.MoveNext()) leafCount++;

            lastColorCount = leafCount;
            Int32 paletteIndex = 0;
            List<OctreeNode> sortedNodeList;

            // goes thru all the levels starting at the deepest, and goes upto a root level
            for (Int32 level = 6; level >= 0; level--)
            {
                // if level contains any node
                if (levels[level].Count > 0)
                {
                    // orders the level node list by pixel presence (those with least pixels are at the top)
                    sortedNodeList = new List<OctreeNode>(levels[level]);
                    sortedNodeList.Sort((OctreeNode A, OctreeNode B) => { return A.ActiveNodesPixelCount.CompareTo(B.ActiveNodesPixelCount); });

                    // removes the nodes unless the count of the leaves is lower or equal than our requested color count
                    foreach (OctreeNode node in sortedNodeList)
                    {
                        // removes a node
                        leafCount -= node.RemoveLeaves(level, leafCount, colorCount, this);

                        // if the count of leaves is lower then our requested count terminate the loop
                        if (leafCount <= colorCount) break;
                    }

                    // if the count of leaves is lower then our requested count terminate the level loop as well
                    if (leafCount <= colorCount) break;

                    // otherwise clear whole level, as it is not needed anymore
                    levels[level].Clear();
                }
            }

            List<OctreeNode> reverseLeaves = new List<OctreeNode>(Leaves);
            reverseLeaves.Sort((OctreeNode A, OctreeNode B) => { return B.ActiveNodesPixelCount.CompareTo(A.ActiveNodesPixelCount); });

            // goes through all the leaves that are left in the tree (there should now be less or equal than requested)
            foreach (OctreeNode node in reverseLeaves)
            {
                if (paletteIndex >= colorCount) break;

                // adds the leaf color to a palette
                if (node.IsLeaf)
                {
                    result.Add(node.Color);
                }

                // and marks the node with a palette index
                node.SetPaletteIndex(paletteIndex++);
            }

            // we're unable to reduce the Octree with enough precision, and the leaf count is zero
            if (result.Count == 0)
            {
                throw new NotSupportedException("The Octree contains after the reduction 0 colors, it may happen for 1-16 colors because it reduces by 1-8 nodes at time. Should be used on 8 or above to ensure the correct functioning.");
            }

            // returns the palette
            return result;
        }

        /// <summary>
        /// See <see cref="BaseColorQuantizer.OnGetPaletteIndex"/> for more details.
        /// </summary>
        protected override void OnGetPaletteIndex(Color color, Int32 key, Int32 x, Int32 y, out Int32 paletteIndex)
        {
            // retrieves a palette index
            paletteIndex = root.GetPaletteIndex(color, 0);
        }

        /// <summary>
        /// See <see cref="BaseColorQuantizer.OnGetColorCount"/> for more details.
        /// </summary>
        protected override Int32 OnGetColorCount()
        {
            // calculates the number of leaves, by parsing the whole tree
            return lastColorCount;
        }

        /// <summary>
        /// See <see cref="BaseColorQuantizer.OnFinish"/> for more details.
        /// </summary>
        protected override void OnFinish()
        {
            base.OnFinish();

            // initializes the octree level lists
            levels = new List<OctreeNode>[7];

            // creates the octree level lists
            for (Int32 level = 0; level < 7; level++)
            {
                levels[level] = new List<OctreeNode>();
            }

            // creates a root node
            root = new OctreeNode(0, this);
        }

        #endregion

        #region << IColorQuantizer >>

        /// <summary>
        /// See <see cref="IColorQuantizer.AllowParallel"/> for more details.
        /// </summary>
        public override Boolean AllowParallel
        {
            get { return false; }
        }

        #endregion
    }
}


