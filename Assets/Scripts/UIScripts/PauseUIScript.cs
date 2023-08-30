using UnityEngine;
using UnityEngine.UI;

public class PauseUIScript : MonoBehaviour
{
    [SerializeField]
    private Text scoreText, timerText, movesText;

    private void Awake()
    {
        scoreText.text = Actions.Score.ToString();
        timerText.text = Timer.GetTimeSpan().ToString(Constants.Time.format);
        movesText.text = Actions.MoveCounter.ToString();
    }
}
