// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace susy_baka.raidsim.Updater
{
    class Program
    {
        // OS helpers
        static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        // Simple logging
        static StreamWriter? _log;
        static bool _logClosed = false;
        static readonly object _logLock = new();

        static int Main(string[] args)
        {
            string gameDir = AppDomain.CurrentDomain.BaseDirectory; // Game directory

            // Linux: We need to use a small helper function to correctly resolve the directory
            if (IsLinux)
                gameDir = GetInstallDir();

            string updaterWin = Path.Combine(gameDir, "updater.exe"); // Windows executable
            string updaterLin = Path.Combine(gameDir, "updater"); // Linux executable
            string stageDir = Path.Combine(gameDir, ".updater"); // Hidden staging/cache location
            string logPath = Path.Combine(gameDir, "updater.log"); // Log file
            bool standaloneRun = false;

            if (File.Exists(logPath))
                File.Delete(logPath); // start fresh each run

            InitLog(gameDir);
            
            // Prepare a hidden staging/cache dir
            DirectoryInfo stageDirInfo = Directory.CreateDirectory(stageDir);
            stageDirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            if (IsWindows)
            {
                Log($"Updater started on platform: Windows64");
            }
            else if (IsLinux)
            {
                // Linux: We need to extract the embedded libraries and resources before running the application,
                // because otherwise it fails to find some of the built-in and third-party libraries.
                Environment.SetEnvironmentVariable("DOTNET_BUNDLE_EXTRACT_BASE_DIR", stageDir);
                Environment.SetEnvironmentVariable("DOTNET_BUNDLE_EXTRACT_TO_TEMP", "0");
                Log($"Updater started on platform: Linux64");
            }
            else
            {
                Log("Unsupported platform. Only Windows and Linux are currently supported.");
                Thread.Sleep(4000);
                OnClose(stageDir);
                return 10;
            }

            if (args.Length < 2)
            {
                Log("Usage: updater <ZipFilePath> <GamePID>");
                Thread.Sleep(4000);
                OnClose(stageDir);
                return 1;
            }

            string zipPath = args[0]; // Path to the downloaded zip file
            if (!File.Exists(zipPath))
            {
                Log($"Zip not found: {zipPath}");
                Thread.Sleep(4000);
                OnClose(stageDir);
                return 2;
            }

            if (!int.TryParse(args[1], out int gamePID))
            {
                if (args[1].Contains("nogame") || args[1].Contains("ng") || args[1].Contains("test"))
                {
                    Log("Standalone run detected. Existing game windows will not be closed.");
                    standaloneRun = true;
                }
                else
                {
                    Log($"Invalid GamePID: {args[1]}");
                    Thread.Sleep(4000);
                    OnClose(stageDir);
                    return 3;
                }
            }

            if (!standaloneRun)
            {
                Log($"Closing the game (PID: {gamePID})...");
                KillProcessByPID(gamePID);
            }
            else
            {
                Log("Standalone run, no game to close.");
            }

            // Linux: We can safely unlink (delete) the running binary directly before extraction.
            if (IsLinux)
            {
                try
                {
                    if (File.Exists(updaterLin))
                    {
                        File.Delete(updaterLin);
                        Log("Deleted running 'updater' to free the path.");
                    }
                }
                catch (Exception ex)
                {
                    Log($"Warning: failed to delete updater before extraction: {ex.Message}");
                    // Not fatal in this case, so continue, since the path may already be free.
                }
            }

            // Extract the update
            Log("Extracting update...");

            // Preflight: ZIP present + size + writeability
            if (!File.Exists(zipPath))
            {
                Log($"ZIP missing: {zipPath}");
                OnClose(stageDir);
                return 5;
            }

            try
            {
                var fi = new FileInfo(zipPath);
                Log($"ZIP: {zipPath} size={fi.Length} bytes");
            }
            catch (Exception ex)
            {
                Log($"ZIP stat failed: {ex}");
            }

            try
            {
                var probe = Path.Combine(gameDir, ".write_probe");
                File.WriteAllText(probe, "x");
                File.Delete(probe);
                Log("Write probe OK.");
            }
            catch (Exception ex)
            {
                Log($"Write probe failed: {ex}");
                OnClose(stageDir);
                return 5; // stop here—no write permission
            }

            if (ExtractZipWithProgress(zipPath, gameDir, stageDir, out var stagedUpdaterPath))
            {
                Log("\nUpdate applied successfully.");

                // Linux: restore executable permissions (zip may not preserve them)
                if (IsLinux)
                {
                    TryChmod("755", Path.Combine(gameDir, "updater"));
                    TryChmod("755", Path.Combine(gameDir, "raidsim"));

                    // Common Unity executables / fallbacks
                    var defaultLinuxExe = Directory.GetFiles(gameDir, "*.x86_64");
                    foreach (var f in defaultLinuxExe)
                        TryChmod("755", f);

                    TryChmod("755", Path.Combine(gameDir, "UnityCrashHandler64"));

                    foreach (var sh in Directory.GetFiles(gameDir, "*.sh"))
                        TryChmod("755", sh);
                }

                Log("Cleaning up zip...");
                TryDelete(zipPath);

                // Relaunch game
                try
                {
                    string gameExePath = FindGameExecutable(gameDir);
                    Log($"Restarting game: {Path.GetFileName(gameExePath)}");
                    Thread.Sleep(2500);
                    StartProcess(gameExePath, gameDir);
                }
                catch (Exception ex)
                {
                    Log($"Failed to restart game: {ex.Message}");
                }

                // Windows: if we staged a new updater.exe, replace it via a batch script AFTER we exit
                if (IsWindows && !string.IsNullOrEmpty(stagedUpdaterPath) && File.Exists(stagedUpdaterPath))
                {
                    string destUpdater = Path.Combine(gameDir, "updater.exe");
                    string bat = Path.Combine(gameDir, "updater.bat");
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
                        Log($"Failed to start replace script: {ex.Message}");
                    }
                }
            }
            else
            {
                Log("Update failed during extraction.");
                Thread.Sleep(4000);
                OnClose(stageDir);
                return 5;
            }

            Log("Closing Updater...");
            Thread.Sleep(2500);
            // Linux: Clean the staging/cache directory here, because we don't have script to do so like on Windows
            // On both close the log manually because we don't call OnClose() here
            if (IsLinux)
            {
                if (Directory.Exists(stageDir))
                    Directory.Delete(stageDir, true);
            }
            CloseLog();
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
                Log("Game process terminated.");
            }
            catch (ArgumentException)
            {
                Log("Game process already not running.");
            }
            catch (Exception e)
            {
                Log($"Failed to terminate game process: {e.Message}");
            }
        }

        static void StartProcess(string exePath, string workingDir)
        {
            try
            {
                ProcessStartInfo psi;

                // Windows: UseShellExecute=true to get normal window behavior and no redirections or
                // other changes to allow this application to close and let the game take over.
                if (IsWindows)
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = workingDir,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    };
                }
                else
                {
                    // Linux: We want don't want to capture stdout/stderr and we don't use UseShellExecute.
                    psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false
                    };
                }

                // Linux: Ensure the working directory is in LD_LIBRARY_PATH for native libs
                if (IsLinux)
                {
                    var existing = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? string.Empty;
                    psi.Environment["LD_LIBRARY_PATH"] = string.IsNullOrEmpty(existing) ? workingDir : $"{workingDir}:{existing}";
                }

                var p = Process.Start(psi);
                if (p == null)
                { 
                    Log("Failed to start process (null).");
                    return; 
                }
            }
            catch (Exception ex)
            {
                Log($"StartProcess error: {ex.Message}");
            }
        }

        static void InitLog(string gameDir)
        {
            var logPath = Path.Combine(gameDir, "updater.log");
            _log = new StreamWriter(new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                AutoFlush = true
            };
            _logClosed = false;
        }

        static void Log(string msg, bool writeConsole = true)
        {
            if (_logClosed || _log is null)
                return;
            lock (_logLock)
            {
                if (writeConsole)
                    Console.WriteLine(msg);
                _log.WriteLine($"[{DateTime.Now:HH.mm.ss}] {msg.Replace("\n", "")}");
            }
        }

        static void CloseLog()
        {
            if (_logClosed)
                return;

            _logClosed = true;
            try
            { 
                _log?.Flush(); 
                _log?.Dispose(); 
            }
            catch 
            { 
                // ignore
            }
            _log = null;
        }

        // Extraction of the zip with progress (SharpCompress)
        static bool ExtractZipWithProgress(string zipPath, string extractPath, string windowsUpdaterStageDir, out string? stagedUpdaterPath)
        {
            stagedUpdaterPath = null;

            try
            {
                // Open archive first
                using var archive = ArchiveFactory.Open(zipPath);

                var entries = archive.Entries.ToList();
                int totalFiles = entries.Count(e => !e.IsDirectory);
                int extractedFiles = 0;

                // Zip-slip guard roots
                string extractRoot = Path.GetFullPath(extractPath) + Path.DirectorySeparatorChar;
                string stageRoot = Path.GetFullPath(windowsUpdaterStageDir) + Path.DirectorySeparatorChar;
                
                Log($"Total entries found in zip: {entries.Count}", false);

                foreach (var entry in entries)
                {
                    // Because SharpCompress uses '/' in it's entry.Key we need to normalize it for Windows
                    string relativePath = (entry.Key ?? string.Empty).Replace('/', Path.DirectorySeparatorChar);
                    bool isWindowsUpdaterExe = IsWindows && string.Equals(Path.GetFileName(relativePath), "updater.exe", StringComparison.OrdinalIgnoreCase);

                    // Decide target root for this entry and on Windows, stage updater.exe
                    string targetRoot = (isWindowsUpdaterExe ? windowsUpdaterStageDir : extractPath);

                    // Build destination path and guard against zip slip
                    string destPath = Path.Combine(targetRoot, relativePath);
                    string fullDest = Path.GetFullPath(destPath);
                    string guardRoot = (isWindowsUpdaterExe ? stageRoot : extractRoot).TrimEnd(Path.DirectorySeparatorChar);
                    if (!fullDest.StartsWith(guardRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                        throw new IOException($"Zip entry escapes target dir: \nentry {entry.Key} \nfullDest {fullDest} \nguardRoot {guardRoot} \nextractRoot {extractRoot}");

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(fullDest);
                        continue;
                    }

                    // Ensure directory exists
                    string? destDir = Path.GetDirectoryName(fullDest);
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    // Extraction with overwrite
                    entry.WriteToDirectory(
                        targetRoot,
                        new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true,
                            PreserveFileTime = true
                        });

                    if (isWindowsUpdaterExe)
                        stagedUpdaterPath = fullDest;

                    // Progress written into the console
                    extractedFiles++;
                    int progress = (int)((extractedFiles / (double)totalFiles) * 100);
                    Console.Write($"\rExtracting: [{new string('#', progress / 2)}{new string('-', 50 - (progress / 2))}] {progress}%");
                }

                Log("\nExtraction complete!");
                return true;
            }
            catch (Exception ex)
            {
                Log($"\nFailed to extract update: {ex.Message}");
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

        static string GetInstallDir()
        {
            // .NET 6+ gives you the path to the single-file bundle on disk
            var procPath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(procPath))
                throw new InvalidOperationException("Cannot resolve process path.");
            return Path.GetDirectoryName(procPath)!;
        }

        static void TryChmod(string mode, string path)
        {
            try
            {
                if (!File.Exists(path))
                    return;
                var psi = new ProcessStartInfo("chmod", $"{mode} \"{path}\"")
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                Process.Start(psi)?.WaitForExit();
                Log($"chmod {mode} {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                Log($"chmod failed for {path}: {ex.Message}");
            }
        }

        static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Log($"Deleted: {Path.GetFileName(path)}");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to delete {path}: {ex.Message}");
            }
        }

        // Small helper function to cleanup temporary directories and close the log
        static void OnClose(string tempDir)
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
            CloseLog();
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
