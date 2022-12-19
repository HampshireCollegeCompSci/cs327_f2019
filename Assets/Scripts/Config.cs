using System;
using System.IO;
using System.Collections.Generic;
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

    private Difficulty _currentDifficulty;
    public Difficulty CurrentDifficulty => _currentDifficulty;

    public void SetDifficulty(int dif)
    {
        if (dif < 0 || dif > GameValues.difficulties.Length)
        {
            throw new IndexOutOfRangeException($"the difficulties index of \"{dif}\" did not fall in the range of 0-{GameValues.difficulties.Length}");
        }
        _currentDifficulty = GameValues.difficulties[dif];
    }

    public void SetDifficulty(string dif)
    {
        for (int i = 0; i < GameValues.difficulties.Length; i++)
        {
            if (dif == GameValues.difficulties[i].Name)
            {
                SetDifficulty(i);
                return;
            }
        }

        throw new KeyNotFoundException($"the difficulty \"{dif}\" was not found");
    }

    private void LoadGameValues()
    {
        Debug.Log("loading gamevalues from json");
        TextAsset jsonTextFile = Resources.Load<TextAsset>(Constants.gameValuesPath);
        GameValues = JsonUtility.FromJson<GameValues>(jsonTextFile.ToString());
        GameValues.highlightColors = new Color[] {
            Color.white,
            GameValues.matchHighlightColor,
            GameValues.moveHighlightColor,
            GameValues.overHighlightColor,
            Color.cyan
        };
    }
}
