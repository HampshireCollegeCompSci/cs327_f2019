using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public GameObject config;
    public GameObject utils;
    public int listLen;
    public int counter;
    void Start()
    {
        SceneManager.UnloadSceneAsync("LoadingScene");

        config = GameObject.Find("Config");
        config.GetComponent<Config>().SetCards();
        config.GetComponent<Config>().gamePaused = false;

        utils = GameObject.Find("Utils");
        utils.GetComponent<UtilsScript>().SetCards();
        listLen = utils.GetComponent<UtilsScript>().selectedCards.Count;
        for (counter = 1; counter <= listLen; counter++)
        {
            utils.GetComponent<UtilsScript>().selectedCards.RemoveAt(0);
        }
    }

}
