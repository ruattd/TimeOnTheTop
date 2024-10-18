using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Win32;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;

namespace TimeOnTheTop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static string _configFile = "";
    private static NotifyIcon? _notifyIcon;

    internal const string AppName = "Time on the TOP";
    internal const string AppId = "TimeOnTheTop";

    internal static string ExecutableFilePath = "";

    internal static Config Config = new();
    internal static bool FirstStart = false;
    internal static bool ConfigChanged = false;

    internal static RegistryKey StartupKey;

    internal static bool AppLightTheme;
    internal static bool SystemLightTheme;

    static App()
    {
        // get startup registry key
        var key = Registry.CurrentUser
            .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        if (key == null) OnInitError("Registry", "Failed to get startup registry key");
        StartupKey = key!;
    }

    internal static void SaveConfig()
    {
        Task.Run(() =>
        {
            var jsonText = JsonSerializer.Serialize(Config);
            File.WriteAllText(_configFile, jsonText);
        });
    }

    public App()
    {
        // get config file path
        var executableFile = Environment.ProcessPath;
        if (executableFile == null) OnInitError("Config", "Executable path not available");
        executableFile = Path.GetFullPath(executableFile!);
        ExecutableFilePath = executableFile;
        var configDir = Path.GetDirectoryName(executableFile)!;
        var configFile = Path.Combine(configDir, $"{AppId}.json");
        _configFile = configFile;

        // load config
        try
        {
            if (!File.Exists(configFile))
            {
                FirstStart = true;
                File.Create(configFile).Close();
                SaveConfig();
            }
            else
            {
                var jsonText = File.ReadAllText(configFile);
                var deserializedConfig = JsonSerializer.Deserialize<Config>(jsonText);
                if (deserializedConfig == null) OnInitError("Config", "Invalid config content");
                Config = deserializedConfig!;
            }
        }
        catch (Exception e)
        {
            OnInitError("Config", $"Error while reading/creating config file:\n{e}");
            throw;
        }

        // create notify icon
        _notifyIcon = new NotifyIcon
        {
            Text = AppName,
            Visible = true
        };
        
        // detect system theme changes
        OnSystemThemeChanged();
        SystemEvents.UserPreferenceChanged += (_, _) => OnSystemThemeChanged();
    }

    private static void OnSystemThemeChanged()
    {
        // get system theme config
        var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        if (key?.GetValue("AppsUseLightTheme") is int appLight) AppLightTheme = appLight != 0;
        if (key?.GetValue("SystemUsesLightTheme") is int sysLight) SystemLightTheme = sysLight != 0;
        // update notify icon
        if (_notifyIcon != null) _notifyIcon.Icon = Icon.ExtractAssociatedIcon(SystemLightTheme ? "assets/appicon.ico" : "assets/appicon_dark.ico");
    }

    internal static void OnInitError(string type, string message)
    {
        Window owner = new()
        {
            AllowsTransparency = true,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None,
            Background = Brushes.Transparent
        };
        owner.Show();
        MessageBox.Show(owner,
            $"[{type}] {message}",
            $"初始化出错 - {AppName}",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        Current.Shutdown();
    }

    internal static void OnExit()
    {
        if (_notifyIcon != null) _notifyIcon.Visible = false;
        Current.Shutdown();
    }

}
