using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Codenet.Drawing.Common;
using Codenet.Drawing.Common.Helpers;
using Codenet.Drawing.Common.PathProviders;

namespace Codenet.Drawing.Quantizers.Quantizers;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// </summary>
public abstract class BaseColorQuantizer : IColorQuantizer
{
    #region Constants

    /// <summary>
    /// This index will represent invalid palette index.
    /// </summary>
    protected const int InvalidIndex = -1;

    #endregion

    #region Fields

    private bool paletteFound;
    private long uniqueColorIndex;
    private IPathProvider pathProvider;
    protected readonly ConcurrentDictionary<uint, short> UniqueColors;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseColorQuantizer"/> class.
    /// </summary>
    protected BaseColorQuantizer()
    {
        pathProvider = null;
        uniqueColorIndex = -1;
        UniqueColors = new ConcurrentDictionary<uint, short>();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Changes the path provider.
    /// </summary>
    /// <param name="pathProvider">The path provider.</param>
    public void ChangePathProvider(IPathProvider pathProvider)
    {
        this.pathProvider = pathProvider;
    }

    #endregion

    #region Helper methods

    private IPathProvider GetPathProvider()
    {
        // if there is no path provider, it attempts to create a default one; integrated in the quantizer
        IPathProvider result = pathProvider ?? (pathProvider = OnCreateDefaultPathProvider());

        // if the provider exists; or default one was created for these purposes.. use it
        if (result == null)
        {
            string message = string.Format("The path provider is not initialized! Please use SetPathProvider() method on quantizer.");
            throw new ArgumentNullException(message);
        }

        // provider was obtained somehow, use it
        return result;
    }

    #endregion

    #region Abstract/virtual methods

    /// <summary>
    /// Called when quantizer is about to be prepared for next round.
    /// </summary>
    protected virtual void OnPrepare(ImageBuffer image)
    {
        uniqueColorIndex = -1;
        paletteFound = false;
        UniqueColors.Clear();
    }

    /// <summary>
    /// Called when color is to be added.
    /// </summary>
    protected virtual void OnAddColor(NeatColor color, uint key, int x, int y)
    {
        UniqueColors.AddOrUpdate(key,
            colorKey => (byte)Interlocked.Increment(ref uniqueColorIndex),
            (colorKey, colorIndex) => colorIndex);
    }

    /// <summary>
    /// Called when a need to create default path provider arisen.
    /// </summary>
    protected virtual IPathProvider OnCreateDefaultPathProvider()
    {
        pathProvider = new StandardPathProvider();
        return new StandardPathProvider();
    }

    /// <summary>
    /// Called when quantized palette is needed.
    /// </summary>
    protected virtual List<NeatColor> OnGetPalette(int colorCount)
    {
        // early optimalization, in case the color count is lower than total unique color count
        if (UniqueColors.Count > 0 && colorCount >= UniqueColors.Count)
        {
            // palette was found
            paletteFound = true;

            // generates the palette from unique numbers
            List<KeyValuePair<uint, short>> orderedDictionary = new List<KeyValuePair<uint, short>>(UniqueColors);
            orderedDictionary.Sort((pairA, pairB) => pairA.Value.CompareTo(pairB.Value));

            List<NeatColor> colors = new List<NeatColor>(orderedDictionary.Count);
            NeatColor color;
            foreach (KeyValuePair<uint, short> pair in orderedDictionary)
            {
                color = new NeatColor(pair.Key);
                color = color.WithAlpha(255);
                colors.Add(color);
            }

            return colors;
        }

        // otherwise make it descendant responsibility
        return null;
    }

    /// <summary>
    /// Called when get palette index for a given color should be returned.
    /// </summary>
    protected virtual void OnGetPaletteIndex(NeatColor color, uint key, int x, int y, out int paletteIndex)
    {
        // by default unknown index is returned
        paletteIndex = InvalidIndex;
        short foundIndex;

        // if we previously found palette quickly (without quantization), use it
        if (paletteFound && UniqueColors.TryGetValue(key, out foundIndex))
        {
            paletteIndex = foundIndex;
        }
    }

    /// <summary>
    /// Called when get color count.
    /// </summary>
    protected virtual int OnGetColorCount()
    {
        return UniqueColors.Count;
    }

    /// <summary>
    /// Called when about to clear left-overs after quantization.
    /// </summary>
    protected virtual void OnFinish()
    {
        // do nothing here
    }

    #endregion

    #region IPathProvider

    /// <summary>
    /// See <see cref="IPathProvider.GetPointPath"/> for more details.
    /// </summary>
    public IList<System.Drawing.Point> GetPointPath(int width, int heigth)
    {
        return GetPathProvider().GetPointPath(width, heigth);
    }

    #endregion

    #region IColorQuantizer

    /// <summary>
    /// See <see cref="IColorQuantizer.AllowParallel"/> for more details.
    /// </summary>
    public abstract bool AllowParallel { get; }

    /// <summary>
    /// See <see cref="IColorQuantizer.Prepare"/> for more details.
    /// </summary>
    public void Prepare(ImageBuffer image)
    {
        OnPrepare(image);
    }

    /// <summary>
    /// See <see cref="IColorQuantizer.AddColor"/> for more details.
    /// </summary>
    public void AddColor(NeatColor color, int x, int y)
    {
        color = QuantizationHelper.ConvertAlphaToSolid(color, out Int32 key);
        OnAddColor(color, (UInt32)key, x, y);
    }

    /// <summary>
    /// See <see cref="IColorQuantizer.GetColorCount"/> for more details.
    /// </summary>
    public int GetColorCount()
    {
        return OnGetColorCount();
    }

    /// <summary>
    /// See <see cref="IColorQuantizer.GetPalette"/> for more details.
    /// </summary>
    public List<NeatColor> GetPalette(int colorCount)
    {
        return OnGetPalette(colorCount);
    }

    /// <summary>
    /// See <see cref="IColorQuantizer.GetPaletteIndex"/> for more details.
    /// </summary>
    public int GetPaletteIndex(NeatColor color, int x, int y)
    {
        int result;
        color = QuantizationHelper.ConvertAlphaToSolid(color, out Int32 key);
        OnGetPaletteIndex(color, (UInt32)key, x, y, out result);
        return result;
    }

    /// <summary>
    /// See <see cref="IColorQuantizer.Finish"/> for more details.
    /// </summary>
    public void Finish()
    {
        OnFinish();
    }

    #endregion
}
