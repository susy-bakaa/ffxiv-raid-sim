// Referenced and inspiration from: https://github.com/haliconfr/unity-autoupdater
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using TMPro;
using Debug = UnityEngine.Debug;
using System.Security.Cryptography;

namespace susy_baka.raidsim.Updater
{
    public class UpdateHandler : MonoBehaviour
    {
        [Header("Functionality")]
        [SerializeField] private string updateUrl = "https://github.com/susy-bakaa/ffxiv-raid-sim/releases/download/v.{0}/raidsim_v.{0}_{1}64.zip";
        [SerializeField] private string checksumUrl = "https://github.com/susy-bakaa/ffxiv-raid-sim/releases/download/v.{0}/checksums.sha256";
        [SerializeField] private string zipFileName = "raidsim_v.{0}_{1}64.zip";
        [SerializeField] private string newestVersion = string.Empty;
        [SerializeField, Label("Update ID")] private int updateId = 1;
        [SerializeField, Label("Latest Update ID")] private int latestUpdateId = -1;
        [Header("User Interface")]
        [SerializeField] private double updateInterval = 0.5;
        [SerializeField] private CanvasGroup updatePromptGroup;
        [SerializeField] private GameObject askPrompt;
        [SerializeField] private GameObject downloadPrompt;
        [SerializeField] private TextMeshProUGUI downloadStatus;
        [SerializeField] private Slider downloadProgressBar;
        [SerializeField] private TextMeshProUGUI downloadProgress;
        [SerializeField] private TextMeshProUGUI downloadEstimatedDuration;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button cancelButton;

        private const string gameVersionUrl = "https://raw.githubusercontent.com/susy-bakaa/ffxiv-raid-sim/refs/heads/main/version.txt";
#if UNITY_STANDALONE_WIN
        private const string platform = "win";
        private const int checksumFileLine = 0;
#elif UNITY_STANDALONE_LINUX
        private const string platform = "linux";
        private const int checksumFileLine = 1;
#elif UNITY_WEBGL
        private const string platform = "webgl";
        private const int checksumFileLine = 2;
#endif
        private bool skipUpdates = false;
        private string zipFilePath;
        private Coroutine ieDownloadUpdate;
        private WebClient webClient;
        private Stopwatch downloadStopwatch;
        private long lastBytesReceived = 0;
        private float uiUpdateTimer = 0f;
        private float lastProgress = 0f;
        private string lastEstimatedTimeText = "";
        private Queue<double> speedHistory = new Queue<double>();
        private int speedHistoryCount = 5;
        private float noDataReceivedTimer = 0f;
        private float connectionLostTimeout = 5f;
        private bool restarting = false;
        private bool destroyed = false;
        private bool showDownloadSpeed = false;
        private bool downloadAborted = false;

        void Start()
        {
#if UNITY_WEBPLAYER
            destroyed = true;
            Destroy(updatePromptGroup.gameObject);
            Destroy(gameObject);
            return;
#elif UNITY_STANDALONE_LINUX
            Debug.LogWarning("Linux platform detected. Automatic updates not supported yet. Skipping update check.");
            destroyed = true;
            Destroy(updatePromptGroup.gameObject);
            Destroy(gameObject);
            return;
#endif
            newestVersion = Application.version;
            newestVersion = newestVersion.TrimEnd('\r', '\n', ' ');
            showDownloadSpeed = false;
            restarting = false;
            destroyed = false;

            downloadStatus.text = "Checking for updates...";
            downloadProgressBar.value = 0f;
            downloadProgress.text = "0%";
            downloadEstimatedDuration.text = "Estimated Time Left: Unknown";

            cancelButton.interactable = true;
            retryButton.interactable = false;
            restartButton.interactable = false;
            retryButton.gameObject.SetActive(false);

            HideUpdatePrompt();

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("No internet connection detected. Skipping automatic update check.");
            }
            else if (!skipUpdates)
                CheckForUpdates();
            else
                Debug.Log("Skipping update check.");
        }

        void Update()
        {
            if (destroyed)
                return;

            if (showDownloadSpeed)
            {
                uiUpdateTimer += Time.deltaTime;
                if (uiUpdateTimer >= updateInterval)
                {
                    uiUpdateTimer = 0f;

                    downloadProgressBar.value = lastProgress;
                    downloadEstimatedDuration.text = lastEstimatedTimeText;
                }
            }

            if (restarting)
                return;

            if (downloadStopwatch != null && downloadStopwatch.IsRunning)
            {
                noDataReceivedTimer += Time.deltaTime;
                if (noDataReceivedTimer >= connectionLostTimeout)
                {
                    showDownloadSpeed = false;
                    restarting = true;
                    Debug.LogWarning("No data received for 5 seconds. Retrying download in 10 seconds.");
                    downloadStatus.text = "Connection Lost";
                    downloadEstimatedDuration.text = "Retrying download...";
                    RestartDownload();
                }
            }
        }

        void OnDestroy()
        {
            destroyed = true;

            if (webClient != null)
                webClient.CancelAsync();
            if (downloadStopwatch != null)
                downloadStopwatch.Stop();
        }

        public void ShowUpdatePrompt()
        {
            if (destroyed)
                return;

            showDownloadSpeed = false;

            updatePromptGroup.alpha = 1f;
            updatePromptGroup.interactable = true;
            updatePromptGroup.blocksRaycasts = true;
        }

        public void HideUpdatePrompt()
        {
            if (destroyed)
                return;

            updatePromptGroup.alpha = 0f;
            updatePromptGroup.interactable = false;
            updatePromptGroup.blocksRaycasts = false;

            if (webClient != null)
                webClient.CancelAsync();
            if (downloadStopwatch != null)
                downloadStopwatch.Stop();

            downloadStatus.text = "Updates skipped";
            downloadProgressBar.value = 0f;
            downloadProgress.text = "0%";
            downloadEstimatedDuration.text = "Estimated Time Left: Unknown";
        }

        public void SetSkipUpdates(bool value)
        {
            skipUpdates = value;
        }

        public void CheckForUpdates()
        {
            if (destroyed)
                return;

            showDownloadSpeed = false;

            webClient = new WebClient();
            Stream stream = webClient.OpenRead(gameVersionUrl);
            StreamReader sRead = new StreamReader(stream);
            string results = sRead.ReadToEnd();

            string[] parts = results.Split(',');

            if (int.TryParse(parts[0], out latestUpdateId))
            {
                if (latestUpdateId > updateId)
                {
                    Debug.Log("Update available!");
                    ShowUpdatePrompt();
                }
                else
                {
                    Debug.Log("No updates available.");
                }
                if (parts.Length > 1)
                {
                    newestVersion = parts[1];
                    newestVersion = newestVersion.TrimEnd('\r', '\n', ' ');
                    Debug.Log("Version: " + parts[1]);
                }
                else
                {
                    Debug.LogWarning("Failed to parse version from remote source.");
                }
            }
            else
            {
                Debug.LogWarning("Failed to parse version index from remote source.");
            }

            cancelButton.interactable = true;

            updateUrl = string.Format(updateUrl, newestVersion, platform);
            checksumUrl = string.Format(checksumUrl, newestVersion);
        }

        public void DownloadUpdate()
        {
            if (destroyed)
                return;

            if (latestUpdateId > updateId)
            {
                if (ieDownloadUpdate == null && !downloadAborted)
                    ieDownloadUpdate = StartCoroutine(IE_DownloadUpdate());
            }
        }

        public IEnumerator IE_DownloadUpdate()
        {
            if (destroyed || downloadAborted)
            {
                if (webClient != null)
                {
                    webClient.CancelAsync();
                    webClient.Dispose();
                    webClient = null;
                }
                StopCoroutine(ieDownloadUpdate);
                ieDownloadUpdate = null;
            }
            else if (!downloadAborted)
            {
                showDownloadSpeed = false;
                restarting = false;

                if (latestUpdateId > updateId)
                {
                    askPrompt.SetActive(false);
                    downloadPrompt.SetActive(true);
                    webClient = new WebClient();
                    string gameDir = Path.GetDirectoryName(Application.dataPath);
                    string persistentDir = Application.persistentDataPath;
                    string fileName = string.Format(zipFileName, newestVersion, platform);
                    zipFilePath = Path.Combine(persistentDir, fileName);

                    cancelButton.interactable = true;
                    retryButton.interactable = false;
                    restartButton.interactable = false;
                    retryButton.gameObject.SetActive(false);
                    downloadStatus.text = "Downloading update...";
                    downloadProgressBar.value = 0f;
                    downloadProgress.text = "0%";
                    downloadEstimatedDuration.text = "Estimated Time Left: Unknown";

                    //long existingFileSize = 0;
                    //bool resumeDownload = false;

                    if (File.Exists(zipFilePath))
                    {
                        Debug.Log("Update ZIP already exists. Verifying integrity...");

                        downloadStatus.text = "Existing Update Found";
                        downloadProgressBar.value = 0f;
                        downloadProgress.text = "0%";
                        downloadEstimatedDuration.text = "Verifying integrity...";

                        yield return new WaitForSeconds(0.5f);

                        if (VerifyFileSHA256(zipFilePath) && IsValidZip(zipFilePath))
                        {
                            Debug.Log("ZIP file is valid. Prompting to launch updater...");
                            downloadStatus.text = "Existing Update Found";
                            downloadProgressBar.value = 1f;
                            downloadProgress.text = "100%";
                            downloadEstimatedDuration.text = "Update is valid. Restart is required.";
                            restartButton.gameObject.SetActive(true);
                            restartButton.interactable = true;
                            retryButton.interactable = false;
                            retryButton.gameObject.SetActive(false);
                            if (webClient != null)
                            {
                                webClient.CancelAsync();
                                webClient.Dispose();
                                webClient = null;
                            }
                            StopCoroutine(ieDownloadUpdate);
                            ieDownloadUpdate = null;
                            downloadAborted = true;
                            yield return null;
                        }
                        else
                        {
                            Debug.LogWarning("ZIP file is corrupt. Redownloading...");
                            downloadStatus.text = "Existing Update Found";
                            downloadEstimatedDuration.text = "Update is invalid. Redownloading...";
                            File.Delete(zipFilePath);
                            downloadAborted = false;
                            yield return new WaitForSeconds(5f);
                        }
                    }

                    if (!downloadAborted)
                    {
                        Debug.Log("Downloading update...");
                        downloadStatus.text = "Downloading update...";
                        downloadProgressBar.value = 0f;
                        downloadProgress.text = "0%";
                        downloadEstimatedDuration.text = "Estimated Time Left: Unknown";

                        downloadStopwatch = new Stopwatch();
                        lastBytesReceived = 0;
                        noDataReceivedTimer = 0;
                        speedHistory.Clear();

                        Uri uri = new Uri(updateUrl);
                        webClient.DownloadProgressChanged += (sender, data) =>
                        {
                            float progress = data.ProgressPercentage / 100f;
                            long bytesReceived = data.BytesReceived;
                            long totalBytes = data.TotalBytesToReceive;
                            double elapsedTime = downloadStopwatch.Elapsed.TotalSeconds;

                            if (!destroyed)
                            {
                                downloadStatus.text = "Downloading update...";
                                downloadProgressBar.value = progress;
                                downloadProgress.text = $"{data.ProgressPercentage}%";
                            }

                            if (elapsedTime > 0 && bytesReceived > 0 && totalBytes > 0)
                            {
                                double speedMbps = ((bytesReceived - lastBytesReceived) / (1024.0 * 1024.0)) / elapsedTime;
                                lastBytesReceived = bytesReceived;
                                downloadStopwatch.Restart();
                                noDataReceivedTimer = 0;

                                // Store the speed in a rolling history
                                if (speedHistory.Count >= speedHistoryCount)
                                {
                                    speedHistory.Dequeue();
                                }
                                speedHistory.Enqueue(speedMbps);

                                // Calculate rolling average speed
                                double averageSpeed = 0;
                                foreach (double speed in speedHistory)
                                {
                                    averageSpeed += speed;
                                }
                                averageSpeed /= speedHistory.Count;

                                double remainingTime = (totalBytes - bytesReceived) / (averageSpeed * 1024 * 1024);
                                string estimatedTimeText = FormatTime(remainingTime);

                                lastProgress = progress;
                                lastEstimatedTimeText = $"Estimated Time Left: {estimatedTimeText} ({averageSpeed:F2} MB/s)";
                                showDownloadSpeed = true;

                                Debug.Log($"Downloading update... {data.ProgressPercentage}% - {averageSpeed:F2} MB/s - Time Left: {estimatedTimeText}");
                            }
                        };

                        webClient.DownloadFileCompleted += (sender, args) =>
                        {
                            downloadStopwatch.Stop();
                            showDownloadSpeed = false;

                            if (args.Error == null)
                            {
                                Debug.Log("Download complete.");
                                lastProgress = 1f;

                                if (!destroyed)
                                {
                                    downloadProgressBar.value = 1f;
                                    downloadProgress.text = "100%";
                                }

                                if (VerifyFileSHA256(zipFilePath) && IsValidZip(zipFilePath))
                                {
                                    Debug.Log("ZIP file verified. Prompting to launch updater...");
                                    downloadStatus.text = "Download Complete";
                                    downloadEstimatedDuration.text = "Restart is required.";
                                    restartButton.gameObject.SetActive(true);
                                    restartButton.interactable = true;
                                    retryButton.interactable = false;
                                    retryButton.gameObject.SetActive(false);
                                }
                                else
                                {
                                    Debug.LogError("Downloaded ZIP is invalid. Deleting file.");
                                    downloadStatus.text = "Broken Download";
                                    downloadEstimatedDuration.text = "Would you like to retry?";
                                    File.Delete(zipFilePath);
                                    ShowRetryButton();
                                }
                            }
                            else if (args.Cancelled)
                            {
                                showDownloadSpeed = false;
                                Debug.LogWarning($"Download was cancelled.");
                            }
                            else
                            {
                                showDownloadSpeed = false;
                                Debug.LogError($"Download failed: {args.Error.Message}");
                                downloadStatus.text = "Download Failed";
                                downloadEstimatedDuration.text = "Would you like to retry?";
                                ShowRetryButton();
                            }
                        };

                        webClient.DownloadFileAsync(uri, zipFilePath);
                        downloadStopwatch.Start();
                    }
                }
                else
                {
                    showDownloadSpeed = false;
                    Debug.Log("No update needed.");
                    downloadStatus.text = "Up To Date";
                    downloadProgressBar.value = 1f;
                    downloadProgress.text = "100%";
                    downloadEstimatedDuration.text = "No download required";
                    restartButton.interactable = false;
                    retryButton.interactable = false;
                    restartButton.gameObject.SetActive(true);
                    retryButton.gameObject.SetActive(false);
                    askPrompt.SetActive(false);
                    downloadPrompt.SetActive(true);
                    cancelButton.interactable = false;
                    Utilities.FunctionTimer.Create(this, () =>
                    {
                        HideUpdatePrompt();
                    }, 4f, "HideUpdatePrompt", true, true);
                }
            }
        }

        private void RestartDownload()
        {
            if (destroyed)
                return;

            Debug.LogWarning("Restarting download...");
            downloadAborted = true;
            if (ieDownloadUpdate != null)
                StopCoroutine(ieDownloadUpdate);
            ieDownloadUpdate = null;
            showDownloadSpeed = false;
            downloadAborted = false;

            if (webClient != null)
            {
                webClient.CancelAsync();
                webClient.Dispose();
                webClient = null;
            }
            
            Invoke(nameof(CheckConnectionAndRetry), 5f);
        }

        private void CheckConnectionAndRetry()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("No internet connection detected. Waiting to retry...");
                Invoke(nameof(CheckConnectionAndRetry), 3f); // Keep checking every 3 seconds
            }
            else
            {
                Debug.Log("Internet connection restored. Restarting download.");
                if (ieDownloadUpdate == null) 
                    ieDownloadUpdate = StartCoroutine(IE_DownloadUpdate());
            }
        }

        private bool VerifyFileSHA256(string filePath)
        {
            try
            {
                Debug.Log("Downloading expected SHA256 hash...");

                string checksumFilePath = Path.Combine(Application.persistentDataPath, "checksums.sha256");

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(checksumUrl, checksumFilePath);
                }

                // Read the checksum file
                string checksums = File.ReadAllText(checksumFilePath);

                // Get all expected hashes
                Dictionary<string, string> checksumMap = ExtractChecksums(checksums);
                string expectedHash;

                if (!checksumMap.TryGetValue(Path.GetFileName(filePath), out expectedHash))
                {
                    Debug.LogError($"SHA256 hash not found for {Path.GetFileName(filePath)} in checksums.sha256!");
                    return false;
                }

                // Compute the SHA256 of the downloaded file
                string computedHash = ComputeSHA256(filePath);
                Debug.Log($"Expected: {expectedHash}");
                Debug.Log($"Computed: {computedHash}");

                return string.Equals(expectedHash, computedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.LogError($"SHA256 verification failed: {ex.Message}");
                return false;
            }
        }

        private string ComputeSHA256(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }

        private Dictionary<string, string> ExtractChecksums(string checksumFileContents)
        {
            Dictionary<string, string> checksumMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string line in checksumFileContents.Split('\n'))
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2) // Ensure format: SHA256_HASH  FILENAME
                {
                    string fileHash = parts[0].Trim();
                    string fileName = parts[1].Trim();
                    checksumMap[fileName] = fileHash;
                }
            }

            return checksumMap;
        }

        private bool IsValidZip(string zipPath)
        {
            try
            {
                using (System.IO.Compression.ZipArchive zip = System.IO.Compression.ZipFile.OpenRead(zipPath))
                {
                    return zip.Entries.Count > 0;
                }
            }
            catch (IOException)
            {
                Debug.LogWarning($"ZIP file '{zipPath}' is currently in use!");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"ZIP validation failed: {ex.Message}");
                return false;
            }
        }

        public void LaunchUpdater()
        {
            if (destroyed)
                return;

            restartButton.interactable = false;
            retryButton.interactable = false;

            downloadStatus.text = "Download Complete";
            downloadProgressBar.value = 1f;
            downloadProgress.text = "100%";
            downloadEstimatedDuration.text = "Launching updater...";

            string gameDir = Path.GetDirectoryName(Application.dataPath);
            string updaterPath = Path.Combine(gameDir, "updater.exe");

            if (File.Exists(updaterPath))
            {
                int pid = Process.GetCurrentProcess().Id;
                ProcessStartInfo psi = new ProcessStartInfo(updaterPath, $"\"{zipFilePath}\" {pid}");
                psi.UseShellExecute = true;
                Process.Start(psi);
                Application.Quit();
            }
            else
            {
                downloadEstimatedDuration.text = "Failed to find the updater!";
                Debug.LogError("Updater not found in the game directory!");
            }
        }

        private void ShowRetryButton()
        {
            if (destroyed)
                return;

            retryButton.gameObject.SetActive(true);
            retryButton.interactable = true;
            restartButton.interactable = false;
            restartButton.gameObject.SetActive(false);
        }

        public void RetryDownload()
        {
            if (destroyed)
                return;

            Debug.Log("Retrying download...");
            retryButton.interactable = false;
            retryButton.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(true);
            restartButton.interactable = false;
            if (ieDownloadUpdate == null)
                ieDownloadUpdate = StartCoroutine(IE_DownloadUpdate());
        }

        private string FormatTime(double seconds)
        {
            if (seconds < 60)
                return $"{seconds:F0}s";
            else if (seconds < 3600)
                return $"{(seconds / 60):F0}m {(seconds % 60):F0}s";
            else if (seconds < 86400)
                return $"{(seconds / 3600):F0}h {(seconds % 3600 / 60):F0}m";
            else if (seconds < 31536000)
                return $"{(seconds / 86400):F0}d {(seconds % 86400 / 3600):F0}h";
            else
                return $"{(seconds / 31536000):F0}y {(seconds % 31536000 / 86400):F0}d";
        }
    }
}