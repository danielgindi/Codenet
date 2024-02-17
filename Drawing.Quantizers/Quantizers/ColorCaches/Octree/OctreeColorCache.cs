using System;
using System.Collections.Generic;
using Codenet.Drawing.Common;
using Codenet.Drawing.Common.Helpers;

namespace Codenet.Drawing.Quantizers.ColorCaches.Octree;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public class OctreeColorCache : BaseColorCache
{
    #region Fields

    private OctreeCacheNode root;

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether this instance is color model supported.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is color model supported; otherwise, <c>false</c>.
    /// </value>
    public override Boolean IsColorModelSupported
    {
        get { return false; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="OctreeColorCache"/> class.
    /// </summary>
    public OctreeColorCache()
    {
        ColorModel = ColorModel.RedGreenBlue;
        root = new OctreeCacheNode();
    }

    #endregion

    #region BaseColorCache

    /// <summary>
    /// See <see cref="BaseColorCache.Prepare"/> for more details.
    /// </summary>
    public override void Prepare()
    {
        base.Prepare();
        root = new OctreeCacheNode();
    }

    /// <summary>
    /// See <see cref="BaseColorCache.OnCachePalette"/> for more details.
    /// </summary>
    protected override void OnCachePalette(IList<NeatColor> palette)
    {
        Int32 index = 0;

        foreach (NeatColor color in palette)
        {
            root.AddColor(color, index++, 0);
        }
    }

    /// <summary>
    /// See <see cref="BaseColorCache.OnGetColorPaletteIndex"/> for more details.
    /// </summary>
    protected override void OnGetColorPaletteIndex(NeatColor color, out Int32 paletteIndex)
    {
        Dictionary<Int32, NeatColor> candidates = root.GetPaletteIndex(color, 0);

        paletteIndex = 0;
        Int32 index = 0;
        Int32 colorIndex = ColorModelHelper.GetEuclideanDistance(color, ColorModel, new List<NeatColor>(candidates.Values));

        foreach (Int32 colorPaletteIndex in candidates.Keys)
        {
            if (index == colorIndex)
            {
                paletteIndex = colorPaletteIndex;
                break;
            }

            index++;
        }
    }

    #endregion
}
