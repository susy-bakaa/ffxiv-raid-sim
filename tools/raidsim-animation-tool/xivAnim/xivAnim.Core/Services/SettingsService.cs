using System.Text.Json;

namespace dev.susy_baka.xivAnim.Core
{
    public static class SettingsService
    {
        public static AppSettings Load()
        {
            if (!File.Exists(AppPaths.SettingsPath))
                return new AppSettings();

            string json = File.ReadAllText(AppPaths.SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public static void Save(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(AppPaths.SettingsPath, json);
        }

        public static bool FileExists()
        {
            return File.Exists(AppPaths.SettingsPath);
        }
    }
}
