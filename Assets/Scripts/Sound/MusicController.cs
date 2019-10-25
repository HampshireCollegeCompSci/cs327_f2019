using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource soundController;

    public void LoseMusic()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/music_lose");
        soundController.clip = sound;
        soundController.Play();
    }

    public void WinMusic()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/music_win");
        soundController.clip = sound;
        soundController.Play();
    }

    public void MainMenuMusic()
    {
        AudioClip sound = Resources.Load<AudioClip>("Audio/music_menu");
        soundController.clip = sound;
        soundController.Play();
    }

    public void GameMusic()
    {
        soundController.clip = null;
        soundController.Play();
    }
}
