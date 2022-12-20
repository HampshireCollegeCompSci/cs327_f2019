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
    public int consecutiveMatches;

    // long term tracking
    public int moveCounter;
    public int matchCounter;

    private Difficulty _currentDifficulty;
    private Suit[] _suits;
    private Rank[] _ranks;

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
            LoadValues();
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

    public Difficulty CurrentDifficulty => _currentDifficulty;

    public Suit[] Suits => _suits;
    public Rank[] Ranks => _ranks;

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

    private void LoadValues()
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

        _suits = new Suit[4]
        {
            new Suit("spades", 0, Color.black),
            new Suit("clubs", 1 , Color.black),
            new Suit("diamonds", 2, Color.red),
            new Suit("hearts", 3, Color.red)
        };

        _ranks = new Rank[13];
        _ranks[0] = new Rank("A", 1, 1);
        _ranks[10] = new Rank("J", 11, 10);
        _ranks[11] = new Rank("Q", 12, 10);
        _ranks[12] = new Rank("K", 13, 10);
        for (int i = 2; i < 11; i++)
        {
            _ranks[i - 1] = new Rank(i.ToString(), i, i);
        }
    }
}
