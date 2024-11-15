using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows;
using AdonisUI;
using H.NotifyIcon;
using H.NotifyIcon.EfficiencyMode;
using Microsoft.Win32;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace TimeOnTheTop;

public partial class App
{
    private static string _configFile = "";
    private static TaskbarIcon? _taskbarIcon;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        AllowTrailingCommas = true,
    };

    internal const string AppName = "Time on the TOP";
    internal const string AppId = "TimeOnTheTop";
    internal const string AppVersion = "1.1.2";

    internal static string ExecutableFilePath { get; private set; } = "";

    // config
    internal static Config Config { get; private set; } = new();
    internal static bool FirstStart { get; set; } = false;
    internal static bool ConfigChanged { get; set; } = false;

    // registry
    internal static RegistryKey StartupKey { get; private set; }

    // theme
    internal static bool AppLightTheme { get; private set; }
    internal static bool SystemLightTheme { get; private set; }

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
            var jsonText = JsonSerializer.Serialize(Config, JsonOptions);
            File.WriteAllText(_configFile, jsonText);
        });
    }

    internal static void SaveChangedConfig()
    {
        ConfigChanged = true;
        SaveConfig();
    }

    private static readonly bool Is16299OrHigher = Environment.OSVersion.Version >= new Version(10, 0, 16299);
    internal static void SetEfficiencyMode(bool value)
    {
        if (Is16299OrHigher)
        {
#pragma warning disable CA1416
            EfficiencyModeUtilities.SetEfficiencyMode(value);
#pragma warning restore CA1416 
        }
    }
    
    public App()
    {
        // get config file path
        var executableFile = Environment.ProcessPath;
        if (executableFile == null) OnInitError("Config", "Executable path not available");
        executableFile = Path.GetFullPath(executableFile!);
        ExecutableFilePath = executableFile;
        var configDir = Path.GetDirectoryName(executableFile)!;
        
        // global exception handler
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var content = args.ExceptionObject.ToString();
            File.WriteAllText(Path.Combine(configDir, $"{AppId}_Exception.txt"), content);
            MessageBox.Show(
                text: content,
                caption: $"Unhandled Exception - {AppName}",
                buttons: MessageBoxButton.OK,
                icon: MessageBoxImage.Error);
        };
        
        // load config
        var configFile = Path.Combine(configDir, $"{AppId}.json");
        _configFile = configFile;
        try
        {
            if (File.Exists(configFile))
            {
                var jsonText = File.ReadAllText(configFile);
                var deserializedConfig = JsonSerializer.Deserialize<Config>(jsonText, JsonOptions);
                if (deserializedConfig == null) OnInitError("Config", "Invalid config content");
                Config = deserializedConfig!;
            }
            else
            {
                FirstStart = true;
                File.Create(configFile).Close();
                SaveConfig();
            }
        }
        catch (Exception e)
        {
            OnInitError("Config", $"Error while reading/creating config file:\n{e}");
            throw;
        }

        InitializeComponent();

        // setup resources
        _taskbarIcon = (Resources["TaskbarIcon"] as TaskbarIcon)!;
        
        // detect system theme changes
        OnSystemThemeChanged();
        SystemEvents.UserPreferenceChanged += (_, _) => OnSystemThemeChanged();

        // configure taskbar icon
        _taskbarIcon.ToolTipText = AppName;
        _taskbarIcon.ForceCreate();
    }

    private void OnSystemThemeChanged()
    {
        // get system theme config

        var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        if (key?.GetValue("AppsUseLightTheme") is int appLight) AppLightTheme = appLight != 0;
        if (key?.GetValue("SystemUsesLightTheme") is int sysLight) SystemLightTheme = sysLight != 0;

        // update icons

        var iconName = SystemLightTheme ? "appicon_light.ico" : "appicon_dark.ico";
        _taskbarIcon!.UpdateIcon(new Icon(GetResourceStream(new Uri($"pack://application:,,,/assets/{iconName}"))!.Stream));

        // update theme & color scheme

        WindowHelper.SetPreferredAppMode(AppLightTheme ? WindowHelper.APPMODE_LIGHT : WindowHelper.APPMODE_DARK);
        WindowHelper.FlushMenuThemes();
        ResourceLocator.SetColorScheme(Application.Current.Resources,
            AppLightTheme ? ResourceLocator.LightColorScheme : ResourceLocator.DarkColorScheme);

        var cfgWindow = ConfigWindow.Current;
        if (cfgWindow?.IsInitialized == true) cfgWindow.UpdateWindowTheme();
    }

    internal static void OnInitError(string type, string message)
    {
        MessageBox.Show(
            text: $"[{type}] {message}",
            caption: $"初始化出错 - {AppName}",
            buttons: MessageBoxButton.OK,
            icon: MessageBoxImage.Error);
        Current.Shutdown();
    }

    internal static void OnExit()
    {
        _taskbarIcon?.Dispose();
        Current.Shutdown();
    }

}
