namespace Codenet.Drawing.ImageProcessing
{
    public enum ExifPropertyTag : int
    {
        /// <summary>
        /// Null-terminated character string that specifies the name of the person who created the image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagArtist = 0x013B,

        /// <summary>
        /// Number of bits per color component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeShort * Number of samples (components) per pixel
        /// </summary>
        PropertyTagBitsPerSample = 0x0102,

        /// <summary>
        /// Height of the dithering or halftoning matrix.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagCellHeight = 0x0109,

        /// <summary>
        /// Width of the dithering or halftoning matrix.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagCellWidth = 0x0108,

        /// <summary>
        /// Chrominance table.
        /// The luminance table and the chrominance table are used to control JPEG quality.
        /// A valid luminance or chrominance table has 64 entries of type PropertyTagTypeShort.
        /// If an image has a luminance table or a chrominance table, it must have both tables.
        ///  
        ///  PropertyTagTypeShort * 64
        /// </summary>
        PropertyTagChrominanceTable = 0x5091,

        /// <summary>
        /// Color palette (lookup table) for a palette-indexed image.
        ///  
        ///  PropertyTagTypeShort * Number of 16-bit words required for the palette
        /// </summary>
        PropertyTagColorMap = 0x0140,

        /// <summary>
        /// Table of values that specify color transfer functions.
        ///  
        ///  PropertyTagTypeUndefined * Any
        /// </summary>
        PropertyTagColorTransferFunction = 0x501A,

        /// <summary>
        /// Compression scheme used for the image data.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagCompression = 0x0103,

        /// <summary>
        /// Null-terminated character string that contains copyright information.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagCopyright = 0x8298,

        /// <summary>
        /// Date and time the image was created.
        ///  
        ///  PropertyTagTypeASCII * 20
        /// </summary>
        PropertyTagDateTime = 0x0132,

        /// <summary>
        /// Null-terminated character string that specifies the name of the document from which the image was scanned.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagDocumentName = 0x010D,

        /// <summary>
        /// Color component values that correspond to a 0 percent dot and a 100 percent dot.
        ///  
        ///  PropertyTagTypeByte or PropertyTagTypeShort * 2 or 2×PropertyTagSamplesPerPixel
        /// </summary>
        PropertyTagDotRange = 0x0150,

        /// <summary>
        /// Null-terminated character string that specifies the manufacturer of the equipment used to record the image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagEquipMake = 0x010F,

        /// <summary>
        /// Null-terminated character string that specifies the model name or model number of the equipment used to record the image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagEquipModel = 0x0110,

        /// <summary>
        /// Lens aperture.
        /// The unit is the APEX value.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifAperture = 0x9202,

        /// <summary>
        /// Brightness value.
        /// The unit is the APEX value. Ordinarily it is given in the range of -99.99 to 99.99.
        ///  
        ///  PropertyTagTypeSRational * 1
        /// </summary>
        PropertyTagExifBrightness = 0x9203,

        /// <summary>
        /// The color filter array (CFA) geometric pattern of the image sensor when a one-chip color area sensor is used. It does not apply to all sensing methods.
        ///  
        ///  PropertyTagTypeUndefined * Any
        /// </summary>
        PropertyTagExifCfaPattern = 0xA302,

        /// <summary>
        /// Color space specifier.
        /// Normally sRGB (=1) is used to define the color space based on the PC monitor conditions and environment.
        /// If a color space other than sRGB is used, Uncalibrated (=0xFFFF) is set.
        /// Image data recorded as Uncalibrated can be treated as sRGB when it is converted to FlashPix.
        ///
        /// PropertyTagExifColorSpace Value		Description
        /// 0x1									The sRGB color space.
        /// 0xFFFF								Uncalibrated.
        /// Other								Reserved.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagExifColorSpace = 0xA001,

        /// <summary>
        /// Information specific to compressed data. The compression mode used for a compressed image is indicated in unit BPP.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifCompBPP = 0x9102,

        /// <summary>
        /// Information specific to compressed data. The channels of each component are arranged in order from the first component to the fourth.
        /// For uncompressed data, the data arrangement is given in the PropertyTagPhotometricInterp tag. However, because PropertyTagPhotometricInterp can only express the order of Y, Cb, and Cr, this property is provided for cases when compressed data uses components other than Y, Cb, and Cr and to support other sequences.
        /// Component Identifier	Description
        /// 0						The color component does not exist.
        /// 1						The luminance (Y) color component.
        /// 2						The blue chrominance (Cb) color component.
        /// 3						The red chrominance (Cr) color component.
        /// 4						The red (R) color component.
        /// 5						The green (G) color component.
        /// 6						The blue (B) color component.
        /// Other					Reserved.
        /// The default setting for the PropertyTagExifCompConfig value is '4 5 6 0' for uncompressed RGB data an '1 2 3 0' for all other cases.
        ///  
        ///  PropertyTagTypeUndefined * 4
        /// </summary>
        PropertyTagExifCompConfig = 0x9101,

        /// <summary>
        /// Date and time when the image was stored as digital data.
        /// The format is YYYY:MM:DD HH:MM:SS, with time shown in 24-hour format and the date and time separated by one blank character (0x2000).
        /// The character string length is 20 bytes including the NULL terminator.
        /// When the field is empty, it is treated as unknown.
        ///  
        ///  PropertyTagTypeASCII * 20
        /// </summary>
        PropertyTagExifDTDigitized = 0x9004,

        /// <summary>
        /// Null-terminated character string that specifies a fraction of a second for the PropertyTagExifDTDigitized tag.
        /// Tag 0x9292 
        /// Type ASCII 
        /// Count Length of the string including the NULL terminato
        /// PropertyTagExifDTOrig
        /// Date and time when the original image data was generated.
        /// For a digital camera, the date and time when the picture was taken.
        /// The format is YYYY:MM:DD HH:MM:SS with time shown in 24-hour format and the date and time separated by one blank character (0x2000).
        /// The character string length is 20 bytes including the NULL terminator.
        /// When the field is empty, it is treated as unknown.
        ///  
        ///  PropertyTagTypeASCII * 20
        /// </summary>
        PropertyTagExifDTDigSS = 0x9003,

        /// <summary>
        /// Null-terminated character string that specifies a fraction of a second for the PropertyTagExifDTOrig tag.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagExifDTOrigSS = 0x9291,

        /// <summary>
        /// Null-terminated character string that specifies a fraction of a second for the PropertyTagDateTime tag.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagExifDTSubsec = 0x9290,

        /// <summary>
        /// Exposure bias.
        /// The unit is the APEX value.
        /// Ordinarily it is given in the range -99.99 to 99.99.
        ///  
        ///  PropertyTagTypeSRational * 1
        /// </summary>
        PropertyTagExifExposureBias = 0x9204,

        /// <summary>
        /// Exposure index selected on the camera or input device at the time the image was captured.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifExposureIndex = 0xA215,

        /// <summary>
        /// Class of the program used by the camera to set exposure when the picture is taken.
        /// PropertyTagExifExposureProg Value	Description
        /// 0									Not defined
        /// 1									Manual
        /// 2									Normal program
        /// 3									Aperture priority
        /// 4									Shutter priority
        /// 5									Creative program (biased toward depth of field)
        /// 6									Action program (biased toward fast shutter speed)
        /// 7									Portrait mode (for close-up photos with the background out of focus)
        /// 8									Landscape mode (for landscape photos with the background in focus)
        /// 9 to 255							Reserved
        /// The default setting for the this property is 0
        /// Tag 0x8822 
        /// Type SHORT 
        /// Count 1
        /// PropertyTagExifExposureTime
        /// Exposure time, measured in second
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifExposureProg = 0x829A,

        /// <summary>
        /// The image source.
        /// If a digital camera recorded the image, the setting for this property is 3.
        ///  
        ///  PropertyTagTypeUndefined * 1
        /// </summary>
        PropertyTagExifFileSource = 0xA300,

        /// <summary>
        /// Flash status.
        /// This property is recorded when an image is taken using a strobe light (flash).
        /// Bit 0 indicates the flash firing status, and bits 1 and 2 indicate the flash return status.
        /// The following table shows the possible values for bit 0.
        /// Bit 0 Value			Description
        /// 0					The flash did not fire
        /// 1					The flash fired
        /// The following table shows the possible values for bits 1 and 2.
        /// Bit 1 and 2 Values	Description
        /// 00					No strobe return detection function
        /// 01					Reserved
        /// 10					Strobe return light not detected
        /// 11					Strobe return light detected
        /// The following table shows examples of PropertyTagExifFlash settings.
        /// PropertyTagExifFlash Setting	Description
        /// 0x0000							Flash did not fire
        /// 0x0001							Flash fired
        /// 0x0005							Flash fired, but no strobe return light was detected
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagExifFlash = 0x9209,

        /// <summary>
        /// Strobe energy, in Beam Candle Power Seconds (BCPS), at the time the image was captured.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifFlashEnergy = 0xA20B,

        /// <summary>
        /// The F number at the time the image was taken.
        /// Tag 0x829D 
        /// Type RATIONAL 
        /// Count 1
        /// PropertyTagExifFocalLength
        /// Actual focal length, in millimeters, of the lens.
        /// Conversion is not made to the focal length of a 35 millimeter film camera.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifFNumber = 0x920A,

        /// <summary>
        /// Unit of measure for PropertyTagExifFocalXRes and PropertyTagExifFocalYRes.
        /// A setting of 2 indicates inches and a setting of 3 indicates centimeters.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagExifFocalResUnit = 0xA210,

        /// <summary>
        /// Number of pixels in the image width (x) direction per unit on the camera focal plane.
        /// The unit is specified in PropertyTagExifFocalResUnit.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifFocalXRes = 0xA20E,

        /// <summary>
        /// Number of pixels in the image height (y) direction per unit on the camera focal plane.
        /// The unit is specified in PropertyTagExifFocalResUnit.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifFocalYRes = 0xA20F,

        /// <summary>
        /// FlashPix format version supported by an FPXR file.
        /// If the FPXR function supports FlashPix format version 1.0, this is indicated similarly to PropertyTagExifVer by recording 0100 as a 4-byte ASCII string.
        /// Because the type is PropertyTagTypeUndefined, there is no NULL terminator.
        /// The default setting for this property is 0100, all other values are reserved.
        ///  
        ///  PropertyTagTypeUndefined * 4
        /// </summary>
        PropertyTagExifFPXVer = 0xA000,

        /// <summary>
        /// Private property used by the Imaging API.
        /// Not for public use.
        /// The Imaging API uses this property to locate Exif-specific information.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagExifIFD = 0x8769,

        /// <summary>
        /// Offset to a block of property items that contain interoperability information.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagExifInterop = 0xA005,

        /// <summary>
        /// ISO speed and ISO latitude of the camera or input device as specified in ISO 12232.
        ///  
        ///  PropertyTagTypeShort * Any
        /// </summary>
        PropertyTagExifISOSpeed = 0x8827,

        /// <summary>
        /// Type of light source.
        /// PropertyTagExifLightSource Settings		Description
        /// 0										Unknown lighting
        /// 1										Daylight
        /// 2										Fluorescent lighting
        /// 3										Tungsten lighting
        /// 17										Standard Light A
        /// 18										Standard Light B
        /// 19										Standard Light C
        /// 20										D55 light
        /// 21										D65 light
        /// 22										D75 light
        /// 23 to 254								Reserved
        /// 255										Other lighting
        /// The default setting for this property is 0.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagExifLightSource = 0x9208,

        /// <summary>
        /// Note tag. A property used by manufacturers of EXIF writers to record information. The contents are up to the manufacturer.
        ///  
        ///  PropertyTagTypeUndefined * Any
        /// </summary>
        PropertyTagExifMakerNote = 0x927C,

        /// <summary>
        /// Smallest F number of the lens.
        /// The unit is the APEX value.
        /// Ordinarily it is given in the range of 00.00 to 99.99, but it is not limited to this range.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifMaxAperture = 0x9205,

        /// <summary>
        /// Metering mode.
        /// PropertyTagExifMeteringMode Setting			Description
        /// 0											Unknown
        /// 1											Average
        /// 2											Center-weighted average
        /// 3											Spot
        /// 4											Multi-spot
        /// 5											Pattern
        /// 6											Partial
        /// 7 to 254									Reserved
        /// 255											Other
        /// The default setting for this property is 0.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagExifMeteringMode = 0x9207,

        /// <summary>
        /// Optoelectronic conversion function (OECF) specified in ISO 14524.
        /// The OECF is the relationship between the camera optical input and the image values.
        ///  
        ///  PropertyTagTypeUndefined * Any
        /// </summary>
        PropertyTagExifOECF = 0x8828,

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
        /// The name of an audio file related to the image data.
        /// The only relational information recorded is the EXIF audio file name and extension (an ASCII string that consists of 8 characters plus a period (.), plus 3 characters).
        /// The path is not recorded.
        /// When you use this tag, audio files must be recorded in conformance with the EXIF audio format.
        /// Writers can also store audio data within APP2 as FlashPix extension stream data.
        ///  
        ///  PropertyTagTypeASCII * 13
        /// </summary>
        PropertyTagExifRelatedWav = 0xA004,

        /// <summary>
        /// The type of scene.
        /// If a digital camera recorded the image, the value of this property must be set to 1, indicating that the image was directly photographed.
        ///  
        ///  PropertyTagTypeUndefined * 1
        /// </summary>
        PropertyTagExifSceneType = 0xA301,

        /// <summary>
        /// Image sensor type on the camera or input device.
        /// PropertyTagExifSensingMethod Setting		Description
        /// 1											Not defined
        /// 2											One-chip color area senso
        /// 3											Two-chip color area senso
        /// 4											Three-chip color area senso
        /// 5											Color sequential area senso
        /// 7											Trilinear senso
        /// 8											Color sequential linear senso
        /// Other										Reserved
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagExifSensingMethod = 0xA217,

        /// <summary>
        /// Shutter speed.
        /// The unit is the Additive System of Photographic Exposure (APEX) value.
        ///  
        ///  PropertyTagTypeSRational * 1
        /// </summary>
        PropertyTagExifShutterSpeed = 0x9201,

        /// <summary>
        /// Camera or input device spatial frequency table and SFR values in the image width, image height, and diagonal direction, as specified in ISO 12233.
        ///  
        ///  PropertyTagTypeUndefined * Any
        /// </summary>
        PropertyTagExifSpatialFR = 0xA20C,

        /// <summary>
        /// Null-terminated character string that specifies the spectral sensitivity of each channel of the camera used.
        /// The string is compatible with the standard developed by the ASTM Technical Committee.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagExifSpectralSense = 0x8824,

        /// <summary>
        /// Distance to the subject, measured in meters.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagExifSubjectDist = 0x9206,

        /// <summary>
        /// Location of the main subject in the scene.
        /// The value of this property represents the pixel at the center of the main subject relative to the left edge.
        /// The first value indicates the column number, and the second value indicates the row number.
        ///  
        ///  PropertyTagTypeShort * 2
        /// </summary>
        PropertyTagExifSubjectLoc = 0xA214,

        /// <summary>
        /// Comment tag. A property used by EXIF users to write keywords or comments about the image besides those in PropertyTagImageDescription and without the character-code limitations of the PropertyTagImageDescription tag.
        /// The character code used in the PropertyTagExifUserComment property is identified based on an ID code in a fixed 8-byte area at the start of the property data area.
        /// The unused portion of the area is padded with NULL (0). ID codes are assigned by means of registration.
        /// Because the type is not ASCII, it is not necessary to use a NULL terminator.
        ///  
        ///  PropertyTagTypeUndefined * Any
        /// </summary>
        PropertyTagExifUserComment = 0x9286,

        /// <summary>
        /// Version of the EXIF standard supported.
        /// Nonexistence of this field is taken to mean nonconformance to the standard.
        /// Conformance to the standard is indicated by recording 0210 as a 4-byte ASCII string.
        /// Because the type is PropertyTagTypeUndefined, there is no NULL terminator.
        /// The default setting for this property is 0210.
        ///  
        ///  PropertyTagTypeUndefined * 4
        /// </summary>
        PropertyTagExifVer = 0x9000,

        /// <summary>
        /// Number of extra color components. For example, one extra component might hold an alpha value.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagExtraSamples = 0x0152,

        /// <summary>
        /// Logical order of bits in a byte.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagFillOrder = 0x010A,

        /// <summary>
        /// Time delay, in hundredths of a second, between two frames in an animated GIF image.
        ///  
        ///  PropertyTagTypeLong * Number of frames in the image
        /// </summary>
        PropertyTagFrameDelay = 0x5100,

        /// <summary>
        /// For each string of contiguous unused bytes, the number of bytes in that string.
        ///  
        ///  PropertyTagTypeLong * Number of strings of contiguous unused bytes.
        /// </summary>
        PropertyTagFreeByteCounts = 0x0121,

        /// <summary>
        /// For each string of contiguous unused bytes, the byte offset of that string.
        /// Tag 0x0120 
        /// Type PropertyTagTypeLong
        /// PropertyTagGamma
        /// Gamma value attached to the image.
        /// The gamma value is stored as a rational number (pair of long) with a numerator of 100000. For example, a gamma value of 2.2 is stored as the pair (100000, 45455).
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagFreeOffset = 0x0301,

        /// <summary>
        /// Color palette for an indexed bitmap in a GIF image.
        ///  
        ///  PropertyTagTypeByte * 3 x number of palette entries
        /// </summary>
        PropertyTagGlobalPalette = 0x5102,

        /// <summary>
        /// Altitude, in meters, based on the reference altitude specified by PropertyTagGpsAltitudeRef.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagGpsAltitude = 0x0006,

        /// <summary>
        /// Reference altitude, in meters.
        ///  
        ///  PropertyTagTypeByte * 1
        /// </summary>
        PropertyTagGpsAltitudeRef = 0x0005,

        /// <summary>
        /// Bearing to the destination point.
        /// The range of values is from 0.00 to 359.99.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagGpsDestBear = 0x0018,

        /// <summary>
        /// Null-terminated character string that specifies the reference used for giving the bearing to the destination point.
        /// T specifies true direction, and M specifies magnetic direction.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsDestBearRef = 0x0017,

        /// <summary>
        /// Distance to the destination point.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagGpsDestDist = 0x001A,

        /// <summary>
        /// Null-terminated character string that specifies the unit used to express the distance to the destination point.
        /// K, M, and N represent kilometers, miles, and knots respectively.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsDestDistRef = 0x0019,

        /// <summary>
        /// Latitude of the destination point. The latitude is expressed as three rational values giving the degrees, minutes, and seconds respectively.
        /// When degrees, minutes, and seconds are expressed, the format is dd/1, mm/1, ss/1.
        /// When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format is dd/1, mmmm/100, 0/1.
        ///  
        ///  PropertyTagTypeRational * 3
        /// </summary>
        PropertyTagGpsDestLat = 0x0014,

        /// <summary>
        /// Null-terminated character string that specifies whether the latitude of the destination point is north or south latitude.
        /// N specifies north latitude, and S specifies south latitude.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsDestLatRef = 0x0013,

        /// <summary>
        /// Longitude of the destination point. The longitude is expressed as three rational values giving the degrees, minutes, and seconds respectively.
        /// When degrees, minutes, and seconds are expressed, the format is ddd/1, mm/1, ss/1.
        /// When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format is ddd/1, mmmm/100, 0/1.
        ///  
        ///  PropertyTagTypeRational * 3
        /// </summary>
        PropertyTagGpsDestLong = 0x0016,

        /// <summary>
        /// Null-terminated character string that specifies whether the longitude of the destination point is east or west longitude.
        /// E specifies east longitude, and W specifies west longitude.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsDestLongRef = 0x0015,

        /// <summary>
        /// GPS DOP (data degree of precision).
        /// An HDOP value is written during 2-D measurement, and a PDOP value is written during 3-D measurement.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagGpsGpsDop = 0x000B,

        /// <summary>
        /// Null-terminated character string that specifies the GPS measurement mode.
        /// 2 specifies 2-D measurement, and 3 specifies 3-D measurement.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsGpsMeasureMode = 0x000A,

        /// <summary>
        /// Null-terminated character string that specifies the GPS satellites used for measurements.
        /// This property can be used to specify the ID number, angle of elevation, azimuth, SNR, and other information about each satellite. The format is not specified.
        /// If the GPS receiver is incapable of taking measurements, the value of the property must be set to NULL.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagGpsGpsSatellites = 0x0008,

        /// <summary>
        /// Null-terminated character string that specifies the status of the GPS receiver when the image is recorded.
        /// A means measurement is in progress, and V means the measurement is Interoperability.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsGpsStatus = 0x0009,

        /// <summary>
        /// Time as coordinated universal time (UTC).
        /// The value is expressed as three rational numbers that give the hour, minute, and second.
        ///  
        ///  PropertyTagTypeRational * 3
        /// </summary>
        PropertyTagGpsGpsTime = 0x0007,

        /// <summary>
        /// Offset to a block of GPS property items.
        /// Property items whose tags have the prefix PropertyTagGps are stored in the GPS block. The GPS property items are defined in the EXIF specification.
        /// The Imaging API uses this property to locate GPS information, but it does not expose this property for public use.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagGpsIFD = 0x8825,

        /// <summary>
        /// Direction of the image when it was captured.
        /// The range of values is from 0.00 to 359.99.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagGpsImgDir = 0x0011,

        /// <summary>
        /// Null-terminated character string that specifies the reference for the direction of the image when it is captured.
        /// T specifies true direction, and M specifies magnetic direction.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsImgDirRef = 0x0010,

        /// <summary>
        /// Latitude. Latitude is expressed as three rational values giving the degrees, minutes, and seconds respectively.
        /// When degrees, minutes, and seconds are expressed, the format is dd/1, mm/1, ss/1.
        /// When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format is dd/1, mmmm/100, 0/1.
        ///  
        ///  PropertyTagTypeRational * 3
        /// </summary>
        PropertyTagGpsLatitude = 0x0002,

        /// <summary>
        /// Null-terminated character string that specifies whether the latitude is north or south. N specifies north latitude, and S specifies south latitude.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsLatitudeRef = 0x0001,

        /// <summary>
        /// Longitude. Longitude is expressed as three rational values giving the degrees, minutes, and seconds respectively.
        /// When degrees, minutes and seconds are expressed, the format is ddd/1, mm/1, ss/1.
        /// When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format is ddd/1, mmmm/100, 0/1.
        ///  
        ///  PropertyTagTypeRational * 3
        /// </summary>
        PropertyTagGpsLongitude = 0x0004,

        /// <summary>
        /// Null-terminated character string that specifies whether the longitude is east or west longitude.
        /// E specifies east longitude, and W specifies west longitude.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsLongitudeRef = 0x0003,

        /// <summary>
        /// Null-terminated character string that specifies geodetic survey data used by the GPS receiver.
        /// If the survey data is restricted to Japan, the value of this property is TOKYO or WGS-84.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagGpsMapDatum = 0x0012,

        /// <summary>
        /// Speed of the GPS receiver movement.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagGpsSpeed = 0x000D,

        /// <summary>
        /// Null-terminated character string that specifies the unit used to express the GPS receiver speed of movement.
        /// K, M, and N represent kilometers per hour, miles per hour, and knots respectively.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsSpeedRef = 0x000C,

        /// <summary>
        /// Direction of GPS receiver movement.
        /// The range of values is from 0.00 to 359.99.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagGpsTrack = 0x000F,

        /// <summary>
        /// Null-terminated character string that specifies the reference for giving the direction of GPS receiver movement.
        /// T specifies true direction, and M specifies magnetic direction.
        ///  
        ///  PropertyTagTypeASCII * 2 (one character plus the NULL terminator)
        /// </summary>
        PropertyTagGpsTrackRef = 0x000E,

        /// <summary>
        /// Version of the Global Positioning Systems (GPS) IFD, given as 2.0.0.0.
        /// This property is mandatory when the PropertyTagGpsIFD property is present.
        /// When the version is 2.0.0.0, the property value is 0x02000000.
        ///  
        ///  PropertyTagTypeByte * 4
        /// </summary>
        PropertyTagGpsVer = 0x0000,

        /// <summary>
        /// For each possible pixel value in a grayscale image, the optical density of that pixel value.
        ///  
        ///  PropertyTagTypeShort * Number of possible pixel values
        /// </summary>
        PropertyTagGrayResponseCurve = 0x0123,

        /// <summary>
        /// Precision of the number specified by PropertyTagGrayResponseCurve.
        /// 1 specifies tenths, 2 specifies hundredths, 3 specifies thousandths, and so on.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagGrayResponseUnit = 0x0122,

        /// <summary>
        /// Block of information about grids and guides.
        ///  
        ///  PropertyTagTypeUndefined * Any
        /// </summary>
        PropertyTagGridSize = 0x5011,

        /// <summary>
        /// Angle for screen.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagHalftoneDegree = 0x500C,

        /// <summary>
        /// Information used by the halftone function
        ///  
        ///  PropertyTagTypeShort * 2
        /// </summary>
        PropertyTagHalftoneHints = 0x0141,

        /// <summary>
        /// Ink's screen frequency, in lines per inch.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagHalftoneLPI = 0x500A,

        /// <summary>
        /// Units for the screen frequency.
        /// A setting of 1 indicates lines per inch and a setting of 2 indicates lines per centimete
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagHalftoneLPIUnit = 0x500B,

        /// <summary>
        /// Miscellaneous halftone information.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagHalftoneMisc = 0x500E,

        /// <summary>
        /// Boolean value that specifies whether to use the printer's default screens.
        /// A setting of 1 indicates that the printer's default screens should be used and a setting of 2 indicates otherwise.
        ///  
        ///  PropertyTagTypeByte * 1
        /// </summary>
        PropertyTagHalftoneScreen = 0x500F,

        /// <summary>
        /// Shape of the halftone dots.
        /// PropertyTagHalftoneShape Setting		Description
        /// 0										Round
        /// 1										Ellipse
        /// 2										Line
        /// 3										Square
        /// 4										Cross
        /// 6										Diamond
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagHalftoneShape = 0x500D,

        /// <summary>
        /// Null-terminated character string that specifies the computer and/or operating system used to create the image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagHostComputer = 0x013C,

        /// <summary>
        /// International Color Consortium (ICC) profile embedded in the image.
        ///  
        ///  PropertyTagTypeByte * Length of the profile
        /// </summary>
        PropertyTagICCProfile = 0x8773,

        /// <summary>
        /// Null-terminated character string that identifies an ICC profile.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagICCProfileDescriptor = 0x0302,

        /// <summary>
        /// Null-terminated character string that specifies the title of the image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagImageDescription = 0x010E,

        /// <summary>
        /// Number of pixel rows.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagImageHeight = 0x0101,

        /// <summary>
        /// Null-terminated character string that specifies the title of the image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagImageTitle = 0x0320,

        /// <summary>
        /// Number of pixels per row.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagImageWidth = 0x0100,

        /// <summary>
        /// Index of the background color in the palette of a GIF image.
        ///  
        ///  PropertyTagTypeByte * 1
        /// </summary>
        PropertyTagIndexBackground = 0x5103,

        /// <summary>
        /// Index of the transparent color in the palette of a GIF image.
        ///  
        ///  PropertyTagTypeByte * 1
        /// </summary>
        PropertyTagIndexTransparent = 0x5104,

        /// <summary>
        /// Sequence of concatenated, null-terminated, character strings that specify the names of the inks used in a separated image.
        ///  
        ///  PropertyTagTypeASCII * Total length of the sequence of strings including the NULL terminators
        /// </summary>
        PropertyTagInkNames = 0x014D,

        /// <summary>
        /// Set of inks used in a separated image.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagInkSet = 0x014C,

        /// <summary>
        /// For each color component, the offset to the AC Huffman table for that component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeLong * Number of samples (components) per pixel
        /// </summary>
        PropertyTagJPEGACTables = 0x0209,

        /// <summary>
        /// For each color component, the offset to the DC Huffman table (or lossless Huffman table) for that component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeLong * Number of samples (components) per pixel
        /// </summary>
        PropertyTagJPEGDCTables = 0x0208,

        /// <summary>
        /// Offset to the start of a JPEG bitstream.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagJPEGInterFormat = 0x0201,

        /// <summary>
        /// Length, in bytes, of the JPEG bitstream.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagJPEGInterLength = 0x0202,

        /// <summary>
        /// For each color component, a lossless predictor-selection value for that component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeShort * Number of samples (components) per pixel
        /// </summary>
        PropertyTagJPEGLosslessPredictors = 0x0205,

        /// <summary>
        /// For each color component, a point transformation value for that component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeShort * Number of samples (components) per pixel
        /// </summary>
        PropertyTagJPEGPointTransforms = 0x0206,

        /// <summary>
        /// JPEG compression process.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagJPEGProc = 0x0200,

        /// <summary>
        /// For each color component, the offset to the quantization table for that component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeLong * Number of samples (components) per pixel
        /// </summary>
        PropertyTagJPEGQTables = 0x0207,

        /// <summary>
        /// Private property used by the Adobe Photoshop format.
        /// Not for public use.
        ///  
        ///  PropertyTagTypeShort * Any
        /// </summary>
        PropertyTagJPEGQuality = 0x5010,

        /// <summary>
        /// Length of the restart interval.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagJPEGRestartInterval = 0x0203,

        /// <summary>
        /// For an animated GIF image, the number of times to display the animation.
        /// A value of 0 specifies that the animation should be displayed infinitely.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagLoopCount = 0x5101,

        /// <summary>
        /// Luminance table. The luminance table and the chrominance table are used to control JPEG quality.
        /// A valid luminance or chrominance table has 64 entries of type PropertyTagTypeShort.
        /// If an image has a luminance table or a chrominance table, it must have both tables.
        ///  
        ///  PropertyTagTypeShort * 64
        /// </summary>
        PropertyTagLuminanceTable = 0x5090,

        /// <summary>
        /// For each color component, the maximum value assigned to that component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeShort * Number of samples (components) per pixel
        /// </summary>
        PropertyTagMaxSampleValue = 0x0119,

        /// <summary>
        /// For each color component, the minimum value assigned to that component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeShort * Number of samples (components) per pixel
        /// </summary>
        PropertyTagMinSampleValue = 0x0118,

        /// <summary>
        /// Type of data in a subfile.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagNewSubfileType = 0x00FE,

        /// <summary>
        /// Number of inks.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagNumberOfInks = 0x014E,

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

        /// <summary>
        /// Null-terminated character string that specifies the name of the page from which the image was scanned.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagPageName = 0x011D,

        /// <summary>
        /// Page number of the page from which the image was scanned.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagPageNumber = 0x0129,

        /// <summary>
        /// Palette histogram.
        ///  
        ///  PropertyTagTypeByte * Length of the histogram
        /// </summary>
        PropertyTagPaletteHistogram = 0x5113,

        /// <summary>
        /// How pixel data will be interpreted.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagPhotometricInterp = 0x0106,

        /// <summary>
        /// Pixels per unit in the x direction.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagPixelPerUnitX = 0x5111,

        /// <summary>
        /// Pixels per unit in the y direction.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagPixelPerUnitY = 0x5112,

        /// <summary>
        /// Unit for PropertyTagPixelPerUnitX and PropertyTagPixelPerUnitY.
        /// A setting of 0 indicates that property is unknown.
        ///  
        ///  PropertyTagTypeByte * 1
        /// </summary>
        PropertyTagPixelUnit = 0x5110,

        /// <summary>
        /// Whether pixel components are recorded in chunky or planar format.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagPlanarConfig = 0x011C,

        /// <summary>
        /// Type of prediction scheme that was applied to the image data before the encoding scheme was applied.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagPredictor = 0x013D,

        /// <summary>
        /// For each of the three primary colors in the image, the chromaticity of that color.
        ///  
        ///  PropertyTagTypeRational * 6
        /// </summary>
        PropertyTagPrimaryChromaticities = 0x013F,

        /// <summary>
        /// Sequence of one-byte Boolean values that specify printing options.
        ///  
        ///  PropertyTagTypeASCII * Number of flags
        /// </summary>
        PropertyTagPrintFlags = 0x5005,

        /// <summary>
        /// Print flags bleed width.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagPrintFlagsBleedWidth = 0x5008,

        /// <summary>
        /// Print flags bleed width scale.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagPrintFlagsBleedWidthScale = 0x5009,

        /// <summary>
        /// Print flags center crop marks.
        ///  
        ///  PropertyTagTypeByte * 1
        /// </summary>
        PropertyTagPrintFlagsCrop = 0x5007,

        /// <summary>
        /// Print flags version.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagPrintFlagsVersion = 0x5006,

        /// <summary>
        /// Reference black point value and reference white point value.
        ///  
        ///  PropertyTagTypeRational * 6
        /// </summary>
        PropertyTagREFBlackWhite = 0x0214,

        /// <summary>
        /// Unit of measure for the horizontal resolution and the vertical resolution.
        /// A setting of 2 indicates inches and a setting of 3 indicates centimeters.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagResolutionUnit = 0x0128,

        /// <summary>
        /// Units in which to display the image width.
        /// PropertyTagResolutionXLengthUnit Setting	Description
        /// 1											Inches
        /// 2											Centimeters
        /// 3											Points
        /// 4											Picas
        /// 5											Columns
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagResolutionXLengthUnit = 0x5003,

        /// <summary>
        /// Units in which to display horizontal resolution.
        /// A setting of 1 indicates pixels per inch and a setting of 2 indicates pixels per centimeter.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagResolutionXUnit = 0x5001,

        /// <summary>
        /// Units in which to display the image height.
        /// PropertyTagResolutionYLengthUnit Setting		Description
        /// 1												Inches
        /// 2												Centimeters
        /// 3												Points
        /// 4												Picas
        /// 5												Columns
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagResolutionYLengthUnit = 0x5004,

        /// <summary>
        /// Units in which to display vertical resolution.
        /// A setting of 1 indicates pixels per inch and a setting of 2 indicates pixels per centimeter.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagResolutionYUnit = 0x5002,

        /// <summary>
        /// Number of rows per strip. See also PropertyTagStripBytesCount and PropertyTagStripOffsets.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagRowsPerStrip = 0x0116,

        /// <summary>
        /// For each color component, the numerical format (unsigned, signed, floating point) of that component. See also PropertyTagSamplesPerPixel.
        ///  
        ///  PropertyTagTypeShort * Number of samples (components) per pixel
        /// </summary>
        PropertyTagSampleFormat = 0x0153,

        /// <summary>
        /// Number of color components per pixel.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagSamplesPerPixel = 0x0115,

        /// <summary>
        /// For each color component, the maximum value of that component. See also PropertyTagSamplesPerPixel.
        /// Tag 0x0155 
        /// Type The type that best matches the pixel component data 
        /// Count Number of samples (components) per pixel
        /// PropertyTagSMinSampleValue
        /// For each color component, the minimum value of that component. See also PropertyTagSamplesPerPixel.
        /// Tag 0x0154 
        /// Type The type that best matches the pixel component data 
        /// Count Number of samples (components) per pixel
        /// PropertyTagSoftwareUsed
        /// Null-terminated character string that specifies the name and version of the software or firmware of the device used to generate the image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagSMaxSampleValue = 0x0131,

        /// <summary>
        /// How the image should be displayed as defined by the International Color Consortium (ICC).
        /// If an Image object is constructed with the useEmbeddedColorManagement parameter set to TRUE, then the Imaging API renders the image according to the specified rendering intent.
        /// The following table shows the possible settings for this property.
        /// PropertyTagSRGBRenderingIntent Settings		Description
        /// 0	Perceptual intent, which is suitable for photographs, gives good adaptation to the display device gamut at the expense of colorimetric accuracy.
        /// 1	Relative colorimetric intent is suitable for images (for example, logos) that require color appearance matching that is relative to the display device white point.
        /// 2	Saturation intent, which is suitable for charts and graphs, preserves saturation at the expense of hue and lightness.
        /// 3	Absolute colorimetric intent is suitable for proofs (previews of images destined for a different display device) that require preservation of absolute colorimetry.
        /// Tag 0x0303 
        /// Type BYTE 
        /// Count 1
        /// PropertyTagStripBytesCount
        /// For each strip, the total number of bytes in that strip.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * Number of strips
        /// </summary>
        PropertyTagSRGBRenderingIntent = 0x0117,

        /// <summary>
        /// For each strip, the byte offset of that strip. See also PropertyTagRowsPerStrip> and PropertyTagStripBytesCount.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * Number of strips
        /// </summary>
        PropertyTagStripOffsets = 0x0111,

        /// <summary>
        /// Type of data in a subfile.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagSubfileType = 0x00FF,

        /// <summary>
        /// Set of flags that relate to T4 encoding.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagT4Option = 0x0124,

        /// <summary>
        /// Set of flags that relate to T6 encoding.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagT6Option = 0x0125,

        /// <summary>
        /// Null-terminated character string that describes the intended printing environment.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagTargetPrinter = 0x0151,

        /// <summary>
        /// Technique used to convert from gray pixels to black and white pixels.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThreshHolding = 0x0107,

        /// <summary>
        /// Null-terminated character string that specifies the name of the person who created the thumbnail image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagThumbnailArtist = 0x5034,

        /// <summary>
        /// Number of bits per color component in the thumbnail image. See also PropertyTagThumbnailSamplesPerPixel.
        ///  
        ///  PropertyTagTypeShort * Number of samples (components) per pixel in the thumbnail image
        /// </summary>
        PropertyTagThumbnailBitsPerSample = 0x5022,

        /// <summary>
        /// Bits per pixel (BPP) for the thumbnail image.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailColorDepth = 0x5015,

        /// <summary>
        /// Compressed size, in bytes, of the thumbnail image.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailCompressedSize = 0x5019,

        /// <summary>
        /// Compression scheme used for thumbnail image data.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailCompression = 0x5023,

        /// <summary>
        /// Null-terminated character string that contains copyright information for the thumbnail image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagThumbnailCopyRight = 0x503B,

        /// <summary>
        /// Raw thumbnail bits in JPEG or RGB format. Depends on PropertyTagThumbnailFormat.
        ///  
        ///  PropertyTagTypeByte * Variable
        /// </summary>
        PropertyTagThumbnailData = 0x501B,

        /// <summary>
        /// Date and time the thumbnail image was created. See also PropertyTagDateTime.
        ///  
        ///  PropertyTagTypeASCII * 20
        /// </summary>
        PropertyTagThumbnailDateTime = 0x5033,

        /// <summary>
        /// Null-terminated character string that specifies the manufacturer of the equipment used to record the thumbnail image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagThumbnailEquipMake = 0x5026,

        /// <summary>
        /// Null-terminated character string that specifies the model name or model number of the equipment used to record the thumbnail image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagThumbnailEquipModel = 0x5027,

        /// <summary>
        /// Format of the thumbnail image.
        /// A setting of 1 indicates raw RGB and a setting of 2 indicates JPEG.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailFormat = 0x5012,

        /// <summary>
        /// Height, in pixels, of the thumbnail image.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailHeight = 0x5014,

        /// <summary>
        /// Null-terminated character string that specifies the title of the image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagThumbnailImageDescription = 0x5025,

        /// <summary>
        /// Number of pixel rows in the thumbnail image.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailImageHeight = 0x5021,

        /// <summary>
        /// Number of pixels per row in the thumbnail image.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailImageWidth = 0x5020,

        /// <summary>
        /// Thumbnail image orientation in terms of rows and columns. See also PropertyTagOrientation.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailOrientation = 0x5029,

        /// <summary>
        /// How thumbnail pixel data will be interpreted.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailPhotometricInterp = 0x5024,

        /// <summary>
        /// Whether pixel components in the thumbnail image are recorded in chunky or planar format. See also PropertyTagPlanarConfig.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailPlanarConfig = 0x502F,

        /// <summary>
        /// Number of color planes for the thumbnail image.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailPlanes = 0x5016,

        /// <summary>
        /// For each of the three primary colors in the thumbnail image, the chromaticity of that color. See also PropertyTagPrimaryChromaticities.
        ///  
        ///  PropertyTagTypeRational * 6
        /// </summary>
        PropertyTagThumbnailPrimaryChromaticities = 0x5036,

        /// <summary>
        /// Byte offset between rows of pixel data.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailRawBytes = 0x5017,

        /// <summary>
        /// Reference black point value and reference white point value for the thumbnail image. See also PropertyTagREFBlackWhite.
        ///  
        ///  PropertyTagTypeRational * 6
        /// </summary>
        PropertyTagThumbnailRefBlackWhite = 0x503A,

        /// <summary>
        /// Unit of measure for the horizontal resolution and the vertical resolution of the thumbnail image. See also PropertyTagResolutionUnit.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailResolutionUnit = 0x5030,

        /// <summary>
        /// Thumbnail resolution in the width direction. The resolution unit is given in PropertyTagThumbnailResolutionUnit
        /// </summary>
        PropertyTagThumbnailResolutionX = 0x502B,

        /// <summary>
        /// Thumbnail resolution in the height direction. The resolution unit is given in PropertyTagThumbnailResolutionUnit
        /// </summary>
        PropertyTagThumbnailResolutionY = 0x502D,

        /// <summary>
        /// Number of rows per strip in the thumbnail image. See also PropertyTagThumbnailStripBytesCount and PropertyTagThumbnailStripOffsets.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailRowsPerStrip = 0x502E,

        /// <summary>
        /// Number of color components per pixel in the thumbnail image.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailSamplesPerPixel = 0x502A,

        /// <summary>
        /// Total size, in bytes, of the thumbnail image.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailSize = 0x5018,

        /// <summary>
        /// Null-terminated character string that specifies the name and version of the software or firmware of the device used to generate the thumbnail image.
        ///  
        ///  PropertyTagTypeASCII * Length of the string including the NULL terminato
        /// </summary>
        PropertyTagThumbnailSoftwareUsed = 0x5032,

        /// <summary>
        /// For each thumbnail image strip, the total number of bytes in that strip.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * Number of strips in the thumbnail image
        /// </summary>
        PropertyTagThumbnailStripBytesCount = 0x502C,

        /// <summary>
        /// For each strip in the thumbnail image, the byte offset of that strip. See also PropertyTagThumbnailRowsPerStrip and PropertyTagThumbnailStripBytesCount.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * Number of strips
        /// </summary>
        PropertyTagThumbnailStripOffsets = 0x5028,

        /// <summary>
        /// Tables that specify transfer functions for the thumbnail image. See also PropertyTagTransferFunction.
        ///  
        ///  PropertyTagTypeShort * Total number of 16-bit words required for the tables
        /// </summary>
        PropertyTagThumbnailTransferFunction = 0x5031,

        /// <summary>
        /// Chromaticity of the white point of the thumbnail image. See also PropertyTagWhitePoint.
        ///  
        ///  PropertyTagTypeRational * 2
        /// </summary>
        PropertyTagThumbnailWhitePoint = 0x5035,

        /// <summary>
        /// Width, in pixels, of the thumbnail image.
        ///  
        ///  PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagThumbnailWidth = 0x5013,

        /// <summary>
        /// Coefficients for transformation from RGB to YCbCr data for the thumbnail image. See also PropertyTagYCbCrCoefficients.
        ///  
        ///  PropertyTagTypeRational * 3
        /// </summary>
        PropertyTagThumbnailYCbCrCoefficients = 0x5037,

        /// <summary>
        /// Position of chrominance components in relation to the luminance component for the thumbnail image. See also PropertyTagYCbCrPositioning.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagThumbnailYCbCrPositioning = 0x5039,

        /// <summary>
        /// Sampling ratio of chrominance components in relation to the luminance component for the thumbnail image. See also PropertyTagYCbCrSubsampling.
        ///  
        ///  PropertyTagTypeShort * 2
        /// </summary>
        PropertyTagThumbnailYCbCrSubsampling = 0x5038,

        /// <summary>
        /// For each tile, the number of bytes in that tile.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * Number of tiles
        /// </summary>
        PropertyTagTileByteCounts = 0x0145,

        /// <summary>
        /// Number of pixel rows in each tile.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagTileLength = 0x0143,

        /// <summary>
        /// For each tile, the byte offset of that tile.
        ///  
        ///  PropertyTagTypeLong * Number of tiles
        /// </summary>
        PropertyTagTileOffset = 0x0144,

        /// <summary>
        /// Number of pixel columns in each tile.
        ///  
        ///  PropertyTagTypeShort or PropertyTagTypeLong * 1
        /// </summary>
        PropertyTagTileWidth = 0x0142,

        /// <summary>
        /// Tables that specify transfer functions for the image.
        ///  
        ///  PropertyTagTypeShort * Total number of 16-bit words required for the tables
        /// </summary>
        PropertyTagTransferFunction = 0x012D,

        /// <summary>
        /// Table of values that extends the range of the transfer function.
        ///  
        ///  PropertyTagTypeShort * 6
        /// </summary>
        PropertyTagTransferRange = 0x0156,

        /// <summary>
        /// Chromaticity of the white point of the image.
        ///  
        ///  PropertyTagTypeRational * 2
        /// </summary>
        PropertyTagWhitePoint = 0x013E,

        /// <summary>
        /// Offset from the left side of the page to the left side of the image. The unit of measure is specified by PropertyTagResolutionUnit.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagXPosition = 0x011E,

        /// <summary>
        /// Number of pixels per unit in the image width (x) direction. The unit is specified by PropertyTagResolutionUnit
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagXResolution = 0x011A,

        /// <summary>
        /// Coefficients for transformation from RGB to YCbCr image data.
        ///  
        ///  PropertyTagTypeRational * 3
        /// </summary>
        PropertyTagYCbCrCoefficients = 0x0211,

        /// <summary>
        /// Position of chrominance components in relation to the luminance component.
        ///  
        ///  PropertyTagTypeShort * 1
        /// </summary>
        PropertyTagYCbCrPositioning = 0x0213,

        /// <summary>
        /// Sampling ratio of chrominance components in relation to the luminance component.
        ///  
        ///  PropertyTagTypeShort * 2
        /// </summary>
        PropertyTagYCbCrSubsampling = 0x0212,

        /// <summary>
        /// Offset from the top of the page to the top of the image. The unit of measure is specified by PropertyTagResolutionUnit.
        ///  
        ///  PropertyTagTypeRational * 1
        /// </summary>
        PropertyTagYPosition = 0x011F,

        /// <summary>
        /// Number of pixels per unit in the image height (y) direction. The unit is specified by PropertyTagResolutionUnit.
        ///  
        ///  PropertyTagTypeRational  * 1
        /// </summary>
        PropertyTagYResolution = 0x011B,


    }
}