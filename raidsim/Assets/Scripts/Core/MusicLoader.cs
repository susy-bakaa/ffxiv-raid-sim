// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using dev.susybaka.raidsim.Core;
using Random = UnityEngine.Random;

namespace dev.susybaka.raidsim.Audio
{
    public class MusicLoader : MonoBehaviour
    {
        private string completePath = string.Empty;
        [SerializeField] private string path = "/test/bgm.ogg";
        [SerializeField] private bool useRelativeToExecutable = true;
        [SerializeField] private bool randomize = false;
        [SerializeField] private int songCount = 0;
        [SerializeField] private int loopStart = -1;
        [SerializeField] private int loopEnd = -1;
        private AudioSource startSource = null;
        private AudioSource loopSource = null;

        public void Load()
        {
#if UNITY_WEBPLAYER
            Destroy(gameObject);
            return;
#else

            startSource = transform.GetChild(0).GetComponent<AudioSource>();
            loopSource = transform.GetChild(1).GetComponent<AudioSource>();

            if (GlobalVariables.muteBgm)
            {
                startSource.volume = 0f;
                loopSource.volume = 0f;
            }
            else
            {
                startSource.volume = 1f;
                loopSource.volume = 1f;
            }

            if (startSource != null && loopSource != null)
            {
                // Load the metadata for dynamic looping etc.
                LoadMetadata();
                // Start loading the song asynchronously
                LoadSongAsync();
            }
#endif
        }

        private void OnDestroy()
        {
#if UNITY_WEBPLAYER
        return;
#else
            if (startSource == null || loopSource == null)
                return;

            startSource.Stop();
            AudioClip startClip = startSource.clip;
            startSource.clip = null;
            loopSource.Stop();
            AudioClip loopClip = loopSource.clip;
            loopSource.clip = null;

            if (startClip != null)
                Destroy(startClip);
            if (loopClip != null)
                Destroy(loopClip);

            // Unload unused assets to free up memory
            Resources.UnloadUnusedAssets();
#endif
        }

        private async void LoadSongAsync()
        {
            if (string.IsNullOrEmpty(completePath))
            {
                completePath = CompilePath();
            }

            // Check if the file exists before trying to load it
            if (!File.Exists(completePath))
            {
                Debug.Log("Music file does not exist: " + completePath);
                return; // Skip loading if the file doesn't exist
            }

            if (completePath.EndsWith(".mp3") || completePath.EndsWith(".wav"))
            {
                Debug.Log("Invalid music file type (Only ogg vorbis is supported): " + completePath);
                return; // Skip loading if the path points to an unsupported file
            }

            AudioClip originalClip;
            try
            {
                originalClip = await LoadOggClipFromDiskAsync(completePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error loading music: " + ex.Message);
                return;
            }

            if (originalClip == null)
            {
                Debug.LogWarning("Error loading music: AudioClip was null");
                return;
            }

            Debug.Log("Loading music from: " + completePath);

            if (loopStart > -1)
            {
                // Extract and create clips
                AudioClip startClip = CreateClipSegment(originalClip, 0, loopStart);
                AudioClip loopClip = CreateClipSegment(originalClip, loopStart, loopEnd);

                // Assign clips to audio sources
                startSource.clip = startClip;
                startSource.clip.name = "bgm_intro";
                loopSource.clip = loopClip;
                loopSource.clip.name = "bgm_loop";

                // Play the start segment
                startSource.Play();

                // Schedule the loop segment to start playing when the start segment ends
                float startClipDuration = (float)loopStart / originalClip.frequency;
                loopSource.PlayScheduled(AudioSettings.dspTime + startClipDuration);
            }
            else
            {
                loopSource.clip = originalClip;
                loopSource.clip.name = "bgm";
                loopSource.Play();
            }
        }

        private void LoadMetadata()
        {
            if (string.IsNullOrEmpty(completePath))
            {
                completePath = CompilePath();
            }

            // Assume `completePath` is the path to another file available in the class
            string directoryPath = System.IO.Path.GetDirectoryName(completePath); // Get the directory of the existing file
            string loopFilePath = System.IO.Path.Combine(directoryPath, "meta"); // Add "loop" to the directory path

            // Check if the file exists
            if (!System.IO.File.Exists(loopFilePath))
            {
                Debug.Log($"Audio metadata file not found: {loopFilePath}");
                loopStart = -1;
                loopEnd = -1;
                return;
            }

            try
            {
                // Read all lines from the file
                string[] lines = System.IO.File.ReadAllLines(loopFilePath);

                // Validate the file's content
                if (lines.Length != 2)
                {
                    Debug.LogWarning($"Invalid audio metadata file format: {loopFilePath}. Expected exactly 2 lines.");
                    return;
                }

                // Parse the lines as integers
                if (int.TryParse(lines[0], out loopStart) && int.TryParse(lines[1], out loopEnd))
                {
                    Debug.Log($"Successfully loaded audio metadata: loopStart = {loopStart}, loopEnd = {loopEnd}");
                }
                else
                {
                    loopStart = -1;
                    loopEnd = -1;
                    Debug.LogError($"Invalid number format in audio metadata file: {loopFilePath}");
                }
            }
            catch (Exception ex)
            {
                loopStart = -1;
                loopEnd = -1;
                Debug.LogError($"Error reading audio metadata file: {loopFilePath}. Exception: {ex.Message}");
            }
        }

        private string CompilePath()
        {
            string finalPath = string.Empty;

            if (useRelativeToExecutable)
            {
                finalPath = string.Format("{0}{1}", GlobalVariables.bgmPath, path);
            }
            else
            {
                finalPath = path;
            }

            if (randomize && songCount > 0)
            {
                finalPath = finalPath.Replace("#", Random.Range(0, songCount + 1).ToString());
            }

            return Path.Combine(Application.streamingAssetsPath, finalPath);
        }

        private static async Task<AudioClip> LoadOggClipFromDiskAsync(string fullPath)
        {
            // Make sure format is a proper file:// URI (handles Linux/macOS paths and escaping spaces)
            var uri = new Uri(fullPath).AbsoluteUri;

            // Build request manually so we can set handler flags BEFORE sending
            using var req = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbGET);
            var dh = new DownloadHandlerAudioClip(uri, AudioType.OGGVORBIS);
            dh.streamAudio = false;
            dh.compressed  = false;
            req.downloadHandler = dh;

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
                throw new Exception(req.error);

            var clip = dh.audioClip;
            if (clip == null)
                throw new Exception("DownloadHandlerAudioClip produced a null clip.");

            // Force audio data load now (so slicing has real PCM to read)
            clip.LoadAudioData();
            while (clip.loadState == AudioDataLoadState.Loading)
                await Task.Yield();

            if (clip.loadState == AudioDataLoadState.Failed)
                throw new Exception("AudioClip decode/load failed (loadState == Failed).");

            return clip;
        }

        private AudioClip CreateClipSegment(AudioClip original, int startSample, int endSample)
        {
            int sampleCount = endSample - startSample;
            float[] samples = new float[sampleCount * original.channels];

            // Extract samples from the original clip
            original.GetData(samples, startSample);

            // Create a new clip and set its data
            AudioClip newClip = AudioClip.Create("segment", sampleCount, original.channels, original.frequency, false);
            newClip.SetData(samples, 0);

            return newClip;
        }
    }
}