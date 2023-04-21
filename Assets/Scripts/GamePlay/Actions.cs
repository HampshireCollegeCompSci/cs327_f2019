using UnityEngine;

public static class Actions
{
    public static void MakeActionsMaxButton()
    {
        if (GameInput.Instance.InputStopped) return;
        Debug.Log("make actions max button");
        ActionCountScript.Instance.KnobDown();
        SoundEffectsController.Instance.VibrateMedium();
        NextCycle.Instance.StartCycle(manuallyTriggered: true);
    }

    public static void MatchUpdate(bool tryGameWon)
    {
        Config.Instance.consecutiveMatches++;
        Config.Instance.moveCounter++;

        if (!(tryGameWon && TryGameWon())) // if a match didn't win the game
        {
            Debug.LogWarning(tryGameWon);
            StateLoader.Instance.TryWriteState();
        }
        Alert(IsInAlertThreshold(), isMatchRelated: true);
    }

    public static void MatchUndoUpdate()
    {
        Config.Instance.consecutiveMatches = 0;
        Config.Instance.moveCounter++;
        StateLoader.Instance.TryWriteState();
        Alert(IsInAlertThreshold(), isMatchRelated: true);
    }

    public static void MoveUpdate(bool checkGameOver = false)
    {
        Config.Instance.consecutiveMatches = 0;
        Config.Instance.moveCounter++;

        bool wasInAlertThreshold = IsInAlertThreshold();
        Config.Instance.actions++;
        ActionCountScript.Instance.UpdateActionText();

        if (Config.Instance.actions >= Config.Instance.CurrentDifficulty.MoveLimit)
        {
            NextCycle.Instance.StartCycle();
            return;
        }

        // foundation moves trigger this as they are the only ones that can cause a gameover via winning
        // reactors trigger their own gameovers
        if (checkGameOver && TryGameWon()) return;

        if (!Config.Instance.gameOver)
        {
            StateLoader.Instance.TryWriteState();
        }

        Alert(wasInAlertThreshold);
    }

    public static void StartGameUpdate(int actionUpdate = 0)
    {
        bool wasInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions <= GameValues.GamePlay.turnAlertThreshold;
        Config.Instance.actions = actionUpdate;
        ActionCountScript.Instance.UpdateActionText();
        Alert(wasInAlertThreshold);
    }

    public static void UndoUpdate(int actionUpdate)
    {
        TypicalUpdate(actionUpdate);
    }

    public static void NextCycleUpdate()
    {
        // if nextcycle caused a Game Win
        if (TryGameWon())
        {
            Config.Instance.moveCounter++;
            return;
        }
        TypicalUpdate(0);
    }

    private static void TypicalUpdate(int actionUpdate)
    {
        Config.Instance.consecutiveMatches = 0;
        Config.Instance.moveCounter++;
        bool wasInAlertThreshold = IsInAlertThreshold();
        Config.Instance.actions = actionUpdate;
        ActionCountScript.Instance.UpdateActionText();
        StateLoader.Instance.TryWriteState();
        Alert(wasInAlertThreshold);
    }

    private static void Alert(bool wasInAlert, bool isMatchRelated = false)
    {
        // rules for alerts
        // the alert sound effect only plays when the new remaining moves left is equal to the turnAlertThreshold, or when there is only one move left before a next cycle
        // the baby sound effect only plays when any reactor initially goes into high alert (will be over limit if a next cycle occurs)
        // the music will only transition when the move counter is reset from 0 or when it reaches the turnAlertThreshold

        bool isInAlert = IsInAlertThreshold();
        if (!wasInAlert && !isInAlert) return; // no alert

        if (wasInAlert && !isInAlert) // no more alert
        {
            foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
            {
                reactorScript.Alert = false;
            }
            MusicController.Instance.GameMusic();
            ActionCountScript.Instance.AlertLevel = GameValues.Colors.normal;
        }
        else if (!wasInAlert && isInAlert) // new alert
        {
            bool reactorsNearOverLimit = CheckReactors();
            if (reactorsNearOverLimit)
            {
                SpaceBabyController.Instance.BabyReactorHigh();
                ActionCountScript.Instance.AlertLevel = Config.Instance.CurrentColorMode.Over;
            }
            else
            {
                ActionCountScript.Instance.AlertLevel = Config.Instance.CurrentColorMode.Move;
            }

            MusicController.Instance.AlertMusic();
            SoundEffectsController.Instance.AlertSound();
        }
        else // already in alert
        {
            bool reactorsNearOverLimit = CheckReactors();
            bool currentlyHighAlertLevel = ActionCountScript.Instance.AlertLevel.ColorLevel == Constants.ColorLevel.Over;
            bool oneMoveBeforeNextCycle = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions == 1;

            if (oneMoveBeforeNextCycle && !isMatchRelated)
            {
                SoundEffectsController.Instance.AlertSound();
            }

            if (reactorsNearOverLimit && !currentlyHighAlertLevel)
            {
                SpaceBabyController.Instance.BabyReactorHigh();
            }

            ActionCountScript.Instance.AlertLevel = reactorsNearOverLimit ?
                Config.Instance.CurrentColorMode.Over : Config.Instance.CurrentColorMode.Move;
        }
    }

    private static bool CheckReactors()
    {
        bool overLimitSoon = false;
        foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
        {
            if (reactorScript.OverLimitSoon())
            {
                overLimitSoon = true;
                reactorScript.Alert = true;
            }
            else
            {
                reactorScript.Alert = false;
            }
        }
        return overLimitSoon;
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
        foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
        {
            if (foundationScript.CardList.Count != 0)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsInAlertThreshold() =>
        Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions <= GameValues.GamePlay.turnAlertThreshold;
}
