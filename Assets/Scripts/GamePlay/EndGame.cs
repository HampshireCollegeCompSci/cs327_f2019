using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public GameObject errorObject;

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
        Config.Instance.gameWin = didWin;

        // overwritten when manually won (cheated)
        Config.Instance.matchCounter = (byte)(MatchedPileScript.Instance.cardList.Count / 2);

        if (didWin)
        {
            SpaceBabyController.Instance.BabyHappy();
            SoundEffectsController.Instance.WinSound();
        }
        else
        {
            SpaceBabyController.Instance.BabyLoseTransition();
            SoundEffectsController.Instance.LoseSound();

            errorObject.SetActive(true);
        }

        MusicController.Instance.FadeMusicOut();
        StartCoroutine(FadeGameplayOut(didWin));
    }

    private IEnumerator FadeGameplayOut(bool gameWin)
    {
        Image fadeInScreen = this.gameObject.GetComponent<Image>();
        Color fadeColor = gameWin ? Color.white : Color.black;
        fadeColor.a = 0;
        fadeInScreen.color = fadeColor;
        fadeInScreen.enabled = true;

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
