using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TimeOnTheTop;

public partial class ConfigWindow
{
    public ConfigWindow()
    {
        InitializeComponent();

        Title = $"设置 - {App.AppName}";
        AppNameLabel.Text = $"{App.AppName} (ver {App.AppVersion})";

        UpdateContents();

        if (App.FirstStart)
        {
            var args = Environment.GetCommandLineArgs();
            var autostart = args.Length > 0 && args[0].Equals("autostart", StringComparison.OrdinalIgnoreCase);
            _ = RegistryConfigWindow.CheckAndSetStartup(this, !autostart);
            App.FirstStart = false;
        }
    }

    public static ConfigWindow? Current { get; private set; }

    public static void ActiveOrCreate()
    {
        if (Current == null)
        {
            var window = new ConfigWindow();
            window.Show();
            App.UpdateWindowTheme(window); // update theme
            App.SetEfficiencyMode(false);
            Current = window;
        }
        else Current.Activate();
    }

    // CONTENT UPDATES

    private void UpdateContents()
    {
        var config = App.Config;
        ComboBoxFont.SelectedItem = new FontFamily(config.FontFamily);
        ComboBoxFontWeight.SelectedItem = FontWeight.FromOpenTypeWeight(config.FontWeight);
        SliderFontSize.Value = config.FontSize;
        TextBoxFontSize.Text = config.FontSize.ToString("F1");
        ComboBoxTextAlignment.SelectedItem = config.TextAlignment;
        Resources["Color1"] = Config.HexToColor(config.Color1);
        Resources["Color2"] = Config.HexToColor(config.Color2);
        CheckBoxEnableGradient.IsChecked = config.EnableGradient;
        CheckBoxEnableShadow.IsChecked = config.EnableShadow;
        Resources["ColorShadow"] = Config.HexToColor(config.ShadowColor);
        SliderShadowBlurRadius.Value = config.ShadowBlurRadius;
        SliderShadowOpacity.Value = config.ShadowOpacity * 100;
        SliderShadowDepth.Value = config.ShadowDepth;
        SliderShadowDirection.Value = config.ShadowDirection;
        const string doubleFormat = "F2";
        TextBoxPadding.Text = config.Padding.ToString(doubleFormat);
        TextBoxMaxHeight.Text = config.MaxHeight.ToString(doubleFormat);
        TextBoxRefreshDelay.Text = config.RefreshDelay.ToString("D");
        TextBoxExpression.Text = config.Expression;
        // disable apply button
        ButtonApply.IsEnabled = false;
    }

    private void UpdateConfig()
    {
        var config = App.Config;
        config.FontFamily = ((FontFamily)ComboBoxFont.SelectedItem).ToString();
        config.FontWeight = ((FontWeight)ComboBoxFontWeight.SelectedItem).ToOpenTypeWeight();
        config.FontSize = double.Parse(TextBoxFontSize.Text);
        config.TextAlignment = (TextAlignment)ComboBoxTextAlignment.SelectedItem;
        config.Color1 = Config.ColorToHex((Color)Resources["Color1"]);
        config.Color2 = Config.ColorToHex((Color)Resources["Color2"]);
        config.EnableGradient = CheckBoxEnableGradient.IsChecked == true;
        config.EnableShadow = CheckBoxEnableShadow.IsChecked == true;
        config.ShadowColor = Config.ColorToHex((Color)Resources["ColorShadow"]);
        config.ShadowBlurRadius = SliderShadowBlurRadius.Value;
        config.ShadowOpacity = SliderShadowOpacity.Value / 100;
        config.ShadowDepth = SliderShadowDepth.Value;
        config.ShadowDirection = SliderShadowDirection.Value;
        config.Padding = double.Parse(TextBoxPadding.Text);
        config.MaxHeight = double.Parse(TextBoxMaxHeight.Text);
        config.RefreshDelay = int.Parse(TextBoxRefreshDelay.Text);
        config.Expression = TextBoxExpression.Text;
        // save & active config
        App.SaveConfig();
        App.ConfigChanged = true;
    }

    // OVERRIDES

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Current = null;
        App.SetEfficiencyMode(true);
    }

    // EVENTS
    
    private void ButtonOK_OnClick(object sender, RoutedEventArgs e)
    {
        UpdateConfig();
        Close();
    }
    
    private bool _resetWhenClickCancel = false;
    private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
    {
        if (_resetWhenClickCancel) UpdateContents();
        else Close();
    }

    private void ButtonApply_OnClick(object sender, RoutedEventArgs e)
    {
        UpdateConfig();
        ButtonApply.IsEnabled = false;
    }

    private void OnValueChanged(object sender, EventArgs e)
    {
        if (ButtonApply != null) ButtonApply.IsEnabled = true;
    }

    private void ComboBoxFont_OnSelectionChanged(object sender, RoutedEventArgs e)
    {
    }

    private void Window_OnKey(object sender, KeyEventArgs e)
    {
        if (e.SystemKey is Key.LeftAlt or Key.RightAlt)
        {
            if (e.IsDown)
            {
                ButtonCancel.Content = "复位";
                _resetWhenClickCancel = true;
            }
            else
            {
                ButtonCancel.Content = "取消";
                _resetWhenClickCancel = false;
            }
        }
        
    }

    private bool _fontSizeTextIsAlreadyChanged = false;
    private void SliderFontSize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_fontSizeTextIsAlreadyChanged)
        {
            _fontSizeTextIsAlreadyChanged = false;
            return;
        }
        var value = Math.Round(SliderFontSize.Value);
        TextBoxFontSize.Text = value.ToString("F1");
    }

    private void TextBoxFontSize_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (double.TryParse(TextBoxFontSize.Text, out var n))
        {
            _fontSizeTextIsAlreadyChanged = true;
            SliderFontSize.Value = n;
        }
    }

    private void OnPickColor(object sender, RoutedEventArgs e, string colorName, string title)
    {
        var color = ColorDialog.PickColor((Color)Resources[colorName], $"选择 {title}", this);
        if (color == null) return;
        Resources[colorName] = color;
        OnValueChanged(sender, e);
    }

    private void ButtonColor1_OnClick(object sender, RoutedEventArgs e)
    {
        OnPickColor(sender, e, "Color1", "颜色A");
    }

    private void ButtonColor2_OnClick(object sender, RoutedEventArgs e)
    {
        OnPickColor(sender, e, "Color2", "颜色B");
    }

    private void ButtonColorShadow_OnClick(object sender, RoutedEventArgs e)
    {
        OnPickColor(sender, e, "ColorShadow", "阴影颜色");
    }

    private void TextBox_OnPreviewTextInput_Double(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !double.TryParse(e.Text, out _);
    }

    private void TextBox_OnPreviewTextInput_Integer(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void ButtonStartupManager_OnClick(object sender, RoutedEventArgs e)
    {
        new RegistryConfigWindow(this).ShowDialog();
    }
}
