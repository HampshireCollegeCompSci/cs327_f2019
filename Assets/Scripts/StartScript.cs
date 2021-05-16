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
        byte suitSpritesIndex = 0;
        for (int i = 0; i < 4; i++)
        {
            topSuitObjects[i].GetComponent<SpriteRenderer>().sprite = suitSprites[suitSpritesIndex];
            bottomSuitObjects[i].GetComponent<SpriteRenderer>().sprite = suitSprites[suitSpritesIndex];
            suitSpritesIndex++;
        }

        // getting the suit sprites to use for the token/cards
        Sprite[] suitSpritesSubset = new Sprite[4];
        Array.Copy(suitSprites, 0, suitSpritesSubset, 0, 4);

        // starting the game
        deckButton.GetComponent<DeckScript>().DeckStart(suitSpritesSubset);
    }

}
