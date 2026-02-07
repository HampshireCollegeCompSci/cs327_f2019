using System.Collections;
using UnityEngine.Audio;
using UnityEngine;

// https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/
public static class FadeMixerGroup
{
    public enum FadeType
    {
        play,
        stop,
        persist
    }

    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVol,
        AudioSource audioSource, FadeType type)
    {
        if (type == FadeType.play)
        {
            audioSource.Play();
        }
        else if (type == FadeType.persist)
        {
            audioSource.UnPause();
        }

        yield return null;

        float currentTime = 0;
        audioMixer.GetFloat(exposedParam, out float startVol);
        startVol = Mathf.Pow(10, startVol / 20);
        float targetValue = Mathf.Clamp(targetVol, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            float newVol = Mathf.Lerp(startVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }

        if (targetVol < 0.0002f)
            audioMixer.SetFloat(exposedParam, -80);
        else
            audioMixer.SetFloat(exposedParam, Mathf.Log10(targetVol) * 20);

        if (type == FadeType.stop)
        {
            yield return null;
            audioSource.Pause();
        }
    }

    public static IEnumerator VolumeChange(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        yield return null;
        float currentTime = 0;
        audioMixer.GetFloat(exposedParam, out float startVol);
        startVol = Mathf.Pow(10, startVol / 20f);
        float targetVol = Mathf.Clamp(targetVolume, 0.0001f, 1);

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            float newVol = Mathf.Lerp(startVol, targetVol, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }

        if (targetVol < 0.0002f)
            audioMixer.SetFloat(exposedParam, -80);
        else
            audioMixer.SetFloat(exposedParam, Mathf.Log10(targetVol) * 20);
    }
}
