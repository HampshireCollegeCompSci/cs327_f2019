public static class Actions
{
    public static void MatchUpdate(bool tryGameWon)
    {
        Config.Instance.consecutiveMatches++;
        Config.Instance.moveCounter++;

        if (tryGameWon || EndGame.Instance.GameCanEnd)
        {
            bool allCardsHaveBeenMatched = MatchedPileScript.Instance.CardList.Count == GameValues.GamePlay.cardCount;
            if (allCardsHaveBeenMatched)
            {
                EndGame.Instance.GameOver(true);
                return;
            }
            TryEnableGameCanEnd();
        }
        StateLoader.Instance.TryWriteState();
        Alert(IsInAlertThreshold(), isMatchRelated: true);
    }

    public static void MatchUndoUpdate()
    {
        Config.Instance.consecutiveMatches = 0;
        Config.Instance.moveCounter++;
        StateLoader.Instance.TryWriteState();
        TryDisableGameCanEnd();
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
            Config.Instance.moveCounter--; // needed to register this cycle's moves as a part of this move
            NextCycle.Instance.StartCycle();
            return;
        }

        // foundation moves trigger this as they are the only ones that can cause a gameover via winning
        // reactors trigger their own gameovers
        if (checkGameOver)
        {
            TryEnableGameCanEnd();
        }
        else
        {
            TryDisableGameCanEnd();
        }

        if (!Config.Instance.gameOver)
        {
            StateLoader.Instance.TryWriteState();
        }

        Alert(wasInAlertThreshold);
    }

    public static void StartSavedGameUpdate(int actionUpdate)
    {
        bool wasInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions <= GameValues.GamePlay.turnAlertThreshold;
        Config.Instance.actions = actionUpdate;
        ActionCountScript.Instance.UpdateActionText();
        Alert(wasInAlertThreshold);
        TryEnableGameCanEnd();
    }

    public static void StartNewGameUpdate()
    {
        bool wasInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - Config.Instance.actions <= GameValues.GamePlay.turnAlertThreshold;
        Config.Instance.actions = 0;
        ActionCountScript.Instance.UpdateActionText();
        Alert(wasInAlertThreshold);
    }

    public static void UndoUpdate(int actionUpdate, bool checkGameOver = false)
    {
        if (checkGameOver)
        {
            TryEnableGameCanEnd();
        }
        else
        {
            TryDisableGameCanEnd();
        }
        TypicalUpdate(actionUpdate);
    }

    public static void NextCycleUpdate()
    {
        TryEnableGameCanEnd();
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

    private static void TryEnableGameCanEnd()
    {
        if (!EndGame.Instance.GameCanEnd && !Config.Instance.gameOver && AreFoundationsEmpty())
        {
            EndGame.Instance.GameCanEnd = true;
        }
    }

    private static void TryDisableGameCanEnd()
    {
        if (EndGame.Instance.GameCanEnd && !AreFoundationsEmpty())
        {
            EndGame.Instance.GameCanEnd = false;
        }
    }

    public static bool AreFoundationsEmpty()
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
