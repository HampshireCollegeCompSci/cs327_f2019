using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    public GameObject config;
    public GameObject utils;

    void Start()
    {
        config = GameObject.Find("Config");
        config.GetComponent<Config>().SetCards();

        utils = GameObject.Find("Utils");
        utils.GetComponent<UtilsScript>().SetCards();
    }

    public void ProcessAction(GameObject input)
    {
        Application.LoadLevel("MainMenuScene");//resets the level
        Debug.Log("hit button");
        return;
    }
}
