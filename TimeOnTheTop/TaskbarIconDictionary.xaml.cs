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
    public class Style
    {
        // color
        public bool EnableGradient = true;
        public uint Color1;
        public uint Color2;

        // shadow
        public bool EnableShadow = true;
        public double ShadowBlurRadius = 5;
        public uint ShadowColor = 0xFF000000;
        public double ShadowOpacity = 1.0;
        public double ShadowDepth = 0;
        public double ShadowDirection = 0;
    }

    public static readonly ReadOnlyCollection<string> Formats = new([
        "HH:mm",
        "HH:mm:ss",
        "HH:mm:ss.fff",
        "h:mm tt",
        "h:mm:ss tt",
        "yyyy/MM/dd",
        "yyyy/MM/dd HH:mm:ss",
    ]);

    public static readonly ReadOnlyDictionary<string, Style> Styles = new(new Dictionary<string, Style>
    {
        ["默认"] = new()
        {
            Color1 = 0xB6FFFFFF,
            Color2 = 0x8278FF20,
            ShadowBlurRadius = 5,
            ShadowDepth = 0,
            ShadowDirection = 0,
        },
        ["辉光, 50%"] = new()
        {
            Color1 = 0x7F00FFD4,
            Color2 = 0x7FFFE500,
            ShadowBlurRadius = 4.2,
            ShadowDepth = 2.5,
            ShadowDirection = 315,
        },
    });

    public static ReadOnlyDictionary<string, Style>.KeyCollection StyleNames => Styles.Keys;

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
        var item = (MenuItem)e.OriginalSource;
        App.Config.Expression = (item.Header as string)!;
        App.SaveChangedConfig();
    }

    private void MenuItemStyles_OnClick(object sender, RoutedEventArgs e)
    {
        var choice = (((MenuItem)e.OriginalSource).Header as string)!;
        var style = Styles[choice];
        var config = App.Config;
        config.EnableGradient = style.EnableGradient;
        config.Color1 = style.Color1;
        config.Color2 = style.Color2;
        config.EnableShadow = style.EnableShadow;
        config.ShadowBlurRadius = style.ShadowBlurRadius;
        config.ShadowColor = style.ShadowColor;
        config.ShadowDirection = style.ShadowDirection;
        config.ShadowOpacity = style.ShadowOpacity;
        config.ShadowDepth = style.ShadowDepth;
        App.SaveChangedConfig();
    }
}
