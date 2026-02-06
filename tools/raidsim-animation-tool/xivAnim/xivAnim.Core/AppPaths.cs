namespace dev.susy_baka.xivAnim.Core
{
    public static class AppPaths
    {
        public static string GetAppDataDir()
        {
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(baseDir, "xivAnim");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static string SettingsPath => Path.Combine(GetAppDataDir(), "config.json");

        // Default job.json next to the EXE
        public static string DefaultJobPath => Path.Combine(AppContext.BaseDirectory, "job.json");
    }
}
