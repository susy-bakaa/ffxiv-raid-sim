using System.Diagnostics;
using dev.susy_baka.xivAnim.Core;

namespace dev.susy_baka.xivAnim.Cli
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Log.Initialize();
            Log.MessageLogged += line => Console.WriteLine(line);

            try
            {
                return Run(args);
            }
            catch (Exception ex)
            {
                Log.Error($"Unhandled exception: {ex}");
                return 1;
            }
        }

        private static int Run(string[] args)
        {
            if (args.Length == 0)
            {
                Log.Info("No arguments provided, launching GUI.");
                LaunchGUI();
                return 0;
            }

            if (args[0] == "-h" || args[0] == "--help")
            {
                ShowHelp();
                return 0;
            }

            // Simple parse for settings
            if (args[0] == "-s" || args[0] == "--settings")
            {
                return ConfigureAndMaybeRun(args, false);
            }
            else if (args[0] == "-fs" || args[0] == "--force-settings")
            {
                return ConfigureAndMaybeRun(args, true);
            }

            // Simple parse for job run / clear
            if (args[0] == "-j" || args[0] == "--job")
            {
                if (args.Length < 2 || !Path.Exists(args[1]))
                {
                    Log.Error("Missing valid job path after -j.");
                    return 1;
                }
                var jobPath = args[1];
                return RunJob(jobPath);
            }
            else if (args[0] == "-cj" || args[0] == "--clearjob")
            {
                if (args.Length < 2 || !Path.Exists(args[1]))
                {
                    Log.Error("Missing valid job path after -cj.");
                    return 1;
                }
                var jobPath = args[1];
                return ClearJob(jobPath);
            }

            // If first arg is not a switch, assume it's a job.json
            if (!args[0].StartsWith("-") && Path.Exists(args[0]))
            {
                var jobPath = args[0];
                return RunJob(jobPath);
            }
            else if (!args[0].StartsWith("-") && !Path.Exists(args[0]))
            {
                Log.Error($"Job file '{args[0]}' does not exist.");
                return 1;
            }

            ShowHelp();
            return 0;
        }

        private static void ShowHelp()
        {
            Log.Info("Usage:");
            Log.Info("  xivAnim.exe | Launches the GUI.");
            Log.Info("  xivAnim.exe -h | --help | Shows this help message.");
            Log.Info("  xivAnim.exe <job.json> | Runs the provided job.");
            Log.Info("  xivAnim.exe -j job.json | Runs the provided job.");
            Log.Info("  xivAnim.exe -cj job.json | Deletes the output files of the provided job.");
            Log.Info("  xivAnim.exe -s <ffxivSqpackPath> <blenderPath> <multiAssistPath> [-j job.json] [-cj job.json] | Configures the persistent settings if they do not exist and optionally runs a job or clears output of a job after.");
            Log.Info("  xivAnim.exe -fs <ffxivSqpackPath> <blenderPath> <multiAssistPath> [-j job.json] [-cj job.json]| Configures the persistent settings overwriting any existing ones and optionally runs a job or clears output of a job after.");
        }

        private static int ConfigureAndMaybeRun(string[] args, bool force)
        {
            if (args.Length < 4)
            {
                Log.Error("Missing paths. Usage: -s <ffxiv sqpack> <blender> <multiassist> [-j job.json] [-cj job.json]");
                return 1;
            }

            string ffxiv = args[1];
            string blender = args[2];
            string multi = args[3];

            if (!ffxiv.Contains("sqpack"))
            {
                Log.Error("FFXIV path does not appear to be a valid sqpack directory. Please input a valid 'FINAL FANTASY XIV Online\\game\\sqpack' directory.");
            }

            bool exists = SettingsService.FileExists();

            AppSettings settings = SettingsService.Load();
            
            if (exists && !force)
            {
                Log.Info("Configuration file already exists. Use -fs to force reconfiguration.");
            }
            else
            {
                settings.ffxivGamePath = ffxiv;
                settings.blenderPath = blender;
                settings.multiAssistPath = multi;
                SettingsService.Save(settings);
                Log.Info("Configuration saved.");
            }

            // Do we have a -j / -cj job.json after that?
            string? jobPath = null;
            bool clear = false;
            for (int i = 4; i < args.Length - 1; i++)
            {
                if (args[i] == "-j" || args[i] == "--job")
                {
                    clear = false;
                    jobPath = args[i + 1];
                    break;
                }
                else if (args[i] == "-cj" || args[i] == "--clearjob")
                {
                    clear = true;
                    jobPath = args[i + 1];
                    break;
                }
            }

            if (jobPath != null)
            {
                if (!clear)
                {
                    Log.Info($"Running job from {jobPath}...");
                    return RunJob(jobPath);
                }
                else
                {
                    Log.Info($"Clearing job output from {jobPath}...");
                    return ClearJob(jobPath);
                }
            }

            return 0;
        }

        private static int RunJob(string jobPath)
        {
            var settings = SettingsService.Load();
            if (string.IsNullOrWhiteSpace(settings.ffxivGamePath)
                || string.IsNullOrWhiteSpace(settings.blenderPath)
                || string.IsNullOrWhiteSpace(settings.multiAssistPath)
                || !settings.ffxivGamePath.Contains("sqpack"))
            {
                Log.Error("Settings incomplete or wrong. Run -s first to configure persistent paths.");
                return 1;
            }

            var pipeline = new PipelineService(settings);
            pipeline.RunJob(jobPath);
            return 0;
        }

        private static int ClearJob(string jobPath)
        {
            var settings = SettingsService.Load();
            if (string.IsNullOrWhiteSpace(settings.ffxivGamePath)
                || string.IsNullOrWhiteSpace(settings.blenderPath)
                || string.IsNullOrWhiteSpace(settings.multiAssistPath)
                || !settings.ffxivGamePath.Contains("sqpack"))
            {
                Log.Error("Settings incomplete or wrong. Run -s first to configure persistent paths.");
                return 1;
            }

            var pipeline = new PipelineService(settings);
            pipeline.ClearJob(jobPath);
            return 0;
        }

        private static int LaunchGUI()
        {
            string exeDir = AppContext.BaseDirectory;
            string guiExePath = Path.Combine(exeDir, "xivAnim.Gui.exe");
            if (!File.Exists(guiExePath))
            {
                Log.Error("GUI executable not found.");
                return 1;
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = guiExePath,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            return 0;
        }
    }
}