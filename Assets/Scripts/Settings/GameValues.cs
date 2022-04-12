﻿using UnityEngine;
[System.Serializable]
public class GameValues
{
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

    public float musicFadeIn;
    public float musicFadeOutFast;
    public float musicFadeOutSlow;

    public float musicDefaultVolume;
    public float soundEffectsDefaultVolume;
    public bool vibrationEnabledDefault;
    public bool foodSuitsEnabledDefault;

    public float fadeOutButtonsSpeed;
    public int zoomFactor;
    public float panAndZoomSpeed;
    public float startGameFadeInSpeed;
    public float endGameFadeOutSpeed;
    public float summaryTransitionSpeed;

    public string[] difficulties;
    public int[] reactorLimits;
    public int[] moveLimits;

    public float selectedCardOpacity;

    public float[] cardObstructedColorValues;
    public Color cardObstructedColor;

    public float[] cardMoveHighlightColorValues;
    public Color cardMoveHighlightColor;

    public float[] cardMatchHighlightColorValues;
    public Color cardMatchHighlightColor;

    public float[] pointColorValues;
    public Color pointColor;

    public string[] menuButtonsTxtEnglish;
    public string[] levelButtonsTxtEnglish;
    public string backButtonTxtEnglish;
    public string loadingTxtEnglish;
    public string[] pauseButtonsTxtEnglish;
    public string[] summaryButtonsTxtEnglish;
    public string[] scoreActionLabelsTxtEnglish;
    public string[] gameStateTxtEnglish;

}
