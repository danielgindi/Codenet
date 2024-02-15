﻿using System;
using System.Runtime.InteropServices;
using Codenet.Drawing.Common.Helpers.Pixels;
using Codenet.Drawing.Common.Helpers.Pixels.Indexed;
using Codenet.Drawing.Common.Helpers.Pixels.NonIndexed;
using System.Security;

namespace Codenet.Drawing.Common;

/// <summary>
/// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
/// 
/// This is a pixel format independent pixel.
/// </summary>
[SuppressUnmanagedCodeSecurity()]
public class PixelAccess : IDisposable
{
    #region Constants

    internal const byte Zero = 0;
    internal const byte One = 1;
    internal const byte Two = 2;
    internal const byte Four = 4;
    internal const byte Eight = 8;

    internal const byte NibbleMask = 0xF;
    internal const byte ByteMask = 0xFF;

    internal const int AlphaShift = 24;
    internal const int RedShift = 16;
    internal const int GreenShift = 8;
    internal const int BlueShift = 0;

    internal const uint AlphaMask = (uint)ByteMask << AlphaShift;
    internal const uint RedGreenBlueMask = 0xFFFFFF;

    #endregion

    #region Fields

    private Type pixelType;
    private int bitOffset;
    private object pixelData;
    private IntPtr pixelDataPointer;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the X.
    /// </summary>
    public int X { get; private set; }

    /// <summary>
    /// Gets the Y.
    /// </summary>
    public int Y { get; private set; }

    /// <summary>
    /// Gets the parent buffer.
    /// </summary>
    public ImageBuffer Parent { get; private set; }

    #endregion

    #region Calculated properties

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    /// <value>The index.</value>
    public byte Index
    {
        get { return ((IIndexedPixel)pixelData).GetIndex(bitOffset); }
        set { ((IIndexedPixel)pixelData).SetIndex(bitOffset, value); }
    }

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    /// <value>The color.</value>
    public NeatColor Color
    {
        get { return ((INonIndexedPixel)pixelData).GetColor(); }
        set { ((INonIndexedPixel)pixelData).SetColor(value); }
    }

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    /// <value>The color.</value>
    public ulong FullNonIndexedValue
    {
        get { return ((INonIndexedPixel)pixelData).Value; }
        set { ((INonIndexedPixel)pixelData).Value = value; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is indexed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is indexed; otherwise, <c>false</c>.
    /// </value>
    public bool IsIndexed
    {
        get { return Parent.IsIndexed; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelAccess"/> struct.
    /// </summary>
    public PixelAccess(ImageBuffer parent)
    {
        Parent = parent;

        Initialize();
    }

    private void Initialize()
    {
        // creates pixel data
        pixelType = IsIndexed ? GetIndexedType(Parent.PixelFormat) : GetNonIndexedType(Parent.PixelFormat);
        pixelData = Activator.CreateInstance(pixelType);
        pixelDataPointer = MarshalToPointer(pixelData);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the type of the indexed pixel format.
    /// </summary>
    internal Type GetIndexedType(PixelFormat pixelFormat)
    {
        switch (pixelFormat)
        {
            case PixelFormat.Format1bppIndexed: return typeof(PixelData1Indexed);
            case PixelFormat.Format4bppIndexed: return typeof(PixelData4Indexed);
            case PixelFormat.Format8bppIndexed: return typeof(PixelData8Indexed);

            default:
                string message = string.Format("This pixel format '{0}' is either non-indexed, or not supported.", pixelFormat);
                throw new NotSupportedException(message);
        }
    }

    /// <summary>
    /// Gets the type of the non-indexed pixel format.
    /// </summary>
    internal Type GetNonIndexedType(PixelFormat pixelFormat)
    {
        switch (pixelFormat)
        {
            case PixelFormat.Format16bppArgb1555: return typeof(PixelDataArgb1555);
            case PixelFormat.Format16bppGrayScale: return typeof(PixelDataGray16);
            case PixelFormat.Format16bppRgb555: return typeof(PixelDataRgb555);
            case PixelFormat.Format16bppRgb565: return typeof(PixelDataRgb565);
            case PixelFormat.Format24bppRgb: return typeof(PixelDataRgb888);
            case PixelFormat.Format32bppRgb: return typeof(PixelDataRgb8888);
            case PixelFormat.Format32bppArgb: return typeof(PixelDataArgb8888);
            case PixelFormat.Format48bppRgb: return typeof(PixelDataRgb48);
            case PixelFormat.Format64bppArgb: return typeof(PixelDataArgb64);

            default:
                string message = string.Format("This pixel format '{0}' is either indexed, or not supported.", pixelFormat);
                throw new NotSupportedException(message);
        }
    }

    private static IntPtr MarshalToPointer(object data)
    {
        int size = Marshal.SizeOf(data);
        IntPtr pointer = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(data, pointer, false);
        return pointer;
    }

    #endregion

    #region Update methods

    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public void Set(int x, int y)
    {
        X = x;
        Y = y;
        bitOffset = Parent.GetBitOffset(x);
    }

    /// <summary>
    /// Reads the raw data.
    /// </summary>
    /// <param name="imagePointer">The image pointer.</param>
    public void ReadRawData(IntPtr imagePointer)
    {
        pixelData = Marshal.PtrToStructure(imagePointer, pixelType);
    }

    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    public void ReadData(byte[] buffer, int offset)
    {
        Marshal.Copy(buffer, offset, pixelDataPointer, Parent.BytesPerPixel);
        pixelData = Marshal.PtrToStructure(pixelDataPointer, pixelType);
    }

    /// <summary>
    /// Writes the raw data.
    /// </summary>
    /// <param name="imagePointer">The image pointer.</param>
    public void WriteRawData(IntPtr imagePointer)
    {
        Marshal.StructureToPtr(pixelData, imagePointer, false);
    }

    /// <summary>
    /// Writes the data.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    public void WriteData(byte[] buffer, int offset)
    {
        Marshal.Copy(pixelDataPointer, buffer, offset, Parent.BytesPerPixel);
    }

    #endregion

    #region IDisposable

    private bool _Disposed = false;

    public void Dispose()
    {
        Dispose(true);
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
            if (pixelDataPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pixelDataPointer);
                pixelDataPointer = IntPtr.Zero;
            }

            _Disposed = true;
        }
    }

    ~PixelAccess()
    {
        Dispose(false);
    }

    #endregion
}