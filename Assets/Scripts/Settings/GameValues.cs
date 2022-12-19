using System;
using UnityEngine;

[Serializable]
public class GameValues
{
    public bool enableCheat;

    public int foundationStartingSize;
    public byte cardsToDeal;

    public float relativeCardScale;
    public float matchExplosionScale;

    public float draggedCardScale;
    public float draggedCardOffset;

    public int turnAlertSmallThreshold;
    public int turnAlertThreshold;

    public int matchPoints;
    public int scoreMultiplier;
    public bool enableBonusPoints;
    public int emptyReactorPoints;
    public int perfectGamePoints;

    public int wastepileAnimationSpeedSlow;
    public int wastepileAnimationSpeedFast;
    public int cardsToReactorspeed;

    public float musicFadeInDurationSec;
    public float musicFadeOutDurationSec;
    public float musicFadeOutSlowDurationSec;

    public int musicDefaultVolume;
    public int soundEffectsDefaultVolume;
    public bool vibrationEnabledDefault;
    public bool foodSuitsEnabledDefault;

    public float fadeOutButtonsSpeed;
    public int zoomFactor;
    public float panAndZoomSpeed;
    public float startGameFadeInSpeed;
    public float endGameFadeOutSpeed;
    public float summaryTransitionSpeed;

    public float reactorMeltDownSpeed;

    public Difficulty[] difficulties;

    public float selectedCardOpacity;

    public Color cardObstructedColor;

    public Color matchHighlightColor;
    public Color moveHighlightColor;
    public Color overHighlightColor;
    public Color[] highlightColors;

    public Color pointColor;

    public Color tutorialObjectHighlightColor;

    public Color fadeDarkColor;

    public Color fadeLightColor;

    public MenuText menuText;

    [Serializable]
    public class MenuText
    {
        public string[] menuButtons;
        public string[] levelButtons;
        public string backButton;
        public string loading;
        public string[] pauseButtons;
        public string[] summaryButtons;
        public string[] scoreActionLabels;
        public string[] gameState;
    }
}

[Serializable]
public struct Difficulty
{
    [SerializeField]
    private string name;
    public string Name { get => name; }
    [SerializeField]
    private int reactorLimit;
    public int ReactorLimit { get => reactorLimit; }
    [SerializeField]
    private int moveLimit;
    public int MoveLimit { get => moveLimit; }
}
