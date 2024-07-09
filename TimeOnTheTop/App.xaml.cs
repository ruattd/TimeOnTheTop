using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace TimeOnTheTop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static string _configFile = "";

    internal static string ExecutableFilePath = "";
    internal static Config Config = new();
    internal static bool FirstStart = false;
    internal static bool ConfigChanged = false;

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
        var configFile = Path.Combine(configDir, "TimeOnTheTop.json");
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
            OnInitError("Config", "Error while reading/creating config file:\n" + e);
            throw;
        }
    }

    private static void OnInitError(string type, string message)
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
            "[" + type + "] " + message,
            "初始化出错 - Time on the TOP",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        Current.Shutdown();
    }

}
