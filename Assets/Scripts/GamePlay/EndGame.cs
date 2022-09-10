using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public GameObject errorObject;
    public GameObject gameOverPanel;
    public GameObject gameOverTextPanel;
    public Text gameOverText;
    public Text wonlostText;
    public Text continueText;

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
                foundationScript.GlowForGameEnd();
            }
        }
        else
        {
            foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
            {
                reactorScript.TryHighlightOverloaded();
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
        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        Color fadeColor;

        if (Config.Instance.gameWin)
        {
            fadeColor = Config.GameValues.fadeLightColor;
            SoundEffectsController.Instance.WinSound();
        }
        else
        {
            fadeColor = Config.GameValues.fadeDarkColor;
            errorObject.SetActive(true);
            SoundEffectsController.Instance.LoseSound();
        }

        fadeColor.a = 0;
        fadeInScreen.color = fadeColor;
        fadeInScreen.enabled = true;
        yield return null;

        while (fadeColor.a < 0.3)
        {
            fadeColor.a += Time.deltaTime * 0.3f;
            fadeInScreen.color = fadeColor;
            yield return null;
        }

        if (Config.Instance.gameWin)
        {
            gameOverText.color = Color.cyan;
            wonlostText.color = Color.cyan;
            wonlostText.text = "YOU WON";
            continueText.color = Color.cyan;
            SpaceBabyController.Instance.BabyHappy();
        }
        else
        {
            gameOverText.color = Color.red;
            wonlostText.color = Color.red;
            wonlostText.text = "YOU LOST";
            continueText.color = Color.red;
            SpaceBabyController.Instance.BabyLoseTransition();
        }

        this.gameObject.GetComponent<Button>().enabled = true;
        gameOverTextPanel.SetActive(true);
    }

    public void EndGameOverTransition()
    {
        this.gameObject.GetComponent<Button>().enabled = false;
        gameOverTextPanel.SetActive(false);

        if (Config.Instance.gameWin)
        {
            SoundEffectsController.Instance.WinTransition();
            MusicController.Instance.FadeMusicOut();
            StartCoroutine(FadeGameplayOut());
        }
        else
        {
            StartCoroutine(ReactorMeltdown());
        }
    }

    private IEnumerator ReactorMeltdown()
    {
        float explosionDelay = 0;
        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            explosionDelay += reactorScript.cardList.Count;
        }
        explosionDelay *= 1.2f;
        explosionDelay = 1f / explosionDelay;

        GameObject matchExplosion;
        bool reactorExploded;
        foreach (ReactorScript reactorScript in UtilsScript.Instance.reactorScripts)
        {
            reactorExploded = false;
            foreach (GameObject card in reactorScript.cardList)
            {
                reactorExploded = true;
                matchExplosion = Instantiate(UtilsScript.Instance.matchPrefab, card.transform.position, Quaternion.identity);
                matchExplosion.transform.localScale = new Vector3(Config.GameValues.matchExplosionScale, Config.GameValues.matchExplosionScale);
                yield return new WaitForSeconds(0.2f);
                card.SetActive(false);
                SoundEffectsController.Instance.ExplosionSound();
                yield return new WaitForSeconds(explosionDelay);
            }

            if (reactorExploded)
            {
                matchExplosion = Instantiate(UtilsScript.Instance.matchPrefab, reactorScript.gameObject.transform.position, Quaternion.identity);
                matchExplosion.transform.localScale = new Vector3(Config.GameValues.matchExplosionScale / 2, Config.GameValues.matchExplosionScale / 2);
                matchExplosion.GetComponent<Animator>().Play("LoseExplosionAnim");
            }
        }
        
        MusicController.Instance.FadeMusicOut();
        StartCoroutine(FadeGameplayOut());
    } 

    private IEnumerator FadeGameplayOut()
    {
        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        Color fadeColor = fadeInScreen.color;

        while (fadeColor.a < 1)
        {
            fadeColor.a += Time.deltaTime * Config.GameValues.endGameFadeOutSpeed;
            fadeInScreen.color = fadeColor;
            yield return null;
        }

        TransitionToSummaryScene();
    }

    private void TransitionToSummaryScene()
    {
        if (Config.Instance.gameWin)
        {
            MusicController.Instance.WinMusic();
        }
        else
        {
            MusicController.Instance.LoseMusic();
        }

        SceneManager.LoadScene(Constants.summaryScene);
    }
}
