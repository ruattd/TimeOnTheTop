using System.Windows;
using System.Windows.Controls;

namespace TimeOnTheTop;

public partial class MessageDialog
{
    public const int ButtonOk = 0;
    public const int ButtonYesNo = 1;
    public const int ButtonOkCancel = 2;
    public const int ButtonYesNoCancel = 3;
    
    public const int ResultOk = 0;
    public const int ResultYes = 0;
    public const int ResultNo = 1;
    public const int ResultCancel = 2;

    public const string TextOk = "确定";
    public const string TextCancel = "取消";
    public const string TextYes = "是";
    public const string TextNo = "否";

    public string Message
    {
        get => TextBlockMessage.Text;
        set => TextBlockMessage.Text = value;
    }

    public int Result { get; private set; }

    public MessageDialog(Window? owner = null)
    {
        if (owner != null)
        {
            Owner = owner;
            Title = owner.Title;
        }
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        WindowHelper.SetWindowFrameworkDarkMode(WindowHelper.GetHandle(this), !App.AppLightTheme);
    }

    public void AddContent(params UIElement[] elements)
    {
        foreach (var element in elements) StackPanelContent.Children.Add(element);
    }

    public void AddButton(params UIElement[] elements)
    {
        foreach (var element in elements) StackPanelButtons.Children.Add(element);
    }

    public Button CancelButton(Action? onClick = null)
    {
        var button = new Button { Content = TextCancel };
        button.Click += (_, _) =>
        {
            Result = ResultCancel;
            onClick?.Invoke();
            Close();
        };
        return button;
    }

    public Button OkButton(Action? onClick = null)
    {
        var button = new Button { Content = TextOk };
        button.Click += (_, _) =>
        {
            Result = ResultOk;
            onClick?.Invoke();
            Close();
        };
        return button;
    }

    public Button YesButton(Action? onClick = null)
    {
        var button = new Button { Content = TextYes };
        button.Click += (_, _) =>
        {
            Result = ResultYes;
            onClick?.Invoke();
            Close();
        };
        return button;
    }

    public Button NoButton(Action? onClick = null)
    {
        var button = new Button { Content = TextNo };
        button.Click += (_, _) =>
        {
            Result = ResultNo;
            onClick?.Invoke();
            Close();
        };
        return button;
    }

    public static int Show(
        string message,
        Window? owner = null,
        int button = ButtonOk,
        string? caption = null,
        int defaultResult = ResultCancel)
    {
        var d = new MessageDialog(owner)
        {
            Message = message,
            Result = defaultResult
        };
        if (caption != null) d.Title = caption;

        switch (button)
        {
            case ButtonOk: d.AddButton(d.OkButton()); break;
            case ButtonOkCancel: d.AddButton(d.OkButton(), d.CancelButton()); break;
            case ButtonYesNo: d.AddButton(d.YesButton(), d.NoButton()); break;
            case ButtonYesNoCancel: d.AddButton(d.YesButton(), d.NoButton(), d.CancelButton()); break;
        }

        d.ShowDialog();
        return d.Result;
    }
}
