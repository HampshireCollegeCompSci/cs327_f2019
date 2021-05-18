using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public GameObject deckButton;
    public GameObject[] topSuitObjects, bottomSuitObjects;
    public Sprite[] suitSprites;

    void Start()
    {
        // unloading the loading scene if it's still active
        if (SceneManager.GetActiveScene().name == "LoadingScene")
            SceneManager.UnloadSceneAsync("LoadingScene");

        // setting stuff up for the game
        GameObject config = GameObject.Find("Config");
        config.GetComponent<Config>().StartupFindObjects();
        config.GetComponent<Config>().gamePaused = false;

        // assigning sprites to the reactor suits
        bool isOn;
        if (System.Boolean.TryParse(PlayerPrefs.GetString(PlayerPrefKeys.foodSuitsEnabledKey), out isOn))
        { }
        else
        {
            // unable to parse
            isOn = false;
        }

        // the food sprites start at index 0, classic at 4
        int suitSpritesIndex;
        suitSpritesIndex = isOn ? 0 : 4;

        // getting a subset list of suit sprites to use for the token/cards
        Sprite[] suitSpritesSubset = new Sprite[4];
        Array.Copy(suitSprites, suitSpritesIndex, suitSpritesSubset, 0, 4);

        // setting up the reactor suit images
        for (int i = 0; i < 4; i++)
        {
            topSuitObjects[i].GetComponent<SpriteRenderer>().sprite = suitSprites[suitSpritesIndex];
            bottomSuitObjects[i].GetComponent<SpriteRenderer>().sprite = suitSprites[suitSpritesIndex];
            suitSpritesIndex++;
        }

        // starting the game
        deckButton.GetComponent<DeckScript>().DeckStart(suitSpritesSubset);
    }

}
