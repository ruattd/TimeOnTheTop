using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

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
        if (App.FirstStart) CheckAndSetStartup();
    }

    private static ConfigWindow? _current;

    public static void ActiveOrCreate()
    {
        if (_current == null)
        {
            _current = new ConfigWindow();
            _current.Show();
        }
        else _current.Activate();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _current = null;
    }

    private async void CheckAndSetStartup()
    {
        var order = -1;
        var isSet = await Task.Run(() =>
        {
            // check
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
}
