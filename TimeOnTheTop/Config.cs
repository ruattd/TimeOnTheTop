using System.Windows;
using System.Windows.Media;

namespace TimeOnTheTop;

internal class Config
{
    // text
    public string FontFamily = new FontFamily("Microsoft YaHei").ToString();
    public double FontSize = 40;
    public int FontWeight = FontWeights.Regular.ToOpenTypeWeight();
    public TextAlignment TextAlignment = TextAlignment.Right;

    // color
    public bool EnableGradient = true;
    public uint Color1 = 0xB6FFFFFF;
    public uint Color2 = 0x8278FF20;

    // shadow
    public bool EnableShadow = true;
    public double ShadowBlurRadius = 5;
    public uint ShadowColor = 0xFF000000;
    public double ShadowOpacity = 0.9;
    public double ShadowDepth = 0;
    public double ShadowDirection = 0;

    // positions
    public double Padding = 40;
    public double MaxHeight = 200;

    // time
    public int RefreshDelay = 1000;
    public string Expression = "HH:mm";

    // value converters
    public static Color HexToColor(uint argb)
    {
        var bytes = BitConverter.GetBytes(argb);
        return Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
    }
    public static uint ColorToHex(Color color)
    {
        var a = color.A << 24;
        var r = color.R << 16;
        var g = color.G << 8;
        var b = color.B << 0;
        return (uint)(a | r | g | b);
    }
}
