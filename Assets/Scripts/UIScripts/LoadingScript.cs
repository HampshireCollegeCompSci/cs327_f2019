using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);
        Config.config.GetComponent<MusicController>().GameMusic(force: true);
    }
}