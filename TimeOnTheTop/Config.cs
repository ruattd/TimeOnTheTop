using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public Color Color1 = Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF);
    public Color Color2 = Color.FromArgb(0x30, 0x20, 0xFF, 0xE4);

    // shadow
    public bool EnableShadow = true;
    public double ShadowBlurRadius = 5;
    public Color ShadowColor = Colors.Black;
    public double ShadowOpacity = 0.9;
    public double ShadowDepth = 0;

    // positions
    public double Padding = 40;
    public double MaxHeight = 200;

    // time
    public int RefreshDelay = 1000;
    public string Expression = "HH:mm";
}
