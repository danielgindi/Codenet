using System.Drawing;

namespace Codenet.Drawing.ImageProcessing.Processing
{
    public class ColorArgument
    {
        public bool IncludeAlpha = false;
        public short A, R, G, B;

        public ColorArgument(short A, short R, short G, short B, bool includeAlpha)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
            this.IncludeAlpha = includeAlpha;
        }
        public ColorArgument(short A, short R, short G, short B)
            : this(A, R, G, B, false)
        {
        }
        public ColorArgument(Color color, bool includeAlpha)
            : this(color.A, color.R, color.G, color.B, includeAlpha)
        {
        }
        public ColorArgument(Color color)
            : this(color, false)
        {
        }

        public bool Is64Bit
        {
            get 
            {
                if (A > 255) return true;
                if (R > 255) return true;
                if (G > 255) return true;
                if (B > 255) return true;
                return false;
            }
        }
    }
}