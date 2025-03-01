using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace susy_baka.raidsim.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: updater.exe <ZipFilePath> <GamePID>");
                Thread.Sleep(3500);
                return;
            }

            string zipPath = args[0];   // Path to the downloaded ZIP file
            int gamePID = int.Parse(args[1]);   // Process ID of the running game
            string gameDir = AppDomain.CurrentDomain.BaseDirectory; // Game directory
            string updaterPath = Path.Combine(gameDir, "updater.exe");
            string updaterDllPath = Path.Combine(gameDir, "updater.dll");
            string updaterConfigPath = Path.Combine(gameDir, "updater.runtimeconfig.json");
            string oldUpdaterPath = Path.Combine(gameDir, "updater.old.exe");
            string oldUpdaterDllPath = Path.Combine(gameDir, "updater.old.dll");
            string oldUpdaterConfigPath = Path.Combine(gameDir, "updater.runtimeconfig.old.json");
            string batchScriptPath = Path.Combine(gameDir, "updater.bat");

            Console.WriteLine($"Closing the game (PID: {gamePID})...");
            KillProcessByPID(gamePID);

            // Rename the current updater.exe before extracting
            if (File.Exists(updaterPath))
            {
                try
                {
                    if (File.Exists(oldUpdaterPath))
                        File.Delete(oldUpdaterPath);
                    if (File.Exists(oldUpdaterDllPath))
                        File.Delete(oldUpdaterDllPath);
                    if (File.Exists(oldUpdaterConfigPath))
                        File.Delete(oldUpdaterConfigPath);
                    File.Move(updaterPath, oldUpdaterPath);
                    File.Move(updaterDllPath, oldUpdaterDllPath);
                    File.Move(updaterConfigPath, oldUpdaterConfigPath);
                    Console.WriteLine("Renamed updater.exe to updater.old.exe for replacement.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to rename updater.exe: {ex.Message}");
                    return;
                }
            }

            Console.WriteLine("Extracting update...");
            if (ExtractZipWithProgress(zipPath, gameDir))
            {
                Console.WriteLine("Update applied successfully.");
                Console.WriteLine("Cleaning up...");

                try
                {
                    File.Delete(zipPath);
                    Console.WriteLine("ZIP file deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete ZIP file: {ex.Message}");
                }

                Console.WriteLine("Restarting game...");
                string gameExePath = Path.Combine(gameDir, "raidsim.exe");
                Thread.Sleep(3500);
                Process.Start(gameExePath);
            }
            else
            {
                Console.WriteLine("Update failed.");
            }

            Console.WriteLine("Creating cleanup script for old updater...");
            File.WriteAllText(batchScriptPath, GenerateCleanupBatchScript());

            ProcessStartInfo psi = new ProcessStartInfo(batchScriptPath)
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi);

            Console.WriteLine("Closing Updater...");
            Thread.Sleep(2000);
            Environment.Exit(0);
        }

        static void KillProcessByPID(int pid)
        {
            try
            {
                Process gameProcess = Process.GetProcessById(pid);
                gameProcess.Kill();
                gameProcess.WaitForExit();
                Console.WriteLine("Game process terminated.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to terminate process: {e.Message}");
            }
        }

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
                        string filePath = Path.Combine(extractPath, entry.FullName);
                        string? directoryPath = Path.GetDirectoryName(filePath);

                        if (!string.IsNullOrEmpty(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        entry.ExtractToFile(filePath, true);
                        extractedFiles++;

                        // Progress bar
                        int progress = (int)(((float)extractedFiles / totalFiles) * 100);
                        Console.Write($"\rExtracting: [{new string('#', progress / 2)}{new string('-', 50 - (progress / 2))}] {progress}%");
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

        static string GenerateCleanupBatchScript()
        {
            return @"
            @echo off
            timeout /t 4 /nobreak >nul
            del ""updater.old.exe""
            del ""updater.old.dll""
            del ""updater.runtimeconfig.old.json""
            del ""%~f0""
            exit";
        }
    }
}