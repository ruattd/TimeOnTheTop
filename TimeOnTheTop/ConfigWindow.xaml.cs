﻿using System;
using System.Collections.Generic;
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
/// ConfigWindow.xaml 的交互逻辑
/// </summary>
public partial class ConfigWindow
{
    public ConfigWindow()
    {
        InitializeComponent();
        if (App.FirstStart) CheckAndSetStartup();
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
            "Time on the TOP",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            App.StartupKey.SetValue($"TimeOnTheTop{order+1}", App.ExecutableFilePath, RegistryValueKind.String);
        }
        App.FirstStart = false;
    }
}
