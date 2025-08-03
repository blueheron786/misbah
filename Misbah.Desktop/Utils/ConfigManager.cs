using System.IO;
using System.Text.Json;

namespace Misbah.Desktop.Utils
{
    public class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(".misbah", "config.json");

        public class Config
        {
            public string? LastHubPath { get; set; }
        }

        public static Config Load()
        {
            if (!File.Exists(ConfigPath))
                return new Config();
            var json = File.ReadAllText(ConfigPath);
            // Migrate old config if needed
            var doc = JsonDocument.Parse(json);
            var config = new Config();
            if (doc.RootElement.TryGetProperty("LastHubPath", out var hubProp))
            {
                config.LastHubPath = hubProp.GetString();
            }
            else if (doc.RootElement.TryGetProperty("LastVaultPath", out var vaultProp))
            {
                config.LastHubPath = vaultProp.GetString();
            }
            return config;
        }

        public static void Save(Config config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
