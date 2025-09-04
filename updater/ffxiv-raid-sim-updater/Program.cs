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

            Console.WriteLine($"Closing the game (PID: {gamePID})...");
            KillProcessByPID(gamePID);

            // Prepare self-replacement
            if (IsWindows)
            {
                // Windows: Rename the current updater.exe before extracting so it can be overwritten.
                if (File.Exists(updaterWin))
                {
                    try
                    {
                        if (File.Exists(oldUpdaterWin))
                            File.Delete(oldUpdaterWin);

                        File.Move(updaterWin, oldUpdaterWin);
                        Console.WriteLine("Renamed updater.exe -> updater.old.exe for replacement.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to rename updater.exe: {ex.Message}");
                        return 4;
                    }
                }
            }
            else if (IsLinux)
            {
                // Linux: We can safely unlink (delete) the running binary directly before extraction.
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
            if (ExtractZipWithProgress(zipPath, gameDir))
            {
                Console.WriteLine("\nUpdate applied successfully.");

                // Linux: restore executable permissions (zip may not preserve them)
                if (IsLinux)
                {
                    // Preferred name "raidsim"
                    TryChmodX(Path.Combine(gameDir, "updater"));
                    TryChmodX(Path.Combine(gameDir, "raidsim"));

                    // Fallbacks
                    foreach (var bin in Directory.GetFiles(gameDir, "*.x86_64"))
                        TryChmodX(bin);
                    foreach (var sh in Directory.GetFiles(gameDir, "*.sh"))
                        TryChmodX(sh);
                }

                Console.WriteLine("Cleaning up zip...");
                TryDelete(zipPath);

                // Relaunch the game
                try
                {
                    string gameExePath = FindGameExecutable(gameDir);
                    Console.WriteLine($"Restarting game: {Path.GetFileName(gameExePath)}");
                    Thread.Sleep(2000);
                    StartProcess(gameExePath, gameDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restart game: {ex.Message}");
                }

                // Windows: cleanup old updater via generated batch script
                if (IsWindows && File.Exists(oldUpdaterWin))
                {
                    Console.WriteLine("Scheduling cleanup of old updater...");
                    string bat = Path.Combine(gameDir, "updater.bat");
                    File.WriteAllText(bat, GenerateCleanupBatchScript());
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
                        Console.WriteLine($"Failed to start cleanup script: {ex.Message}");
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
        static bool ExtractZipWithProgress(string zipPath, string extractPath)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    int totalFiles = archive.Entries.Count;
                    int extractedFiles = 0;

                    foreach (var entry in archive.Entries)
                    {
                        // Skip directory entries explicitly
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            // Ensure directory exists
                            string dirPathOnly = Path.Combine(extractPath, entry.FullName);
                            Directory.CreateDirectory(dirPathOnly);
                        }
                        else
                        {
                            string filePath = Path.Combine(extractPath, entry.FullName);
                            string? directoryPath = Path.GetDirectoryName(filePath);
                            if (!string.IsNullOrEmpty(directoryPath))
                                Directory.CreateDirectory(directoryPath);

                            entry.ExtractToFile(filePath, true);
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
        static string GenerateCleanupBatchScript()
        {
            // Wait a bit for this process to exit, then delete the old updater & self-delete.
            return @"
            @echo off
            timeout /t 3 /nobreak >nul
            del ""updater.old.exe""
            del ""%~f0""
            exit";
        }
    }
}
