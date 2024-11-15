using System.Windows;
using Color = System.Windows.Media.Color;

namespace TimeOnTheTop;

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
