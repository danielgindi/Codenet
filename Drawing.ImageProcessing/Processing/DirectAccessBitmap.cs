using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Codenet.Drawing.ImageProcessing.Processing;

public class DirectAccessBitmap : IDisposable
{
    #region Private Member Variables
    private DirectAccessBitmap _Owner;
    private SharedPinnedByteArray _ByteArray;
    private Bitmap _Bitmap;
    private int _Stride;
    private int _PixelFormatSize;
    private int _StartX;
    private int _StartY;
    private int _Width;
    private int _Height;
    private int _OriginalWidth;
    private int _OriginalHeight;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new DirectAccessBitmap with the specified pixel format and size, 
    /// and copies the bitmap passed in onto the buffer. The source bitmap is stretched to 
    /// fit the new size.
    /// </summary>
    /// <param name="source">source bitmap to copy from</param>
    public DirectAccessBitmap(Bitmap source)
        : this(source, source.PixelFormat)
    {
    }

    /// <summary>
    /// Creates a new DirectAccessBitmap with the specified pixel format and size, 
    /// and copies the bitmap passed in onto the buffer. The source bitmap is stretched to 
    /// fit the new size.
    /// </summary>
    /// <param name="source">source bitmap to copy from</param>
    /// <param name="pixelFormat">pixelFormat for the new bitmap</param>
    public DirectAccessBitmap(Bitmap source, PixelFormat pixelFormat)
        : this(source.Width, source.Height, pixelFormat)
    {
        using (Graphics g = Graphics.FromImage(_Bitmap))
        {
            g.DrawImage(source, 0, 0, source.Width, source.Height);
        }
    }

    /// <summary>
    /// Creates a new, blank DirectAccessBitmap with the specified width, height, and pixel format.
    /// </summary>
    /// <param name="width">width of the new bitmap</param>
    /// <param name="height">height of the new bitmap</param>
    /// <param name="pixelFormat">pixel format of the new bitmap</param>
    public DirectAccessBitmap(int width, int height, PixelFormat pixelFormat)
    {
        if ((pixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            throw new ArgumentException("Cannot work with indexed image formats. you need to convert to non-indexed, and after processing convert back to indexed.", @"pixelFormat");
        _PixelFormatSize = Image.GetPixelFormatSize(pixelFormat);
        _Stride = ComputeStride(width, _PixelFormatSize);
        _ByteArray = new SharedPinnedByteArray(_Stride * height);
        _Bitmap = new Bitmap(width, height, _Stride, pixelFormat, _ByteArray.BitsPtr);
        _StartX = _StartY = 0;
        _Width = _OriginalWidth = width;
        _Height = _OriginalHeight = height;
    }

    #endregion

    #region Public Properties
    /// <summary>
    /// The <see cref="DirectAccessBitmap"/> that this <see cref="DirectAccessBitmap"/> is a view on.
    /// This property's value will be null if this DirectAccessBitmap is not a view on another 
    /// <see cref="DirectAccessBitmap"/>.
    /// </summary>
    public DirectAccessBitmap Owner
    {
        get { return _Owner; }
    }

    /// <summary>
    /// Returns the array of the bitmap's bits.
    /// </summary>
    public byte[] Bits
    {
        get { return _ByteArray.Bits; }
    }

    /// <summary>
    /// Gets a safe pointer to the buffer containing the bitmap bits.
    /// </summary>
    public IntPtr BitsPtr
    {
        get { return _ByteArray.BitsPtr; }
    }

    /// <summary>
    /// Gets the underlying <see cref="System.Drawing.Bitmap"/>
    /// that this DirectAccessBitmap wraps.
    /// </summary>
    public Bitmap Bitmap
    {
        get { return _Bitmap; }
    }

    /// <summary>
    /// Gets the pixel format size in bits
    /// </summary>
    public int PixelFormatSize
    {
        get { return _PixelFormatSize; }
    }

    /// <summary>
    /// Gets the stride of the bitmap.
    /// </summary>
    public int Stride
    {
        get { return _Stride; }
    }

    /// <summary>
    /// Gets the x position of the beginning of the current view
    /// </summary>
    public int StartX
    {
        get { return _StartX; }
    }

    /// <summary>
    /// Gets the y position of the beginning of the current view
    /// </summary>
    public int StartY
    {
        get { return _StartY; }
    }

    /// <summary>
    /// Gets the width of the current bitmap.
    /// </summary>
    public int Width
    {
        get { return _Width; }
    }

    /// <summary>
    /// Gets the height of the current bitmap.
    /// </summary>
    public int Height
    {
        get { return _Height; }
    }

    /// <summary>
    /// Gets the original width of the current bitmap (if this is a view on another bitmap).
    /// </summary>
    public int OriginalWidth
    {
        get { return _OriginalWidth; }
    }

    /// <summary>
    /// Gets the original height of the current bitmap(if this is a view on another bitmap).
    /// </summary>
    public int OriginalHeight
    {
        get { return _OriginalHeight; }
    }

    #endregion

    #region View Support

    /// <summary>
    /// Creates an <see cref="DirectAccessBitmap"/> as a view on a section of an existing <see cref="DirectAccessBitmap"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="viewArea"></param>
    protected DirectAccessBitmap(DirectAccessBitmap source, Rectangle viewArea)
    {
        _Owner = source;
        _PixelFormatSize = source._PixelFormatSize;
        _Stride = source._Stride;
        _ByteArray = source._ByteArray;
        _ByteArray.AddReference();
        _OriginalWidth = source._OriginalWidth;
        _OriginalHeight = source._OriginalHeight;

        if (viewArea.X < 0) viewArea.X = 0;
        if (viewArea.Y < 0) viewArea.Y = 0;
        if (viewArea.Width > Owner.Width) viewArea.Width = Owner.Width;
        if (viewArea.Height > Owner.Height) viewArea.Height = Owner.Height;

        _StartX = Owner.StartX + viewArea.X;
        _StartY = Owner.StartY + viewArea.Y;

        try
        {
            int ownerStartOffset = (_Stride * _StartY) + (_StartX * (_PixelFormatSize / 8));
            _Bitmap = new Bitmap(viewArea.Width, viewArea.Height, _Stride, source.Bitmap.PixelFormat,
                (IntPtr)(((int)_ByteArray.BitsPtr) + ownerStartOffset));
            _Width = viewArea.Width;
            _Height = viewArea.Height;
        }
        finally
        {
            if (_Bitmap == null)
            {
                _ByteArray.ReleaseReference();
                _ByteArray = null;
            }
        }
    }
	
    /// <summary>
    /// Creates an <see cref="DirectAccessBitmap"/> as a view on a section of an existing <see cref="DirectAccessBitmap"/>.
    /// </summary>
    /// <param name="viewArea">The area that should form the bounds of the view.</param>
    public DirectAccessBitmap CreateView(Rectangle viewArea)
    {
        if(_Disposed) throw new ObjectDisposedException("this");
        return new DirectAccessBitmap(this, viewArea);
    }

    public Bitmap DetachBitmap()
    {
        Bitmap bmp = _Bitmap;
        _Bitmap = null;

        if (_ByteArray != null)
        {
            _ByteArray.ReleaseReference();
            if (Owner != null)
            {
                _ByteArray.Dispose();
            }
            _ByteArray = null;
        }

        return bmp;
    }

    private static int ComputeStride(int width, int bitsPerPixel)
    {
        int stride = width * (bitsPerPixel / 8);
        int padding = (stride % 4);
        stride += (padding == 0) ? 0 : (4 - padding);
        return stride;
    }

    #endregion

    #region IDisposable Members

    private bool _Disposed = false;
    public void Dispose()
    {
        Dispose(true);
    }

    protected void Dispose(bool disposing)
    {
        if (_Disposed) return;

        if (disposing)
        {
            // Release Managed Resources
            if (_ByteArray != null)
            {
                _ByteArray.ReleaseReference();
                if (Owner != null)
                {
                    _ByteArray.Dispose();
                }
                _ByteArray = null;
            }
            if (_Bitmap != null)
            {
                _Bitmap.Dispose();
                _Bitmap = null;
            }
        }
        // Now clean up Native Resources (Pointers)
        _Disposed = true;
    }

    ~DirectAccessBitmap()
    {
        Dispose(false);
    }

    #endregion
}
