namespace Codenet.Drawing.ImageProcessing.Processing.Filters
{
    public class Emboss : ConvolutionMatrix
    {
        public class Amount
        {
            private float _Value = 50f;
            public Amount(float weight)
            {
                Value = weight;
            }
            public float Value
            {
                get { return _Value; }
                set
                {
                    if (value < 1f) value = 1f;
                    else if (value > 100f) value = 100f;
                    _Value = value;
                }
            }
        }

        public new FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            Matrix3x3 kernel = new Matrix3x3(
                -2, -1, 0,
                -1, 1, 1,
                0, 1, 2,
                1, 0, true);
            Amount amount = new Amount(1);
            FilterColorChannel channels = FilterColorChannel.None;

            foreach (object arg in args)
            {
                if (arg is Amount)
                {
                    amount = arg as Amount;
                }
                else if (arg is FilterColorChannel)
                {
                    channels |= (FilterColorChannel)arg;
                }
            }
            kernel.Matrix[1][1] = 5 - amount.Value / 20;

            return base.ProcessImage(bmp, kernel, channels);
        }
    }
}
