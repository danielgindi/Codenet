using System;
using System.Collections.Generic;
using System.Drawing;

namespace Codenet.Drawing.ImageProcessing.Quantizers.DistinctSelection
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public class DistinctBucket
    {
        public DistinctColorInfo ColorInfo { get; private set; }
        public DistinctBucket[] Buckets { get; private set; }

        public DistinctBucket()
        {
            Buckets = new DistinctBucket[16];
        }

        public void StoreColor(Color color)
        {
            Int32 redIndex = color.R >> 5;
            DistinctBucket redBucket = Buckets[redIndex];

            if (redBucket == null)
            {
                redBucket = new DistinctBucket();
                Buckets[redIndex] = redBucket;
            }

            Int32 greenIndex = color.G >> 5;
            DistinctBucket greenBucket = redBucket.Buckets[greenIndex];

            if (greenBucket == null)
            {
                greenBucket = new DistinctBucket();
                redBucket.Buckets[greenIndex] = greenBucket;
            }

            Int32 blueIndex = color.B >> 5;
            DistinctBucket blueBucket = greenBucket.Buckets[blueIndex];

            if (blueBucket == null)
            {
                blueBucket = new DistinctBucket();
                greenBucket.Buckets[blueIndex] = blueBucket;
            }

            DistinctColorInfo colorInfo = blueBucket.ColorInfo;

            if (colorInfo == null)
            {
                colorInfo = new DistinctColorInfo(color);
                blueBucket.ColorInfo = colorInfo;
            }
            else
            {
                colorInfo.IncreaseCount();
            }
        }
        
        public List<DistinctColorInfo> GetValues()
        {
            List<DistinctColorInfo> allColorInfo = new List<DistinctColorInfo>();
            foreach (DistinctBucket redBucket in Buckets)
            {
                if (redBucket != null)
                {
                    foreach (DistinctBucket greenBucket in redBucket.Buckets)
                    {
                        if (greenBucket != null)
                        {
                            foreach (DistinctBucket blueBucket in greenBucket.Buckets)
                            {
                                if (blueBucket != null)
                                {
                                    allColorInfo.Add(blueBucket.ColorInfo);
                                }
                            }
                        }
                    }
                }
            }
            return allColorInfo;
        }
    }
}
