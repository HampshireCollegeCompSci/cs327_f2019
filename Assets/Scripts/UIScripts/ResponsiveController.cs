using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveController : MonoBehaviour
{
    GameObject[] cards, foundations, reactors, reactorHolders, decks;
    bool updated;
    float width, height, backGroundSizeX, backGroundSizeY;
    Vector3 backgroundOriginalScale;

    // Start is called before the first frame update
    void Start()
    {
        width = Config.config.GetScreenToWorldWidth();
        height = Config.config.GetScreenToWorldHeight();

        //resize background image
        backGroundSizeX = GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().bounds.size.x;
        backGroundSizeY = GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().bounds.size.y;

        backgroundOriginalScale = GameObject.FindGameObjectWithTag("Background").transform.localScale;

        GameObject.FindGameObjectWithTag("Background").transform.localScale = new Vector2(backgroundOriginalScale.x * width / backGroundSizeX, backgroundOriginalScale.y * height / backGroundSizeY);

    }

    private void Update()
    {
        if (!updated)
        {
            updated = true;

            //find cards by tag
            cards = GameObject.FindGameObjectsWithTag("Card");

            //find decks
            decks = GameObject.FindGameObjectsWithTag("Deck");

            //find foundation
            foundations = GameObject.FindGameObjectsWithTag("Foundation");
        }

        else if (Config.config.GetScreenToWorldWidth() != width)
        {
            float scale = Config.config.GetScreenToWorldWidth() / width;
            width = Config.config.GetScreenToWorldWidth();

            //scale cards by tag

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].transform.localScale *= scale;
            }

            //scale deck
            for (int i = 0; i < decks.Length; i++)
            {
                decks[i].transform.localScale *= scale;
            }

            //scale foundation
            for (int i = 0; i < foundations.Length; i++)
            {
                foundations[i].transform.localScale *= scale;
            }

            //scale reactor 
            //for (int i = 0; i < reactors.Length; i++)
            //{
            //    reactors[i].transform.localScale = singleReactorOriginalScale * width * Config.config.relativeCardScale;
            //}

            //scale reactor holder

            //for (int i = 0; i < reactorHolders.Length; i++)
            //{
            //    reactorHolders[i].transform.localScale = reactorHolderOriginalScale * width * Config.config.relativeCardScale;
            //}

            //scale background
            GameObject.FindGameObjectWithTag("Background").transform.localScale = new Vector2(backgroundOriginalScale.x * width / backGroundSizeX, backgroundOriginalScale.y * height / backGroundSizeY);

        }
    }

}
