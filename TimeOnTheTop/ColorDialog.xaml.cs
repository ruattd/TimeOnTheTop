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
using ColorPicker.Models;
using Color = System.Windows.Media.Color;

namespace TimeOnTheTop;

/// <summary>
/// Interaction logic for ColorDialog.xaml
/// </summary>
public partial class ColorDialog
{
    public static Color? PickColor(Color initial, string title = "", Window? owner = null, Action<Color?>? onPickFinished = null)
    {
        var instance = new ColorDialog(initial, title, owner);

        if (onPickFinished == null)
        {
            instance.ShowDialog();
            return instance.Color;
        }

        instance.Closed += (_, _) => onPickFinished.Invoke(instance.Color);
        instance.Show();
        return null;
    }

    public Color? Color { get; private set; } = null;

    public ColorDialog(Color initial, string title, Window? owner)
    {
        InitializeComponent();

        Title = title;
        ColorPicker.SelectedColor = initial;

        if (owner != null)
        {
            Owner = owner;
            Icon = owner.Icon;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        WindowHelper.SetWindowFrameworkDarkMode(WindowHelper.GetHandle(this), !App.AppLightTheme);
    }

    private void ButtonOK_OnClick(object sender, RoutedEventArgs e)
    {
        Color = ColorPicker.SelectedColor;
        Close();
    }

    private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
