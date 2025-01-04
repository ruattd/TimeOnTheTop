using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;

namespace TimeOnTheTop;

public partial class RegistryConfigWindow
{

    public class StartupListItem(string path, int order, bool isCurrent) : IComparable<StartupListItem>
    {
        public string Path { get; } = path;
        public int Order { get; } = order;
        public string OrderString => Order.ToString();
        public bool IsCurrent { get; set; } = isCurrent;
        public string IsCurrentString => IsCurrent ? "[当前实例]" : "";
        public bool IsRunning { get; set; }
        public string IsRunningString => IsRunning ? "[运行中]" : "";

        public int CompareTo(StartupListItem? other)
        {
            return Order.CompareTo(other?.Order);
        }
    }

    public static readonly ObservableCollection<StartupListItem> Items = [];

    public RegistryConfigWindow(Window owner)
    {
        Owner = owner;
        Title = "启动项管理器";

        InitializeComponent();

        Dispatcher.InvokeAsync(UpdateItems);
    }

    internal static async Task<bool> CheckAndSetStartup(Window owner, bool showMessage = true)
    {
        var order = -1;

        // check
        var isSet = await Task.Run(() =>
        {
            var r = false;
            foreach (var name in App.StartupKey.GetValueNames())
            {
                if (!name.StartsWith(App.AppId)) continue;
                if (App.StartupKey.GetValue(name) is string value && value == App.ExecutableFilePath)
                {
                    r = true;
                    break;
                }
                if (name.Length > 13)
                {
                    var ns = name[13..];
                    if (int.TryParse(ns, out var n) && n > order) order = n;
                }
            }
            return r;
        });
        if (isSet) return false;

        // add startup registry
        var key = $"{App.AppId}_{order + 1}";
        if (showMessage)
        {
            var result = MessageBox.Show(owner,
                text: $"是否添加启动项 [{key}]",
                caption: App.AppName,
                buttons: MessageBoxButton.YesNo,
                icon: MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return true;
        }
        App.StartupKey.SetValue(key, App.ExecutableFilePath, RegistryValueKind.String);
        return true;
    }

    private static void UpdateItems()
    {
        // get current session id
        var sessionId = Process.GetCurrentProcess().SessionId;

        // filter registry items
        var items = new List<StartupListItem>();
        foreach (var name in App.StartupKey.GetValueNames())
        {
            if (!name.StartsWith(App.AppId)
                || !int.TryParse(name[13..], out var order)
                || App.StartupKey.GetValue(name) is not string path)
            {
                continue;
            }

            if (path.StartsWith('"')) path = path[1..path.IndexOf('"', 1)];
            var isRunning = IsFileRunning(path, out var directory, sessionId);
            var isCurrent = App.ConfigDirectory == directory;

            items.Add(new StartupListItem(path, order, isCurrent) { IsRunning = isRunning });
        }
        items.Sort();

        // update
        Items.Clear();
        items.ForEach(Items.Add);
    }

    private static bool IsFileRunning(string path, out string? directory, int? currentSessionId = null)
    {
        directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory)) return false;
        currentSessionId ??= Process.GetCurrentProcess().SessionId;

        var fileName = Path.GetFileNameWithoutExtension(path).ToLower();
        foreach (var p in Process.GetProcessesByName(fileName))
        {
            if (p.SessionId == currentSessionId
                && p.MainModule is { } module
                && module.FileName.StartsWith(directory, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void ButtonReload_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(UpdateItems);
    }

    private void ButtonAddCurrent_OnClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;
        Dispatcher.InvokeAsync(async () =>
        {
            var successful = await CheckAndSetStartup(this);
            if (successful) UpdateItems();
            else MessageBox.Show(this, text: "当前实例的启动项已存在", caption: App.AppName);
            button.IsEnabled = true;
        });
    }

    private void ButtonCreateNew_OnClick(object sender, RoutedEventArgs e)
    {
        // TODO create new instance
    }

    private void ButtonDeleteSelected_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            var selected = (StartupListItem)ListBoxStartupItems.SelectedItem;
            if (selected == null) return;
            var key = $"{App.AppId}_{selected.Order}";

            var result = MessageBox.Show(this,
                text: $"是否删除启动项 [{key}]",
                caption: App.AppName,
                buttons: MessageBoxButton.YesNo,
                icon: MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            App.StartupKey.DeleteValue(key);
            UpdateItems();
        });
    }

    private void ButtonReorderSelected_OnClick(object sender, RoutedEventArgs e)
    {
        // TODO change selected order
    }
}
