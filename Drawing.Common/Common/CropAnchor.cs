namespace Codenet.Drawing.Common;

public class CropAnchor
{
    public static readonly CropAnchor None = null;
    public static readonly CropAnchor Top = new CropAnchor(0.5f, 0.0f);
    public static readonly CropAnchor Right = new CropAnchor(1f, 0.5f);
    public static readonly CropAnchor Bottom = new CropAnchor(0.5f, 1.0f);
    public static readonly CropAnchor Left = new CropAnchor(0.0f, 0.5f);
    public static readonly CropAnchor Center = new CropAnchor(0.5f, 0.5f);
    public static readonly CropAnchor TopLeft = new CropAnchor(0.0f, 0.0f);
    public static readonly CropAnchor TopCenter = new CropAnchor(0.5f, 0.0f);
    public static readonly CropAnchor TopRight = new CropAnchor(1.0f, 0.0f);
    public static readonly CropAnchor CenterLeft = new CropAnchor(0.0f, 0.5f);
    public static readonly CropAnchor CenterRight = new CropAnchor(1.0f, 0.5f);
    public static readonly CropAnchor BottomLeft = new CropAnchor(0.0f, 0.0f);
    public static readonly CropAnchor BottomCenter = new CropAnchor(0.5f, 0.0f);
    public static readonly CropAnchor BottomRight = new CropAnchor(1.0f, 0.0f);
    
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
