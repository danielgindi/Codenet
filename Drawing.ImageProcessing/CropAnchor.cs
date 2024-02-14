namespace Codenet.Drawing.ImageProcessing;

public class CropAnchor
{
    public static CropAnchor None = null;
    public static CropAnchor Top = new CropAnchor(0.5f, 0.0f);
    public static CropAnchor Right = new CropAnchor(1f, 0.5f);
    public static CropAnchor Bottom = new CropAnchor(0.5f, 1.0f);
    public static CropAnchor Left = new CropAnchor(0.0f, 0.5f);
    public static CropAnchor Center = new CropAnchor(0.5f, 0.5f);
    public static CropAnchor TopLeft = new CropAnchor(0.0f, 0.0f);
    public static CropAnchor TopCenter = new CropAnchor(0.5f, 0.0f);
    public static CropAnchor TopRight = new CropAnchor(1.0f, 0.0f);
    public static CropAnchor CenterLeft = new CropAnchor(0.0f, 0.5f);
    public static CropAnchor CenterRight = new CropAnchor(1.0f, 0.5f);
    public static CropAnchor BottomLeft = new CropAnchor(0.0f, 0.0f);
    public static CropAnchor BottomCenter = new CropAnchor(0.5f, 0.0f);
    public static CropAnchor BottomRight = new CropAnchor(1.0f, 0.0f);
    
    public float X { get; set; }
    public float Y { get; set; }

    public CropAnchor()
    {
        this.X = 0.5f;
        this.Y = 0.5f;
    }

    public CropAnchor(float x, float y)
    {
        this.X = x;
        this.Y = y;
    }

    static public implicit operator CropAnchor(int value)
    {
        int x = (value / 10) * 10;
        int y = value / 10;

        return new CropAnchor(
            x == 1.0f ? 0.5f : x == 2.0f ? 1.0f : 0.0f,
            y == 1.0f ? 0.5f : y == 2.0f ? 1.0f : 0.0f
            );
    }
}
