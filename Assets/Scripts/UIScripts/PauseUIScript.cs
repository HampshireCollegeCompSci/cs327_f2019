using UnityEngine;
using UnityEngine.UI;

public class PauseUIScript : MonoBehaviour
{
    [SerializeField]
    private Text scoreText;

    private void Awake()
    {
        scoreText.text = Config.Instance.score.ToString();
    }
}
