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
using Forms = System.Windows.Forms;
using Application = System.Windows.Application;
using ComboBox = System.Windows.Controls.ComboBox;
using FontFamily = System.Windows.Media.FontFamily;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Color = System.Windows.Media.Color;
using System.Drawing;

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

        // theme
        ResourceLocator.SetColorScheme(Application.Current.Resources,
            light ? ResourceLocator.LightColorScheme : ResourceLocator.DarkColorScheme);

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
        // save & active config
        App.SaveConfig();
        App.ConfigChanged = true;
    }

    // OVERRIDES

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Current = null;
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
            "是否向注册表添加启动项？",
            App.AppName,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
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

    private void TextBoxFontSize_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var result = double.TryParse(e.Text, out _);
        e.Handled = !result;
    }

    private void TextBoxFontSize_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (double.TryParse(TextBoxFontSize.Text, out var n))
        {
            _fontSizeTextIsAlreadyChanged = true;
            SliderFontSize.Value = n;
        }
    }

    private void ButtonColor1_OnClick(object sender, RoutedEventArgs e)
    {
        var color = ColorDialog.PickColor((Color)Resources["Color1"], "选择 颜色A", this);
        if (color == null) return;
        Resources["Color1"] = color;
        OnValueChanged(sender, e);
    }

    private void ButtonColor2_OnClick(object sender, RoutedEventArgs e)
    {
        var color = ColorDialog.PickColor((Color)Resources["Color2"], "选择 颜色B", this);
        if (color == null) return;
        Resources["Color2"] = color;
        OnValueChanged(sender, e);
    }
}
