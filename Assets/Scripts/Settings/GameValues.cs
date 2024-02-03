using System;
using System.Collections.ObjectModel;
using UnityEngine;

public static class GameValues
{
    public static class GamePlay
    {
        public const int foundationStartingSize = 7;
        public const int cardsToDeal = 3;
        public const int turnAlertThreshold = 5;

        public const int suitCount = 4;
        public const int rankCount = 13;
        public const int cardCount = suitCount * rankCount;
        public const int matchCount = cardCount / 2;

        public static readonly ReadOnlyCollection<Suit> suits = Array.AsReadOnly(new Suit[suitCount]
        {
            new Suit("spades", 0, Color.black),
            new Suit("clubs", 1 , Color.black),
            new Suit("diamonds", 2, Color.red),
            new Suit("hearts", 3, Color.red)
        });

        public static readonly ReadOnlyCollection<Rank> ranks = Array.AsReadOnly(new Rank[rankCount]
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

        public const float zoomFactor = 0.3f;
    }

    public static class Points
    {
        public const int matchPoints = 200;
        public const int scoreMultiplier = 50;
    }

    public static class Music
    {
        public const float musicFadeInDurationSec = 2.5f;
        public const float musicFadeOutDurationSec = 1;
        public const float musicFadeOutSlowDurationSec = 2f;
    }

    public static class Settings
    {
        public const int musicDefaultVolume = 20; // 0-20
        public const int musicVolumeDenominator = 20; // how many steps

        public const int soundEffectsDefaultVolume = 20; // 0-20
        public const int soundEffectsVolumeDenominator = 20; // how many steps

        public const bool achievementPopupsEnabledDefault = true;
        public const bool vibrationEnabledDefault = true;
        public const bool foodSuitsEnabledDefault = false;

        public const bool saveGameStateDefault = true;
        public const int movesUntilSaveDefault = 50;

        public const bool hintsEnabledDefault = true;
    }

    public static class Text
    {
        public const string gameWon = "YOU WON";
        public const string gameLost = "YOU LOST";
        public const string noValue = "---";
    }

    public static class Colors
    {
        public static readonly Color whiteAlphaLow = new(1, 1, 1, 0.6f);
        public const float selectedCardOpacity = 0.25f;
        public static readonly Color cardObstructedColor = new(0.6f, 0.6f, 0.6f);
        public const float cardHologramAlpha = 0.8f;
        public static readonly Color cardHologramColor = new(0, 0, 0, 1 - cardHologramAlpha);
        public static readonly HighLightColor normal = new(Color.white, Constants.ColorLevel.None);
        public static readonly HighLightColor card = new("#1af4ff", Constants.ColorLevel.None);

        public static class Modes
        {
            public static readonly ColorMode normal = new("Normal",
                new HighLightColor(Color.green, Constants.ColorLevel.Match),
                new HighLightColor("#FFDF00", Constants.ColorLevel.Move),
                new HighLightColor(Color.red, Constants.ColorLevel.Over),
                new HighLightColor("#4691D5", Constants.ColorLevel.Notify)
                );

            public static readonly ColorMode deuteranopia = new("Deuteranopia",
                new HighLightColor("#56F0B7", Constants.ColorLevel.Match),
                new HighLightColor("#FFE043", Constants.ColorLevel.Move),
                new HighLightColor("#F45DFF", Constants.ColorLevel.Over),
                new HighLightColor("#1F00F5", Constants.ColorLevel.Notify)
                );

            public static readonly ColorMode protanopia = new("Protanopia",
                new HighLightColor("#00D8F5", Constants.ColorLevel.Match),
                new HighLightColor("#F1FF00", Constants.ColorLevel.Move),
                new HighLightColor("#FF8C81", Constants.ColorLevel.Over),
                new HighLightColor("#9F00F5", Constants.ColorLevel.Notify)
                );

            public static readonly ColorMode tritanopia = new("Tritanopia",
                new HighLightColor("#92F05F", Constants.ColorLevel.Match),
                new HighLightColor("#FFA446", Constants.ColorLevel.Move),
                new HighLightColor("#ED00FF", Constants.ColorLevel.Over),
                new HighLightColor("#0161F5", Constants.ColorLevel.Notify)
                );

            public static readonly ReadOnlyCollection<ColorMode> List = Array.AsReadOnly(new ColorMode[]
            {
                normal, deuteranopia, protanopia, tritanopia
            });
        }
    }

    public static class AnimationDurataions
    {
        // all durations are in seconds

        // main menu
        public const float logoDelay = 2;
        public const float buttonFadeOut = 0.5f;
        public const float zoomAndFade = 1.7f;

        // screen fades 
        public const float startGameFadeIn = 0.5f; // fades out the game startup logos
        public const float gameplayFadeIn = 0.5f; // fades in the gameplay scene
        public const float gameOverFade = 1f; // fades in the game over pop-up
        public const float gameEndWonFade = 1.7f; // fades out of the gameplay scene to summary - game was won
        public const float gameEndLostFade = 1f; // fades out of the gameplay scene to summary - game was lost
        public const float summaryFadeIn = 0.5f; // fades into the summary scene
        public const float playAgainFadeOut = 1; // fades out of the summary scene to gameplay

        // Achievements
        public const float achievementPopup = 2;
        public const float achievementPopupFade = 0.5f;

        // gameplay
        public const float cardHologramFadeIn = 2; // fades in the cards holograms
        public const float cardsToReactor = 0.5f; // movement of the cards to reactor during a nextcycle
        // match effect
        public const float comboPointsFadeIn = 0.5f; // fade in and scale up the point text
        public const float comboWait = 0.5f; // wait a bit
        public const float comboFadeOut = 1; // fade out and scale up the point text and food combo object

        public const float alertFade = 1; // action counter's alert siren
        public const float reactorExplosionDelay = 0.4f; // the delay between reactors exploding when losing

        public const float gameSummaryBabyFade = 1.2f; // fade in, then out baby win transitioning in the summary scene
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
