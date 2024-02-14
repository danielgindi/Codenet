using Codenet.IO;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Codenet.Drawing;

public static class ImageDimensionsDecoder
{
    private readonly static byte[] JPEG_HEADER = new byte[] { 0xff, 0xd8 };
    private readonly static byte[] JPEG_EXIF_HEADER = new byte[] { (byte)'E', (byte)'x', (byte)'i', (byte)'f' };
    private readonly static byte[] PNG_HEADER = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private readonly static byte[] GIF_HEADER = new byte[] { (byte)'G', (byte)'I', (byte)'F' };
    private readonly static byte[] BMP_HEADER = new byte[] { 0x42, 0x4D };
    private readonly static byte[] ICNS_HEADER = new byte[] { (byte)'i', (byte)'c', (byte)'n', (byte)'s' };

    private const UInt16 EXIF_TAGTYPE_BYTE = 1;		// BYTE	    8-bit unsigned integer
    private const UInt16 EXIF_TAGTYPE_SHORT = 3;	// SHORT    16-bit unsigned integer
    private const UInt16 EXIF_TAGTYPE_LONG = 4;		// LONG     32-bit unsigned integer
    private const UInt16 EXIF_TAGTYPE_SBYTE = 6;	// SBYTE    8-bit signed integer
    private const UInt16 EXIF_TAGTYPE_SSHORT = 8;	// SSHORT   16-bit signed integer
    private const UInt16 EXIF_TAGTYPE_SLONG = 9;	// SLONG    32-bit signed integer

    private static ImageSize? GetImageSize_EXIF(FileStream stream)
    {
        byte[] buffer = new byte[4];

        long offset = stream.Position;

        // Read Byte alignment
        if (stream.Read(buffer, 0, 2) != 2) return null;

        bool littleEndian = false;
        if (buffer[0] == 0x49 && buffer[1] == 0x49)
        {
            littleEndian = true;
        }
        else if (buffer[0] != 0x4D && buffer[1] != 0x4D) return null;

        using (var reader = new AnyEndianReader(stream))
        {
            reader.LittleEndian = littleEndian;

            // TIFF tag marker
            if (reader.ReadUInt16() != 0x002A) return null;

            // Directory offset bytes
            UInt32 dirOffset = reader.ReadUInt32();

            ExifPropertyTag tag;
            UInt16 numberOfTags, tagType;
            UInt32 /*tagLength, */tagValue;
            int orientation = 1, width = 0, height = 0;
            UInt32 exifIFDOffset = 0;

            while (dirOffset != 0)
            {
                stream.Seek(offset + dirOffset, SeekOrigin.Begin);

                numberOfTags = reader.ReadUInt16();

                for (UInt16 i = 0; i < numberOfTags; i++)
                {
                    tag = (ExifPropertyTag)reader.ReadUInt16();
                    tagType = reader.ReadUInt16();
                    /*tagLength*/_ = reader.ReadUInt32();

                    if (tag == ExifPropertyTag.PropertyTagOrientation ||
                        tag == ExifPropertyTag.PropertyTagExifPixXDim ||
                        tag == ExifPropertyTag.PropertyTagExifPixYDim ||
                        tag == ExifPropertyTag.PropertyTagImageWidth ||
                        tag == ExifPropertyTag.PropertyTagImageHeight ||
                        tag == ExifPropertyTag.PropertyTagExifIFD)
                    {
                        switch (tagType)
                        {
                            default:
                            case EXIF_TAGTYPE_BYTE:
                            case EXIF_TAGTYPE_SBYTE:
                                tagValue = reader.ReadByte();
                                stream.Seek(3, SeekOrigin.Current);
                                break;
                            case EXIF_TAGTYPE_SHORT:
                            case EXIF_TAGTYPE_SSHORT:
                                tagValue = reader.ReadUInt16();
                                stream.Seek(2, SeekOrigin.Current);
                                break;
                            case EXIF_TAGTYPE_LONG:
                                tagValue = reader.ReadUInt32();
                                break;
                            case EXIF_TAGTYPE_SLONG:
                                tagValue = (UInt32)reader.ReadInt32();
                                break;
                        }

                        switch (tag)
                        {
                            case ExifPropertyTag.PropertyTagOrientation:
                                // Orientation tag
                                orientation = (int)tagValue;
                                break;
                            case ExifPropertyTag.PropertyTagExifPixXDim:
                            case ExifPropertyTag.PropertyTagImageWidth:
                                // Width tag
                                width = (int)tagValue;
                                break;
                            case ExifPropertyTag.PropertyTagExifPixYDim:
                            case ExifPropertyTag.PropertyTagImageHeight:
                                // Height tag
                                height = (int)tagValue;
                                break;
                            case ExifPropertyTag.PropertyTagExifIFD:
                                // EXIF IFD offset tag
                                exifIFDOffset = tagValue;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        stream.Seek(4, SeekOrigin.Current);
                    }
                }

                if (dirOffset == exifIFDOffset)
                {
                    break;
                }

                dirOffset = reader.ReadUInt32();

                if (dirOffset == 0)
                {
                    dirOffset = exifIFDOffset;
                }
            }

            if (width > 0 && height > 0)
            {
                return new ImageSize
                {
                    RawSize = new Size(width, height),
                    ExifOrientation = orientation
                };
            }
        }

        return null;
    }

    private static ImageSize? GetImageSize_JPEG(FileStream stream)
    {
        byte[] buffer = new byte[4];

        while (stream.Read(buffer, 0, 2) == 2 && buffer[0] == 0xFF &&
            ((buffer[1] >= 0xE0 && buffer[1] <= 0xEF) ||
            buffer[1] == 0xDB ||
            buffer[1] == 0xC4 || buffer[1] == 0xC2 ||
            buffer[1] == 0xC0))
        {
            if (buffer[1] == 0xE1)
            { // Parse APP1 EXIF

                // Marker segment length
                if (stream.Read(buffer, 0, 2) != 2) return null;
                // int blockLength = ((buffer[0] << 8) | buffer[1]) - 2;

                // Exif
                if (stream.Read(buffer, 0, 4) != 4
                    || !CompareBytes(buffer, JPEG_EXIF_HEADER, 4)) return null;

                // Read Byte alignment offset
                if (stream.Read(buffer, 0, 2) != 2 ||
                    buffer[0] != 0x00 || buffer[1] != 0x00) return null;

                var size = GetImageSize_EXIF(stream);
                if (size != null)
                    return size;
            }
            else if (buffer[1] == 0xC0 || buffer[1] == 0xC2)
            { // Parse SOF0 (Start of Frame, Baseline DCT or Progressive DCT)

                // Skip LF, P
                stream.Seek(3, SeekOrigin.Current);

                // Read Y,X
                if (stream.Read(buffer, 0, 4) != 4) return null;

                var originalSize = new Size(buffer[2] << 8 | buffer[3], buffer[0] << 8 | buffer[1]);

                return new ImageSize
                {
                    RawSize = originalSize,
                    ExifOrientation = 1
                };
            }
            else
            { // Skip APPn segment
                if (stream.Read(buffer, 0, 2) == 2)
                { // Marker segment length
                    stream.Seek((int)((buffer[0] << 8) | buffer[1]) - 2, SeekOrigin.Current);
                }
                else
                {
                    return null;
                }
            }
        }

        return null;
    }

    #region ICNS Definitions

    private struct AppleIconInfo
    {
        public AppleIconInfo(string osType, Int32 width, Int32 height)
        {
            this.osType = ASCIIEncoding.ASCII.GetBytes(osType.ToCharArray(), 0, 4);
            this.width = width;
            this.height = height;
        }

        public byte[] osType;
        public Int32 width;
        public Int32 height;
    }

    private static AppleIconInfo[] appleIconInfoTable = new AppleIconInfo[]{
       //	OSType	Width, Height		        // Length	Size	Supported OS Ver	Description
       //					                    // (bytes)	(pixels)
       new AppleIconInfo("ICON", 32, 32),		// 128		32	    1.0	                32—32 1-bit mono icon
       new AppleIconInfo("ICN#", 32, 32),		// 256		32	    6.0	                32—32 1-bit mono icon with 1-bit mask
       new AppleIconInfo("icm#", 16, 12),		// 48		16	    6.0	                16—12 1 bit mono icon with 1-bit mask
       new AppleIconInfo("icm4", 16, 12),		// 96		16	    7.0	                16—12 4 bit icon
       new AppleIconInfo("icm8", 16, 12),		// 192		16	    7.0	                16—12 8 bit icon
       new AppleIconInfo("ics#", 16, 16),		// 64 (32 img + 32 mask) 16	6.0	        16—16 1-bit mask
       new AppleIconInfo("ics4", 16, 16),		// 128		16	    7.0	                16—16 4-bit icon
       new AppleIconInfo("ics8", 16, 16),		// 256		16	    7.0	                16x16 8 bit icon
       new AppleIconInfo("is32", 16, 16),		// varies (768)	16	8.5	                16—16 24-bit icon
       new AppleIconInfo("s8mk", 16, 16),		// 256		16	    8.5	                16x16 8-bit mask
       new AppleIconInfo("icl4", 32, 32),		// 512		32	    7.0	                32—32 4-bit icon
       new AppleIconInfo("icl8", 32, 32),		// 1,024	32	    7.0	                32—32 8-bit icon
       new AppleIconInfo("il32", 32, 32),		// var (3,072) 32	8.5	                32x32 24-bit icon
       new AppleIconInfo("l8mk", 32, 32),		// 1,024	32	    8.5	                32—32 8-bit mask
       new AppleIconInfo("ich#", 48, 48),		// 288		48	    8.5	                48—48 1-bit mask
       new AppleIconInfo("ich4", 48, 48),		// 1,152	48	    8.5	                48—48 4-bit icon
       new AppleIconInfo("ich8", 48, 48),		// 2,304	48	    8.5	                48—48 8-bit icon
       new AppleIconInfo("ih32", 48, 48),		// var (6,912) 48	8.5	                48—48 24-bit icon
       new AppleIconInfo("h8mk", 48, 48),		// 2,304	48	    8.5	                48—48 8-bit mask
       new AppleIconInfo("it32", 128, 128),	    // var (49,152) 128	10.0	            128—128 24-bit icon
       new AppleIconInfo("t8mk", 128, 128),	    // 16,384	128	    10.0	            128—128 8-bit mask
       new AppleIconInfo("icp4", 16, 16),		// var	    16	    10.7	            16x16 icon in JPEG 2000 or PNG format
       new AppleIconInfo("icp5", 32, 32),		// var	    32	    10.7	            32x32 icon in JPEG 2000 or PNG format
       new AppleIconInfo("icp6", 64, 64),		// var	    64	    10.7	            64x64 icon in JPEG 2000 or PNG format
       new AppleIconInfo("ic07", 128, 128),	    // var	    128	    10.7	            128x128 icon in JPEG 2000 or PNG format
       new AppleIconInfo("ic08", 256, 256),	    // var	    256	    10.5	            256—256 icon in JPEG 2000 or PNG format
       new AppleIconInfo("ic09", 512, 512),	    // var	    512	    10.5	            512—512 icon in JPEG 2000 or PNG format
       new AppleIconInfo("ic10", 1024, 1024),	// var	    1024	10.7	            1024—1024 in 10.7 (or 512x512@2x "retina" in 10.8) icon in JPEG 2000 or PNG format
       new AppleIconInfo("ic11", 32, 32),		// var	    32	    10.8	            16x16@2x "retina" icon in JPEG 2000 or PNG format
       new AppleIconInfo("ic12", 64, 64),		// var	    64	    10.8	            32x32@2x "retina" icon in JPEG 2000 or PNG format
       new AppleIconInfo("ic13", 256, 256),	    // var	    256	    10.8	            128x128@2x "retina" icon in JPEG 2000 or PNG format
       new AppleIconInfo("ic14", 512, 512),	    // var	    512	    10.8	            256x256@2x "retina" icon in JPEG 2000 or PNG format
       new AppleIconInfo("----", 0, 0),		    // end marker for search failure
    };

    #endregion

    private static ImageSize? GetImageSize_ICNS(FileStream stream)
    {
        ImageSize? size = null;
        byte[] buffer = new byte[4];
        byte[] iconType = new byte[4];

        // Attempt to read ICNS header
        // Read ICNS magic number (always "icns")
        if (stream.Read(buffer, 0, 4) != 4
            || !CompareBytes(buffer, ICNS_HEADER, 4)) return null;

        int width = 0, height = 0;

        using (var reader = new AnyEndianReader(stream))
        {
            reader.LittleEndian = false;

            // Read the length of the file in bytes
            UInt32 fileSize = reader.ReadUInt32();

            UInt32 dataLength, filepos = 8;
            int i;

            do
            {
                // Read the icon type
                if (stream.Read(iconType, 0, 4) != 4) return null;

                // Read the Length of data, in bytes (including type and length), msb first
                dataLength = reader.ReadUInt32();

                for (i = 0; appleIconInfoTable[i].width > 0; ++i)
                {
                    if (CompareBytes(iconType, appleIconInfoTable[i].osType))
                    {
                        if (appleIconInfoTable[i].width > width)
                        {
                            width = appleIconInfoTable[i].width;
                        }
                        if (appleIconInfoTable[i].height > height)
                        {
                            height = appleIconInfoTable[i].height;
                        }
                        break;
                    }
                }

                filepos += dataLength;
            } while (filepos < fileSize && stream.Seek(dataLength - 8, SeekOrigin.Current) == 0);
        }

        if (width > 0 && height > 0)
        {
            size = new ImageSize { RawSize = new Size(width, height) };
        }

        return size;
    }

    public static ImageSize? GetImageRawSizeAndOrientation(string path)
    {
        using (var fileStream = File.OpenRead(path))
        {
            byte[] buffer = new byte[4];

            try
            {
                if (fileStream.Read(buffer, 0, 2) == 2 &&
                    CompareBytes(buffer, JPEG_HEADER, 2))
                {// JPEG                        
                    var size = GetImageSize_JPEG(fileStream);
                    if (size != null)
                        return size;
                }
            }
            catch { }

            try
            {
                fileStream.Seek(0, SeekOrigin.Begin);

                byte[] buffer8 = new byte[8];

                if (fileStream.Read(buffer8, 0, 8) == 8 &&
                    CompareBytes(buffer, PNG_HEADER))
                {
                    // PNG

                    fileStream.Seek(8, SeekOrigin.Current);

                    var originalSize = new Size();
                    var success = false;

                    if (fileStream.Read(buffer, 0, 4) == 4)
                    {
                        originalSize.Width = (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
                    }
                    if (fileStream.Read(buffer, 0, 4) == 4)
                    {
                        originalSize.Height = (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
                        success = true;
                    }

                    if (success)
                        return new ImageSize { RawSize = originalSize };
                }
            }
            catch { }

            try
            {
                fileStream.Seek(0, SeekOrigin.Begin);

                if (fileStream.Read(buffer, 0, 3) == 3 &&
                    CompareBytes(buffer, GIF_HEADER, 3))
                {
                    // GIF

                    fileStream.Seek(3, SeekOrigin.Current); // 87a / 89a

                    var originalSize = new Size();
                    var success = false;

                    if (fileStream.Read(buffer, 0, 4) == 4)
                    {
                        originalSize.Width = (buffer[1] << 8) | buffer[0];
                        originalSize.Height = (buffer[3] << 8) | buffer[2];
                        success = true;
                    }

                    if (success)
                        return new ImageSize { RawSize = originalSize };
                }
            }
            catch { }

            try
            {
                fileStream.Seek(0, SeekOrigin.Begin);

                if (fileStream.Read(buffer, 0, 2) == 2 &&
                    CompareBytes(buffer, BMP_HEADER, 2))
                {
                    // BMP

                    fileStream.Seek(16, SeekOrigin.Current);

                    var originalSize = new Size();
                    var success = false;

                    if (fileStream.Read(buffer, 0, 4) == 4)
                    {
                        originalSize.Width = (buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | buffer[0];
                    }

                    if (fileStream.Read(buffer, 0, 4) == 4)
                    {
                        originalSize.Height = (buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | buffer[0];
                        success = true;
                    }
                        
                    if (success)
                        return new ImageSize { RawSize = originalSize };
                }
            }
            catch { }

            try
            {
                fileStream.Seek(0, SeekOrigin.Begin);

                // Try the ICNS format
                var size = GetImageSize_ICNS(fileStream);
                if (size != null)
                    return size;
            }
            catch { }

            try
            {
                fileStream.Seek(0, SeekOrigin.Begin);

                // TIFF starts with just plain EXIF
                var size = GetImageSize_EXIF(fileStream);
                if (size != null)
                    return size;
            }
            catch { }
        }

        return null;
    }

    public static Size? GetImageSize(string path)
    {
        return GetImageRawSizeAndOrientation(path)?.TransformedSize;
    }

    #region Bytes compare functions

    private static bool CompareBytes(byte[] a, byte[] b, int length)
    {
        int aLen = a.Length;
        int bLen = b.Length;
        if ((aLen < length ? aLen : length) != (bLen < length ? bLen : length)) return false;

        for (int i = 0; i < length; i++)
        {
            if (a[i] != b[i])
                return false;
        }
        return true;
    }

    private static bool CompareBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;

        int length = a.Length;
        for (int i = 0; i < length; i++)
        {
            if (a[i] != b[i])
                return false;
        }
        return true;
    }

    #endregion

    public struct ImageSize
    {
        /// <summary>
        /// Returns <code>RawSize.IsEmpty</code>
        /// </summary>
        public bool IsEmpty { get { return RawSize.IsEmpty; } }

        private Size _TransformedSize;

        public Size RawSize { get; set; }

        public Size TransformedSize
        {
            get
            {
                if (_TransformedSize.IsEmpty)
                {
                    if (RawSize == Size.Empty)
                    {
                        _TransformedSize = RawSize;
                    }
                    else
                    {
                        if (ExifOrientation >= 5 && ExifOrientation <= 8)
                        {
                            _TransformedSize = new Size(RawSize.Height, RawSize.Width);
                        }
                        else
                        {
                            _TransformedSize = new Size(RawSize.Width, RawSize.Height);
                        }
                    }
                }

                return _TransformedSize;
            }
        }

        /// <summary>
        /// Exif orientation tag. Values are:
        /// 1										Horizontal (normal) 
        /// 2										Mirror horizontal
        /// 3										Rotate 180
        /// 4										Mirror vertical
        /// 5										Mirror horizontal and rotate 270 CW
        /// 6										Rotate 90 CW
        /// 7										Mirror horizontal and rotate 90 CW
        /// 8										Rotate 270 CW
        /// </summary>
        public int ExifOrientation { get; set; }

        public bool ShouldFlipHorizontally
        {
            get { return ExifOrientation == 2 || ExifOrientation == 5 || ExifOrientation == 7; }
        }

        public bool ShouldFlipVertically
        {
            get { return ExifOrientation == 4; }
        }

        public int DegreesToRotate
        {
            get
            {
                switch (RequiredRotationToRevert)
                {
                    default: return 0;

                    case RotateFlipType.Rotate180:
                        return 180;

                    case RotateFlipType.Rotate270:
                    case RotateFlipType.FlipXRotate270:
                        return 270;

                    case RotateFlipType.Rotate90:
                    case RotateFlipType.FlipXRotate90:
                        return 90;
                }
            }
        }

        public RotateFlipType RequiredRotationToRevert
        {
            get
            {
                switch (ExifOrientation)
                {
                    default:
                    case 1: return RotateFlipType.None;
                    case 2: return RotateFlipType.FlipX;
                    case 3: return RotateFlipType.Rotate180;
                    case 4: return RotateFlipType.FlipY;
                    case 5: return RotateFlipType.FlipXRotate270;
                    case 6: return RotateFlipType.Rotate90;
                    case 7: return RotateFlipType.FlipXRotate90;
                    case 8: return RotateFlipType.Rotate270;
                }
            }
        }
    }

    private enum ExifPropertyTag : int
    {
        /// <summary>
        /// Private property used by the Imaging API.
        /// Not for public use.
        /// The Imaging API uses this property to locate Exif-specific information.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagExifIFD = 0x8769,

        /// <summary>
        /// Information specific to compressed data.
        /// When a compressed file is recorded, the valid width of the meaningful image must be recorded in this tag, whether or not there is padding data or a restart marker.
        /// This property should not exist in an uncompressed file.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagExifPixXDim = 0xA002,

        /// <summary>
        /// Information specific to compressed data.
        /// When a compressed file is recorded, the valid height of the meaningful image must be recorded in this property's setting whether or not there is padding data or a restart marker.
        /// This property should not exist in an uncompressed file.
        /// Because data padding is unnecessary in the vertical direction, the number of lines recorded in this valid image height property will be the same as that recorded in the SOF.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagExifPixYDim = 0xA003,

        /// <summary>
        /// Number of pixel rows.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagImageHeight = 0x0101,

        /// <summary>
        /// Number of pixels per row.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagImageWidth = 0x0100,

        /// <summary>
        /// Image orientation viewed in terms of rows and columns.
        /// PropertyTagOrientation Setting			Description
        /// 1										Horizontal (normal) 
        /// 2										Mirror horizontal
        /// 3										Rotate 180
        /// 4										Mirror vertical
        /// 5										Mirror horizontal and rotate 270 CW
        /// 6										Rotate 90 CW
        /// 7										Mirror horizontal and rotate 90 CW
        /// 8										Rotate 270 CW
        /// 
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagOrientation = 0x0112,
    }

    public enum RotateFlipType
    {
        None,
        FlipX,
        Rotate180,
        FlipY,
        FlipXRotate270,
        Rotate90,
        Rotate270FlipY = 5,
        FlipXRotate90,
        Rotate270,
    }
}
