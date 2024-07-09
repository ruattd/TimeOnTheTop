using System;
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

namespace TimeOnTheTop;

/// <summary>
/// ConfigWindow.xaml 的交互逻辑
/// </summary>
public partial class ConfigWindow
{
    public ConfigWindow()
    {
        InitializeComponent();
        if (App.FirstStart)
        {
            // add startup registry
            var result = MessageBox.Show(
                this,
                "是否向注册表添加启动项？",
                "Time on the TOP",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Registry.CurrentUser
                    .OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)
                    ?.SetValue("Time on the TOP", App.ExecutableFilePath, RegistryValueKind.String);
            }
            App.FirstStart = false;
        }
    }
}
