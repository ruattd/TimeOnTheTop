using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimeOnTheTop;
public partial class TaskbarIconDictionary
{
    public class ColorStyle
    {
        public bool EnableGradient = true;
        public uint Color1 = 0xB6FFFFFF;
        public uint Color2 = 0x8278FF20;
    }

    public class ShadowStyle
    {
        public bool EnableShadow = true;
        public double ShadowBlurRadius = 5;
        public uint ShadowColor = 0xFF000000;
        public double ShadowOpacity = 1.0;
        public double ShadowDepth = 0;
        public double ShadowDirection = 0;
    }

    public class Format
    {
        public string Expression = "HH:mm";
        public int RefreshDelay = 1000;
    }

    public static readonly ReadOnlyDictionary<string, Format> Formats = new(new Dictionary<string, Format>
    {
        ["24 小时制"] = new(),
        ["24 小时制, 精确到秒"] = new() { Expression = "HH:mm:ss", RefreshDelay = 100 },
        ["24 小时制, 精确到毫秒 (性能警告)"] = new() { Expression = "HH:mm:ss.fff", RefreshDelay = 1 },
        ["12 小时制"] = new() { Expression = "h:mm tt" },
        ["12 小时制, 精确到秒"] = new() { Expression = "h:mm:ss tt", RefreshDelay = 100 },
        ["仅日期"] = new() { Expression = "yyyy/MM/dd" },
        ["ISO 日期"] = new() { Expression = "yyyy-MM-dd" },
        ["完整日期时间"] = new() { Expression = "yyyy/MM/dd HH:mm:ss", RefreshDelay = 100 },
        ["ISO 日期时间"] = new() { Expression = "yyyy-MM-dd\\THH:mm:ss", RefreshDelay = 100 },
    });

    public static readonly ReadOnlyDictionary<string, ColorStyle> ColorStyles = new(new Dictionary<string, ColorStyle>
    {
        ["默认"] = new(),
        ["辉光, 50%"] = new() { Color1 = 0x7F00FFD4, Color2 = 0x7FFFE500 },
    });
    
    public static readonly ReadOnlyDictionary<string, ShadowStyle> ShadowStyles = new(new Dictionary<string, ShadowStyle>
    {
        ["默认"] = new(),
        ["Material"] = new() { ShadowBlurRadius = 4.2, ShadowDepth = 2.5, ShadowDirection = 315, },
    });

    private void MenuItemShow_OnClick(object sender, RoutedEventArgs e)
    {
        var window = Application.Current.MainWindow;
        if (window != null) window.Visibility = ((MenuItem)sender).IsChecked ? Visibility.Visible : Visibility.Hidden;
    }

    private void MenuItemSettings_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigWindow.ActiveOrCreate();
    }

    private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
    {
        App.OnExit();
    }

    private void MenuItemFormats_OnClick(object sender, RoutedEventArgs e)
    {
        var choice = (((MenuItem)e.OriginalSource).Header as string)!;
        var config = App.Config;
        var format = Formats[choice];
        config.Expression = format.Expression;
        config.RefreshDelay = format.RefreshDelay;
        App.SaveChangedConfig();
    }

    private void MenuItemColorStyles_OnClick(object sender, RoutedEventArgs e)
    {
        var choice = (((MenuItem)e.OriginalSource).Header as string)!;
        var config = App.Config;
        var style = ColorStyles[choice];
        config.EnableGradient = style.EnableGradient;
        config.Color1 = style.Color1;
        config.Color2 = style.Color2;
        App.SaveChangedConfig();
    }

    private void MenuItemShadowStyles_OnClick(object sender, RoutedEventArgs e)
    {
        var choice = (((MenuItem)e.OriginalSource).Header as string)!;
        var config = App.Config;
        var style = ShadowStyles[choice];
        config.EnableShadow = style.EnableShadow;
        config.ShadowBlurRadius = style.ShadowBlurRadius;
        config.ShadowColor = style.ShadowColor;
        config.ShadowDirection = style.ShadowDirection;
        config.ShadowOpacity = style.ShadowOpacity;
        config.ShadowDepth = style.ShadowDepth;
        App.SaveChangedConfig();
    }
}
