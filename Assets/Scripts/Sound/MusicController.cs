using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource soundTrack1;
    public AudioSource soundTrack2;
    private bool playing1 = false;
    private byte playingTrack = 0;

    public void LoadGap()
    {
        soundTrack1.Stop();
        soundTrack1.clip = null;
        soundTrack2.Stop();
        soundTrack2.clip = null;
        Resources.UnloadUnusedAssets();
    }
    public void MainMenuMusic()
    {
        if (playingTrack == 1)
        {
            return;
        }

        playingTrack = 1;
        Transition(Resources.Load<AudioClip>("Audio/music_menu"));
    }

    public void GameMusic(bool startNew = false)
    {
        if (playingTrack == 2)
        {
            return;
        }

        playingTrack = 2;
        Transition(Resources.Load<AudioClip>("Audio/music_main_theme"));
    }

    public void AlertMusic()
    {
        if (playingTrack == 3)
        {
            return;
        }

        playingTrack = 3;
        Transition(Resources.Load<AudioClip>("Audio/music_transition"));
    }

    public void LoseMusic()
    {
        if (playingTrack == 4)
        {
            return;
        }

        playingTrack = 4;
        Transition(Resources.Load<AudioClip>("Audio/music_lose"));
    }

    public void WinMusic()
    {
        if (playingTrack == 5)
        {
            return;
        }

        playingTrack = 5;
        Transition(Resources.Load<AudioClip>("Audio/music_win"));
    }

    public void Transition(AudioClip newTrack)
    {
        StopAllCoroutines();
        if (playing1)
        {
            soundTrack2.clip = newTrack;
            StartCoroutine(FadeOut(soundTrack1, 1f));
            StartCoroutine(FadeIn(soundTrack2, 1f));
        }
        else
        {
            soundTrack1.clip = newTrack;
            StartCoroutine(FadeOut(soundTrack2, 1f));
            StartCoroutine(FadeIn(soundTrack1, 1f));
        }
        playing1 = !playing1;
    }

    // https://medium.com/@wyattferguson/how-to-fade-out-in-audio-in-unity-8fce422ab1a8
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        if (!audioSource.isPlaying)
        {
            yield break;
        }

        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }
        audioSource.Stop();
        audioSource.clip = null;
        Resources.UnloadUnusedAssets();
    }
    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        audioSource.volume = 0;
        audioSource.Play();
        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime / FadeTime;
            yield return null;
        }
    }

}
