using System;
using System.Collections.Generic;
using System.IO;

#if !UNITY_WEBGL
    using System.Threading;
    using System.Threading.Tasks;
#endif

using UnityEngine;

public class StateLoader : MonoBehaviour
{
    // Singleton instance.
    public static StateLoader Instance;

    #if !UNITY_WEBGL
        private CancellationTokenSource tokenSource;
        private Task saveTask;
    #endif

    private List<SaveMove> saveMoveLog;
    private bool saveMovesDisabled;
    private int movesUntilSave;
    private int movesSinceLastSave;
    private int lastSavedMove;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            #if !UNITY_WEBGL
                tokenSource = new CancellationTokenSource();
            #endif
        }
        else if (Instance != this)
        {
            throw new System.Exception("two of these scripts should not exist at the same time");
        }
    }

    private void Start()
    {
        saveMoveLog = new();
        movesUntilSave = PersistentSettings.MovesUntilSave;
        saveMovesDisabled = !PersistentSettings.SaveGameStateEnabled;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        //Debug.LogWarning("OnApplicationFocus: " + hasFocus);
        if (!hasFocus)
        {
            TryForceWriteState();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        //Debug.LogWarning("OnApplicationPause: " + pauseStatus);
        if (pauseStatus)
        {
            TryForceWriteState();
        }
    }

    void OnApplicationQuit()
    {
        //Debug.LogWarning("Application ending after " + Time.time + " seconds");
        TryForceWriteState();
    }

    public void GameStart()
    {
        saveMoveLog.Clear();
        movesSinceLastSave = 0;
    }

    public void AddMove(Move newMove)
    {
        SaveMove newSaveMove = new()
        {
            c = newMove.card.GetComponent<CardScript>().CardID,
            t = newMove.containerType,
            i = newMove.containerType switch
            {
                Constants.CardContainerType.Reactor => Array.IndexOf(GameInput.Instance.reactors, newMove.origin),
                Constants.CardContainerType.Foundation => Array.IndexOf(GameInput.Instance.foundations, newMove.origin),
                _ => 0,
            },
            m = newMove.moveType,
            h = Convert.ToByte(newMove.nextCardWasHidden),
            a = Convert.ToByte(newMove.isAction),
            r = newMove.remainingActions,
            s = newMove.score,
            n = newMove.moveNum
        };

        saveMoveLog.Add(newSaveMove);
    }

    public void RemoveMove()
    {
        saveMoveLog.RemoveAt(saveMoveLog.Count - 1);
    }

    public void SetGameStateSaving(bool enabled)
    {
        Debug.Log($"updating game state saving to: {enabled}");
        saveMovesDisabled = !enabled;
        if (enabled && movesSinceLastSave >= movesUntilSave)
        {
            TryForceWriteState();
        }
    }

    public void UpdateMovesUntilSave(int update)
    {
        Debug.Log($"updating moves until save to: {update}");
        if (update == movesUntilSave) return;
        movesUntilSave = update;
        if (movesSinceLastSave >= update)
        {
            TryForceWriteState();
        }
    }

    public void TryForceWriteState()
    {
        if (saveMovesDisabled || Config.Instance.gameOver || lastSavedMove == Config.Instance.moveCounter) return;
        Debug.Log("forcing write state");
        movesSinceLastSave = movesUntilSave;
        TryWriteState();
    }

    public void TryWriteState()
    {
        if (Config.Instance.tutorialOn) return;
        movesSinceLastSave++;
        if (saveMovesDisabled || movesSinceLastSave < movesUntilSave) return;
        movesSinceLastSave = 0;
        lastSavedMove = Config.Instance.moveCounter;
        Debug.Log("writing state");

        // if this isn't running on WebGL (no thread support)
        #if !UNITY_WEBGL
            if (saveTask != null && !saveTask.IsCompleted)
            {
                Debug.LogWarning("canceling the previous save task");
                tokenSource.Cancel();
                try
                {
                    saveTask.Wait();
                }
                // TaskCanceledException is being thrown as expected, but I can't catch it for some reason
                catch (Exception)
                {
                    Debug.LogWarning("the save task was successfully canceled");
                }
                tokenSource = new CancellationTokenSource();
                saveTask = null;
            }
        #endif

        GameState<int> gameState = new()
        {
            difficulty = Config.Instance.CurrentDifficulty.Name,
            moveCounter = Config.Instance.moveCounter,
            actions = Config.Instance.actions,
            score = Config.Instance.score,
            consecutiveMatches = Config.Instance.consecutiveMatches,

            wastePile = ConvertCardListToStringList(WastepileScript.Instance.CardList),
            deck = ConvertCardListToStringList(DeckScript.Instance.CardList),
            matches = ConvertCardListToStringList(MatchedPileScript.Instance.CardList),
            moveLog = saveMoveLog,
        };

        for (int i = 0; i < GameInput.Instance.foundationScripts.Length; i++)
        {
            foreach (GameObject card in GameInput.Instance.foundationScripts[i].CardList)
            {
                CardScript cardScript = card.GetComponent<CardScript>();
                if (cardScript.Hidden)
                {
                    gameState.foundations[i].hidden.Add(cardScript.CardID);
                }
                else
                {
                    gameState.foundations[i].unhidden.Add(cardScript.CardID);
                }
            }
        }
        for (int i = 0; i < GameInput.Instance.reactorScripts.Length; i++)
        {
            gameState.reactors[i].cards = ConvertCardListToStringList(GameInput.Instance.reactorScripts[i].CardList);
        }

        string content = JsonUtility.ToJson(gameState, Application.isEditor);

        // again, WebGL has no thread support
        #if !UNITY_WEBGL
            Debug.Log("starting the task to write the save file");
            saveTask = File.WriteAllTextAsync(SaveFile.GetPath(), content, tokenSource.Token);
        #else
            Debug.Log("writing the save file");
            File.WriteAllText(SaveFile.GetPath(), content);
        #endif
    }

    public void LoadSaveState()
    {
        Debug.Log("loading save state");

        // load the save file from the save path and unpack it
        string jsonTextFile = File.ReadAllText(SaveFile.GetPath());
        GameState<int> saveState = JsonUtility.FromJson<GameState<int>>(jsonTextFile);
        UnpackGameState(saveState);
    }

    public void LoadTutorialState(string fileName)
    {
        Debug.Log($"loading tutorial state: {fileName}");
        string filePath = Constants.Tutorial.tutorialResourcePath + fileName;

        // load the asset from resources and unpack it
        string jsonTextFile = Resources.Load<TextAsset>(filePath).ToString();
        GameState<string> tutorialState = JsonUtility.FromJson<GameState<string>>(jsonTextFile);
        UnpackGameState(tutorialState, isTutorial: true);
    }

    private void UnpackGameState<T>(GameState<T> state, bool isTutorial = false)
    {
        Debug.Log($"unpacking state");

        //set up simple variables
        Config.Instance.SetDifficulty(state.difficulty);
        ScoreScript.Instance.SetScore(state.score);
        Config.Instance.consecutiveMatches = state.consecutiveMatches;
        Config.Instance.moveCounter = state.moveCounter;
        lastSavedMove = state.moveCounter;
        // more is done at the end

        // if the tutorial isn't being loaded then we need to setup the move log
        if (LoadPileScript.Instance.CardList.Count == 0)
        {
            throw new NullReferenceException("there are no cards in the load pile when starting to load the game");
        }

        if (!isTutorial)
        {
            SetUpMoveLog(state.moveLog, LoadPileScript.Instance.CardList);
        }

        //set up foundations
        for (int i = 0; i < state.foundations.Length; i++)
        {
            SetUpLocationWithCards(state.foundations[i].hidden, Constants.CardContainerType.Foundation, GameInput.Instance.foundations[i], isHidden: true);
            SetUpLocationWithCards(state.foundations[i].unhidden, Constants.CardContainerType.Foundation, GameInput.Instance.foundations[i]);
        }

        //set up reactors
        for (int i = 0; i < state.reactors.Length; i++)
        {
            SetUpLocationWithCards(state.reactors[i].cards, Constants.CardContainerType.Reactor, GameInput.Instance.reactors[i]);
        }

        //set up wastepile
        SetUpLocationWithCards(state.wastePile, Constants.CardContainerType.WastePile, WastepileScript.Instance.gameObject);

        //set up matches
        SetUpLocationWithCards(state.matches, Constants.CardContainerType.MatchedPile, MatchedPileScript.Instance.gameObject);

        //set up deck
        if (isTutorial)
        {
            // during the tutorial the deck order doesn't matter
            int cardCount = LoadPileScript.Instance.CardList.Count;
            while (cardCount != 0)
            {
                // move from top down for efficiency, LoadPileScript.Remove() takes advantage of this
                LoadPileScript.Instance.CardList[^1].GetComponent<CardScript>().MoveCard(Constants.CardContainerType.Deck, DeckScript.Instance.gameObject, false, false, false);
                cardCount--;
            }
        }
        else
        {
            SetUpLocationWithCards(state.deck, Constants.CardContainerType.Deck, DeckScript.Instance.gameObject);
        }

        // if the game state has missing cards they will be leftover in the load pile cardlist
        if (LoadPileScript.Instance.CardList.Count != 0)
        {
            Debug.LogError("there are cards still in the load pile after loading the game");
            throw new NullReferenceException("there are cards still in the load pile after loading the game");
        }

        foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
        {
            reactorScript.SetReactorScore();
        }

        Actions.StartGameUpdate(state.actions);
        DeckScript.Instance.UpdateDeckCounter();
    }

    private List<int> ConvertCardListToStringList(List<GameObject> cardList)
    {
        List<int> newCardList = new(cardList.Count);
        foreach (GameObject card in cardList)
        {
            newCardList.Add(card.GetComponent<CardScript>().CardID);
        }
        return newCardList;
    }

    private void SetUpMoveLog(List<SaveMove> moves, List<GameObject> cardList)
    {
        saveMoveLog = moves;

        Stack<Move> newMoveLog = new();

        // going through all the saved moves and recreate them
        foreach (SaveMove saveMove in moves)
        {
            Move newMove = new()
            {
                card = cardList[saveMove.c - 1],
                containerType = saveMove.t,
                origin = saveMove.t switch
                {
                    Constants.CardContainerType.Reactor => GameInput.Instance.reactors[saveMove.i],
                    Constants.CardContainerType.Foundation => GameInput.Instance.foundations[saveMove.i],
                    Constants.CardContainerType.Deck => DeckScript.Instance.gameObject,
                    Constants.CardContainerType.WastePile => WastepileScript.Instance.gameObject,
                    Constants.CardContainerType.MatchedPile => MatchedPileScript.Instance.gameObject,
                    _ => throw new System.ArgumentException($"{saveMove.t} is not a valid saved origin")
                },
                moveType = saveMove.m,
                nextCardWasHidden = System.Convert.ToBoolean(saveMove.h),
                isAction = System.Convert.ToBoolean(saveMove.a),
                remainingActions = saveMove.r,
                score = saveMove.s,
                moveNum = saveMove.n
            };

            newMoveLog.Push(newMove);
        }

        UndoScript.Instance.SetMoveLog(newMoveLog);
    }

    private void SetUpLocationWithCards<T>(List<T> cardList, Constants.CardContainerType newContainer, GameObject newLocation, bool isHidden = false)
    {
        foreach (T cardID in cardList)
        {
            GameObject card = cardID switch
            {
                int ID => FindCardBinarySearch(ID),
                string name => LoadPileScript.Instance.CardList.Find(card => card.GetComponent<CardScript>().name == name),
                _ => null
            };

            if (card == null)
            {
                throw new KeyNotFoundException($"the card \"{cardID}\" was not found");
            }

            CardScript cardScript = card.GetComponent<CardScript>();
            cardScript.MoveCard(newContainer, newLocation, doLog: false, isAction: false);
            if (isHidden)
            {
                cardScript.Hidden = true;
            }
        }
    }

    private GameObject FindCardBinarySearch(int ID)
    {
        var list = LoadPileScript.Instance.CardList;
        int left = 0, right = list.Count - 1;
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            CardScript cs = list[mid].GetComponent<CardScript>();
            if (cs.CardID == ID)
            {
                return list[mid];
            }
            else if (cs.CardID < ID)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return null;
    }
}
