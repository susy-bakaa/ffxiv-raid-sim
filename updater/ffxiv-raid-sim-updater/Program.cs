using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace susy_baka.raidsim.Updater
{
    class Program
    {
        // OS helpers
        static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: updater <ZipFilePath> <GamePID>");
                Thread.Sleep(4000);
                return 1;
            }

            string zipPath = args[0]; // Path to the downloaded zip file
            if (!File.Exists(zipPath))
            {
                Console.WriteLine($"Zip not found: {zipPath}");
                Thread.Sleep(4000);
                return 2;
            }

            if (!int.TryParse(args[1], out int gamePID))
            {
                Console.WriteLine($"Invalid GamePID: {args[1]}");
                Thread.Sleep(4000);
                return 3;
            }

            string gameDir = AppDomain.CurrentDomain.BaseDirectory; // Game directory
            string updaterWin = Path.Combine(gameDir, "updater.exe");
            string updaterLin = Path.Combine(gameDir, "updater");
            string oldUpdaterWin = Path.Combine(gameDir, "updater.old.exe");
            string stageDir = Path.Combine(gameDir, ".updater"); // Windows staging for updater.exe
            DirectoryInfo stageDirInfo = Directory.CreateDirectory(stageDir);
            stageDirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            Console.WriteLine($"Closing the game (PID: {gamePID})...");
            KillProcessByPID(gamePID);

            // Linux: We can safely unlink (delete) the running binary directly before extraction.
            if (IsLinux)
            {
                try
                {
                    if (File.Exists(updaterLin))
                    {
                        File.Delete(updaterLin);
                        Console.WriteLine("Deleted running 'updater' to free the path.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: failed to delete updater before extraction: {ex.Message}");
                    // Not fatal in this case, so continue, since the path may already be free.
                }
            }

            // Extract the update
            Console.WriteLine("Extracting update...");
            if (ExtractZipWithProgress(zipPath, gameDir, stageDir, out var stagedUpdaterPath))
            {
                Console.WriteLine("\nUpdate applied successfully.");

                // Linux: restore executable permissions (zip may not preserve them)
                if (IsLinux)
                {
                    TryChmodX(Path.Combine(gameDir, "updater"));
                    TryChmodX(Path.Combine(gameDir, "raidsim"));
                    foreach (var bin in Directory.GetFiles(gameDir, "*.x86_64"))
                        TryChmodX(bin);
                    foreach (var sh in Directory.GetFiles(gameDir, "*.sh"))
                        TryChmodX(sh);
                }

                Console.WriteLine("Cleaning up zip...");
                TryDelete(zipPath);

                // ---- Relaunch game
                try
                {
                    string gameExePath = FindGameExecutable(gameDir);
                    Console.WriteLine($"Restarting game: {Path.GetFileName(gameExePath)}");
                    Thread.Sleep(2500);
                    StartProcess(gameExePath, gameDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restart game: {ex.Message}");
                }

                // Windows: if we staged a new updater.exe, replace it via a batch script AFTER we exit
                if (IsWindows && !string.IsNullOrEmpty(stagedUpdaterPath) && File.Exists(stagedUpdaterPath))
                {
                    string destUpdater = Path.Combine(gameDir, "updater.exe");
                    string bat = Path.Combine(gameDir, "updater_replace.bat");
                    File.WriteAllText(bat, GenerateCleanupBatchScript(stagedUpdaterPath, destUpdater, stageDir));

                    try
                    {
                        Process.Start(new ProcessStartInfo(bat)
                        {
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            WorkingDirectory = gameDir
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to start replace script: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Update failed during extraction.");
                Thread.Sleep(4000);
                return 5;
            }

            Console.WriteLine("Closing Updater...");
            Thread.Sleep(4000);
            return 0;
        }

        // Process helpers
        static void KillProcessByPID(int pid)
        {
            try
            {
                var proc = Process.GetProcessById(pid);
                proc.Kill();
                proc.WaitForExit();
                Console.WriteLine("Game process terminated.");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Game process already not running.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to terminate game process: {e.Message}");
            }
        }

        static void StartProcess(string exePath, string workingDir)
        {
            var psi = new ProcessStartInfo(exePath)
            {
                UseShellExecute = false,
                WorkingDirectory = workingDir
            };
            Process.Start(psi);
        }

        // Extraction of the zip with progress
        static bool ExtractZipWithProgress(string zipPath, string extractPath, string windowsUpdaterStageDir, out string? stagedUpdaterPath)
        {
            stagedUpdaterPath = null;
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    int totalFiles = archive.Entries.Count;
                    int extractedFiles = 0;

                    foreach (var entry in archive.Entries)
                    {
                        bool isDirEntry = string.IsNullOrEmpty(entry.Name);
                        bool isWindowsUpdaterExe = IsWindows &&
                            entry.Name.Equals("updater.exe", StringComparison.OrdinalIgnoreCase);

                        // Decide where to place this entry:
                        string targetRoot = (isWindowsUpdaterExe ? windowsUpdaterStageDir : extractPath);
                        string targetPath = Path.Combine(targetRoot, entry.FullName);

                        if (isDirEntry)
                        {
                            Directory.CreateDirectory(targetPath);
                        }
                        else
                        {
                            string? directoryPath = Path.GetDirectoryName(targetPath);
                            if (!string.IsNullOrEmpty(directoryPath))
                                Directory.CreateDirectory(directoryPath);

                            entry.ExtractToFile(targetPath, true);

                            if (isWindowsUpdaterExe)
                                stagedUpdaterPath = targetPath;

                            extractedFiles++;
                            int progress = (int)((extractedFiles / (double)totalFiles) * 100);
                            Console.Write($"\rExtracting: [{new string('#', progress / 2)}{new string('-', 50 - (progress / 2))}] {progress}%");
                        }
                    }
                }

                Console.WriteLine("\nExtraction complete!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nFailed to extract update: {ex.Message}");
                return false;
            }
        }

        // Platform-specific helpers
        static string FindGameExecutable(string gameDir)
        {
            if (IsWindows)
            {
                string win = Path.Combine(gameDir, "raidsim.exe");
                if (File.Exists(win))
                    return win;
                throw new FileNotFoundException("Expected file 'raidsim.exe' not found.");
            }
            else if (IsLinux)
            {
                // Prefer plain "raidsim"
                string linuxPreferred = Path.Combine(gameDir, "raidsim");
                if (File.Exists(linuxPreferred))
                    return linuxPreferred;

                // Fallback to Unity's default binary naming
                var linuxDefault = Directory.GetFiles(gameDir, "*.x86_64").FirstOrDefault();
                if (linuxDefault != null)
                    return linuxDefault;

                throw new FileNotFoundException("Expected file 'raidsim' (or raidsim.x86_64) not found.");
            }

            throw new PlatformNotSupportedException("Unsupported platform.");
        }

        static void TryChmodX(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return;

                var psi = new ProcessStartInfo("chmod", $"+x \"{path}\"")
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                var p = Process.Start(psi);
                p?.WaitForExit();
                Console.WriteLine($"chmod +x {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"chmod failed for {path}: {ex.Message}");
            }
        }

        static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Console.WriteLine($"Deleted: {Path.GetFileName(path)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete {path}: {ex.Message}");
            }
        }

        // Windows cleanup batch script to remove the old updater
        static string GenerateCleanupBatchScript(string stagedUpdaterPath, string destUpdaterPath, string stageDir)
        {
            // Wait a bit for this updater process to fully exit,
            // copy staged updater over the original, remove stage dir, then self-delete.
            return $@"
            @echo off
            setlocal
            timeout /t 3 /nobreak >nul
            copy /y ""{stagedUpdaterPath}"" ""{destUpdaterPath}"" >nul
            rd /s /q ""{stageDir}""
            del ""%~f0""
            endlocal
            exit";
        }
    }
}
