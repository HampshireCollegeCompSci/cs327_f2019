using System.Collections;
using UnityEngine;

public class Actions : MonoBehaviour
{
    private static Actions Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    public static void UpdateActions(int actionUpdate, bool setAsValue = false, bool checkGameOver = false, bool startingGame = false, bool isMatch = false)
    {
        // so that during a game start consecutiveMatches is not set to 0 after being set from a saved game
        if (!startingGame)
        {
            if (isMatch)
            {
                Config.Instance.consecutiveMatches++;
            }
            else
            {
                Config.Instance.consecutiveMatches = 0;
            }
        }

        // so that a nextcycle trigger doesn't save the state before and after, we only need the after
        bool doSaveState = true;

        // detecting if the game is starting or if a nextcycle will be triggered
        if (startingGame || (!setAsValue && ((Config.Instance.actions + actionUpdate) >= Config.Instance.CurrentDifficulty.MoveLimit)))
        {
            doSaveState = false;
        }
        else
        {
            Config.Instance.moveCounter++;
        }

        bool wasInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions <= GameValues.GamePlay.turnAlertThreshold;

        // loading a saved game triggers this
        if (startingGame)
        {
            Config.Instance.actions = actionUpdate;
        }
        // a nextcycle after it's done triggers this
        else if (setAsValue)
        {
            // if nextcycle caused a Game Over
            if (TryGameWon()) return;

            Config.Instance.actions = actionUpdate;
        }
        else if (actionUpdate == 0) // a match was made
        {
            // check if reactor alerts should be turned off
            if (wasInAlertThreshold)
            {
                Alert(false, true, true);
            }

            if (!TryGameWon()) // if a match didn't win the game
            {
                StateLoader.Instance.TryWriteState();
            }
            return;
        }
        else if (actionUpdate == -1) // a match undo
        {
            if (wasInAlertThreshold)
            {
                Alert(false, true, true);
            }
            StateLoader.Instance.TryWriteState();
            return;
        }
        else
        {
            Config.Instance.actions += actionUpdate;
        }

        ActionCountScript.Instance.UpdateActionText();

        if (Config.Instance.actions >= Config.Instance.CurrentDifficulty.MoveLimit)
        {
            StartNextCycle();
            return;
        }

        // foundation moves trigger this as they are the only ones that can cause a gameover via winning
        // reactors trigger their own gameovers
        if (checkGameOver && TryGameWon()) return;

        if (doSaveState && !Config.Instance.gameOver)
        {
            StateLoader.Instance.TryWriteState();
        }

        // time to determine if the alert should be turned on
        bool isInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions <= GameValues.GamePlay.turnAlertThreshold;
        if (!wasInAlertThreshold && !isInAlertThreshold)
        {
            // do nothing
        }
        else if (wasInAlertThreshold && isInAlertThreshold)
        {
            Alert(false, true);
        }
        else if (wasInAlertThreshold && !isInAlertThreshold)
        {
            Alert(false);
        }
        else if (!wasInAlertThreshold && isInAlertThreshold)
        {
            Alert(true);
        }
    }

    public static void MakeActionsMaxButton()
    {
        if (UtilsScript.Instance.InputStopped) return;
        Debug.Log("make actions max button");
        ActionCountScript.Instance.KnobDown();
        SoundEffectsController.Instance.VibrateMedium();
        StartNextCycle(manuallyTriggered: true);
    }

    private static void StartNextCycle(bool manuallyTriggered = false)
    {
        if (Config.Instance.tutorialOn)
        {
            if (Config.Instance.nextCycleEnabled)
            {
                Config.Instance.nextCycleEnabled = false;
            }
            else
            {
                return;
            }
        }
        UtilsScript.Instance.InputStopped = true;
        Instance.StartCoroutine(Instance.NextCycle(manuallyTriggered));
    }

    private IEnumerator NextCycle(bool manuallyTriggered)
    {
        SpaceBabyController.Instance.BabyActionCounter();

        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.CardList.Count == 0) continue;

            GameObject topFoundationCard = foundationScript.CardList[^1];
            CardScript topCardScript = topFoundationCard.GetComponent<CardScript>();
            ReactorScript reactorScript = UtilsScript.Instance.reactorScripts[topCardScript.Card.Suit.Index];

            // turn off the moving cards hologram and make it appear in front of everything
            topCardScript.Hologram = false;
            topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = Config.Instance.SelectedCardsLayer;
            topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = Config.Instance.SelectedCardsLayer;

            // immediately unhide the next possible top foundation card and start its hologram
            if (foundationScript.CardList.Count > 1)
            {
                CardScript nextTopFoundationCard = foundationScript.CardList[^2].GetComponent<CardScript>();
                if (nextTopFoundationCard.Hidden)
                {
                    nextTopFoundationCard.NextCycleReveal();
                }
            }

            yield return Animate.SmoothstepTransform(topFoundationCard.transform,
                topFoundationCard.transform.position,
                reactorScript.GetNextCardPosition(),
                GameValues.AnimationDurataions.cardsToReactor);

            // set the sorting layer back to default
            topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = Config.Instance.CardLayer;
            topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = Config.Instance.CardLayer;

            SoundEffectsController.Instance.CardToReactorSound();
            topCardScript.MoveCard(Constants.CardContainerType.Reactor, reactorScript.gameObject, isCycle: true);

            // if the game is lost during the next cycle stop immediately
            if (Config.Instance.gameOver)
            {
                Config.Instance.moveCounter += 1;
                UtilsScript.Instance.InputStopped = false;
                if (manuallyTriggered)
                {
                    ActionCountScript.Instance.KnobUp();
                }
                yield break;
            }
        }

        UtilsScript.Instance.InputStopped = false;
        if (manuallyTriggered)
        {
            ActionCountScript.Instance.KnobUp();
        }
        Actions.UpdateActions(0, setAsValue: true);
    }

    private static void Alert(bool turnOnAlert, bool checkAgain = false, bool matchRelated = false)
    {
        bool highAlertTurnedOn = false;

        // if turning on the alert for the first time
        // or checking again if the previous move changed something
        if (turnOnAlert || checkAgain)
        {
            foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
            {
                // if a nextcyle will overload the reactor
                if (reactorScript.OverLimitSoon())
                {
                    highAlertTurnedOn = true;
                }
                else if (checkAgain) // try turning the glow off just in case if it already on
                {
                    reactorScript.Alert = false;
                }
            }
        }
        else // we are done with the alert
        {
            MusicController.Instance.GameMusic();

            foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
            {
                reactorScript.Alert = false;
            }
        }

        if (turnOnAlert || checkAgain)
        {
            MusicController.Instance.AlertMusic();
        }

        // if there is one move left
        if (turnOnAlert || (checkAgain && !matchRelated && Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions == 1))
        {
            SoundEffectsController.Instance.AlertSound();
        }

        if (highAlertTurnedOn) // if the high alert was turned on during this check
        {
            // if the alert was not already on turn it on
            if (!ActionCountScript.Instance.AlertLevel.Equals(GameValues.AlertLevels.high)) 
            {
                ActionCountScript.Instance.AlertLevel = GameValues.AlertLevels.high;
                SpaceBabyController.Instance.BabyReactorHigh();
            }
            // or if there is only 1 move left now
            else if (!matchRelated && Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions == 1)
            {
                SpaceBabyController.Instance.BabyReactorHigh();
            }
        }
        else if (turnOnAlert || checkAgain)
        {
            ActionCountScript.Instance.AlertLevel = GameValues.AlertLevels.low;
        }
        else // the action counter is not low so turn stuff off
        {
            ActionCountScript.Instance.AlertLevel = GameValues.AlertLevels.none;
        }
    }

    private static bool TryGameWon()
    {
        if (!Config.Instance.gameOver && AreFoundationsEmpty())
        {
            EndGame.Instance.GameOver(true);
            return true;
        }
        return false;
    }

    private static bool AreFoundationsEmpty()
    {
        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.CardList.Count != 0)
            {
                return false;
            }
        }
        return true;
    }
}
