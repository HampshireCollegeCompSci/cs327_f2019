using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    private GameObject config;
    public GameObject deckButton;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene")
            SceneManager.UnloadSceneAsync("LoadingScene");

        config = GameObject.Find("Config");
        config.GetComponent<Config>().SetCards();
        config.GetComponent<Config>().gamePaused = false;

        deckButton.GetComponent<DeckScript>().DeckStart();
    }

}
