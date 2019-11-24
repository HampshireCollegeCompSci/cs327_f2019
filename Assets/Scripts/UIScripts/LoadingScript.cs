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
        if (SceneManager.GetActiveScene().name == "LevelSelectScene")
            SceneManager.UnloadSceneAsync("LevelSelectScene");
        StartCoroutine(UpdateLoadingText());
    }

    private IEnumerator UpdateLoadingText()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);

        float dotNum = 1f;
        GameObject txt = GameObject.Find("Loading");

        while (!operation.isDone)
        {
            //if (dotNum % 3 == 0)
            //    txt.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish.ToUpper() + "...";
            //else if (dotNum % 2 == 0)
            //    txt.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish.ToUpper() + "..";
            //else
            //    txt.GetComponent<Text>().text = Config.config.loadingSceneTxtEnglish.ToUpper() + ".";
            //dotNum++;
            yield return null;
        }

        //Config.config.GetComponent<MusicController>().GameMusic();
    }

}
