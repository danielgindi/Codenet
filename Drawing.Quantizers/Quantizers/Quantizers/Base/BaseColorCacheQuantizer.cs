﻿using System;
using System.Collections.Generic;
using Codenet.Drawing.Common;
using Codenet.Drawing.Quantizers.ColorCaches;

namespace Codenet.Drawing.Quantizers.Quantizers;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public abstract class BaseColorCacheQuantizer : BaseColorQuantizer
{
    #region Fields

    private IColorCache colorCache;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseColorCacheQuantizer"/> class.
    /// </summary>
    protected BaseColorCacheQuantizer()
    {
        colorCache = null;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Changes the cache provider.
    /// </summary>
    /// <param name="colorCache">The color cache.</param>
    public void ChangeCacheProvider(IColorCache colorCache)
    {
        this.colorCache = colorCache;
    }

    /// <summary>
    /// Caches the palette.
    /// </summary>
    /// <param name="palette">The palette.</param>
    public void CachePalette(IList<NeatColor> palette)
    {
        GetColorCache().CachePalette(palette);
    }

    #endregion

    #region Helper methods

    private IColorCache GetColorCache()
    {
        // if there is no cache, it attempts to create a default cache; integrated in the quantizer
        IColorCache result = colorCache ?? (colorCache = OnCreateDefaultCache());

        // if the cache exists; or default one was created for these purposes.. use it
        if (result == null)
        {
            string message = string.Format("The color cache is not initialized! Please use SetColorCache() method on quantizer.");
            throw new ArgumentNullException(message);
        }

        // cache is fine, return it
        return result;
    }

    #endregion

    #region Abstract/virtual methods

    /// <summary>
    /// Called when it is needed to create default cache (no cache is supplied from outside).
    /// </summary>
    /// <returns></returns>
    protected abstract IColorCache OnCreateDefaultCache();

    /// <summary>
    /// Redirection to retrieve palette to be cached, if palette is not available yet.
    /// </summary>
    protected abstract List<NeatColor> OnGetPaletteToCache(int colorCount);

    #endregion

    #region BaseColorCacheQuantizer

    /// <summary>
    /// See <see cref="BaseColorQuantizer.OnPrepare"/> for more details.
    /// </summary>
    protected override void OnPrepare(ImageBuffer image)
    {
        base.OnPrepare(image);

        GetColorCache().Prepare();
    }

    /// <summary>
    /// See <see cref="BaseColorQuantizer.OnGetPalette"/> for more details.
    /// </summary>
    protected sealed override List<NeatColor> OnGetPalette(int colorCount)
    {
        // use optimization, or calculate new palette if color count is lower than unique color count
        List<NeatColor> palette = base.OnGetPalette(colorCount) ?? OnGetPaletteToCache(colorCount);
        GetColorCache().CachePalette(palette);
        return palette;
    }

    /// <summary>
    /// See <see cref="BaseColorQuantizer.OnGetPaletteIndex"/> for more details.
    /// </summary>
    protected override void OnGetPaletteIndex(NeatColor color, uint key, int x, int y, out int paletteIndex)
    {
        base.OnGetPaletteIndex(color, key, x, y, out paletteIndex);

        // if not determined, use cache to determine the index
        if (paletteIndex == InvalidIndex)
        {
            GetColorCache().GetColorPaletteIndex(color, out paletteIndex);
        }
    }

    #endregion
}
