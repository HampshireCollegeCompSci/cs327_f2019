using UnityEngine;
using UnityEngine.UI;

public class PauseUIScript : MonoBehaviour
{
    [SerializeField]
    private Text difficultyText, scoreText, timerText, movesText;

    private void Awake()
    {
        difficultyText.text = Config.Instance.CurrentDifficulty.Name;
        scoreText.text = Actions.Score.ToString();
        timerText.text = Timer.GetTimeSpan().ToString(Constants.Time.format);
        movesText.text = Actions.MoveCounter.ToString();
    }
}
