using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    // Singleton instance.
    public static Config Instance;

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
    private int _selectedCardsLayer, _cardLayer;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            // make instance persist across scenes
            DontDestroyOnLoad(this.gameObject);
            Instance = this;

            // These must be done in this order
            // Setup the Vibration Package
            Vibration.Init();
            // Check Player Preferences
            PersistentSettings.CheckKeys();
            // Check if the game state version needs updating and if the save file needs deleting
            SaveFile.CheckNewGameStateVersion();
            // Set the application frame rate to what was saved
            Debug.Log($"setting frame rate to: {PersistentSettings.FrameRate}");
            Application.targetFrameRate = PersistentSettings.FrameRate;


            _selectedCardsLayer = SortingLayer.NameToID(Constants.SortingLayers.selectedCards);
            _cardLayer = SortingLayer.NameToID(Constants.SortingLayers.card);
        }
        else if (Instance != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    public Difficulty CurrentDifficulty => _currentDifficulty;

    public int SelectedCardsLayer => _selectedCardsLayer;

    public int CardLayer => _cardLayer;

    public void SetDifficulty(Difficulty dif)
    {
        Debug.Log($"setting difficulty to: {dif}");
        _currentDifficulty = dif;
    }

    public void SetDifficulty(string dif)
    {
        for (int i = 0; i < GameValues.GamePlay.difficulties.Count; i++)
        {
            if (dif == GameValues.GamePlay.difficulties[i].Name)
            {
                SetDifficulty(GameValues.GamePlay.difficulties[i]);
                return;
            }
        }

        throw new KeyNotFoundException($"the difficulty \"{dif}\" was not found");
    }
}
