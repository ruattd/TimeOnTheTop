using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimeOnTheTop;
public partial class TaskbarIconDictionary
{
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
}
