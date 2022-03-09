using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    //internal variables
    private GameObject fadeOutImage;

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
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            Instance = this;
            LoadGameValues();
        }
        else if (Instance != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }

        PlayerPrefKeys.CheckKeys();
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

    public void GameOver(bool didWin)
    {
        gameOver = true;
        gameWin = didWin;
        
        // overwritten when manually won (cheated)
        matchCounter = (byte) (MatchedPileScript.Instance.cardList.Count / 2);
        
        fadeOutImage.SetActive(true);

        //delay to show summary
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        float countdown = GameValues.delayToShowGameSummary;

        if (gameWin)
        {
            SpaceBabyController.Instance.BabyHappy();
            SoundEffectsController.Instance.WinSound();

            while (countdown > 0)
            {
                yield return new WaitForSeconds(0.01f);
                fadeOutImage.GetComponent<Image>().color = new Color(1, 1, 1, 1 - (countdown / GameValues.delayToShowGameSummary));
                countdown -= 0.02f;
            }
        }

        else
        {
            SpaceBabyController.Instance.BabyLoseTransition();
            SoundEffectsController.Instance.LoseSound();

            UtilsScript.Instance.errorImage.SetActive(true);
            while (countdown > 0)
            {
                yield return new WaitForSeconds(0.01f);
                fadeOutImage.GetComponent<Image>().color = new Color(0, 0, 0, 1 - (countdown / GameValues.delayToShowGameSummary));
                countdown -= 0.02f;
            }
        }

        SceneManager.LoadScene(Constants.summaryScene);

        if (gameWin)
        {
            MusicController.Instance.WinMusic();
        }
        else
        {
            MusicController.Instance.LoseMusic();
        }
        SaveState.Delete();
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

        fadeOutImage = GameObject.Find("FadeOutImage");
        if (fadeOutImage != null)
        {
            fadeOutImage.SetActive(false);
        }
    }

    public int CountFoundationCards()
    {
        if (foundation1 != null && foundation2 != null && foundation3 != null && foundation4 != null)
            return foundation1.GetComponent<FoundationScript>().cardList.Count + foundation2.GetComponent<FoundationScript>().cardList.Count +
                foundation3.GetComponent<FoundationScript>().cardList.Count + foundation4.GetComponent<FoundationScript>().cardList.Count;
        else
            return -1;
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
