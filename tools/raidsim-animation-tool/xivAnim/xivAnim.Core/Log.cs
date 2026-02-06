namespace dev.susy_baka.xivAnim.Core
{
    public static class Log
    {
        private static readonly object _sync = new();
        private static readonly string _logPath = Path.Combine(AppContext.BaseDirectory, "output.log");
        private static bool _initialized = false;

        public static event Action<string>? MessageLogged;

        private static void Write(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            lock (_sync)
            {
                File.AppendAllText(_logPath, line + Environment.NewLine);
            }

            MessageLogged?.Invoke(line);
        }

        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            if (File.Exists(_logPath))
            {
                File.Delete(_logPath);
            }
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Error(string message) => Write("ERROR", message);
        public static void Debug(string message) => Write("DEBUG", message);
        public static void Warning(string message) => Write("WARNING", message);
    }
}
