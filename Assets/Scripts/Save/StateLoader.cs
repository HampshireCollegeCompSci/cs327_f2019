using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StateLoader : MonoBehaviour
{
    public GameState gameState;

    // Singleton instance.
    public static StateLoader Instance;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject); //makes instance persist across scenes
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); //deletes copies of global which do not need to exist, so right version is used to get info from
        }
    }

    public void WriteState(/*string path*/)
    {
        Debug.Log("writing state");

        gameState = new GameState() {
            foundations = new List<StringListWrapper>(),
            reactors = new List<StringListWrapper>()
        };

        //save foundations
        foreach (GameObject foundation in Config.Instance.foundations)
        {
            gameState.foundations.Add(new StringListWrapper() {
                stringList = ConvertCardListToStringList(foundation.GetComponent<FoundationScript>().cardList)
            });
        }

        //save reactors
        foreach (GameObject reactor in Config.Instance.reactors)
        {
            gameState.reactors.Add(new StringListWrapper() {
                stringList = ConvertCardListToStringList(reactor.GetComponent<ReactorScript>().cardList)
            });
        }

        //save wastepile
        gameState.wastePile = ConvertCardListToStringList(WastepileScript.Instance.cardList);

        //save deck
        gameState.deck = ConvertCardListToStringList(DeckScript.Instance.cardList);

        //save matches
        gameState.matches = ConvertCardListToStringList(MatchedPileScript.Instance.cardList);

        //save undo
        CardScript cardScriptRef;
        List<AltMove> saveMoveLog = new List<AltMove>();
        foreach (Move move in UndoScript.Instance.moveLog)
        {
            cardScriptRef = move.card.GetComponent<CardScript>();

            saveMoveLog.Add(new AltMove() {
                cardName = $"{cardScriptRef.suit}_{cardScriptRef.cardNum}",
                originName = move.origin.name,
                moveType = move.moveType,
                nextCardWasHidden = move.nextCardWasHidden,
                isAction = move.isAction,
                remainingActions = move.remainingActions,
                score = move.score,
                moveNum = move.moveNum
            });
        }

        gameState.moveLog = saveMoveLog;

        //save other data
        gameState.score = Config.Instance.score;
        gameState.consecutiveMatches = Config.Instance.consecutiveMatches;
        gameState.moveCounter = Config.Instance.moveCounter;
        gameState.actions = Config.Instance.actions;
        gameState.difficulty = Config.Instance.currentDifficulty;

        //saving to json
        string json;
        if (Application.isEditor)
        {
            json = JsonUtility.ToJson(gameState, true);
            File.WriteAllText("Assets/Resources/GameStates/testState.json", json);
        }
        else
        {
            json = JsonUtility.ToJson(gameState);
            File.WriteAllText(Application.persistentDataPath + "/testState.json", json);
        }

        //UnityEditor.AssetDatabase.Refresh();
    }

    private List<string> ConvertCardListToStringList(List<GameObject> cardList)
    {
        List<string> stringList = new List<string>();
        CardScript cardScriptRef;
        
        // go backwards through the list as the top cards are at index 0
        for (int i = cardList.Count - 1; i != -1; i--)
        {
            cardScriptRef = cardList[i].GetComponent<CardScript>();
            stringList.Add($"{cardScriptRef.suit}_{cardScriptRef.cardNum}_{cardScriptRef.IsHidden}");
        }

        return stringList;
    }

    public void LoadState()
    {
        Debug.Log("loading state");

        string path;
        if (Application.isEditor)
        {
            path = "GameStates/testState";
        }
        else
        {
            path = Application.persistentDataPath + "/testState.json";
        }
        //load the json into a GameState
        gameState = CreateFromJSON(path);
    }

    public void LoadTutorialState(string fileName)
    {
        Debug.Log("loading tutorial state");

        if (!File.Exists($"Assets/Resources/Tutorial/{fileName}.json"))
        {
            throw new System.IO.FileNotFoundException($"Assets/Resources/Tutorial/{fileName}.json");
        }

        //load the json into a GameState
        gameState = CreateFromJSON($"Tutorial/{fileName}", true);
    }

    public void UnpackState(GameState state, bool isTutorial)
    {
        Debug.Log($"unpacking state, isTutorial: {isTutorial}");

        // if the tutorial isn't being loaded then we need to make new cards and setup the move log
        if (!isTutorial)
        {
            DeckScript.Instance.InstantiateCards(addToLoadPile: true);
            SetUpMoveLog(state.moveLog, LoadPileScript.Instance.cardList);
        }

        // sharing the index variable for the foundations and reactors
        int index;

        //set up foundations
        index = 0;
        foreach (StringListWrapper lw in state.foundations)
        {
            SetUpLocationWithCards(lw.stringList, LoadPileScript.Instance.cardList, Config.Instance.foundations[index]);
            index++;
        }

        //set up reactors
        index = 0;
        foreach (StringListWrapper lw in state.reactors)
        {
            SetUpLocationWithCards(lw.stringList, LoadPileScript.Instance.cardList, Config.Instance.reactors[index]);
            index++;
        }

        //set up wastepile
        SetUpLocationWithCards(state.wastePile, LoadPileScript.Instance.cardList, WastepileScript.Instance.gameObject);

        //set up matches
        SetUpLocationWithCards(state.matches, LoadPileScript.Instance.cardList, MatchedPileScript.Instance.gameObject);

        //set up deck
        if (!isTutorial)
        {
            SetUpLocationWithCards(state.deck, LoadPileScript.Instance.cardList, DeckScript.Instance.gameObject);
        }
        else
        {
            // during the tutorial the deck order doesn't matter
            int cardCount = LoadPileScript.Instance.cardList.Count;
            while (cardCount != 0)
            {
                LoadPileScript.Instance.cardList[0].GetComponent<CardScript>().MoveCard(DeckScript.Instance.gameObject, false, false, false);
                cardCount--;
            }
        }

        //set up simple variables
        Config.Instance.currentDifficulty = state.difficulty;
        Config.Instance.score = state.score;
        Config.Instance.consecutiveMatches = state.consecutiveMatches;
        Config.Instance.moveCounter = state.moveCounter;
        UtilsScript.Instance.UpdateActions(state.actions, startingGame: true);
    }

    private void SetUpMoveLog(List<AltMove> moves, List<GameObject> cardList)
    {
        //set up undo log

        // creating a list of card containing objects to reference from later
        List<GameObject> origins = new List<GameObject>()
        {
            DeckScript.Instance.gameObject,
            WastepileScript.Instance.gameObject,
            MatchedPileScript.Instance.gameObject
        };
        origins.AddRange(Config.Instance.foundations);
        origins.AddRange(Config.Instance.reactors);

        // variables that will be used repeatedly when setting up the move log
        Move tempMove;
        string[] segments;
        string suite;
        int number;

        CardScript cardScriptRef;

        // going backwards through all the saved moves and recreate them
        for (int i = moves.Count - 1; i != -1; i--)
        {
            tempMove = new Move();

            // extracting move info relating to the specific card
            segments = moves[i].cardName.Split('_');
            suite = segments[0];
            number = System.Int32.Parse(segments[1]);

            // looking for the card that the move is associated with
            foreach (GameObject card in cardList)
            {
                cardScriptRef = card.GetComponent<CardScript>();
                if (cardScriptRef.cardNum == number && cardScriptRef.suit == suite)
                {
                    tempMove.card = card;
                    break;
                }
            }

            // looking for the move's card containg object
            foreach (GameObject origin in origins)
            {
                if (origin.name == moves[i].originName)
                {
                    tempMove.origin = origin;
                    break;
                }
            }

            if (tempMove.card == null)
            {
                throw new System.NullReferenceException($"card \"{moves[i].cardName}\" was not found");
            }
            if (tempMove.origin == null)
            {
                throw new System.NullReferenceException($"origin \"{moves[i].originName}\" was not found");
            }

            // other variables
            tempMove.isAction = moves[i].isAction;
            tempMove.moveType = moves[i].moveType;
            tempMove.nextCardWasHidden = moves[i].nextCardWasHidden;
            tempMove.remainingActions = moves[i].remainingActions;
            tempMove.score = moves[i].score;
            tempMove.moveNum = moves[i].moveNum;

            // making the move official
            UndoScript.Instance.moveLog.Push(tempMove);
        }
    }

    private void SetUpLocationWithCards(List<string> stringList, List<GameObject> cardList, GameObject newLocation)
    {
        // seting up the new location with cards using the given commands, and cards

        // variables that will be used repeatedly when setting up the location
        string[] segments;
        string suite;
        int number;
        bool hiddenState;

        bool foundCard;
        CardScript cardScriptRef;

        foreach (string s in stringList)
        {
            // extracting info relating to the specific card
            segments = s.Split('_');
            suite = segments[0];
            number = System.Int32.Parse(segments[1]);
            hiddenState = bool.Parse(segments[2]);

            // looking for the card that needs to be moved
            foundCard = false;
            foreach (GameObject card in cardList)
            {
                cardScriptRef = card.GetComponent<CardScript>();
                if (cardScriptRef.cardNum == number && cardScriptRef.suit == suite)
                {
                    cardScriptRef.MoveCard(newLocation, false, false, false);
                    if (hiddenState)
                    {
                        cardScriptRef.SetFoundationVisibility(false);
                    }

                    foundCard = true;
                    break;
                }
            }

            if (!foundCard)
            {
                throw new System.NullReferenceException($"card \"{s}\" was not found");
            }
        }
    }
       
    private GameState CreateFromJSON(string path, bool tutorial = false)
    {
        Debug.Log("creating gamestate from path: " + path);

        if (tutorial)
        {
            var jsonTextFile = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<GameState>(jsonTextFile.ToString());
        }
        else if (Application.isEditor)
        {
            var jsonTextFile = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<GameState>(jsonTextFile.ToString());
        }
        else
        {
            var jsonTextFile = File.ReadAllText(path);
            return JsonUtility.FromJson<GameState>(jsonTextFile);
        }
    }
}
