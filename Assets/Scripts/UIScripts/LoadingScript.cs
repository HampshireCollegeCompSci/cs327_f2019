using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadSceneAsync(Constants.gameplayScene, LoadSceneMode.Additive);
        MusicController.Instance.GameMusic();
    }
}