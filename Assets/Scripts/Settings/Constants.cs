public static class Constants
{
    public const string projectWebsite = "https://github.com/HampshireCollegeCompSci/cs327_f2019#readme";

    public static class GameStates
    {
        public const string versionKey = "GameStateVersion";
        public const string version = "2";

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

    public static class Time
    {
        public const string format = "mm\\:ss\\.ff";
    }

    public static class Settings
    {
        public const string soundEffectsVolumeKey = "SoundEffectsVolume";
        public const string musicVolumeKey = "MusicVolume";
        public const string vibrationEnabledKey = "VibrationEnabled";
        public const string achievementPopupsEnabledKey = "AchievementPopupsEnabled";
        public const string foodSuitsEnabledKey = "FoodSuitsEnabled";
        public const string frameRateKey = "FrameRate";
        public const string saveGameStateKey = "SaveGameState";
        public const string movesUntilSaveKey = "MovesUntilSave";
        public const string hintsEnabledKey = "HintsEnabled";
        public const string colorMode = "ColorMode";
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
        public const string achievement = "AchievementsScene";
        public const string stats = "StatsScene";
    }

    public static class SortingLayers
    {
        public const string selectedCards = "SelectedCards";
        public const string card = "Card";
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

    public enum CardContainerType
    {
        None,
        Loadpile,
        MatchedPile,
        Foundation,
        Reactor,
        Deck,
        WastePile
    }

    public enum LogMoveType
    {
        Move,
        Stack,
        Match,
        Draw,
        Cycle,
        Deckreset
    }

    public enum ColorLevel
    {
        None,
        Match,
        Move,
        Over,
        Notify
    };
}
