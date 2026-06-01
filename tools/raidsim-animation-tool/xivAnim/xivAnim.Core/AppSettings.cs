namespace dev.susy_baka.xivAnim.Core
{
    public class AppSettings
    {
        public string ffxivGamePath { get; set; } = "";
        public string multiAssistPath { get; set; } = "";
        public string blenderPath { get; set; } = "";

        public List<string> RecentJobs { get; set; } = new();
        public bool debugMode { get; set; } = false;
    }
}
