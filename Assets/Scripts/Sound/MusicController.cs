using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource soundTrack1;
    public AudioSource soundTrack2;
    private bool playing1 = false;

    public void LoadGap()
    {
        soundTrack1.Stop();
        soundTrack2.Stop();
    }

    public void LoseMusic()
    {
        Transition(Resources.Load<AudioClip>("Audio/music_lose"));
    }

    public void WinMusic()
    {
        Transition(Resources.Load<AudioClip>("Audio/music_win"));
    }

    public void MainMenuMusic()
    {
        Transition(Resources.Load<AudioClip>("Audio/music_menu"));
    }

    public void AlertMusic()
    {
        Transition(Resources.Load<AudioClip>("Audio/music_transition"));
    }

    public void GameMusic(bool startNew = false)
    {
        if (startNew)
        {
            AudioClip gm = Resources.Load<AudioClip>("Audio/music_main_theme");
            if (playing1 && soundTrack1.clip == gm)
            {
                soundTrack1.volume = 1;
            }
            else if (!playing1 && soundTrack2.clip == gm)
            {
                soundTrack2.volume = 1;
            }
            else
            {
                Transition(Resources.Load<AudioClip>("Audio/music_main_theme"));
                return;
                soundTrack1.Stop();
                soundTrack2.Stop();
                soundTrack1.clip = gm;
                soundTrack1.volume = 1;
                soundTrack1.Play();
                playing1 = true;
            }
        }
        else
        {
            Transition(Resources.Load<AudioClip>("Audio/music_main_theme"));
        }
    }

    public void Transition(AudioClip newTrack)
    {
        if (playing1)
        {
            if (soundTrack1.clip == newTrack)
            {
                if (soundTrack1.volume != 1)
                {
                    StartCoroutine(FadeIn(soundTrack1, 1f, true));
                }
                return;
            }

            soundTrack2.clip = newTrack;
            StartCoroutine(FadeOut(soundTrack1, 1f));
            StartCoroutine(FadeIn(soundTrack2, 1f));
        }
        else
        {
            if (soundTrack2.clip == newTrack)
            {
                if (soundTrack2.volume != 1)
                {
                    StartCoroutine(FadeIn(soundTrack2, 1f, true));
                }
                return;
            }

            soundTrack1.clip = newTrack;
            StartCoroutine(FadeOut(soundTrack2, 1f));
            StartCoroutine(FadeIn(soundTrack1, 1f));
        }
        playing1 = !playing1;
    }

    // https://medium.com/@wyattferguson/how-to-fade-out-in-audio-in-unity-8fce422ab1a8
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }
        audioSource.Stop();
    }
    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime, bool keepOnGoing = false)
    {
        audioSource.Play();
        if (!keepOnGoing)
        {
            audioSource.volume = 0f;
        }
        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime / FadeTime;
            yield return null;
        }
    }

}
