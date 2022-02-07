using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public Text gameScore;

    public void Start()
    {
        gameScore.text = Config.Instance.score.ToString();
    }
}
