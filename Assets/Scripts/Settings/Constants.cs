﻿using UnityEngine;

public static class Constants
{
    // Game Values
    public const string gameValuesPath = "GameConfigurations/gameValues";

    // Save States
    public const string saveStatePathInEditor = "Assets/Resources/GameStates/";
    public const string saveStateFileName = "saveState";

    public const string saveStateFileNameJson = saveStateFileName + ".json";
    public const string saveStateFilePathJsonInEditor = saveStatePathInEditor + saveStateFileNameJson;
    public const string saveStateFileNameMeta = saveStateFileNameJson + ".meta";
    public const string saveStateFilePathMetaInEditor = saveStatePathInEditor + saveStateFileNameMeta;

    // Tutorial
    public const string tutorialResourcePath = "Tutorial/";
    public const string tutorialCommandListFilePath = tutorialResourcePath + "TutorialCommandList";
    public const string tutorialStateStartFileName = "tutorialState_0";

    // Other
    public static readonly bool inEditor = Application.isEditor;

    // Begin Keys
    // Summary stats
    public const string highScoreKey = "HighScore";
    public const string leastMovesKey = "LeastMoves";

    // Settings
    public const string soundEffectsVolumeKey = "SoundEffectsVolume";
    public const string musicVolumeKey = "MusicVolume";
    public const string vibrationEnabledKey = "VibrationEnabled";
    public const string foodSuitsEnabledKey = "FoodSuitsEnabled";
    // End Keys

    // Scenes
    public const string mainMenuScene = "MainMenuScene";
    public const string aboutScene = "AboutScene";
    public const string settingsScene = "SettingsScene";
    public const string gameplayScene = "GameplayScene";
    public const string pauseScene = "PauseScene";
    public const string summaryScene = "SummaryScene";

    // Sorting Layers
    public const string selectedCardsSortingLayer = "SelectedCards";
    public const string gameplaySortingLayer = "Gameplay";

    // Tags
    public const string cardTag = "Card";
    public const string reactorTag = "Reactor";
    public const string foundationTag = "Foundation";
    public const string deckTag = "Deck";
    public const string wastepileTag = "Wastepile";
    public const string matchedPileTag = "MatchedPile";
    public const string loadPileTag= "LoadPile";

    // Card Suits
    public const string spadesSuit = "spades";
    public const string clubsSuit = "clubs";
    public const string diamondsSuit = "diamonds";
    public const string heartsSuit = "hearts";
    public static readonly string[] suits = { spadesSuit , clubsSuit , diamondsSuit , heartsSuit };

    // Log moves
    public const string stackLogMove = "stack";
    public const string moveLogMove = "move";
    public const string matchLogMove = "match";
    public const string drawLogMove = "draw";
    public const string cycleLogMove = "cycle";
    public const string deckresetLogMove = "deckreset";

    // Card Levels
    public const byte defaultHighlightColorLevel = 0;
    public const byte matchHighlightColorLevel = 1;
    public const byte moveHighlightColorLevel = 2;
    public const byte overHighlightColorLevel = 3;
}
