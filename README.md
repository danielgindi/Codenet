Codenet
=======

Several utility libraries, available on nuget.  
Some of it was migrated from the antique `dg.Utilities` and from `Imaging.net`.

Packages:
* `Codenet.Collections` - helpers regarding collections
* `Codenet.Drawing` - helpers regarding drawing (css color parsing, `Size` bounds calculation with aspect ratio)
* `Codenet.Drawing.ImageDimensionsDecoder` - A fast decoder for image dimensions only, supporting several popular formats (jpeg, tiff
* `Codenet.Drawing.ImageProcessing` - Image processing utilities migrated from `Imaging.net`
  * A set of features to process images, with some ready-made filters and allows custom filters.
  * Builtin filters: `blur`, `brightness`, `color filter`, `contrast`, `convolution matrix`, `emboss`, `equalize histogram`, `exact color replace`, `flip`, `gamma correction`, `grayscale`, `guassian blur`, `invert`, `laplace edge detection`, `pixellate`, `red eye reduction`, `reduce transparancy`, `saturate`, `sepia`, `sharpen`, `sobel edge detection`)
  * Method to resize/crop/add-borders/round-corners/zoom/set-bg-color in one take. You can anchor the crop, keep aspect ratio etc.
  * Method to crop solid color margins.
  * Method to iterate over each frame of an image, with automatic encoding of the output.
  * Encodes JPEGs using Libjpeg (`BitMiracle.LibJpeg.NET`), to achieve better compression ratios.
  * Encodes (animated!) GIFs with high quality quantizers (You can choose between many algorithms). The quantizers were written by `Smart K8` and are under the `The Code Project Open License (CPOL)` license.
* `Codenet.Geography` - Stuff like distance between coordinates
* `Codenet.IO` - File/folder utilities
* `Codenet.IO.AnyEndianReader` - big/little endian reader, switchable during reading
* `Codenet.IO.CsvReader` - a full featured streaming csv reader (not depending on random file access)
* `Codenet.Serialization` - Serialization utilities
* `Codenet.Text` - Text utilities and extensions
* `Codenet.Threading` - Threading utilities

## Me
* Hi! I'm Daniel.
* danielgindi@gmail.com is my email address.
* That's all you need to know.

## Help

If you want to buy me a beer, you are very welcome to
[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=8VJRAFLX66N54)
 Thanks :-)

## License

Note: Some code under Codenet.Drawing.ImageProcessing may be subject to The Code Project Open License (CPOL). Especially the `Quantizers` namespace by `Smart K8`.

All the code here is under MIT license. Which means you could do virtually anything with the code.
I will appreciate it very much if you keep an attribution where appropriate.

    The MIT License (MIT)
    
    Copyright (c) 2013 Daniel Cohen Gindi (danielgindi@gmail.com)
    
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
