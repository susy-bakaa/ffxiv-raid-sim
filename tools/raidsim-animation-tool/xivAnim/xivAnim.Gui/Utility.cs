using dev.susy_baka.xivAnim.Core;

namespace dev.susy_baka.xivAnim.EtoGui
{
    public static class Utility
    {
        public static T Also<T>(this T control, Action<T> configure)
        {
            configure(control);
            return control;
        }

        public static string GetRememberedDirectory(AppSettings settings, string key, string fallback)
        {
            if (settings.LastDialogDirectories.TryGetValue(key, out var dir) &&
                !string.IsNullOrWhiteSpace(dir) &&
                Directory.Exists(dir))
            {
                return dir;
            }

            if (!string.IsNullOrWhiteSpace(fallback) && Directory.Exists(fallback))
                return fallback;

            return AppPaths.UserHomeDirectory;
        }

        public static void RememberDirectory(AppSettings settings, string key, string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return;

            settings.LastDialogDirectories[key] = directory;
            SettingsService.Save(settings);
        }

        public static void RememberDirectoryFromFile(AppSettings settings, string key, string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir))
                RememberDirectory(settings, key, dir);
        }

        public static Uri DirectoryUri(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                directory = AppPaths.UserHomeDirectory;

            if (!Directory.Exists(directory))
                directory = AppPaths.UserHomeDirectory;

            return new Uri(Path.GetFullPath(directory) + Path.DirectorySeparatorChar);
        }
    }
}
