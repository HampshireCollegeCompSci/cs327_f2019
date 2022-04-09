using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    // the undo log
    public List<Move> moveLog = new List<Move>();

    // game settings
    public bool tutorialOn;
    public bool continuing;

    public bool prettyColors;

    public string currentDifficulty;
    public int maxReactorVal;
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

    // foundations
    public GameObject foundation1;
    public GameObject foundation2;
    public GameObject foundation3;
    public GameObject foundation4;
    public GameObject[] foundations;

    // reactors
    public GameObject reactor1;
    public GameObject reactor2;
    public GameObject reactor3;
    public GameObject reactor4;
    public GameObject[] reactors;

    // Singleton instance.
    public static Config Instance = null;
    public static GameValues GameValues = null;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            // make instance persist across scenes
            DontDestroyOnLoad(this.gameObject);
            Instance = this;

            // Load the game values from the file and then check keys
            LoadGameValues();
            PlayerPrefKeys.CheckKeys();
        }
        else if (Instance != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    private void LoadGameValues()
    {
        Debug.Log("loading gamevalues from json");
        TextAsset jsonTextFile = Resources.Load<TextAsset>(Constants.gameValuesPath);
        GameValues = JsonUtility.FromJson<GameValues>(jsonTextFile.ToString());

        // Colors need to be reconstructed
        GameValues.cardObstructedColor = CreateColor(GameValues.cardObstructedColorValues);
        GameValues.cardMoveHighlightColor = CreateColor(GameValues.cardMoveHighlightColorValues);
        GameValues.cardMatchHighlightColor = CreateColor(GameValues.cardMatchHighlightColorValues);
        GameValues.pointColor = CreateColor(GameValues.pointColorValues);
    }

    private Color CreateColor(float [] colorV)
    {
        return new Color(colorV[0], colorV[1], colorV[2], colorV[3]);
    }

    public void StartupFindObjects()
    {
        foundation1 = GameObject.Find("Foundation (0)");
        foundation2 = GameObject.Find("Foundation (1)");
        foundation3 = GameObject.Find("Foundation (2)");
        foundation4 = GameObject.Find("Foundation (3)");
        foundations = new GameObject[] { foundation1, foundation2, foundation3, foundation4 };

        reactor1 = GameObject.Find("ReactorPile (0)");
        reactor2 = GameObject.Find("ReactorPile (1)");
        reactor3 = GameObject.Find("ReactorPile (2)");
        reactor4 = GameObject.Find("ReactorPile (3)");
        reactors = new GameObject[] { reactor1, reactor2, reactor3, reactor4 };
    }

    public void SetDifficulty(int dif)
    {
        currentDifficulty = GameValues.difficulties[dif];
        maxReactorVal = GameValues.reactorLimits[dif];
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
}
