using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource soundTrack1;
    public AudioSource soundTrack2;

    public AudioClip menuMusic, themeMusic, transitionMusic, loseMusic, winMusic;
    private bool playing1 = false;
    private byte playingTrack = 0;


    public void LoadGap()
    {
        soundTrack1.Stop();
        soundTrack1.clip = null;
        soundTrack2.Stop();
        soundTrack2.clip = null;
    }
    public void MainMenuMusic()
    {
        if (playingTrack == 1)
        {
            return;
        }

        playingTrack = 1;
        Transition(menuMusic);
    }

    public void GameMusic()
    {
        if (playingTrack == 2)
        {
            return;
        }

        playingTrack = 2;
        Transition(themeMusic);
    }

    public void AlertMusic()
    {
        if (playingTrack == 3 || playingTrack != 2) // will play over win/lose music without
        {
            return;
        }

        playingTrack = 3;
        Transition(transitionMusic);
    }

    public void LoseMusic()
    {
        if (playingTrack == 4)
        {
            return;
        }

        playingTrack = 4;
        Transition(loseMusic);
    }

    public void WinMusic()
    {
        if (playingTrack == 5)
        {
            return;
        }

        playingTrack = 5;
        Transition(winMusic);
    }

    private void Transition(AudioClip newTrack)
    {
        StopAllCoroutines();
        if (playing1)
        {
            soundTrack2.clip = newTrack;
            StartCoroutine(FadeOut(soundTrack1));
            StartCoroutine(FadeIn(soundTrack2));
        }
        else
        {
            soundTrack1.clip = newTrack;
            StartCoroutine(FadeOut(soundTrack2));
            StartCoroutine(FadeIn(soundTrack1));
        }
        playing1 = !playing1;
    }

    // https://medium.com/@wyattferguson/how-to-fade-out-in-audio-in-unity-8fce422ab1a8
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime = 2f)
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
        //audioSource.clip.UnloadAudioData();
    }
    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime = 2f)
    {
        audioSource.volume = 0;
        audioSource.Play();
        while (audioSource.volume < 0.5)
        {
            audioSource.volume += Time.deltaTime / FadeTime;
            yield return null;
        }
    }

}
