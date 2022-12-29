﻿using System;
using System.Collections.ObjectModel;
using UnityEngine;

public static class GameValues
{
    public static class GamePlay
    {
        public const bool enableCheat = false;
        public const int foundationStartingSize = 7;
        public const int cardsToDeal = 3;
        public const int turnAlertThreshold = 5;

        public static readonly ReadOnlyCollection<Difficulty> difficulties = Array.AsReadOnly(new Difficulty[3]
        {
            new Difficulty("EASY", 24, 24),
            new Difficulty("MEDIUM", 21, 21),
            new Difficulty("HARD", 18, 18),
        });

        public static readonly ReadOnlyCollection<Suit> suits = Array.AsReadOnly(new Suit[4]
        {
            new Suit("spades", 0, Color.black),
            new Suit("clubs", 1 , Color.black),
            new Suit("diamonds", 2, Color.red),
            new Suit("hearts", 3, Color.red)
        });

        public static readonly ReadOnlyCollection<Rank> ranks = Array.AsReadOnly(new Rank[13]
        {
            new Rank("A", 1, 1),
            new Rank("2", 2, 2),
            new Rank("3", 3, 3),
            new Rank("4", 4, 4),
            new Rank("5", 5, 5),
            new Rank("6", 6, 6),
            new Rank("7", 7, 7),
            new Rank("8", 8, 8),
            new Rank("9", 9, 9),
            new Rank("10", 10, 10),
            new Rank("J", 11, 10),
            new Rank("Q", 12, 10),
            new Rank("K", 13, 10)
        });
    }

    public static class Transforms
    {
        public const float matchExplosionScale = 0.3f;

        public const float draggedCardScale = 0.15f;
        public const float draggedCardYOffset = 0.4f;

        public const int zoomFactor = 20;
        public const float panAndZoomSpeed = 30;
    }

    public static class Points
    {
        public const int matchPoints = 200;
        public const int scoreMultiplier = 50;
        public const bool enableBonusPoints = false;
        public const int emptyReactorPoints = 1000;
        public const int perfectGamePoints = 2800;
    }

    public static class Music
    {
        public const float musicFadeInDurationSec = 2.5f;
        public const float musicFadeOutDurationSec = 1;
        public const float musicFadeOutSlowDurationSec = 2.5f;
    }

    public static class Settings
    {
        public const int musicDefaultVolume = 15;
        public const int musicVolumeDenominator = 20;

        public const int soundEffectsDefaultVolume = 15;
        public const int soundEffectsVolumeDenominator = 20;

        public const bool vibrationEnabledDefault = true;
        public const bool foodSuitsEnabledDefault = false;
    }

    public static class MenuText
    {
        public static readonly string[] menuButtons = new string[] { "PLAY", "TUTORIAL", "SETTINGS", "ABOUT" };
        public static readonly string[] levelButtons = new string[] { "CONTINUE", "EASY", "MEDIUM", "HARD" };
        public static readonly string backButton = "BACK";
        public static readonly string loading = "LOADING...";
        public static readonly string[] pauseButtons = new string[] { "RESUME", "RESTART", "SETTINGS", "MAIN\nMENU" };
        public static readonly string[] summaryButtons = new string[] { "MAIN\nMENU", "PLAY\nAGAIN" };
        public static readonly string[] scoreActionLabels = new string[] { "Score", "Action" };
        public static readonly string[] gameState = new string[] { "YOU WON", "YOU LOST" };
    }

    public static class Colors
    {
        public const float selectedCardOpacity = 0.25f;
        public static readonly Color cardObstructedColor = new(0.6f, 0.6f, 0.6f);

        public static readonly Color pointColor = Color.green;

        public static readonly Color tutorialObjectHighlightColor = new(1, 1, 0, 0.35f);

        public static readonly Color gameOverWin = Color.cyan;
        public static readonly Color gameOverLose = Color.red;

        public static class Highlight
        {
            public static readonly HighLightColor none = new(Color.white);
            public static readonly HighLightColor match = new(Color.green);
            public static readonly HighLightColor move = new(Color.yellow);
            public static readonly HighLightColor over = new(Color.red);
            public static readonly HighLightColor win = new(Color.cyan);

            public static readonly ReadOnlyCollection<HighLightColor> colors = Array.AsReadOnly(new HighLightColor[]
            {
                none,
                match,
                move,
                over,
                win
            });
        }
    }

    public static class AlertLevels
    {
        // for the action counter
        public static readonly AlertLevel none = new(new Color(0.725f, 0.725f, 0.725f), Color.white);
        public static readonly AlertLevel low = new(new Color(0.941f, 0.706f, 0.055f), new Color(0.6f, 0.45f, 0.039f));
        public static readonly AlertLevel high = new(new Color(0.835f, 0.2f, 0.098f), new Color(0.56f, 0.141f, 0.11f));
    }

    public static class AnimationDurataions
    {
        // all durations are in seconds

        // main menu
        public const float logoDelay = 2;
        public const float buttonFadeOut = 0.5f;

        // screen fades 
        public const float startGameFadeIn = 1; // fades out the game startup logos
        public const float gameplayFadeIn = 1; // fades in the gameplay scene
        public const float gameOverFade = 1f; // fades in the game over pop-up
        public const float gameEndFade = 1.5f; // fades out of the gameplay scene to summary
        public const float summaryFadeIn = 1f; // fades into the summary scene
        public const float playAgainFadeOut = 2; // fades out of the summary scene to gameplay

        // gameplay

        public const float cardHologramFadeIn = 2; // fades in the cards holograms
        public const float cardsToReactor = 0.6f; // movement of the cards to reactor during a nextcycle
        // match effect
        public const float comboPointsFadeIn = 0.5f; // fade in and scale up the point text
        public const float comboWait = 0.5f; // wait a bit
        public const float comboFadeOut = 1; // fade out and scale up the point text and food combo object

        public const float alertFade = 1; // action counter's alert siren

        public const float gameSummaryBabyFade = 2; // fade in, then out baby win transitioning in the summary scene
    }

    public static class FadeColors
    {
        // the colors that will be faded from and to during screen fades
        public static readonly Color blackA0 = new(0, 0, 0, 0);
        public static readonly Color blackA1 = new(0, 0, 0, 1);

        public static readonly Color grayA0 = new(0.6f, 0.6f, 0.6f, 0);
        public static readonly Color grayA1 = new(0.6f, 0.6f, 0.6f, 1);

        public static readonly FadeColorPair blackFadeOut = new(blackA1, blackA0);
        public static readonly FadeColorPair backFadeIn = new(blackA0, blackA1);
        
        public static readonly FadeColorPair grayFadeIn = new(grayA0, grayA1);
        public static readonly FadeColorPair grayFadeOut = new(grayA1, grayA0);
    }
}
