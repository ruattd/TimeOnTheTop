using System.ComponentModel;
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
using H.NotifyIcon.EfficiencyMode;
using FontFamily = System.Windows.Media.FontFamily;

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
            ConfigWindow.ActiveOrCreate();
        }

        // initialize window
        InitializeComponent();
        Title = App.AppName;
        Width = SystemParameters.PrimaryScreenWidth;
        ApplyTextStyle();

        // enable efficiency mode
        EfficiencyModeUtilities.SetEfficiencyMode(true);
    }

    private DispatcherTimer? _timer;

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
            TimeText.Foreground = new LinearGradientBrush(Config.HexToColor(config.Color1), Config.HexToColor(config.Color2), 0);
        else
            TimeText.Foreground = new SolidColorBrush(Config.HexToColor(config.Color1));

        // shadow
        if (config.EnableShadow)
            TimeText.Effect = new DropShadowEffect()
            {
                BlurRadius = config.ShadowBlurRadius,
                Color = Config.HexToColor(config.ShadowColor),
                Opacity = config.ShadowOpacity,
                Direction = config.ShadowDirection,
                ShadowDepth = config.ShadowDepth
            };
        else
            TimeText.Effect = null;

        // timer
        _timer?.Stop();
        _timer = new DispatcherTimer(
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
        _timer.Start();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // set ExTransparent to implement mouse pass-through
        WindowHelper.SetWindowExTransparent(WindowHelper.GetHandle(this));
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = true; // disable closing event
    }
}
