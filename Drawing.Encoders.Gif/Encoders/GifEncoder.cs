using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Codenet.Drawing.Common;
using Codenet.Drawing.Quantizers.NeuQuant;
using Codenet.Drawing.Quantizers.Helpers;

namespace Codenet.Drawing.Encoders;

/// <summary>
/// Based on Nased Kevin Weiner, FM Software
/// </summary>
public class GifEncoder : IDisposable
{
    #region Protected Variables

    private int _Width;
    private int _Height;
    private Color _TransparentColor = Color.Empty;
    private int _TransparentColorIndex;
    private int _Repeat = -1;
    private int _Duration = 0;
    private bool _Started = false;
    private FileStream _FileStream;

    private ImageBuffer _FrameBuffer;
    private byte[] _IndexedPixels;
    private int _ColorDepth;
    private byte[] _ColorTable;
    private int _ColorTableSize = 7;
    private int _DisposalCode = -1;
    private bool _CloseStreamWhenFinished = false;
    private bool _IsFirstFrame = true;
    private bool _HasSize = false;

    #endregion

    #region Protected Properties

    protected int Width
    {
        get { return _Width; }
        set { _Width = value; }
    }

    protected int Height
    {
        get { return _Height; }
        set { _Height = value; }
    }

    protected Color TransparentColor
    {
        get { return _TransparentColor; }
        set { _TransparentColor = value; }
    }

    protected int TransparentColorIndex
    {
        get { return _TransparentColorIndex; }
        set { _TransparentColorIndex = value; }
    }

    protected int Repeat
    {
        get { return _Repeat; }
        set { _Repeat = value; }
    }

    protected int Duration
    {
        get { return _Duration; }
        set { _Duration = value; }
    }

    protected bool Started
    {
        get { return _Started; }
        set { _Started = value; }
    }

    protected FileStream FileStream
    {
        get { return _FileStream; }
        set { _FileStream = value; }
    }

    protected ImageBuffer FrameImage
    {
        get { return _FrameBuffer; }
        set { _FrameBuffer = value; }
    }

    protected byte[] IndexedPixels
    {
        get { return _IndexedPixels; }
        set { _IndexedPixels = value; }
    }

    protected int ColorDepth
    {
        get { return _ColorDepth; }
        set { _ColorDepth = value; }
    }

    protected byte[] ColorTable
    {
        get { return _ColorTable; }
        set { _ColorTable = value; }
    }

    protected int ColorTableSize
    {
        get { return _ColorTableSize; }
        set { _ColorTableSize = value; }
    }

    protected int DisposalCode
    {
        get { return _DisposalCode; }
        set { _DisposalCode = value; }
    }

    protected bool CloseStreamWhenFinished
    {
        get { return _CloseStreamWhenFinished; }
        set { _CloseStreamWhenFinished = value; }
    }

    protected bool IsFirstFrame
    {
        get { return _IsFirstFrame; }
        set { _IsFirstFrame = value; }
    }

    protected bool HasSize
    {
        get { return _HasSize; }
        set { _HasSize = value; }
    }

    #endregion

    #region Public Frame Setters

    /// <summary>
    /// Sets the delay time between each frame, or changes it
    /// for subsequent frames (applies to last frame added).
    /// </summary>
    /// <param name="DurationMilliseconds">delay time in milliseconds</param>
    public void SetNextFrameDuration(int DurationMilliseconds)
    {
        _Duration = (int)Math.Round(DurationMilliseconds / 10.0f);
    }

    /// <summary>
    /// Sets the GIF frame disposal code for the last added frame
    /// and any subsequent frames.  Default is 0 if no transparent
    /// color has been set, otherwise 2.
    /// </summary>
    /// <param name="DisposalCode">disposal code</param>
    public void SetNextFrameDispose(int DisposalCode)
    {
        if (DisposalCode >= 0)
        {
            _DisposalCode = DisposalCode;
        }
    }

    /// <summary>
    /// Sets the transparent color for the next frame and any subsequent frames.
    /// Since all colors are subject to modification
    /// in the quantization process, the color in the final
    /// palette for each frame closest to the given color
    /// becomes the transparent color for that frame.
    /// May be set to Color.Empty to indicate no transparent color.
    /// </summary>
    /// <param name="TransparentColor">Color to be treated as transparent on display</param>
    public void SetNextFrameTransparentColor(Color TransparentColor)
    {
        _TransparentColor = TransparentColor;
    }

    #endregion
    
    #region Public General Setters
    
    /// <summary>
    /// Sets the GIF frame size. The default size is the size of the first frame added if this method is not invoked.
    /// </summary>
    /// <param name="Width">frame width</param>
    /// <param name="Height">frame height</param>
    public void SetSize(int Width, int Height)
    {
        if (_Started && !_IsFirstFrame) return;
        _Width = Width;
        _Height = Height;
        if (_Width < 1) _Width = 320;
        if (_Height < 1) _Height = 240;
        _HasSize = true;
    }

    /// <summary>
    /// Sets the number of times the set of GIF frames
    /// should be played.  Default is 1; 0 means play
    /// indefinitely.  Must be invoked before the first
    /// image is added.
    /// </summary>
    /// <param name="Repeat">number of iterations</param>
    public void SetRepeat(int Repeat)
    {
        if (Repeat >= 0)
        {
            _Repeat = Repeat;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initiates writing of a GIF file with the specified name.
    /// </summary>
    /// <param name="FilePath">String containing output file name.</param>
    /// <returns>false if open or initial write failed.</returns>
    public bool Start(String FilePath)
    {
        bool ok;
        try
        {
            _FileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            ok = Start(_FileStream);
            _CloseStreamWhenFinished = true;
        }
        catch (IOException)
        {
            ok = false;
        }
        return _Started = ok;
    }

    /// <summary>
    /// Initiates GIF file creation on the given stream. The stream is not closed automatically.
    /// </summary>
    /// <param name="OutputStream">Output stream to which GIF images are written</param>
    /// <returns>false if initial write failed.</returns>
    public bool Start(FileStream OutputStream)
    {
        if (OutputStream == null) return false;
        bool ok = true;
        _CloseStreamWhenFinished = false;
        _FileStream = OutputStream;
        try
        {
            WriteString("GIF89a"); // header
        }
        catch (IOException)
        {
            ok = false;
        }
        return _Started = ok;
    }
    
    /// <summary>
    /// Adds next GIF frame.  The frame is not written immediately, but is
    /// actually deferred until the next frame is received so that timing
    /// data can be inserted.  Invoking <code>finish()</code> flushes all
    /// frames.  If <code>setSize</code> was not invoked, the size of the
    /// first image is used for all subsequent frames.
    /// </summary>
    /// <param name="frameBuffer">Image containing frame to write.</param>
    /// <returns>true if successful.</returns>
    public bool AddFrame(ImageBuffer frameBuffer)
    {
        if ((frameBuffer == null) || !_Started)
        {
            return false;
        }
        bool ok = true;
        try
        {
            if (!_HasSize)
            {
                // use first frame's size
                SetSize(frameBuffer.Width, frameBuffer.Height);
            }
            _FrameBuffer = frameBuffer;
            AnalyzePixels(); // build color table & map pixels
            if (_IsFirstFrame)
            {
                WriteLSD(); // logical screen descriptor
                WritePalette(); // global color table
                if (_Repeat >= 0)
                {
                    // use NS app extension to indicate reps
                    WriteNetscapeExt();
                }
            }
            WriteGraphicCtrlExt(); // write graphic control extension
            WriteImageDesc(); // image descriptor
            if (!_IsFirstFrame)
            {
                WritePalette(); // local color table
            }
            WritePixels(); // encode and write pixel data
            _IsFirstFrame = false;
        }
        catch (IOException)
        {
            ok = false;
        }

        return ok;
    }

    /// <summary>
    /// Flushes any pending data and closes output file.
    /// If writing to an OutputStream, the stream is not
    /// closed.
    /// </summary>
    public bool Finish()
    {
        if (!_Started) return false;
        bool ok = true;
        _Started = false;
        try
        {
            _FileStream.WriteByte(0x3b); // gif trailer
            _FileStream.Flush();
            if (_CloseStreamWhenFinished)
            {
                _FileStream.Close();
            }
        }
        catch (IOException)
        {
            ok = false;
        }

        // reset for subsequent use
        _TransparentColorIndex = 0;
        _FileStream = null;
        _FrameBuffer = null;
        _IndexedPixels = null;
        _ColorTable = null;
        _CloseStreamWhenFinished = false;
        _IsFirstFrame = true;

        return ok;
    }

    #endregion

    #region Inner Workings

    /// <summary>
    /// Analyzes image colors and creates color map.
    /// </summary>
    protected void AnalyzePixels()
    {
        NeuralColorQuantizer quantizer = new NeuralColorQuantizer();

        using (var quantizedImageBuffer = ImageBuffer.Allocate(_FrameBuffer.Width, _FrameBuffer.Height, PixelFormatUtility.GetFormatByColorCount(256)))
        {
            _IndexedPixels = new byte[_FrameBuffer.Width * _FrameBuffer.Height];

            var sourceImageBuffer = _FrameBuffer;
            int transparentPixelX = -1, transparentPixelY = -1;
            var hasTransparentColor = _TransparentColor != Color.Empty && sourceImageBuffer.ScanForColor(
                NeatColor.FromARGB(unchecked((UInt32)_TransparentColor.ToArgb())),
                out transparentPixelX, out transparentPixelY);

            sourceImageBuffer.Quantize(quantizedImageBuffer, quantizer, null, 256, 1);
            List<NeatColor> palette = quantizedImageBuffer.Palette;

            _ColorTable = new byte[768];
            int j = 0;
            foreach (var c in palette)
            {
                _ColorTable[j++] = c.Red;
                _ColorTable[j++] = c.Green;
                _ColorTable[j++] = c.Blue;
            }

            using var pixel = new PixelAccess(quantizedImageBuffer);

            j = 0;
            for (int y = 0, x, w = quantizedImageBuffer.Width, h = quantizedImageBuffer.Height; y < h; y++)
            {
                for (x = 0; x < w; x++)
                {
                    pixel.Set(x, y);
                    if (quantizedImageBuffer.CanRead)
                        quantizedImageBuffer.ReadPixel(pixel);
                    _IndexedPixels[j++] = pixel.Index;
                }
            }

            _ColorDepth = 8;
            _ColorTableSize = 7;

            if (hasTransparentColor)
            {
                //Color c = quantizedImageBuffer.ScanForClosestColor(_TransparentColor);
                //_TransparentColorIndex = c.IsEmpty ? 255 : palette.IndexOf(c);

                pixel.Set(transparentPixelX, transparentPixelY);
                if (quantizedImageBuffer.CanRead) 
                    quantizedImageBuffer.ReadPixel(pixel);
                _TransparentColorIndex = pixel.Index;
            }
            else
            {
                _TransparentColorIndex = quantizedImageBuffer.ScanForFirstUnusedColorIndex();
                if (_TransparentColorIndex == -1) _TransparentColorIndex = 255;
            }
        }
    }

    #endregion

    #region Specific Writers

    /// <summary>
    /// Writes Graphic Control Extension
    /// </summary>
    protected void WriteGraphicCtrlExt()
    {
        _FileStream.WriteByte(0x21); // extension introducer
        _FileStream.WriteByte(0xf9); // GCE label
        _FileStream.WriteByte(4); // data block size
        int transp, disp;
        if (_TransparentColor == Color.Empty)
        {
            transp = 0;
            disp = 0; // dispose = no action
        }
        else
        {
            transp = 1;
            disp = 2; // force clear if using transparent color
        }
        if (_DisposalCode >= 0)
        {
            disp = _DisposalCode & 7; // user override
        }
        disp <<= 2;

        // packed fields
        _FileStream.WriteByte(Convert.ToByte(0 | // 1:3 reserved
            disp | // 4:6 disposal
            0 | // 7   user input - 0 = none
            transp)); // 8   transparency flag

        WriteShort(_Duration); // delay x 1/100 sec
        _FileStream.WriteByte(Convert.ToByte(_TransparentColorIndex)); // transparent color index
        _FileStream.WriteByte(0); // block terminator
    }

    /// <summary>
    /// Writes Image Descriptor
    /// </summary>
    protected void WriteImageDesc()
    {
        _FileStream.WriteByte(0x2c); // image separator
        WriteShort(0); // image position x,y = 0,0
        WriteShort(0);
        WriteShort(_Width); // image size
        WriteShort(_Height);
        // packed fields
        if (_IsFirstFrame)
        {
            // no LCT  - GCT is used for first (or only) frame
            _FileStream.WriteByte(0);
        }
        else
        {
            // specify normal LCT
            _FileStream.WriteByte(Convert.ToByte(0x80 | // 1 local color table  1=yes
                0 | // 2 interlace - 0=no
                0 | // 3 sorted - 0=no
                0 | // 4-5 reserved
                _ColorTableSize)); // 6-8 size of color table
        }
    }

    /// <summary>
    /// Writes Logical Screen Descriptor
    /// </summary>
    protected void WriteLSD()
    {
        // logical screen size
        WriteShort(_Width);
        WriteShort(_Height);
        // packed fields
        _FileStream.WriteByte(Convert.ToByte(0x80 | // 1   : global color table flag = 1 (gct used)
            0x70 | // 2-4 : color resolution = 7
            0x00 | // 5   : gct sort flag = 0
            _ColorTableSize)); // 6-8 : gct size

        _FileStream.WriteByte(0); // background color index
        _FileStream.WriteByte(0); // pixel aspect ratio - assume 1:1
    }

    /// <summary>
    /// Writes Netscape application extension to define
    /// repeat count.
    /// </summary>
    protected void WriteNetscapeExt()
    {
        _FileStream.WriteByte(0x21); // extension introducer
        _FileStream.WriteByte(0xff); // app extension label
        _FileStream.WriteByte(11); // block size
        WriteString("NETSCAPE2.0"); // app id + auth code
        _FileStream.WriteByte(3); // sub-block size
        _FileStream.WriteByte(1); // loop sub-block id
        WriteShort(_Repeat); // loop count (extra iterations, 0=repeat forever)
        _FileStream.WriteByte(0); // block terminator
    }

    /// <summary>
    /// Writes color table
    /// </summary>
    protected void WritePalette()
    {
        _FileStream.Write(_ColorTable, 0, _ColorTable.Length);
        int n = (3 * 256) - _ColorTable.Length;
        for (int i = 0; i < n; i++)
        {
            _FileStream.WriteByte(0);
        }
    }

    /// <summary>
    /// Encodes and writes pixel data
    /// </summary>
    protected void WritePixels()
    {
        LZWEncoder encoder = new LZWEncoder(_Width, _Height, _IndexedPixels, _ColorDepth);
        encoder.Encode(_FileStream);
    }

    #endregion

    #region General Writers

    /// <summary>
    /// Write 16-bit value to output stream, LSB first
    /// </summary>
    protected void WriteShort(int Value)
    {
        _FileStream.WriteByte(Convert.ToByte(Value & 0xff));
        _FileStream.WriteByte(Convert.ToByte((Value >> 8) & 0xff));
    }

    /// <summary>
    /// Writes string to output stream
    /// </summary>
    protected void WriteString(string Value)
    {
        char[] chars = Value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            _FileStream.WriteByte((byte)chars[i]);
        }
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
            Finish();
        }
        // Now clean up Native Resources (Pointers)
        _Disposed = true;
    }

    ~GifEncoder()
    {
        Dispose(false);
    }

    #endregion
}
