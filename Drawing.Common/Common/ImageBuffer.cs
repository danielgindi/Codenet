using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Codenet.Drawing.Common.Extensions;
using Codenet.Drawing.Common.Helpers;
using Codenet.Drawing.Common.PathProviders;

namespace Codenet.Drawing.Common
{
    public class ImageBuffer : IDisposable
    {
        #region Fields

        private int[] fastBitX;
        private int[] fastByteX;
        private int[] fastY;

        private readonly IntPtr _BmpPtr;
        private readonly bool _OwnsPtr;

        #endregion

        #region Delegates

        public delegate bool ProcessPixelFunction(PixelAccess pixel);
        public delegate bool ProcessPixelAdvancedFunction(PixelAccess pixel, ImageBuffer buffer);
        public delegate bool TransformPixelFunction(PixelAccess sourcePixel, PixelAccess targetPixel);
        public delegate bool TransformPixelAdvancedFunction(PixelAccess sourcePixel, PixelAccess targetPixel, ImageBuffer sourceBuffer, ImageBuffer targetBuffer);

        #endregion

        #region Properties

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int Size { get; private set; }
        public int Stride { get; private set; }
        public int BitDepth { get; private set; }
        public int BytesPerPixel { get; private set; }
        public bool IsIndexed { get; private set; }
        public PixelFormat PixelFormat { get; private set; }
        public bool CanRead { get; set; } = false;
        public bool CanWrite { get; set; } = false;

        #endregion

        #region Calculated properties

        private List<NeatColor> _palette;

        public List<NeatColor> Palette
        {
            get
            {
                return _palette;
            }
            set
            {
                _palette = value;
                OnPaletteChange?.Invoke(this, null);
            }
        }

        public event EventHandler OnPaletteChange;

        #endregion

        #region Constructors

        public ImageBuffer(
            int width, int height,
            PixelFormat pixelFormat,
            bool canRead, bool canWrite,
            IntPtr ptr, bool ownsPtr,
            int size = -1)
        {
            _BmpPtr = ptr;
            _OwnsPtr = ownsPtr;
            PixelFormat = pixelFormat;
            Size = size;
            CanRead = canRead;
            CanWrite = canWrite;

            // gathers the informations
            Width = width;
            Height = height;
            IsIndexed = PixelFormat.IsIndexed();
            BitDepth = PixelFormat.GetBitDepth();
            BytesPerPixel = (BitDepth + 7) >> 3;

            // stride should be rounded to 4 bytes boundary
            Stride = BytesPerPixel * Width + 3 & ~3;

            if (Size == -1)
            {
                Size = Stride * Height;
            }

            // precalculates the offsets
            Precalculate();
        }

        public static ImageBuffer Allocate(int width, int height, PixelFormat pixelFormat)
        {
            var bitDepth = pixelFormat.GetBitDepth();
            var bytesPerPixel = (bitDepth + 7) >> 3;
            var stride = bytesPerPixel * width + 3 & ~3;

            int size = stride * height;
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                return new ImageBuffer(width, height, pixelFormat, true, true, ptr, true, size);
            }
            catch
            {
                Marshal.FreeHGlobal(ptr);
                throw;
            }
        }

        #endregion

        #region Maintenance methods

        private void Precalculate()
        {
            fastBitX = new int[Width];
            fastByteX = new int[Width];
            fastY = new int[Height];

            // precalculates the x-coordinates
            for (int x = 0; x < Width; x++)
            {
                var bitX = x * BitDepth;
                fastByteX[x] = bitX >> 3;
                fastBitX[x] = bitX % 8;
            }

            // precalculates the y-coordinates
            for (int y = 0; y < Height; y++)
            {
                fastY[y] = y * Stride;
            }
        }

        public int GetBitOffset(int x)
        {
            return fastBitX[x];
        }

        public byte[] CopyByteArray()
        {
            // transfers whole image to a working memory
            byte[] result = new byte[Size];
            Marshal.Copy(_BmpPtr, result, 0, Size);

            // returns the backup
            return result;
        }

        public void CopyTo(int srcStart, int srcSize, byte[] dest, int destStart)
        {
            if (srcStart >= Size)
                throw new ArgumentOutOfRangeException("srcStart");

            if (destStart >= dest.Length)
                throw new ArgumentOutOfRangeException("destStart");

            if (destStart + srcSize > dest.Length ||
                srcStart + srcSize > Size)
                throw new ArgumentOutOfRangeException("srcSize");

            Marshal.Copy(_BmpPtr + srcStart, dest, destStart, srcSize);
        }

        public void PasteByteArray(byte[] buffer)
        {
            // commits the data to a bitmap
            Marshal.Copy(buffer, 0, _BmpPtr, Math.Min(buffer.Length, Size));
        }

        #endregion

        #region Pixel read methods

        public void ReadPixel(PixelAccess pixel, byte[] buffer = null)
        {
            // determines pixel offset at [x, y]
            int offset = fastByteX[pixel.X] + fastY[pixel.Y];

            // reads the pixel from a bitmap
            if (buffer == null)
            {
                pixel.ReadRawData(_BmpPtr + offset);
            }
            else // reads the pixel from a buffer
            {
                pixel.ReadData(buffer, offset);
            }
        }

        public int GetIndexFromPixel(PixelAccess pixel)
        {
            int result;

            // determines whether the format is indexed
            if (IsIndexed)
            {
                result = pixel.Index;
            }
            else // not possible to get index from a non-indexed format
            {
                throw new NotSupportedException("Cannot retrieve index for a non-indexed format. Please use Color (or Value) property instead.");
            }

            return result;
        }

        public NeatColor GetColorFromPixel(PixelAccess pixel)
        {
            NeatColor result;

            // determines whether the format is indexed
            if (pixel.IsIndexed)
            {
                int index = pixel.Index;
                result = pixel.Parent.GetPaletteColor(index);
            }
            else // gets color from a non-indexed format
            {
                result = pixel.Color;
            }

            // returns the found color
            return result;
        }

        public int ReadIndexUsingPixel(PixelAccess pixel, byte[] buffer = null)
        {
            // reads the pixel from bitmap/buffer
            ReadPixel(pixel, buffer);

            // returns the found color
            return GetIndexFromPixel(pixel);
        }

        public NeatColor ReadColorUsingPixel(PixelAccess pixel, byte[] buffer = null)
        {
            // reads the pixel from bitmap/buffer
            ReadPixel(pixel, buffer);

            // returns the found color
            return GetColorFromPixel(pixel);
        }

        public int ReadIndexUsingPixelFrom(PixelAccess pixel, int x, int y, byte[] buffer = null)
        {
            // redirects pixel -> [x, y]
            pixel.Set(x, y);

            // reads index from a bitmap/buffer using pixel, and stores it in the pixel
            return ReadIndexUsingPixel(pixel, buffer);
        }

        public NeatColor ReadColorUsingPixelFrom(PixelAccess pixel, int x, int y, byte[] buffer = null)
        {
            // redirects pixel -> [x, y]
            pixel.Set(x, y);

            // reads color from a bitmap/buffer using pixel, and stores it in the pixel
            return ReadColorUsingPixel(pixel, buffer);
        }

        #endregion

        #region Pixel write methods

        private void WritePixel(PixelAccess pixel, byte[] buffer = null)
        {
            // determines pixel offset at [x, y]
            int offset = fastByteX[pixel.X] + fastY[pixel.Y];

            // writes the pixel to a bitmap
            if (buffer == null)
            {
                pixel.WriteRawData(_BmpPtr + offset);
            }
            else // writes the pixel to a buffer
            {
                pixel.WriteData(buffer, offset);
            }
        }

        public void SetIndexToPixel(PixelAccess pixel, int index, byte[] buffer = null)
        {
            // determines whether the format is indexed
            if (IsIndexed)
            {
                pixel.Index = (byte)index;
            }
            else // cannot write color to an indexed format
            {
                throw new NotSupportedException("Cannot set index for a non-indexed format. Please use NeatColor (or Value) property instead.");
            }
        }

        public void SetColorToPixel(PixelAccess pixel, NeatColor color, IColorQuantizer quantizer)
        {
            // determines whether the format is indexed
            if (pixel.IsIndexed)
            {
                // last chance if quantizer is provided, use it
                if (quantizer != null)
                {
                    byte index = (byte)quantizer.GetPaletteIndex(color, pixel.X, pixel.Y);
                    pixel.Index = index;
                }
                else // cannot write color to an index format
                {
                    throw new NotSupportedException("Cannot retrieve color for an indexed format. Use GetPixelIndex() instead.");
                }
            }
            else // sets color to a non-indexed format
            {
                pixel.Color = color;
            }
        }

        public void WriteIndexUsingPixel(PixelAccess pixel, int index, byte[] buffer = null)
        {
            // sets index to pixel (pixel's index is updated)
            SetIndexToPixel(pixel, index, buffer);

            // writes pixel to a bitmap/buffer
            WritePixel(pixel, buffer);
        }

        public void WriteColorUsingPixel(PixelAccess pixel, NeatColor color, IColorQuantizer quantizer, byte[] buffer = null)
        {
            // sets color to pixel (pixel is updated with color)
            SetColorToPixel(pixel, color, quantizer);

            // writes pixel to a bitmap/buffer
            WritePixel(pixel, buffer);
        }

        public void WriteIndexUsingPixelAt(PixelAccess pixel, int x, int y, int index, byte[] buffer = null)
        {
            // redirects pixel -> [x, y]
            pixel.Set(x, y);

            // writes color to bitmap/buffer using pixel
            WriteIndexUsingPixel(pixel, index, buffer);
        }

        public void WriteColorUsingPixelAt(PixelAccess pixel, int x, int y, NeatColor color, IColorQuantizer quantizer, byte[] buffer = null)
        {
            // redirects pixel -> [x, y]
            pixel.Set(x, y);

            // writes color to bitmap/buffer using pixel
            WriteColorUsingPixel(pixel, color, quantizer, buffer);
        }

        #endregion

        #region Generic methods

        private static void CheckNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException($"Cannot use '{argumentName}' when it is null!");
            }
        }

        private void ProcessInParallel(ICollection<System.Drawing.Point> path, Action<PixelProcessingParallelTask> process, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(process, "process");

            // prepares parallel processing
            double pointsPerTask = (double)path.Count / parallelTaskCount;
            PixelProcessingParallelTask[] tasks = new PixelProcessingParallelTask[parallelTaskCount];
            double pointOffset = 0.0;

            // creates task for each batch of rows
            for (int index = 0; index < parallelTaskCount; index++)
            {
                tasks[index] = new PixelProcessingParallelTask((int)pointOffset, (int)(pointOffset + pointsPerTask));
                pointOffset += pointsPerTask;
            }

            // process the image in a parallel manner
            Parallel.ForEach(tasks, process);
        }


        #endregion

        #region Processing methods

        private void ProcessPerPixelBase(IList<System.Drawing.Point> path, Delegate processingAction, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(path, "path");
            CheckNull(processingAction, "processPixelFunction");

            // determines mode
            bool isAdvanced = processingAction is ProcessPixelAdvancedFunction;

            // prepares the per pixel task
            Action<PixelProcessingParallelTask> processPerPixel = task =>
            {
                // initializes variables per task
                PixelAccess pixel = new PixelAccess(this);
                var canRead = CanRead;
                var canWrite = CanWrite;

                for (int pathOffset = task.StartOffset; pathOffset < task.EndOffset; pathOffset++)
                {
                    System.Drawing.Point point = path[pathOffset];
                    bool allowWrite;

                    // enumerates the pixel, and returns the control to the outside
                    pixel.Set(point.X, point.Y);

                    // retrieves current value (in bytes)
                    if (canRead) ReadPixel(pixel);

                    // process the pixel by custom user operation
                    if (isAdvanced)
                    {
                        ProcessPixelAdvancedFunction processAdvancedFunction = (ProcessPixelAdvancedFunction)processingAction;
                        allowWrite = processAdvancedFunction(pixel, this);
                    }
                    else // use simplified version with pixel parameter only
                    {
                        ProcessPixelFunction processFunction = (ProcessPixelFunction)processingAction;
                        allowWrite = processFunction(pixel);
                    }

                    // copies the value back to the row buffer
                    if (canWrite && allowWrite) WritePixel(pixel);
                }
            };

            // processes image per pixel
            ProcessInParallel(path, processPerPixel, parallelTaskCount);
        }

        public void ProcessPerPixel(IList<System.Drawing.Point> path, ProcessPixelFunction processPixelFunction, int parallelTaskCount = 4)
        {
            ProcessPerPixelBase(path, processPixelFunction, parallelTaskCount);
        }

        public void ProcessPerPixelAdvanced(IList<System.Drawing.Point> path, ProcessPixelAdvancedFunction processPixelAdvancedFunction, int parallelTaskCount = 4)
        {
            ProcessPerPixelBase(path, processPixelAdvancedFunction, parallelTaskCount);
        }

        #endregion

        #region Transformation functions

        private void TransformPerPixelBase(ImageBuffer target, IList<System.Drawing.Point> path, Delegate transformAction, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(path, "path");
            CheckNull(target, "target");
            CheckNull(transformAction, "transformAction");

            // checks the dimensions
            if (Width != target.Width || Height != target.Height)
            {
                const string message = "Both images have to have the same dimensions.";
                throw new ArgumentOutOfRangeException(message);
            }

            // determines mode
            bool isAdvanced = transformAction is TransformPixelAdvancedFunction;

            // process the image in a parallel manner
            Action<PixelProcessingParallelTask> transformPerPixel = task =>
            {
                // creates individual pixel structures per task
                PixelAccess sourcePixel = new PixelAccess(this);
                PixelAccess targetPixel = new PixelAccess(target);
                var canRead = CanRead;
                var canWrite = target.CanWrite;

                // enumerates the pixels row by row
                for (int pathOffset = task.StartOffset; pathOffset < task.EndOffset; pathOffset++)
                {
                    System.Drawing.Point point = path[pathOffset];
                    bool allowWrite;

                    // enumerates the pixel, and returns the control to the outside
                    sourcePixel.Set(point.X, point.Y);
                    targetPixel.Set(point.X, point.Y);

                    // retrieves current value (in bytes)
                    if (canRead)
                    {
                        ReadPixel(sourcePixel);
                        target.ReadPixel(targetPixel);
                    }

                    // process the pixel by custom user operation
                    if (isAdvanced)
                    {
                        TransformPixelAdvancedFunction transformAdvancedFunction = (TransformPixelAdvancedFunction)transformAction;
                        allowWrite = transformAdvancedFunction(sourcePixel, targetPixel, this, target);
                    }
                    else // use simplified version with pixel parameters only
                    {
                        TransformPixelFunction transformFunction = (TransformPixelFunction)transformAction;
                        allowWrite = transformFunction(sourcePixel, targetPixel);
                    }

                    // copies the value back to the row buffer
                    if (canWrite && allowWrite) target.WritePixel(targetPixel);
                }
            };

            // transforms image per pixel
            ProcessInParallel(path, transformPerPixel, parallelTaskCount);
        }

        public void TransformPerPixel(ImageBuffer target, IList<System.Drawing.Point> path, TransformPixelFunction transformPixelFunction, int parallelTaskCount = 4)
        {
            TransformPerPixelBase(target, path, transformPixelFunction, parallelTaskCount);
        }

        public void TransformPerPixelAdvanced(ImageBuffer target, IList<System.Drawing.Point> path, TransformPixelAdvancedFunction transformPixelAdvancedFunction, int parallelTaskCount = 4)
        {
            TransformPerPixelBase(target, path, transformPixelAdvancedFunction, parallelTaskCount);
        }

        #endregion

        #region Scan colors methods

        public void ScanColors(IColorQuantizer quantizer, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(quantizer, "quantizer");

            // determines which method of color retrieval to use
            IList<System.Drawing.Point> path = quantizer.GetPointPath(Width, Height);

            // use different scanning method depending whether the image format is indexed
            ProcessPixelFunction scanColors = pixel =>
            {
                quantizer.AddColor(GetColorFromPixel(pixel), pixel.X, pixel.Y);
                return false;
            };

            // performs the image scan, using a chosen method
            ProcessPerPixel(path, scanColors, parallelTaskCount);
        }

        /// <summary>
        /// Added by Daniel danielgindi@gmail.com
        /// Check to see if the image contains the specified color.
        /// </summary>
        /// <param name="ColorToFind">A color to find in the image</param>
        /// <param name="FirstX">Output parameter, where the color's first matching X is gonna be</param>
        /// <param name="FirstY">Output parameter, where the color's first matching Y is gonna be</param>
        /// <returns>true if the color is in use in the image.</returns>
        public bool ScanForColor(NeatColor ColorToFind, out int FirstX, out int FirstY)
        {
            PixelAccess pixel = new PixelAccess(this);
            FirstX = FirstY = -1;

            if (IsIndexed)
            {
                NeatColor[] paletteArray = Palette.ToArray();
                if (Array.IndexOf(paletteArray, ColorToFind) < 0) return false;
                var canRead = CanRead;

                for (int x = 0, y, w = Width, h = Height; x < w; x++)
                {
                    for (y = 0; y < h; y++)
                    {
                        pixel.Set(x, y);
                        if (canRead) ReadPixel(pixel);
                        if (paletteArray[pixel.Index] == ColorToFind)
                        {
                            FirstX = x;
                            FirstY = y;
                            return true;
                        }
                    }
                }
            }
            else
            {
                var canRead = CanRead;

                for (int x = 0, y, w = Width, h = Height; x < w; x++)
                {
                    for (y = 0; y < h; y++)
                    {
                        pixel.Set(x, y);
                        if (canRead) ReadPixel(pixel);
                        if (pixel.Color.Equals(ColorToFind))
                        {
                            FirstX = x;
                            FirstY = y;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Added by Daniel danielgindi@gmail.com
        /// Look for the specified color, or its closest match.
        /// </summary>
        /// <param name="ColorToFind">A color to find in the image</param>
        /// <returns>The color that was matches.</returns>
        public NeatColor ScanForClosestColor(NeatColor ColorToFind)
        {
            PixelAccess pixel = new PixelAccess(this);

            int r = ColorToFind.Red;
            int g = ColorToFind.Green;
            int b = ColorToFind.Blue;
            int dmin = 256 * 256 * 256;

            if (IsIndexed)
            {
                List<NeatColor> palette = Palette;

                int dr, dg, db, d, minpos = -1, index = 0;
                foreach (NeatColor c in palette)
                {
                    dr = r - c.Red;
                    dg = g - c.Green;
                    db = b - c.Blue;
                    d = dr * dr + dg * dg + db * db;
                    if (d < dmin)
                    {
                        dmin = d;
                        minpos = index;
                    }
                    index++;
                }
                return minpos == -1 ? NeatColor.Empty : palette[minpos];
            }
            else
            {
                int dr, dg, db, d;
                NeatColor c, foundColor = NeatColor.Empty;
                var canRead = CanRead;

                for (int x = 0, y, w = Width, h = Height; x < w; x++)
                {
                    for (y = 0; y < h; y++)
                    {
                        pixel.Set(x, y);
                        if (canRead) ReadPixel(pixel);
                        c = pixel.Color;
                        dr = r - c.Red;
                        dg = g - c.Green;
                        db = b - c.Blue;
                        d = dr * dr + dg * dg + db * db;
                        if (d < dmin)
                        {
                            dmin = d;
                            foundColor = c;
                        }
                    }
                }
                return foundColor;
            }
        }

        /// <summary>
        /// Added by Daniel danielgindi@gmail.com
        /// Find an unused color in the palette.
        /// </summary>
        /// <returns>Index of any unused color in the pallete, or -1 if not found or not indexed.</returns>
        public int ScanForFirstUnusedColorIndex()
        {
            if (IsIndexed)
            {
                PixelAccess pixel = new PixelAccess(this);

                List<NeatColor> palette = Palette;
                bool[] used = new bool[palette.Count];
                var canRead = CanRead;

                for (int x = 0, y, w = Width, h = Height; x < w; x++)
                {
                    for (y = 0; y < h; y++)
                    {
                        pixel.Set(x, y);
                        if (canRead) ReadPixel(pixel);
                        used[pixel.Index] = true;
                    }
                }

                for (int j = 0; j < used.Length; j++)
                {
                    if (!used[j]) return j;
                }

                return -1;
            }
            else
            {
                return -1;
            }
        }

        #endregion

        #region Synthetize palette methods

        public List<NeatColor> SynthetizePalette(IColorQuantizer quantizer, int colorCount, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(quantizer, "quantizer");

            // Step 1 - prepares quantizer for another round
            quantizer.Prepare(this);

            // Step 2 - scans the source image for the colors
            ScanColors(quantizer, parallelTaskCount);

            // Step 3 - synthetises the palette, and returns the result
            return quantizer.GetPalette(colorCount);
        }

        #endregion

        #region Quantize methods

        public void Quantize(ImageBuffer target, IColorQuantizer quantizer, int colorCount, int parallelTaskCount = 4)
        {
            // performs the pure quantization wihout dithering
            Quantize(target, quantizer, null, colorCount, parallelTaskCount);
        }

        public void Quantize(ImageBuffer target, IColorQuantizer quantizer, IColorDitherer ditherer, int colorCount, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(target, "target");
            CheckNull(quantizer, "quantizer");

            // clamp parallelTaskCount
            if (parallelTaskCount < 1 || !quantizer.AllowParallel)
                parallelTaskCount = 1;

            // initializes quantization parameters
            bool isTargetIndexed = target.PixelFormat.IsIndexed();

            // step 1 - prepares the palettes
            List<NeatColor> targetPalette = isTargetIndexed ? SynthetizePalette(quantizer, colorCount, parallelTaskCount) : null;

            // step 2 - updates the bitmap palette
            target.Palette = targetPalette;

            // step 3 - prepares ditherer (optional)
            if (ditherer != null) ditherer.Prepare(quantizer, colorCount, this, target);

            // step 4 - prepares the quantization function
            TransformPixelFunction quantize = (sourcePixel, targetPixel) =>
            {
                // reads the pixel color
                NeatColor color = GetColorFromPixel(sourcePixel);

                // converts alpha to solid color
                color = QuantizationHelper.ConvertAlphaToSolid(color);

                // quantizes the pixel
                SetColorToPixel(targetPixel, color, quantizer);

                // marks pixel as processed by default
                bool result = true;

                // preforms inplace dithering (optional)
                if (ditherer != null && ditherer.IsInplace)
                {
                    result = ditherer.ProcessPixel(sourcePixel, targetPixel);
                }

                // returns the result
                return result;
            };

            // step 5 - generates the target image
            IList<System.Drawing.Point> path = quantizer.GetPointPath(Width, Height);
            TransformPerPixel(target, path, quantize, parallelTaskCount);

            // step 6 - preforms non-inplace dithering (optional)
            if (ditherer != null && !ditherer.IsInplace)
            {
                Dither(target, ditherer, quantizer, colorCount, 1);
            }

            // step 7 - finishes the dithering (optional)
            if (ditherer != null) ditherer.Finish();

            // step 8 - clean-up
            quantizer.Finish();
        }

        #endregion

        #region Calculate mean error methods

        public double CalculateMeanError(ImageBuffer target, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(target, "target");

            // initializes the error
            long totalError = 0;

            // prepares the function
            TransformPixelFunction calculateMeanError = (sourcePixel, targetPixel) =>
            {
                NeatColor sourceColor = GetColorFromPixel(sourcePixel);
                NeatColor targetColor = GetColorFromPixel(targetPixel);
                totalError += ColorModelHelper.GetColorEuclideanDistance(ColorModel.RedGreenBlue, sourceColor, targetColor);
                return false;
            };

            // performs the image scan, using a chosen method
            var allPointsPath = new StandardPathProvider().GetPointPath(Width, Height);
            TransformPerPixel(target, allPointsPath, calculateMeanError, parallelTaskCount);

            // returns the calculates RMSD
            return Math.Sqrt(totalError / (3.0 * Width * Height));
        }

        public static double CalculateImageMeanError(ImageBuffer source, ImageBuffer target, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(source, "source");

            // use other override to calculate error
            return source.CalculateMeanError(target, parallelTaskCount);
        }

        #endregion

        #region Calculate normalized mean error methods

        public double CalculateNormalizedMeanError(ImageBuffer target, int parallelTaskCount = 4)
        {
            return CalculateMeanError(target, parallelTaskCount) / 255.0;
        }

        public static double CalculateImageNormalizedMeanError(ImageBuffer source, ImageBuffer target, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(source, "source");

            // use other override to calculate error
            return source.CalculateNormalizedMeanError(target, parallelTaskCount);
        }

        #endregion

        #region Change pixel format methods

        public void ChangeFormat(ImageBuffer target, IColorQuantizer quantizer, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(target, "target");

            // gathers some information about the target format
            bool hasSourceAlpha = PixelFormat.HasAlpha();
            bool hasTargetAlpha = target.PixelFormat.HasAlpha();
            bool isTargetIndexed = target.PixelFormat.IsIndexed();
            bool isSourceDeepColor = PixelFormat.IsDeepColor();
            bool isTargetDeepColor = target.PixelFormat.IsDeepColor();

            if (isTargetIndexed)
                CheckNull(quantizer, "quantizer");

            // step 1 to 3 - prepares the palettes
            if (isTargetIndexed)
                SynthetizePalette(quantizer, target.PixelFormat.GetColorCount(), parallelTaskCount);

            // prepares the quantization function
            TransformPixelFunction changeFormat = (sourcePixel, targetPixel) =>
            {
                // if both source and target formats are deep color formats, copies a value directly
                if (isSourceDeepColor && isTargetDeepColor && !sourcePixel.IsIndexed)
                {
                    targetPixel.FullNonIndexedValue = sourcePixel.FullNonIndexedValue;
                }
                else
                {
                    // retrieves a source image color
                    NeatColor color = GetColorFromPixel(sourcePixel);

                    if (!hasSourceAlpha && hasTargetAlpha)
                    {
                        uint argb = 255u << 24 | (uint)color.Red << 16 | (uint)color.Green << 8 | color.Blue;
                        color = new NeatColor(argb);
                    }
                    else if (hasSourceAlpha && !hasTargetAlpha && color.Alpha < 255)
                    {
                        var mul = color.Alpha / 255.0;
                        color = new NeatColor((byte)(color.Red * mul), (byte)(color.Green * mul), (byte)(color.Blue * mul));
                    }

                    // sets the color to a target pixel
                    SetColorToPixel(targetPixel, color, quantizer);
                }

                // allows to write (obviously) the transformed pixel
                return true;
            };

            // step 5 - generates the target image
            var allPointsPath = new StandardPathProvider().GetPointPath(Width, Height);
            TransformPerPixel(target, allPointsPath, changeFormat, 1);
        }

        #endregion

        #region Dithering methods

        public void Dither(ImageBuffer target, IColorDitherer ditherer, IColorQuantizer quantizer, int colorCount, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(target, "target");
            CheckNull(ditherer, "ditherer");
            CheckNull(quantizer, "quantizer");

            // prepares ditherer for another round
            ditherer.Prepare(quantizer, colorCount, this, target);

            // processes the image via the ditherer
            IList<System.Drawing.Point> path = ditherer.GetPointPath(Width, Height);
            TransformPerPixel(target, path, ditherer.ProcessPixel, parallelTaskCount);
        }

        public static void DitherImage(ImageBuffer source, ImageBuffer target, IColorDitherer ditherer, IColorQuantizer quantizer, int colorCount, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(source, "source");

            // use other override to calculate error
            source.Dither(target, ditherer, quantizer, colorCount, parallelTaskCount);
        }

        #endregion

        #region Gamma correction

        public void CorrectGamma(float gamma, IColorQuantizer quantizer, int parallelTaskCount = 4)
        {
            // checks parameters
            CheckNull(quantizer, "quantizer");

            // determines which method of color retrieval to use
            IList<System.Drawing.Point> path = quantizer.GetPointPath(Width, Height);

            // calculates gamma ramp
            int[] gammaRamp = new int[256];

            for (int index = 0; index < 256; ++index)
            {
                gammaRamp[index] = Clamp((int)(255.0f * Math.Pow(index / 255.0f, 1.0f / gamma) + 0.5f), 0, 255);
            }

            // use different scanning method depending whether the image format is indexed
            ProcessPixelFunction correctGamma = pixel =>
            {
                NeatColor oldColor = GetColorFromPixel(pixel);
                int red = gammaRamp[oldColor.Red];
                int green = gammaRamp[oldColor.Green];
                int blue = gammaRamp[oldColor.Blue];
                NeatColor newColor = new NeatColor((byte)red, (byte)green, (byte)blue);
                SetColorToPixel(pixel, newColor, quantizer);
                return true;
            };

            // performs the image scan, using a chosen method
            ProcessPerPixel(path, correctGamma, parallelTaskCount);
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (value < minimum) value = minimum;
            if (value > maximum) value = maximum;
            return value;
        }

        #endregion

        #region Palette methods

        public NeatColor GetPaletteColor(int paletteIndex)
        {
            return Palette[paletteIndex];
        }

        #endregion

        #region IDisposable

        private bool _Disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }

            if (!_Disposed)
            {
                // dispose unmanaged resources
                if (_OwnsPtr && _BmpPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_BmpPtr);
                }

                _Disposed = true;
            }
        }

        ~ImageBuffer()
        {
            Dispose(false);
        }

        #endregion

        #region Subclasses

        private class PixelProcessingParallelTask
        {
            /// <summary>
            /// Gets or sets the start offset.
            /// </summary>
            public int StartOffset { get; private set; }

            /// <summary>
            /// Gets or sets the end offset.
            /// </summary>
            public int EndOffset { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PixelProcessingParallelTask"/> class.
            /// </summary>
            public PixelProcessingParallelTask(int startOffset, int endOffset)
            {
                StartOffset = startOffset;
                EndOffset = endOffset;
            }
        }

        #endregion
    }
}