using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongNAudio : MonoBehaviour
{
    public string path = "c:/song.mp3";
    private AudioSource aud = null;

    private void Start()
    {
        aud = this.GetComponent<AudioSource>();
        StartCoroutine(LoadSongCoroutine());
    }
    private IEnumerator LoadSongCoroutine()
    {
        string url = string.Format("file://{0}", path);
#pragma warning disable CS0618 // Type or member is obsolete
        WWW www = new WWW(url);
#pragma warning restore CS0618 // Type or member is obsolete
        yield return www;

        aud.clip = NAudioPlayer.FromMp3Data(www.bytes);
        aud.Play();
    }
}
