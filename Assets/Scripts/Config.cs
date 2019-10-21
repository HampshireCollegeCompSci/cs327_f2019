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
    public float relativeCardScale;

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

    private GameObject[] foundationList;

    public GameObject wastePile;

    //deck
    public GameObject deck;
    public int cardsToDeal;

    //internal variables
    private int foundationCount = 0;
    private string JSON;
    GameInfo gameInfo;



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
    }


    private void Update()
    {
        //handle game end
        if (gameOver && SceneManager.GetActiveScene().name != "SummaryScene")
        {
            SceneManager.LoadScene("SummaryScene");
        }

    }

    public void ConfigFromJSON()
    {
        wastepileCardsToShow = gameInfo.wastepileCardsToShow;
        foundationStartSize = gameInfo.foundationStartingSize;
        maxReactorVal = gameInfo.reactorLimit;
        nonTopXOffset = foundationStackDensity * gameInfo.nonTopXOffset;
        print(nonTopXOffset);
        cardsToDeal = gameInfo.cardsToDeal;
        relativeCardScale = gameInfo.relativeCardScale;
    }

    public void SetCards()
    {
        foundation1 = GameObject.Find("Foundation (0)");
        foundation2 = GameObject.Find("Foundation (1)");
        foundation3 = GameObject.Find("Foundation (2)");
        foundation4 = GameObject.Find("Foundation (3)");
        wastePile = GameObject.Find("WastePile");
        deck = GameObject.Find("Deck");
        foundationList = new GameObject[] { foundation1, foundation2, foundation3, foundation4 };
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
        return foundation1.GetComponent<FoundationScript>().cardList.Count + foundation2.GetComponent<FoundationScript>().cardList.Count +
            foundation3.GetComponent<FoundationScript>().cardList.Count + foundation4.GetComponent<FoundationScript>().cardList.Count;
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

}
