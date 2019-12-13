using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public GameObject config;
    public GameObject utils;
    public GameObject showpossiblemoves;
    public int listLen;
    public int counter;
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene")
            SceneManager.UnloadSceneAsync("LoadingScene");

        config = GameObject.Find("Config");
        config.GetComponent<Config>().SetCards();
        config.GetComponent<Config>().gamePaused = false;

        GameObject.Find("DeckButton").GetComponent<DeckScript>().DeckStart();

        utils = GameObject.Find("Utils");
        utils.GetComponent<UtilsScript>().SetCards();
        listLen = utils.GetComponent<UtilsScript>().selectedCards.Count;
        for (counter = 1; counter <= listLen; counter++)
        {
            utils.GetComponent<UtilsScript>().selectedCards.RemoveAt(0);
        }

        showpossiblemoves = GameObject.Find("ShowPossibleMoves");
        showpossiblemoves.GetComponent<ShowPossibleMoves>().SetCards();
    }

}
