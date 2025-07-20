using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace dev.susybaka.Shared.Audio
{
    public class AudioManager : MonoBehaviour
    {

        public static AudioManager Instance;

        [Header("Settings")]

        public AudioMixerGroup mixerGroup;
        public bool log = false;

        [Header("Sounds")]

        public Sound[] sounds;

        private AudioSource templateSource;
        private Dictionary<Sound, int> activeSoundInstances = new Dictionary<Sound, int>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            templateSource = transform.GetChild(0).GetComponent<AudioSource>();
            activeSoundInstances = new Dictionary<Sound, int>();

            foreach (Sound s in sounds)
            {
                GameObject source = null;
                GameObject[] sources = null; 

                if (s.positions != null)
                {
                    if (s.positions.Length <= 1)
                    {
                        source = new GameObject($"Source_{s.name}");
                        s.hasMultipleSources = false;
                    }
                    else
                    {
                        sources = new GameObject[s.positions.Length];

                        for (int i = 0; i < s.positions.Length; i++)
                        {
                            sources[i] = new GameObject($"Source_{s.name}_{i}");
                        }
                        s.hasMultipleSources = true;
                    }
                }

                if (s.clip == null && !s.multipleClips && s.clips.Length > 0)
                {
                    s.multipleClips = true;
                }
                else if (s.clip != null && s.multipleClips)
                {
                    s.multipleClips = false;
                }
                else if (s.clip == null && s.clips.Length <= 0)
                {
                    Debug.LogError("No clips found for sound " + s.name);
                }

                if (!s.hasMultipleSources && source != null)
                {
                    s.source = source.AddComponent<AudioSource>();
                    SetupSoundSource(s, s.source);
                }
                else if (s.hasMultipleSources && sources != null && sources.Length > 0)
                {
                    s.sources = new List<AudioSource>();
                    for (int i = 0; i < sources.Length; i++)
                    {
                        AudioSource f = sources[i].AddComponent<AudioSource>();
                        SetupSoundSource(s, f, i);
                        s.sources.Add(f);
                    }
                }
                else
                {
                    Debug.LogError("No source found for sound " + s.name);
                }
            }
        }

        private void SetupSoundSource(Sound s, AudioSource source, int i = 0)
        {
            source.transform.SetParent(transform);

            if (s.positions != null && s.positions.Length > 0 && s.positions.Length < i)
                source.transform.position = s.positions[i];
            else
                source.transform.position = Vector3.zero;

            source.outputAudioMixerGroup = mixerGroup;

            if (!s.multipleClips)
                source.clip = s.clip;

            source.mute = s.mute;
            source.bypassEffects = s.bypassEffects;
            source.bypassListenerEffects = s.bypassListenerEffects;
            source.bypassReverbZones = s.bypassReverbZones;
            if (s.playOnAwake)
            {
                if (!s.multipleClips)
                {
                    if (s.specificScenesOnly == null || s.specificScenesOnly.Length < 1)
                    {
                        source.playOnAwake = s.playOnAwake;
                        Play(s.name);
                    }
                    else if (s.specificScenesOnly != null && s.specificScenesOnly.Length > 0)
                    {
                        if (s.specificScenesOnly.Contains(SceneManager.GetActiveScene().path))
                        {
                            source.playOnAwake = s.playOnAwake;
                            Play(s.name);
                        }
                    }
                }
                else
                {
                    if (s.specificScenesOnly == null || s.specificScenesOnly.Length < 1)
                    {
                        source.playOnAwake = s.playOnAwake;
                        Play(s.name);
                    }
                    else if (s.specificScenesOnly != null && s.specificScenesOnly.Length > 0)
                    {
                        if (s.specificScenesOnly.Contains(SceneManager.GetActiveScene().path))
                        {
                            source.playOnAwake = s.playOnAwake;
                            Play(s.name);
                        }
                    }
                }
            }
            else
            {
                source.playOnAwake = s.playOnAwake;
            }
            source.loop = s.loop;

            source.priority = s.priority;
            source.panStereo = s.stereoPan;
            source.spatialBlend = s.spatialBlend;
            source.reverbZoneMix = s.reverbZoneMix;

            if (s.spatialBlend <= 0f)
            {
                source.dopplerLevel = 0f;
                source.spread = 0f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 1000f;
                source.maxDistance = 10000f;
            }
            else
            {
                source.outputAudioMixerGroup = templateSource.outputAudioMixerGroup;
                source.dopplerLevel = templateSource.dopplerLevel;
                source.spread = templateSource.spread;
                source.rolloffMode = templateSource.rolloffMode;
                source.minDistance = templateSource.minDistance;
                source.maxDistance = templateSource.maxDistance;
                if (templateSource.rolloffMode == AudioRolloffMode.Custom)
                    source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, templateSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
            }
        }

        public void PlayAt(string sound, Vector3 position, float volume = 1f, int index = -1)
        {
            PlayAt(sound, position, transform, volume, index);
        }

        public void PlayAt(string sound, Transform location, float volume = 1f, int index = -1)
        {
            PlayAt(sound, location.position, transform, volume, index);
        }

        public void PlayAt(string sound, Vector3 position, Transform parent, float volume = 1f, int index = -1)
        {
            if (log)
                Debug.Log("[AudioManager] Playing sound: " + sound + " of index: " + index + " at position " + position);

            if (sound.StartsWith("#") || sound == "<None>")
                return;

            Sound s = Array.Find(sounds, item => item.name == sound);
            if (s == null)
            {
                Debug.LogError("Sound: " + sound + " not found!");
                return;
            }

            if (activeSoundInstances.TryGetValue(s, out int count) && count >= s.maxInstances)
                return; // drop sound if at capacity

            GameObject tempSource = new GameObject($"Source_{s.name}_Temp");
            AudioSource source = tempSource.AddComponent<AudioSource>();

            SetupSoundSource(s, source, -1);
            source.loop = false; // Ensure temp sources do not loop
            source.playOnAwake = false; // Ensure temp sources do not play on awake by themselves

            tempSource.transform.position = position;
            tempSource.transform.SetParent(parent);

            if (s.multipleClips && index < 0)
            {
                source.clip = s.clips[UnityEngine.Random.Range(0, s.clips.Length)];
            }
            else if (index >= 0)
            {
                source.clip = s.clips[index];
            }
            source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f)) * volume;
            source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

            if (!activeSoundInstances.ContainsKey(s))
                activeSoundInstances[s] = 0;

            activeSoundInstances[s]++;

            float scale = 1f / Mathf.Sqrt(activeSoundInstances[s]);
            source.volume *= scale;

            source.Play();
            Destroy(tempSource, source.clip.length + 0.5f);
            Utilities.FunctionTimer.Create(this, () => activeSoundInstances[s] = Mathf.Max(0, activeSoundInstances[s] - 1), source.clip.length, $"AudioManager_Decrement_SoundInstance_{activeSoundInstances[s]}_Clip_{s.name}_Length_{source.clip.length}", false, false);
        }

        public void Play(string sound)
        {
            Play(sound, 1f, -1);
        }

        public void Play(string sound, int index)
        {
            Play(sound, 1f, index);
        }

        public void Play(string sound, float volume)
        {
            Play(sound, volume, -1);
        }

        public void Play(string sound, float volume = 1f, int index = -1)
        {
            if (log)
                Debug.Log("[AudioManager] Playing sound: " + sound + " at index: " + index);

            if (sound.StartsWith("#") || sound == "<None>")
                return;

            Sound s = Array.Find(sounds, item => item.name == sound);
            if (s == null)
            {
                Debug.LogError("Sound: " + sound + " not found!");
                return;
            }

            if (!s.hasMultipleSources && s.source != null)
            {
                if (s.multipleClips && index < 0)
                {
                    s.source.clip = s.clips[UnityEngine.Random.Range(0, s.clips.Length)];
                }
                else if (index >= 0)
                {
                    s.source.clip = s.clips[index];
                }
                s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f)) * volume;
                s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

                s.source.Play();
            }
            else if (s.hasMultipleSources && s.sources != null && s.sources.Count > 0)
            {
                for (int i = 0; i < s.sources.Count; i++)
                {
                    if (s.multipleClips && index < 0)
                    {
                        s.sources[i].clip = s.clips[UnityEngine.Random.Range(0, s.clips.Length)];
                    }
                    else if (index >= 0)
                    {
                        s.sources[i].clip = s.clips[index];
                    }
                    s.sources[i].volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f)) * volume;
                    s.sources[i].pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

                    s.sources[i].Play();
                }
            }
        }

        public void StopPlaying(string sound)
        {
            if (log)
                Debug.Log("[AudioManager] Stopping playback of sound: " + sound);

            if (sound.StartsWith("#") || sound == "<None>")
                return;

            Sound s = Array.Find(sounds, item => item.name == sound);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + sound + " not found!");
                return;
            }

            if (!s.hasMultipleSources && s.source != null)
            {
                s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
                s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

                s.source.Stop();
            }
            else if (s.hasMultipleSources && s.sources != null && s.sources.Count > 0)
            {
                for (int i = 0; i < s.sources.Count; i++)
                {
                    s.sources[i].volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
                    s.sources[i].pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

                    s.sources[i].Stop();
                }
            }
        }

        public void StopPlayingAll()
        {
            if (log)
                Debug.Log("[AudioManager] Stopping playback of all sounds");

            foreach (Sound s in sounds)
            {
                if (!s.hasMultipleSources && s.source != null)
                {
                    s.source.Stop();
                }
                else if (s.hasMultipleSources && s.sources != null && s.sources.Count > 0)
                {
                    for (int i = 0; i < s.sources.Count; i++)
                    {
                        s.sources[i].Stop();
                    }
                }
            }
        }

        [System.Serializable]
        public class QueuedSound
        {
            public Queue<AudioSource> sources = new Queue<AudioSource>();
            public bool isPlaying = false;
        }
    }
}