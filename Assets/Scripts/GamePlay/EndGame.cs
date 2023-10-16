using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    // Singleton instance.
    public static EndGame Instance;
    private static readonly WaitForSeconds explosionWait = new(0.5f),
        explosionDelay = new(GameValues.AnimationDurataions.reactorExplosionDelay),
        cardDelay = new(0.2f);

    [SerializeField]
    private GameObject explosionPrefab;
    [SerializeField]
    private GameObject gameEndButton, restartButton, continueButton,
        errorObject, gameOverPanel, gameOverTextPanel;
    [SerializeField]
    private Text gameOverText, wonlostText;

    private bool _gameCanEnd;
    public bool GameCanEnd
    {
        get => _gameCanEnd;
        set
        {
            if (_gameCanEnd == value) return;
            _gameCanEnd = value;
            gameEndButton.SetActive(value);
        }
    }

    private void Awake()
    {
        // Initialize the singleton instance.
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new System.Exception("two of these scripts should not exist at the same time");
        }
    }

    public void ManualGameWinButton()
    {
        if (GameInput.Instance.InputStopped) return;
        if (!GameCanEnd)
        {
            Debug.LogWarning("trying to end the game when it should not be possible");
            gameEndButton.SetActive(false);
            return;
        }
        GameOver(true);
    }

    public void GameOver(bool didWin)
    {
        Debug.Log($"Game Over, won: {didWin}");

        GameInput.Instance.InputStopped = true;
        Actions.GameOver = true;
        Actions.GameWin = didWin;
        GameCanEnd = false;

        Timer.PauseWatch();
        SaveFile.Delete();

        Actions.MatchCounter = Config.Instance.CurrentDifficulty.Equals(Difficulties.cheat) ?
            GameValues.GamePlay.matchCount : MatchedPileScript.Instance.CardList.Count / 2;

        Config.Instance.PreserveOldStats();
        Stats.SaveResults(didWin);

        if (didWin)
        {
            gameOverText.color = Config.Instance.CurrentColorMode.Match.Color;
            wonlostText.color = Config.Instance.CurrentColorMode.Match.Color;
            wonlostText.text = GameValues.Text.gameWon;
            SoundEffectsController.Instance.WinSound();
            foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
            {
                foundationScript.GlowForGameEnd(true);
            }
            AchievementsManager.GameWinLogAchievements();
        }
        else
        {
            errorObject.SetActive(true);
            gameOverText.color = Config.Instance.CurrentColorMode.Over.Color;
            wonlostText.color = Config.Instance.CurrentColorMode.Over.Color;
            wonlostText.text = GameValues.Text.gameLost;
            SoundEffectsController.Instance.LoseSound();
            foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
            {
                if (reactorScript.TryHighlightOverloaded(true)) break;
            }
        }

        StartCoroutine(BeginGameOverTransition(didWin));
    }

    private IEnumerator BeginGameOverTransition(bool didWin)
    {
        gameOverPanel.SetActive(true);
        MusicController.Instance.FadeMusicOut();

        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        CanvasGroup textGroup = gameOverTextPanel.GetComponent<CanvasGroup>();
        Color fadeColor = didWin ? GameValues.FadeColors.grayA1 : GameValues.FadeColors.blackA1;

        fadeInScreen.enabled = true;
        gameOverTextPanel.SetActive(true);

        float duration = GameValues.AnimationDurataions.gameOverFade;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            fadeColor.a = Mathf.Lerp(0, 0.4f, t);
            fadeInScreen.color = fadeColor;
            textGroup.alpha = Mathf.Lerp(0, 1, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fadeColor.a = 0.4f;
        fadeInScreen.color = fadeColor;
        textGroup.alpha = 1;

        if (didWin)
        {
            SpaceBabyController.Instance.BabyHappy();
            MusicController.Instance.WinMusic();
        }
        else
        {
            SpaceBabyController.Instance.BabyLoseTransition();
            MusicController.Instance.LoseMusic();
        }

        restartButton.SetActive(true);
        continueButton.SetActive(true);
    }

    public void RestartGameWhenOver()
    {
        restartButton.SetActive(false);
        continueButton.SetActive(false);
        errorObject.SetActive(false);
        gameOverTextPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        SpaceBabyController.Instance.ResetBaby();

        if (Actions.GameWin)
        {
            foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
            {
                foundationScript.GlowForGameEnd(false);
            }
        }
        else
        {
            foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
            {
                reactorScript.TryHighlightOverloaded(false);
            }
        }

        MusicController.Instance.GameMusic();
        Actions.GameOver = false;
        GameLoader.Instance.RestartGame();
        GameInput.Instance.InputStopped = false;
        this.gameObject.GetComponent<Image>().enabled = false;
    }

    public void ContinueGameOverTransition()
    {
        restartButton.SetActive(false);
        continueButton.SetActive(false);

        if (Actions.GameWin)
        {
            SoundEffectsController.Instance.WinTransition();
            StartCoroutine(FadeGameplayOut(true));
        }
        else
        {
            StartCoroutine(ReactorsMeltdown());
        }
    }

    private IEnumerator ReactorsMeltdown()
    {
        // wait for dramatic effect
        yield return explosionWait;
        foreach (ReactorScript reactorScript in GameInput.Instance.reactorScripts)
        {
            if (reactorScript.CardList.Count != 0)
            {
                StartCoroutine(ReactorMeltdown(reactorScript));
                yield return explosionDelay;
            }
        }
        StartCoroutine(FadeGameplayOut(false));
    }

    private IEnumerator ReactorMeltdown(ReactorScript reactorScript)
    {
        foreach (GameObject card in reactorScript.CardList)
        {
            GameObject matchExplosion = Instantiate(explosionPrefab, card.transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            matchExplosion.transform.localScale = new Vector3(GameValues.Transforms.matchExplosionScale, GameValues.Transforms.matchExplosionScale);
        }
        yield return cardDelay;
        foreach (GameObject card in reactorScript.CardList)
        {
            card.SetActive(false);
        }
        SoundEffectsController.Instance.ExplosionSound();

        GameObject reactorExplosion = Instantiate(explosionPrefab, reactorScript.gameObject.transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        reactorExplosion.transform.localScale = new Vector3(GameValues.Transforms.matchExplosionScale / 2, GameValues.Transforms.matchExplosionScale / 2);
        reactorExplosion.GetComponent<Animator>().Play(Constants.AnimatorIDs.loseExplosionID);
    }

    private IEnumerator FadeGameplayOut(bool won)
    {
        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        Color fadeColor = fadeInScreen.color;
        CanvasGroup textGroup = gameOverTextPanel.GetComponent<CanvasGroup>();

        float startingAlpha = fadeColor.a;
        float duration = won ? GameValues.AnimationDurataions.gameEndWonFade :
            GameValues.AnimationDurataions.gameEndLostFade;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            fadeColor.a = Mathf.Lerp(startingAlpha, 1, t);
            fadeInScreen.color = fadeColor;
            textGroup.alpha = Mathf.Lerp(1, 0, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fadeColor.a = 1;
        fadeInScreen.color = fadeColor;
        textGroup.alpha = 0;

        TransitionToSummaryScene();
    }

    private void TransitionToSummaryScene()
    {
        SceneManager.LoadScene(Constants.ScenesNames.summary);
        Config.Instance.IsGamePlayActive = false;
    }
}
