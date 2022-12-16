using UnityEngine;

public class Config : MonoBehaviour
{
    // Singleton instance.
    public static Config Instance;
    public static GameValues GameValues;

    // game settings
    public bool tutorialOn, nextCycleEnabled;
    public bool continuing;

    public bool prettyColors;

    public string currentDifficulty;
    public int reactorLimit;
    public int actionMax;

    // game values
    public bool gamePaused;
    public bool gameOver;
    public bool gameWin;

    public int actions;
    public int score;
    public byte consecutiveMatches;

    // long term tracking
    public int moveCounter;
    public byte matchCounter;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            // make instance persist across scenes
            DontDestroyOnLoad(this.gameObject);
            Instance = this;

            // These must be done in this order
            // Load the game values from the file
            LoadGameValues();
            // Setup the Vibration Package
            Vibration.Init();
            // Check Player Preferences
            PersistentSettings.CheckKeys();
            // Check if the game state version needs updating and if the save file needs deleting
            SaveFile.CheckNewGameStateVersion();
            // Set the application frame rate to what was saved
            Application.targetFrameRate = PersistentSettings.FrameRate;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    public void SetDifficulty(int dif)
    {
        currentDifficulty = GameValues.difficulties[dif];
        reactorLimit = GameValues.reactorLimits[dif];
        actionMax = GameValues.moveLimits[dif];
    }

    public void SetDifficulty(string dif)
    {
        for (int i = 0; i < GameValues.difficulties.Length; i++)
        {
            if (dif.Equals(GameValues.difficulties[i]))
            {
                SetDifficulty(i);
                return;
            }
        }

        throw new System.Exception($"The difficulty \"{dif}\" was not found.");
    }

    private void LoadGameValues()
    {
        Debug.Log("loading gamevalues from json");
        TextAsset jsonTextFile = Resources.Load<TextAsset>(Constants.gameValuesPath);
        GameValues = JsonUtility.FromJson<GameValues>(jsonTextFile.ToString());

        // Colors need to be reconstructed
        GameValues.cardObstructedColor = CreateColor(GameValues.cardObstructedColorValues);

        GameValues.matchHighlightColor = CreateColor(GameValues.matchHighlightColorValues);
        GameValues.moveHighlightColor = CreateColor(GameValues.moveHighlightColorValues);
        GameValues.overHighlightColor = CreateColor(GameValues.overHighlightColorValues);
        GameValues.highlightColors = new Color[] {
            Color.white,
            GameValues.matchHighlightColor,
            GameValues.moveHighlightColor,
            GameValues.overHighlightColor,
            Color.cyan
        };

        GameValues.pointColor = CreateColor(GameValues.pointColorValues);
        GameValues.tutorialObjectHighlightColor = CreateColor(GameValues.tutorialObjectHighlightColorValues);
        GameValues.fadeDarkColor = CreateColor(GameValues.fadeDarkColorValues);
        GameValues.fadeLightColor = CreateColor(GameValues.fadeLightColorValues);
    }

    private Color CreateColor(float[] colorV)
    {
        if (colorV.Length != 4)
        {
            throw new System.ArgumentException("the array of color values is not a lenght of 4");
        }
        return new Color(colorV[0], colorV[1], colorV[2], colorV[3]);
    }
}
