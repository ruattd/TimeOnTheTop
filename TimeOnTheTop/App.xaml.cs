using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using AdonisUI;
using H.NotifyIcon;
using H.NotifyIcon.EfficiencyMode;
using Microsoft.Win32;

namespace TimeOnTheTop;

// ReSharper disable StringLiteralTypo

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
    internal const string AppVersion = "1.2.0";

    internal static string ExecutableFilePath { get; private set; } = "";

    // config
    internal static Config Config { get; private set; } = new();
    internal static string ConfigDirectory = "";
    internal static bool FirstStart { get; set; } = false;
    internal static bool ConfigChanged { get; set; } = false;

    // registry
    internal static RegistryKey StartupKey { get; private set; }

    // theme
    internal static bool AppLightTheme { get; private set; } = true;
    internal static bool SystemLightTheme { get; private set; } = true;

    static App()
    {
        // catch unhandled exceptions
        System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
        System.Windows.Forms.Application.ThreadException += (_, args) => OnCatchUnhandledException(args.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, args) => OnCatchUnhandledException(args.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, args) => OnCatchUnhandledException(args.Exception);

        // get startup registry key
        var key = Registry.CurrentUser
            .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        if (key == null) OnInitError("注册", "无法获取启动项的注册表键");
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
        // catch dispatcher unhandled exceptions
        Current.DispatcherUnhandledException += (_, args) => OnCatchUnhandledException(args.Exception);

        {   // get executable path
            var executableFile = Environment.ProcessPath;
            if (executableFile == null) OnInitError("配置", "可执行文件目录不可用");
            ExecutableFilePath = Path.GetFullPath(executableFile!);
            ConfigDirectory = Path.GetDirectoryName(executableFile)!;
        }
        
        {   // single instance
            var current = Process.GetCurrentProcess();
            var currentSessionId = current.SessionId;
            var match = false;
            foreach (var process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.SessionId == currentSessionId
                    && Path.GetDirectoryName(process.MainModule?.FileName) is { } directory
                    && directory == ConfigDirectory)
                {
                    if (match) OnInitError("进程",
                        "已有相同实例正在运行，若要同时运行多个实例，请将可执行文件置于不同的目录中" +
                        $"\n当前目录: {directory}\n会话 ID: {currentSessionId}");
                    match = true;
                }
            }
        }
        
        {   // load config
            var configFile = Path.Combine(ConfigDirectory, $"{AppId}.json");
            _configFile = configFile;
            try
            {
                if (File.Exists(configFile))
                {
                    var jsonText = File.ReadAllText(configFile);
                    var deserializedConfig = JsonSerializer.Deserialize<Config>(jsonText, JsonOptions);
                    if (deserializedConfig == null) OnInitError("配置", "非法的配置文件内容");
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
                OnInitError("配置", $"读取/写入配置文件时出现异常:\n{e}");
            }
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

    private static void OnSystemThemeChanged()
    {
        // get system theme config

        var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        if (key?.GetValue("AppsUseLightTheme") is int appLight) AppLightTheme = appLight != 0;
        if (key?.GetValue("SystemUsesLightTheme") is int sysLight) SystemLightTheme = sysLight != 0;

        // update icons

        var iconName = SystemLightTheme ? "appicon_light.ico" : "appicon_dark.ico";
        _taskbarIcon!.UpdateIcon(new System.Drawing.Icon(GetResourceStream(new Uri($"pack://application:,,,/assets/{iconName}"))!.Stream));

        // update theme & color scheme

        _ = WindowHelper.SetPreferredAppMode(AppLightTheme ? WindowHelper.APPMODE_LIGHT : WindowHelper.APPMODE_DARK);
        WindowHelper.FlushMenuThemes();
        ResourceLocator.SetColorScheme(Current.Resources,
            AppLightTheme ? ResourceLocator.LightColorScheme : ResourceLocator.DarkColorScheme);

        var cfgWindow = ConfigWindow.Current;
        if (cfgWindow?.IsInitialized == true) cfgWindow.UpdateWindowTheme();
    }

    internal static void OnCatchUnhandledException(object e)
    {
        var content = e.ToString();
        const string logFileName = $"{AppId}_Exception.txt";
        File.WriteAllText(Path.Combine(ConfigDirectory, logFileName), content);
        MessageBox.Show(
            messageBoxText: $"如果要寻求帮助，请发送这个窗口的截图或程序目录中的 \"{logFileName}\" 文件\n{content}",
            caption: $"未捕获的异常 - {AppName}",
            button: MessageBoxButton.OK,
            icon: MessageBoxImage.Error);
        Process.GetCurrentProcess().Kill();
    }

    internal static void OnInitError(string type, string message)
    {
        MessageBox.Show(
            messageBoxText: $"如果要寻求帮助，请发送这个窗口的截图\n[{type}] {message}",
            caption: $"初始化出错 - {AppName}",
            button: MessageBoxButton.OK,
            icon: MessageBoxImage.Error);
        Process.GetCurrentProcess().Kill();
    }

    internal static void OnExit()
    {
        _taskbarIcon?.Dispose();
        Current.Shutdown();
    }

}
