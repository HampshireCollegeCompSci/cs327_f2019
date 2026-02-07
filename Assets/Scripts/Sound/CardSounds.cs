using System;
using System.Collections.Generic;
using UnityEngine;

public class CardSounds: MonoBehaviour
{
    // Singleton instance.
    public static CardSounds Instance { get; private set; }

    [Header("Progression Curves")]
    [Tooltip("X = Card Count (1-13), Y = Pitch Value")]
    [SerializeField] private AnimationCurve pitchCurve;

    [Tooltip("X = Card Count (1-13), Y = Volume (0-1)")]
    [SerializeField] private AnimationCurve volumeCurve;

    [SerializeField]
    [Range(0f, 1f)]
    private float panIntensity = 0.5f;

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource foodMatchSource, tokenInReactorSource, explosionSource;

    [SerializeField]
    private AudioSource[] tokenSelectSource, tokenStackSource;
    private List<AudioSource> avalibleSourcesTemp;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip[] foodMatchSounds;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        avalibleSourcesTemp = new List<AudioSource>(tokenStackSource.Length);
    }

    public void CardPressSound(Vector2 worldPosition, int cardCount)
    {
        VibrationController.Instance.VibrateSmall();
        if (SoundEffectsController.Instance.Muted) return;
        AudioSource randomSelectSource = GetRandomAvailableSource(tokenSelectSource);
        PlaySound(randomSelectSource, worldPosition, cardCount);
    }

    public void CardStackSound(Vector2 worldPosition, int cardCount)
    {
        VibrationController.Instance.VibrateSmall();
        if (SoundEffectsController.Instance.Muted) return;
        AudioSource randomStackSource = GetRandomAvailableSource(tokenStackSource);
        PlaySound(randomStackSource, worldPosition, cardCount);
    }

    public void CardToReactorSound(Vector2 worldPosition)
    {
        VibrationController.Instance.VibrateSmall();
        if (SoundEffectsController.Instance.Muted) return;
        tokenInReactorSource.panStereo = GetPan(worldPosition);
        tokenInReactorSource.Play();
    }

    public void FoodMatch(Suit suit, Vector2 worldPosition)
    {
        VibrationController.Instance.VibrateMedium();
        if (SoundEffectsController.Instance.Muted) return;
        if (suit.Index < 0 || suit.Index > foodMatchSounds.Length)
        {
            throw new IndexOutOfRangeException($"the suit {suit}'s index is not between 0-{foodMatchSounds.Length}");
        }
        foodMatchSource.panStereo = GetPan(worldPosition);
        foodMatchSource.PlayOneShot(foodMatchSounds[suit.Index]);
    }

    public void ExplosionSound(Vector2 worldPosition)
    {
        VibrationController.Instance.VibrateMedium();
        if (SoundEffectsController.Instance.Muted) return;
        explosionSource.panStereo = GetPan(worldPosition);
        explosionSource.Play();
    }

    private void PlaySound(AudioSource selectedSource, Vector2 worldPosition, int cardCount)
    {
        selectedSource.panStereo = GetPan(worldPosition);

        cardCount = Mathf.Clamp(cardCount, 1, 13);
        selectedSource.pitch = pitchCurve.Evaluate(cardCount);
        selectedSource.volume = volumeCurve.Evaluate(cardCount);

        selectedSource.Play();
    }

    private AudioSource GetRandomAvailableSource(AudioSource[] sourcePool)
    {
        avalibleSourcesTemp.Clear();
        foreach (var s in sourcePool)
            if (!s.isPlaying) avalibleSourcesTemp.Add(s);
        if (avalibleSourcesTemp.Count == 0) return sourcePool[0];
        return avalibleSourcesTemp[UnityEngine.Random.Range(0, avalibleSourcesTemp.Count)];
    }

    private float GetPan(Vector2 worldPosition)
    {
        Vector2 viewportPos = Camera.main.WorldToViewportPoint(worldPosition); // 0 to 1
        float pan = (viewportPos.x * 2f - 1f) * panIntensity; // -1 to 1
        return pan;
    }
}
