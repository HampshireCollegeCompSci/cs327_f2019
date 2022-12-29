public static class Constants
{
    // Game Values
    public const string gameValuesPath = "GameConfigurations/gameValues";

    public static class GameStates
    {
        public const string versionKey = "GameStateVersion";
        public const string version = "1";

        public const string saveStatePathInEditor = "Assets/Resources/GameStates/";
        public const string saveStateFileName = "saveState";

        public const string saveStateFileNameJson = saveStateFileName + ".json";
        public const string saveStateFilePathJsonInEditor = saveStatePathInEditor + saveStateFileNameJson;
        public const string saveStateFileNameMeta = saveStateFileNameJson + ".meta";
        public const string saveStateFilePathMetaInEditor = saveStatePathInEditor + saveStateFileNameMeta;
    }

    public static class Tutorial
    {
        public const string tutorialResourcePath = "Tutorial/";
        public const string tutorialCommandListFilePath = tutorialResourcePath + "TutorialCommandList";
        public const string tutorialStateStartFileName = "tutorialState_default";
    }

    public static class Summary
    {
        public const string highScoreKey = "HighScore";
        public const string leastMovesKey = "LeastMoves";
    }

    public static class Settings
    {
        public const string soundEffectsVolumeKey = "SoundEffectsVolume";
        public const string musicVolumeKey = "MusicVolume";
        public const string vibrationEnabledKey = "VibrationEnabled";
        public const string foodSuitsEnabledKey = "FoodSuitsEnabled";
        public const string frameRateKey = "FrameRate";
    }

    // Music Audio Mixer Exposed Parameters (name)
    public static class AudioMixerNames
    {
        public const string master = "MasterVolume";
        public const string track1 = "Track1Volume";
        public const string track2 = "Track2Volume";
    }

    public static class ScenesNames
    {
        public const string mainMenu = "MainMenuScene";
        public const string about = "AboutScene";
        public const string settings = "SettingsScene";
        public const string gameplay = "GameplayScene";
        public const string pause = "PauseScene";
        public const string summary = "SummaryScene";
    }

    public static class SortingLayers
    {
        public const string selectedCards = "SelectedCards";
        public const string gameplay = "Gameplay";
    }
    
    public static class Tags
    {
        public const string card = "Card";
        public const string reactor = "Reactor";
        public const string foundation = "Foundation";
        public const string deck = "Deck";
        public const string wastepile = "Wastepile";
        public const string matchedPile = "MatchedPile";
        public const string loadPile = "LoadPile";
    }

    public static class LogMoveTypes
    {
        public const byte move = 0;
        public const byte stack = 1;
        public const byte match = 3;
        public const byte draw = 4;
        public const byte cycle = 5;
        public const byte deckreset = 6;
    }
}
