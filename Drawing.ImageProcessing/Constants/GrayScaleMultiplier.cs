namespace Codenet.Drawing.ImageProcessing
{
    public static class GrayScaleMultiplier
    {
        public const float NtscRed = 0.299f;
        public const float NtscGreen = 0.587f;
        public const float NtscBlue = 0.114f;

        public const float NaturalRed = 0.3086f;
        public const float NaturalGreen = 0.6094f;
        public const float NaturalBlue = 0.0820f;

        public const float CssRed = 0.2126f;
        public const float CssGreen = 0.7152f;
        public const float CssBlue = 0.0722f;

        public const float SimpleRed = 1.0f / 3.0f;
        public const float SimpleGreen = 1.0f / 3.0f;
        public const float SimpleBlue = 1.0f / 3.0f;
    }
}
