using UnityEngine;

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
    public const string tutorialStateStartFileName = "tutorialState_start";

    // Other
    public static readonly bool inEditor = Application.isEditor;

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

    // Log moves
    public const string stackLogMove = "stack";
    public const string moveLogMove = "move";
    public const string matchLogMove = "match";
    public const string drawLogMove = "draw";
    public const string cycleLogMove = "cycle";
    public const string deckresetLogMove = "deckreset";

    // Keys
    // Summary stats
    public const string highScoreKey = "HighScore";
    public const string leastMovesKey = "LeastMoves";

    // Settings
    public const string soundEffectsVolumeKey = "SoundEffectsVolume";
    public const string musicVolumeKey = "MusicVolume";
    public const string vibrationEnabledKey = "VibrationEnabled";
    public const string foodSuitsEnabledKey = "FoodSuitsEnabled";
}
