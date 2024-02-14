using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Codenet.Drawing.Common;

namespace Codenet.Drawing.Quantizers.ColorCaches
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public abstract class BaseColorCache : IColorCache
    {
        #region Fields

        private readonly ConcurrentDictionary<Int32, Int32> cache;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the color model.
        /// </summary>
        /// <value>The color model.</value>
        protected ColorModel ColorModel { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is color model supported.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is color model supported; otherwise, <c>false</c>.
        /// </value>
        public abstract Boolean IsColorModelSupported { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseColorCache"/> class.
        /// </summary>
        protected BaseColorCache()
        {
            cache = new ConcurrentDictionary<Int32, Int32>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the color model.
        /// </summary>
        /// <param name="colorModel">The color model.</param>
        public void ChangeColorModel(ColorModel colorModel)
        {
            ColorModel = colorModel;
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// Called when a palette is about to be cached, or precached.
        /// </summary>
        /// <param name="palette">The palette.</param>
        protected abstract void OnCachePalette(IList<NeatColor> palette);

        /// <summary>
        /// Called when palette index is about to be retrieve for a given color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="paletteIndex">Index of the palette.</param>
        protected abstract void OnGetColorPaletteIndex(NeatColor color, out Int32 paletteIndex);

        #endregion

        #region IColorCache

        /// <summary>
        /// See <see cref="IColorCache.Prepare"/> for more details.
        /// </summary>
        public virtual void Prepare()
        {
            cache.Clear();
        }

        /// <summary>
        /// See <see cref="IColorCache.CachePalette"/> for more details.
        /// </summary>
        public void CachePalette(IList<NeatColor> palette)
        {
            OnCachePalette(palette);
        }

        /// <summary>
        /// See <see cref="IColorCache.GetColorPaletteIndex"/> for more details.
        /// </summary>
        public void GetColorPaletteIndex(NeatColor color, out Int32 paletteIndex)
        {
            Int32 key = color.Red << 16 | color.Green << 8 | color.Blue;

            paletteIndex = cache.AddOrUpdate(key,
                colorKey =>
                {
                    Int32 paletteIndexInside;
                    OnGetColorPaletteIndex(color, out paletteIndexInside);
                    return paletteIndexInside;
                }, 
                (colorKey, inputIndex) => inputIndex);
        }

        #endregion
    }
}
