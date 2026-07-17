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

        public static string JobsDirectory
        {
            get
            {
                var dir = Path.Combine(AppContext.BaseDirectory, "jobs");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string DefaultJobPath =>
            Path.Combine(JobsDirectory, "job.json");

        public static string UserHomeDirectory
        {
            get
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                    return dir;

                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }
    }
}
