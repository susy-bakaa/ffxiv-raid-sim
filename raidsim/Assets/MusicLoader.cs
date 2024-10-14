using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class MusicLoader : MonoBehaviour
{
    [SerializeField] private string path = "/test/bgm.mp3";
    [SerializeField] private bool useRelativeToExecutable = true;
    [SerializeField] private bool randomize = false;
    [SerializeField] private int songCount = 0;
    private AudioSource source = null;

    private void Awake()
    {
        source = transform.GetComponentInChildren<AudioSource>();

        // Start loading the song asynchronously
        LoadSongAsync();
    }

    private void OnDestroy()
    {
        source.Stop();
        source.clip = null;
    }

    private async void LoadSongAsync()
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

        string fullPath = Path.Combine(Application.streamingAssetsPath, finalPath);

        // Check if the file exists before trying to load it
        if (!File.Exists(fullPath))
        {
            Debug.Log("Music file does not exist: " + fullPath);
            return; // Skip loading if the file doesn't exist
        }

        byte[] audioData = null;
        try
        {
            // Load the audio data asynchronously from the file
            audioData = await Task.Run(() => File.ReadAllBytes(fullPath));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Error loading music: " + ex.Message);
            return;
        }

        // Ensure we are back on the main thread to assign the audio clip
        if (audioData != null)
        {
            source.clip = NAudioPlayer.FromMp3Data(audioData);
            source.Play();
        }
    }
}