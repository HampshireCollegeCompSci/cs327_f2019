public static class Actions
{
    public static int ActionsDone { get; set; }
    public static int MoveCounter { get; set; }
    public static int MoveTracker { get; set; }
    public static int MatchCounter { get; set; }
    public static int ConsecutiveMatches { get; set; }

    public static bool GameOver { get; set; }
    public static bool GameWin { get; set; }
    public static int Score { get; set; }

    public static void ResetValues()
    {
        ActionsDone = 0;
        MoveCounter = 0;
        MoveTracker = 0;
        MatchCounter = 0;
        ConsecutiveMatches = 0;
        GameOver = false;
        GameWin = false;
        Score = 0;
    }

    public static void MatchUpdate(bool tryGameWon)
    {
        ConsecutiveMatches++;
        MoveCounter++;
        MoveTracker++;

        AchievementsManager.TryTripleCombo();
        Stats.TryUpdateHighestCombo();

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
        ConsecutiveMatches = 0;
        MoveCounter++;
        AchievementsManager.TryRemoveAchievement(--MoveTracker);
        StateLoader.Instance.TryWriteState();
        TryDisableGameCanEnd();
        Alert(IsInAlertThreshold(), isMatchRelated: true);
    }

    public static void MoveUpdate(bool checkGameOver = false)
    {
        ConsecutiveMatches = 0;
        MoveCounter++;
        MoveTracker++;

        bool wasInAlertThreshold = IsInAlertThreshold();
        ActionsDone++;
        ActionCountScript.Instance.UpdateActionText();

        if (ActionsDone >= Config.Instance.CurrentDifficulty.MoveLimit)
        {
            MoveCounter--; // needed to register this cycle's moves as a part of this move
            AchievementsManager.FailedNeverMoves();
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

        if (!Actions.GameOver)
        {
            StateLoader.Instance.TryWriteState();
        }

        Alert(wasInAlertThreshold);
    }

    public static void StartSavedGameUpdate(int actionUpdate)
    {
        bool wasInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - ActionsDone <= GameValues.GamePlay.turnAlertThreshold;
        ActionsDone = actionUpdate;
        ActionCountScript.Instance.UpdateActionText();
        Alert(wasInAlertThreshold);
        if (Config.Instance.TutorialOn) return;
        TryEnableGameCanEnd();
    }

    public static void StartNewGameUpdate()
    {
        bool wasInAlertThreshold = Config.Instance.CurrentDifficulty.MoveLimit - ActionsDone <= GameValues.GamePlay.turnAlertThreshold;
        ActionsDone = 0;
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
        AchievementsManager.TryRemoveAchievement(--MoveTracker);
        TypicalUpdate(actionUpdate);
    }

    public static void NextCycleUpdate()
    {
        TryEnableGameCanEnd();
        TypicalUpdate(0);
    }

    private static void TypicalUpdate(int actionUpdate)
    {
        ConsecutiveMatches = 0;
        MoveCounter++;
        bool wasInAlertThreshold = IsInAlertThreshold();
        ActionsDone = actionUpdate;
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
            if (CheckReactorsNearOverLimit())
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
            bool reactorsNearOverLimit = CheckReactorsNearOverLimit();
            bool currentlyHighAlertLevel = ActionCountScript.Instance.AlertLevel.ColorLevel == Constants.ColorLevel.Over;
            bool oneMoveBeforeNextCycle = Config.Instance.CurrentDifficulty.MoveLimit - ActionsDone == 1;

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

    private static bool CheckReactorsNearOverLimit()
    {
        int numOverLimit = 0;
        foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
        {
            if (reactorScript.OverLimitSoon())
            {
                numOverLimit++;
                reactorScript.Alert = true;
            }
            else
            {
                reactorScript.Alert = false;
            }
        }

        if (numOverLimit != 0)
        {
            AchievementsManager.FailedNeverReactorHighAlert();
            if (numOverLimit == 4) AchievementsManager.AchievedAllReactorsHighAlert();
        }
        return numOverLimit != 0;
    }

    private static void TryEnableGameCanEnd()
    {
        if (!EndGame.Instance.GameCanEnd && !GameOver && AreFoundationsEmpty())
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
        Config.Instance.CurrentDifficulty.MoveLimit - ActionsDone <= GameValues.GamePlay.turnAlertThreshold;
}
