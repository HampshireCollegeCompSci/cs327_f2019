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
    public Stack<Move> moveLog = new Stack<Move>();
    public bool gameOver;
    public bool gameWin;
    public int score;
    public float relativeCardScale;
    public int turnsTillReset;
    public int delayToShowGameSummary;
    public float draggedTokenOffset;
    public float selectedCardOpacity;
    public bool prettyColors;
    public float[] cardHighlightColor;

    //score
    public int matchPoints;
    public int emptyReactorPoints;
    public int perfectGamePoints;

    //card scale
    public Vector3 cardScale;


    //foundations
    public GameObject[] foundationList;
    public float foundationStackDensity;
    public int foundationStartSize;

    //wastepile
    public GameObject wastePile;

    //reactor
    public int maxReactorVal = 18;

    public GameObject foundation1;
    public GameObject foundation2;
    public GameObject foundation3;
    public GameObject foundation4;

    public GameObject reactor1;
    public GameObject reactor2;
    public GameObject reactor3;
    public GameObject reactor4;
    public GameObject[] reactors;

    //deck
    public GameObject deck;
    public int cardsToDeal;

    //matches
    public GameObject matches;

    //UI
    public bool gamePaused;

    //internal variables
    private string JSON;
    GameInfo gameInfo;
    GameObject fadeOutImage;
    GameObject errorImage;

    public string difficulty;

    public int easy;
    public int medium;
    public int hard;

    public int easyMoveCount;
    public int mediumMoveCount;
    public int hardMoveCount;

    public int actionMax;
    public int actions;

    //button txt
    public string[] gameStateTxtEnglish;
    public string[] menuSceneButtonsTxtEnglish;
    public string loadingSceneTxtEnglish;
    public string[] levelSceneButtonsTxtEnglish;
    public string[] pauseSceneButtonsTxtEnglish;
    public string[] summarySceneButtonsTxtEnglish;

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
        SetCards();
        gameObject.GetComponent<MusicController>().MainMenuMusic();
    }

    public void GameOver(bool didWin)
    {
        gameOver = true;
        gameWin = didWin;
        fadeOutImage.SetActive(true);
        //delay to show summary
        StartCoroutine(EndGame());

    }

    IEnumerator EndGame()
    {
        float countdown = delayToShowGameSummary;

        if (gameWin)
        {
            gameObject.GetComponent<MusicController>().WinMusic();
            while (countdown > 0)
            {
                yield return new WaitForSeconds(0.01f);
                fadeOutImage.GetComponent<Image>().color = new Color(1, 1, 1, 1 - (countdown / delayToShowGameSummary));
                countdown -= 0.02f;
            }
        }

        else
        {
            gameObject.GetComponent<MusicController>().LoseMusic();
            errorImage.SetActive(true);
            while (countdown > 0)
            {
                yield return new WaitForSeconds(0.01f);
                fadeOutImage.GetComponent<Image>().color = new Color(0, 0, 0, 1 - (countdown / delayToShowGameSummary));
                countdown -= 0.02f;
            }
        }

        SceneManager.LoadScene("SummaryScene");
    }

    public void ConfigFromJSON()
    {
        foundationStartSize = gameInfo.foundationStartingSize;
        cardsToDeal = gameInfo.cardsToDeal;
        relativeCardScale = gameInfo.relativeCardScale;
        turnsTillReset = gameInfo.turnsTillReset;
        matchPoints = gameInfo.matchPoints;
        emptyReactorPoints = gameInfo.emptyReactorPoints;
        perfectGamePoints = gameInfo.perfectGamePoints;
        delayToShowGameSummary = gameInfo.delayToShowGameSummary;
        easy = gameInfo.easyReactorLimit;
        medium = gameInfo.mediumReactorLimit;
        hard = gameInfo.hardReactorLimit;
        easyMoveCount = gameInfo.easyMoveCount;
        mediumMoveCount = gameInfo.mediumMoveCount;
        hardMoveCount = gameInfo.hardMoveCount;
        draggedTokenOffset = gameInfo.draggedTokenOffset;
        selectedCardOpacity = gameInfo.selectedCardOpacity;
        gameStateTxtEnglish = gameInfo.gameStateTxtEnglish;
        menuSceneButtonsTxtEnglish = gameInfo.menuSceneButtonsTxtEnglish;
        loadingSceneTxtEnglish = gameInfo.loadingSceneTxtEnglish;
        levelSceneButtonsTxtEnglish = gameInfo.levelSceneButtonsTxtEnglish;
        pauseSceneButtonsTxtEnglish = gameInfo.pauseSceneButtonsTxtEnglish;
        summarySceneButtonsTxtEnglish = gameInfo.summarySceneButtonsTxtEnglish;
        cardHighlightColor = gameInfo.cardHighlightColor;
    }

    public void SetCards()
    {
        foundation1 = GameObject.Find("Foundation (0)");
        foundation2 = GameObject.Find("Foundation (1)");
        foundation3 = GameObject.Find("Foundation (2)");
        foundation4 = GameObject.Find("Foundation (3)");
        foundationList = new GameObject[] { foundation1, foundation2, foundation3, foundation4 };

        reactor1 = GameObject.Find("ReactorPile (0)");
        reactor2 = GameObject.Find("ReactorPile (1)");
        reactor3 = GameObject.Find("ReactorPile (2)");
        reactor4 = GameObject.Find("ReactorPile (3)");
        reactors = new GameObject[] { reactor1, reactor2, reactor3, reactor4 };

        wastePile = GameObject.Find("Scroll View");

        deck = GameObject.Find("DeckButton");

        fadeOutImage = GameObject.Find("FadeOutImage");
        if (fadeOutImage != null)
        {
            fadeOutImage.SetActive(false);
        }

        errorImage = GameObject.Find("Error");
		if(errorImage != null)
        {
            errorImage.SetActive(false);
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
    public void setDifficulty(string dif)
    {
        if (dif.Equals("easy"))
        {
            maxReactorVal = easy;
            actionMax = easyMoveCount;
            difficulty = "easy";
        }
        if (dif.Equals("medium"))
        {
            maxReactorVal = medium;
            actionMax = mediumMoveCount;
            difficulty = "medium";
        }
        if (dif.Equals("hard"))
        {
            maxReactorVal = hard;
            actionMax = hardMoveCount;
            difficulty = "hard";
        }
    }


}
