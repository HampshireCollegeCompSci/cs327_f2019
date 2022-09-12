using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public Text scoreText;

    private Coroutine scoreCoroutine;
    private int newScore;

    // Singleton instance.
    public static ScoreScript Instance = null;

    // Initialize the singleton instance.
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

    public void SetScore(int setTo)
    {
        Config.Instance.score = setTo;
        if (scoreCoroutine != null)
        {
            StopCoroutine(scoreCoroutine);
            scoreCoroutine = null;
        }
        scoreText.text = setTo.ToString();
    }

    public void UpdateScore(int addValue)
    {
        int oldScore = Config.Instance.score;
        Config.Instance.score += addValue;
        newScore = Config.Instance.score;
        if (scoreCoroutine != null) return;
        scoreCoroutine = StartCoroutine(AddToScoreText(oldScore));
    }

    private IEnumerator AddToScoreText(int currentValue)
    {
        while (currentValue < newScore)
        {
            yield return new WaitForSeconds(0.05f);
            currentValue += 10;
            scoreText.text = currentValue.ToString();
        }
        scoreCoroutine = null;
    }
}
