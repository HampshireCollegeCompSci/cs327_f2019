using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        /*Debug.Log(SceneManager.GetActiveScene().name);
        if (SceneManager.GetActiveScene().name == "LevelSelectScene")
        {
            SceneManager.UnloadSceneAsync("LevelSelectScene");
            Config.config.GetComponent<MusicController>().LoadGap();
            Debug.Log("unload");
        }*/

        SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);
        Config.config.GetComponent<MusicController>().GameMusic(force: true);

        //StartCoroutine(UpdateLoadingText());
    }

    private IEnumerator UpdateLoadingText()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);

        //float dotNum = 1f;
        //GameObject txt = GameObject.Find("Loading");

        while (!operation.isDone)
        {
            //if (dotNum % 30 == 0)
            //    txt.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish.ToUpper() + "...";
            //else if (dotNum % 20 == 0)
            //    txt.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish.ToUpper() + "..";
            //else if (dotNum % 10 == 0)
            //    txt.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish.ToUpper() + ".";
            //dotNum++;
            yield return null;
        }

        Config.config.GetComponent<MusicController>().GameMusic();
    }

}
