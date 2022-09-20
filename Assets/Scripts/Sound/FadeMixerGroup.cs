using System.Collections;
using UnityEngine.Audio;
using UnityEngine;

// https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/
public static class FadeMixerGroup
{
    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume,
        AudioSource audioSource, bool stopWhenDone)
    {
        if (!stopWhenDone)
        {
            audioSource.Play();
        }

        float currentTime = 0;
        audioMixer.GetFloat(exposedParam, out float currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }

        if (stopWhenDone)
        {
            audioSource.Stop();
        }
    }
}
