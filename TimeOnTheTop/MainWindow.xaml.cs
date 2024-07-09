using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TimeOnTheTop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        // first start
        if (App.FirstStart)
        {
            ConfigWindow window = new();
            window.Show();
        }

        // initialize window
        InitializeComponent();
        Width = SystemParameters.PrimaryScreenWidth;
        ApplyTextStyle();

        // timer
        var config = App.Config;
        DispatcherTimer timer = new(
            TimeSpan.FromMilliseconds(config.RefreshDelay),
            DispatcherPriority.Render,
            (_, _) =>
            {
                // update style
                if (App.ConfigChanged)
                {
                    ApplyTextStyle();
                    App.ConfigChanged = false;
                }
                // update time
                var time = DateTime.Now;
                var text = time.ToString(config.Expression);
                if (TimeText.Text != text) TimeText.Text = text;
            },
            Dispatcher.CurrentDispatcher);
        timer.Start();
    }

    private void ApplyTextStyle()
    {
        var config = App.Config;

        // text
        TimeText.FontSize = config.FontSize;
        TimeText.FontFamily = new FontFamily(config.FontFamily);
        TimeText.FontWeight = FontWeight.FromOpenTypeWeight(config.FontWeight);
        TimeText.TextAlignment = config.TextAlignment;

        // positions
        Height = config.MaxHeight;
        TimeText.Padding = new Thickness(config.Padding);

        // color
        if (config.EnableGradient)
            TimeText.Foreground = new LinearGradientBrush(config.Color1, config.Color2, 0);
        else
            TimeText.Foreground = new SolidColorBrush(config.Color1);

        // shadow
        if (config.EnableShadow)
            TimeText.Effect = new DropShadowEffect()
            {
                BlurRadius = config.ShadowBlurRadius,
                Color = config.ShadowColor,
                Opacity = config.ShadowOpacity,
                Direction = 0,
                ShadowDepth = config.ShadowDepth
            };
        else
            TimeText.Effect = null;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hWnd = new WindowInteropHelper(this).Handle;
        WindowHelper.SetWindowExTransparent(hWnd);
    }
}
