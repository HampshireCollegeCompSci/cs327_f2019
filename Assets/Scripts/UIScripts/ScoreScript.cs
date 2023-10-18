using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    // Singleton instance.
    public static ScoreScript Instance { get; private set; }
    private static readonly WaitForSeconds textDelay = new(0.05f);

    [SerializeField]
    private Text scoreText;

    private Coroutine scoreCoroutine;
    private int newScore;

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
        Actions.Score = setTo;
        if (scoreCoroutine != null)
        {
            StopCoroutine(scoreCoroutine);
            scoreCoroutine = null;
        }
        scoreText.text = setTo.ToString();
    }

    public void UpdateScore(int addValue)
    {
        int oldScore = Actions.Score;
        Actions.Score += addValue;
        newScore = Actions.Score;
        if (scoreCoroutine != null) return;
        scoreCoroutine = StartCoroutine(AddToScoreText(oldScore));
    }

    private IEnumerator AddToScoreText(int currentValue)
    {
        while (currentValue < newScore)
        {
            yield return textDelay;
            currentValue += 10;
            scoreText.text = currentValue.ToString();
        }
        if (currentValue != newScore)
        {
            scoreText.text = newScore.ToString();
        }
        scoreCoroutine = null;
    }
}
