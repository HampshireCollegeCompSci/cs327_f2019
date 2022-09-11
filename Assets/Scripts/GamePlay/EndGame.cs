using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public GameObject restartButton, continueButton;
    public GameObject errorObject;
    public GameObject gameOverPanel;
    public GameObject gameOverTextPanel;
    public Text gameOverText;
    public Text wonlostText;

    // Singleton instance.
    public static EndGame Instance = null;

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

    public void ManualGameWin()
    {
        if (!Config.GameValues.enableCheat || Config.Instance.gamePaused) return;

        GameOver(true);
        // to ensure that the full win animation plays for debug
        Config.Instance.matchCounter = 26;
    }

    public void GameOver(bool didWin)
    {
        Debug.Log($"Game Over, won: {didWin}");

        Config.Instance.gameOver = true;
        Config.Instance.gamePaused = true;
        Config.Instance.gameWin = didWin;

        SaveState.Delete();

        if (Config.GameValues.enableBonusPoints)
        {
            AddExtraEndGameScore();
        }

        // overwritten when manually won (cheated)
        Config.Instance.matchCounter = (byte)(MatchedPileScript.Instance.cardList.Count / 2);

        if (didWin)
        {
            foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
            {
                foundationScript.GlowForGameEnd(true);
            }
        }
        else
        {
            foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
            {
                reactorScript.TryHighlightOverloaded(true);
            }
        }

        StartCoroutine(BeginGameOverTransition());
    }

    private void AddExtraEndGameScore()
    {
        int extraScore = 0;
        if (MatchedPileScript.Instance.cardList.Count == 52)
        {
            extraScore += Config.GameValues.perfectGamePoints;
        }

        foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
        {
            if (foundationScript.cardList.Count == 0)
            {
                extraScore += Config.GameValues.emptyReactorPoints;
            }
        }

        UtilsScript.Instance.UpdateScore(extraScore);
    }

    private IEnumerator BeginGameOverTransition()
    {
        gameOverPanel.SetActive(true);
        MusicController.Instance.FadeMusicOut();

        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        CanvasGroup textGroup = gameOverTextPanel.GetComponent<CanvasGroup>();
        Color fadeColor;

        if (Config.Instance.gameWin)
        {
            fadeColor = Config.GameValues.fadeLightColor;
            SoundEffectsController.Instance.WinSound();
            gameOverText.color = Color.cyan;
            wonlostText.color = Color.cyan;
            wonlostText.text = "YOU WON";
        }
        else
        {
            fadeColor = Config.GameValues.fadeDarkColor;
            errorObject.SetActive(true);
            SoundEffectsController.Instance.LoseSound();
            gameOverText.color = Color.red;
            wonlostText.color = Color.red;
            wonlostText.text = "YOU LOST";
        }

        fadeColor.a = 0;
        fadeInScreen.color = fadeColor;
        fadeInScreen.enabled = true;
        textGroup.alpha = 0;
        gameOverTextPanel.SetActive(true);
        yield return null;

        while (textGroup.alpha < 1)
        {
            fadeColor.a += Time.deltaTime * 0.3f;
            fadeInScreen.color = fadeColor;
            textGroup.alpha += Time.deltaTime;
            yield return null;
        }

        if (Config.Instance.gameWin)
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

    public void RestartGame()
    {
        SoundEffectsController.Instance.ButtonPressSound();
        restartButton.SetActive(false);
        continueButton.SetActive(false);
        gameOverTextPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        SpaceBabyController.Instance.ResetBaby();

        if (Config.Instance.gameWin)
        {
            // save the results
            PlayerPrefKeys.TrySetHighScore(Config.Instance.currentDifficulty, Config.Instance.score);
            PlayerPrefKeys.TrySetLeastMoves(Config.Instance.currentDifficulty, Config.Instance.moveCounter);

            foreach (FoundationScript foundationScript in UtilsScript.Instance.foundationScripts)
            {
                foundationScript.GlowForGameEnd(false);
            }
        }
        else
        {
            foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
            {
                reactorScript.TryHighlightOverloaded(false);
            }
        }

        MusicController.Instance.GameMusic();
        Config.Instance.gameOver = false;
        GameLoader.Instance.RestartGame();
        this.gameObject.GetComponent<Image>().enabled = false;
    }

    public void ContinueGameOverTransition()
    {
        SoundEffectsController.Instance.ButtonPressSound();
        restartButton.SetActive(false);
        continueButton.SetActive(false);

        if (Config.Instance.gameWin)
        {
            SoundEffectsController.Instance.WinTransition();
            StartCoroutine(FadeGameplayOut());
        }
        else
        {
            StartCoroutine(ReactorsMeltdown());
        }
    }

    private IEnumerator ReactorsMeltdown()
    {
        // wait for dramatic effect
        yield return new WaitForSeconds(0.5f);
        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            if (reactorScript.cardList.Count != 0)
            {
                StartCoroutine(ReactorMeltdown(reactorScript));
                yield return new WaitForSeconds(Config.GameValues.reactorMeltDownSpeed);
            }
        }

        StartCoroutine(FadeGameplayOut());
    }

    private IEnumerator ReactorMeltdown(ReactorScript reactorScript)
    {
        foreach (GameObject card in reactorScript.cardList)
        {
            GameObject matchExplosion = Instantiate(UtilsScript.Instance.matchPrefab, card.transform.position, Quaternion.identity);
            matchExplosion.transform.localScale = new Vector3(Config.GameValues.matchExplosionScale, Config.GameValues.matchExplosionScale);
        }
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject card in reactorScript.cardList)
        {
            card.SetActive(false);
        }
        SoundEffectsController.Instance.ExplosionSound();

        GameObject reactorExplosion = Instantiate(UtilsScript.Instance.matchPrefab, reactorScript.gameObject.transform.position, Quaternion.identity);
        reactorExplosion.transform.localScale = new Vector3(Config.GameValues.matchExplosionScale / 2, Config.GameValues.matchExplosionScale / 2);
        reactorExplosion.GetComponent<Animator>().Play("LoseExplosionAnim");
    }

    private IEnumerator FadeGameplayOut()
    {
        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        Color fadeColor = fadeInScreen.color;
        CanvasGroup textGroup = gameOverTextPanel.GetComponent<CanvasGroup>();

        while (textGroup.alpha > 0)
        {
            float alphaChange = Time.deltaTime * Config.GameValues.endGameFadeOutSpeed;
            fadeColor.a += alphaChange;
            fadeInScreen.color = fadeColor;
            textGroup.alpha -= alphaChange;
            yield return null;
        }

        TransitionToSummaryScene();
    }

    private void TransitionToSummaryScene()
    {
        SceneManager.LoadScene(Constants.summaryScene);
    }
}
