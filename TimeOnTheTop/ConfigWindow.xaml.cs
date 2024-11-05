using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AdonisUI;
using Microsoft.Win32;
using H.NotifyIcon.EfficiencyMode;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;

namespace TimeOnTheTop;

/// <summary>
/// Interaction logic for ConfigWindow.xaml
/// </summary>
public partial class ConfigWindow
{
    public ConfigWindow()
    {
        InitializeComponent();

        Title = $"设置 - {App.AppName}";
        AppNameLabel.Text = $"{App.AppName} (ver {App.AppVersion})";

        UpdateContents();

        if (App.FirstStart) CheckAndSetStartup();
    }

    public static ConfigWindow? Current { get; private set; }

    public static void ActiveOrCreate()
    {
        if (Current == null)
        {
            Current = new ConfigWindow();
            Current.Show();
            EfficiencyModeUtilities.SetEfficiencyMode(false);
        }
        else Current.Activate();
    }

    // CONTENT UPDATES

    internal void UpdateWindowTheme()
    {
        var light = App.AppLightTheme;
        var icon = App.AppIcon;

        // window framework
        var hWnd = WindowHelper.GetHandle(this);
        WindowHelper.SetWindowFrameworkDarkMode(hWnd, !light);

        // icon
        Icon = icon;

        // children
        foreach (Window child in OwnedWindows)
        {
            if (child is ColorDialog dialog)
            {
                WindowHelper.SetWindowFrameworkDarkMode(WindowHelper.GetHandle(dialog), !light);
                dialog.Icon = icon;
            }
        }
    }

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
        EfficiencyModeUtilities.SetEfficiencyMode(true);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // update theme
        UpdateWindowTheme();
    }

    // UTILITIES

    private async void CheckAndSetStartup()
    {
        var order = -1;
        // check
        var isSet = await Task.Run(() =>
        {
            var r = false;
            foreach (var name in App.StartupKey.GetValueNames())
            {
                if (!name.StartsWith("TimeOnTheTop")) continue;
                if (App.StartupKey.GetValue(name) is string value && value == App.ExecutableFilePath)
                {
                    r = true;
                    break;
                }
                if (name.Length > 12)
                {
                    var ns = name[12..];
                    if (int.TryParse(ns, out var n) && n > order) order = n;
                }
            }
            return r;
        });
        if (isSet) return;
        // add startup registry
        var result = MessageBox.Show(this,
            text: "是否向注册表添加启动项？",
            caption: App.AppName,
            buttons: MessageBoxButton.YesNo,
            icon: MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            App.StartupKey.SetValue($"TimeOnTheTop{order+1}", App.ExecutableFilePath, RegistryValueKind.String);
        }
        App.FirstStart = false;
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
}
