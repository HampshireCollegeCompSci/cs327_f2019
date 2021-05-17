using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Config : MonoBehaviour
{
    public static Config config; //Creates a new instance if one does not yet exist

    //Variables go here
    public List<Move> moveLog = new List<Move>();
    public bool gameOver;
    public bool gameWin;
    public int score;

    public float relativeCardScale;
    public int turnsTillReset;
    public int delayToShowGameSummary;
    public float draggedTokenOffset;
    public float selectedCardOpacity;
    public bool prettyColors;
    public Color cardObstructedColor;
    public Color cardMoveHighlightColor;
    public Color cardMatchHighlightColor;
    public Color pointColor;

    //score
    public int matchPoints;
    public int emptyReactorPoints;
    public int perfectGamePoints;

    //baby
    private GameObject baby;

    //foundations
    public int foundationStartSize;

    public GameObject foundation1;
    public GameObject foundation2;
    public GameObject foundation3;
    public GameObject foundation4;
    public GameObject[] foundations;

    //wastepile
    public byte wastepileAnimationSpeedSlow;
    public byte wastepileAnimationSpeedFast;

    //reactor
    public int maxReactorVal;

    public GameObject reactor1;
    public GameObject reactor2;
    public GameObject reactor3;
    public GameObject reactor4;
    public GameObject[] reactors;

    //deck
    public byte cardsToDeal;
    public byte cardsToReactorspeed;

    //tutorial
    public bool tutorialOn;

    //UI
    public bool gamePaused;

    //internal variables
    private GameInfo gameInfo;
    private GameObject fadeOutImage;

    public string[] difficulties;
    public int[] reactorLimits;
    public int[] moveLimits;

    public string currentDifficulty;

    public int actionMax;
    public int actions;
    public int turnAlertSmallThreshold;
    public int turnAlertThreshold;

    public byte consecutiveMatches;
    public int scoreMultiplier;

    //button txt
    public string[] gameStateTxtEnglish;
    public string[] menuSceneButtonsTxtEnglish;
    public string loadingSceneTxtEnglish;
    public string[] levelSceneButtonsTxtEnglish;
    public string[] pauseSceneButtonsTxtEnglish;
    public string[] summarySceneButtonsTxtEnglish;

    //vibration
    public int vibrationButton;
    public int vibrationCard;
    public int vibrationMatch;
    public int vibrationExplosion;

    //long term tracking
    //public int moves;
    public int moveCounter;
    public byte matchCounter;

    private void Awake()
    {
        if (config == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            config = this;
        }
        else if (config != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    private void Start()
    {
        string path = "GameConfigurations/gameValues";
        gameInfo = CreateFromJSON(path);
        ConfigFromJSON();
    }

    public void GameOver(bool didWin, bool manualWin = false)
    {
        gameOver = true;
        gameWin = didWin;
        
        if (!manualWin)
            matchCounter = (byte) (MatchedPileScript.Instance.cardList.Count / 2);
        
        fadeOutImage.SetActive(true);
        baby = GameObject.Find("SpaceBaby");

        //delay to show summary
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        float countdown = delayToShowGameSummary;

        if (gameWin)
        {
            baby.GetComponent<SpaceBabyController>().BabyHappyAnim();
            SoundEffectsController.Instance.WinSound();

            while (countdown > 0)
            {
                yield return new WaitForSeconds(0.01f);
                fadeOutImage.GetComponent<Image>().color = new Color(1, 1, 1, 1 - (countdown / delayToShowGameSummary));
                countdown -= 0.02f;
            }
        }

        else
        {
            //baby.GetComponent<SpaceBabyController>().BabyLoseSound();
            SoundEffectsController.Instance.LoseSound();

            UtilsScript.Instance.errorImage.SetActive(true);
            while (countdown > 0)
            {
                yield return new WaitForSeconds(0.01f);
                fadeOutImage.GetComponent<Image>().color = new Color(0, 0, 0, 1 - (countdown / delayToShowGameSummary));
                countdown -= 0.02f;
            }
        }

        SceneManager.LoadScene("SummaryScene");

        if (gameWin)
        {
            MusicController.Instance.WinMusic();
        }
        else
        {
            MusicController.Instance.LoseMusic();
        }
        DeleteSave();
    }

    public void ConfigFromJSON()
    {
        foundationStartSize = gameInfo.foundationStartingSize;
        wastepileAnimationSpeedSlow = gameInfo.wastepileAnimationSpeedSlow;
        wastepileAnimationSpeedFast = gameInfo.wastepileAnimationSpeedFast;
        cardsToDeal = gameInfo.cardsToDeal;
        cardsToReactorspeed = gameInfo.cardsToReactorspeed;
        relativeCardScale = gameInfo.relativeCardScale;
        turnsTillReset = gameInfo.turnsTillReset;
        matchPoints = gameInfo.matchPoints;
        emptyReactorPoints = gameInfo.emptyReactorPoints;
        perfectGamePoints = gameInfo.perfectGamePoints;
        delayToShowGameSummary = gameInfo.delayToShowGameSummary;
        difficulties = gameInfo.difficulties;
        reactorLimits = gameInfo.reactorLimits;
        moveLimits = gameInfo.moveLimits;
        draggedTokenOffset = gameInfo.draggedTokenOffset;
        selectedCardOpacity = gameInfo.selectedCardOpacity;
        gameStateTxtEnglish = gameInfo.gameStateTxtEnglish;
        menuSceneButtonsTxtEnglish = gameInfo.menuSceneButtonsTxtEnglish;
        loadingSceneTxtEnglish = gameInfo.loadingSceneTxtEnglish;
        levelSceneButtonsTxtEnglish = gameInfo.levelSceneButtonsTxtEnglish;
        pauseSceneButtonsTxtEnglish = gameInfo.pauseSceneButtonsTxtEnglish;
        summarySceneButtonsTxtEnglish = gameInfo.summarySceneButtonsTxtEnglish;
        cardObstructedColor = new Color(gameInfo.cardObstructedColor[0],
                                        gameInfo.cardObstructedColor[1],
                                        gameInfo.cardObstructedColor[2],
                                        gameInfo.cardObstructedColor[3]);
        cardMoveHighlightColor = new Color(gameInfo.cardMoveHighlightColor[0],
                                           gameInfo.cardMoveHighlightColor[1],
                                           gameInfo.cardMoveHighlightColor[2],
                                           gameInfo.cardMoveHighlightColor[3]);
        cardMatchHighlightColor = new Color(gameInfo.cardMatchHighlightColor[0],
                                           gameInfo.cardMatchHighlightColor[1],
                                           gameInfo.cardMatchHighlightColor[2],
                                           gameInfo.cardMatchHighlightColor[3]);
        pointColor = new Color(gameInfo.pointColor[0],
                               gameInfo.pointColor[1],
                               gameInfo.pointColor[2],
                               gameInfo.pointColor[3]);

        turnAlertSmallThreshold = gameInfo.turnAlertSmallThreshold;
        turnAlertThreshold = gameInfo.turnAlertThreshold;

        vibrationButton = gameInfo.vibrationButton;
        vibrationCard = gameInfo.vibrationCard;
        vibrationMatch = gameInfo.vibrationMatch;
        vibrationExplosion = gameInfo.vibrationExplosion;
        scoreMultiplier = gameInfo.scoreMultiplier;
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

        score = 0;
    }


    public static GameInfo CreateFromJSON(string path)
    {
        var jsonTextFile = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<GameInfo>(jsonTextFile.ToString());
    }

    [SerializeField]
    string json;
    public string WriteString(string path)
    {
        using (StreamReader stream = new StreamReader(path))
        {
            json = stream.ReadToEnd();
        }
        return json;
    }

    public int CountFoundationCards()
    {
        if (foundation1 != null && foundation2 != null && foundation3 != null && foundation4 != null)
            return foundation1.GetComponent<FoundationScript>().cardList.Count + foundation2.GetComponent<FoundationScript>().cardList.Count +
                foundation3.GetComponent<FoundationScript>().cardList.Count + foundation4.GetComponent<FoundationScript>().cardList.Count;
        else
            return -1;
    }


    public float GetScreenToWorldHeight()
    {
        Vector2 topRightCorner = new Vector2(1, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
        var height = edgeVector.y * 2;
        return height;

    }

    public float GetScreenToWorldWidth()
    {

        Vector2 topRightCorner = new Vector2(1, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
        var width = edgeVector.x * 2;
        return width;

    }

    public void SetDifficulty(string dif)
    {
        currentDifficulty = dif;

        if (dif == difficulties[0])
        {
            maxReactorVal = reactorLimits[0];
            actionMax = moveLimits[0];
        }
        if (dif == difficulties[1])
        {
            maxReactorVal = reactorLimits[1];
            actionMax = moveLimits[1];
        }
        if (dif == difficulties[2])
        {
            maxReactorVal = reactorLimits[2];
            actionMax = moveLimits[2];
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(Application.persistentDataPath + "/testState.json"))
        {
            File.Delete(Application.persistentDataPath + "/testState.json");
            File.Delete(Application.persistentDataPath + "/testState.meta");
        }
        if (Application.isEditor && File.Exists("Assets/Resources/GameStates/testState.json"))
        {
            File.Delete("Assets/Resources/GameStates/testState.json");
            File.Delete("Assets/Resources/GameStates/testState.json.meta");
        }
    }
}
