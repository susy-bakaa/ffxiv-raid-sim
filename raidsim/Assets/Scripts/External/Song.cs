using System.Collections;
using UnityEngine;

// load audio from file: https://www.mirimad.com/unity-load-audio-clip-from-file-or-url/

[RequireComponent(typeof(AudioSource))]
public class Song : MonoBehaviour
{
    private AudioSource aud = null;

    private void Start()
    {
        aud = this.GetComponent<AudioSource>();
        StartCoroutine(LoadAudio());
    }
    private IEnumerator LoadAudio()
    {
        string fullpath = "file:///" + "C:/song.wav";
#pragma warning disable CS0618 // Type or member is obsolete
        WWW url = new WWW(fullpath);
#pragma warning restore CS0618 // Type or member is obsolete
        yield return url;

        aud.clip = url.GetAudioClip(false, true);
        aud.Play();
    }
}
