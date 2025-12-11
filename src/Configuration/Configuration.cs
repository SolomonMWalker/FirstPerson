using System.IO;
using System.Text.Json;
using Godot;

namespace FirstPerson.Configuration;

public static class Configuration
{
    private static ConfigValues _configValues;

    static Configuration()
    {
        SetUpConfiguration();
    }
    
    public static ConfigValues GetConfigValues()
    {
        if (_configValues == null)
        {
            SetUpConfiguration();
        }

        return _configValues;
    }
    
    private static void SetUpConfiguration()
    {
        var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration.json");
        string configJsonString = "";
        try
        {
            configJsonString = File.ReadAllText(configFilePath);
        }
        catch
        {
            GD.PrintErr($"Couldn't read file at {configFilePath}");
        }
        _configValues = JsonSerializer.Deserialize<ConfigValues>(configJsonString)!;
        GD.Print($"Configuration setup with {JsonSerializer.Serialize(_configValues)}");
    }
}

public record ConfigValues(
    string PlayerSceneTreePath,
    string ProjectileDirectoryPath
);