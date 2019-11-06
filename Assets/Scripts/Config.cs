using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

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
    public float delayToShowGameSummary;
    public float countdown;
    public float draggedTokenOffset;
    public float selectedCardOpacity;
    public bool prettyColors;

    //score
    public int matchPoints;
    public int emptyReactorPoints;
    public int perfectGamePoints;

    //card scale
    public Vector3 cardScale;


    //foundations
    public float foundationStackDensity;
    public int foundationStartSize;

    //wastepile
    public float nonTopXOffset = 0.3f * 0.25F; // foundationStackDensity * 0.25
    public int wastepileCardsToShow;

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

    private GameObject[] foundationList;

    public GameObject wastePile;

    //deck
    public GameObject deck;
    public int cardsToDeal;

    //UI
    public bool gamePaused;

    //internal variables
    private int foundationCount = 0;
    private string JSON;
    GameInfo gameInfo;

    public int easy;
    public int medium;
    public int hard;

    public int easyMoveCount;
    public int mediumMoveCount;
    public int hardMoveCount;

    public int actionMax;
    public int actions;


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
        countdown = delayToShowGameSummary;
        gameObject.GetComponent<MusicController>().MainMenuMusic();
    }


    private void Update()
    {
        //handle game end
        if (gameOver && SceneManager.GetActiveScene().name != "SummaryScene")
        {
            //delay to show summary
            if (countdown < 0)
            {
                SceneManager.LoadScene("SummaryScene");
                if (gameWin)
                {
                    gameObject.GetComponent<MusicController>().WinMusic();
                }
                else
                {
                    gameObject.GetComponent<MusicController>().LoseMusic();
                }
                countdown = delayToShowGameSummary;
            }
            else
            {
                countdown -= Time.deltaTime;
            }
        }

    }

    public void ConfigFromJSON()
    {
        wastepileCardsToShow = gameInfo.wastepileCardsToShow;
        foundationStartSize = gameInfo.foundationStartingSize;
        nonTopXOffset = foundationStackDensity * gameInfo.nonTopXOffset;
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

        wastePile = GameObject.Find("WastePile");

        deck = GameObject.Find("DeckButton");

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
        if (dif.Equals("easy")){
            maxReactorVal = easy;
            actionMax = easyMoveCount;
        }
        if (dif.Equals("medium"))
        {
            maxReactorVal = medium;
            actionMax = mediumMoveCount;
        }
        if (dif.Equals("hard"))
        {
            maxReactorVal = hard;
            actionMax = hardMoveCount;
        }
    }


}
