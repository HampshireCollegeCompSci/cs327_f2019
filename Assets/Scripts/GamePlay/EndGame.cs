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

    public void GameOver(bool didWin)
    {
        Debug.Log($"Game Over, won: {didWin}");

        SaveState.Delete();
        Config.Instance.gameOver = true;
        Config.Instance.gamePaused = true;
        Config.Instance.gameWin = didWin;

        if (Config.GameValues.enableBonusPoints)
        {
            AddExtraEndGameScore();
        }

        // overwritten when manually won (cheated)
        Config.Instance.matchCounter = (byte)(MatchedPileScript.Instance.cardList.Count / 2);

        StartCoroutine(BeginGameOverTransition());
    }

    private void AddExtraEndGameScore()
    {
        int extraScore = 0;
        if (MatchedPileScript.Instance.cardList.Count == 52)
        {
            extraScore += Config.GameValues.perfectGamePoints;
        }

        foreach (GameObject foundation in UtilsScript.Instance.foundations)
        {
            if (foundation.GetComponent<FoundationScript>().cardList.Count == 0)
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
            fadeColor = Color.white;
            SoundEffectsController.Instance.WinSound();
        }
        else
        {
            fadeColor = Color.black;
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
            continueText.color = Color.cyan;
            SpaceBabyController.Instance.BabyHappy();
        }
        else
        {
            gameOverText.color = Color.red;
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
            StartCoroutine(FadeGameplayOut(true));
        }
        else
        {
            StartCoroutine(ReactorMeltdown());
        }
    }

    private IEnumerator ReactorMeltdown()
    {
        GameObject matchExplosion;
        bool reactorExploded;
        foreach (GameObject reactor in UtilsScript.Instance.reactors)
        {
            reactorExploded = false;
            foreach (GameObject card in reactor.GetComponent<ReactorScript>().cardList)
            {
                reactorExploded = true;
                matchExplosion = Instantiate(UtilsScript.Instance.matchPrefab, card.transform.position, Quaternion.identity);
                matchExplosion.transform.localScale = new Vector3(Config.GameValues.matchExplosionScale, Config.GameValues.matchExplosionScale);
                yield return new WaitForSeconds(0.2f);
                card.SetActive(false);
                SoundEffectsController.Instance.ExplosionSound();
            }

            if (reactorExploded)
            {
                matchExplosion = Instantiate(UtilsScript.Instance.matchPrefab, reactor.transform.position, Quaternion.identity);
                matchExplosion.transform.localScale = new Vector3(Config.GameValues.matchExplosionScale / 2, Config.GameValues.matchExplosionScale / 2);
                matchExplosion.GetComponent<Animator>().Play("LoseExplosionAnim");
            }
        }
        
        MusicController.Instance.FadeMusicOut();
        StartCoroutine(FadeGameplayOut(false));
    } 

    private IEnumerator FadeGameplayOut(bool gameWin)
    {
        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        Color fadeColor = gameWin ? Color.white : Color.black;
        fadeColor.a = fadeInScreen.color.a;
        //fadeColor.a = 0;

        while (fadeColor.a < 1)
        {
            fadeInScreen.color = fadeColor;
            fadeColor.a += Time.deltaTime * Config.GameValues.endGameFadeOutSpeed;
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
