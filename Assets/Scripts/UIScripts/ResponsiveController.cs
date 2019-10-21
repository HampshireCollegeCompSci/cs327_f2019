using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveController : MonoBehaviour
{
    GameObject[] cards, foundations, reactors;
    bool updated;
    float width;
    Vector3[] cardOriginalScale;
    Vector3 foundationOriginalScale, deckOriginalScale, reactorOriginalScale;

    // Start is called before the first frame update
    void Start()
    {
        width = Config.config.GetScreenToWorldWidth();
        cardOriginalScale = new Vector3[52];

    }

    private void Update()
    {
        if (!updated)
        {
            width = Config.config.GetScreenToWorldWidth();
            updated = true;

            //find cards by tag
            cards = GameObject.FindGameObjectsWithTag("Card");

            for (int i = 0; i < cards.Length; i++)
            {
                cardOriginalScale[i] = cards[i].transform.localScale;
            }

            //find deck
            deckOriginalScale = GameObject.FindGameObjectWithTag("Deck").transform.localScale;

            //find foundation
            foundations = GameObject.FindGameObjectsWithTag("Foundation");
            foundationOriginalScale = foundations[0].transform.localScale;

            //find reactor 
            reactors = GameObject.FindGameObjectsWithTag("Reactor");
            reactorOriginalScale = reactors[0].transform.localScale;

        }
        else if (Config.config.GetScreenToWorldWidth() != width)
        {
            width = Config.config.GetScreenToWorldWidth();

            //find cards by tag
            cards = GameObject.FindGameObjectsWithTag("Card");

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].transform.localScale = cardOriginalScale[i] * width * Config.config.relativeCardScale;
            }

            //find deck
            GameObject.FindGameObjectWithTag("Deck").transform.localScale = deckOriginalScale * width * Config.config.relativeCardScale;

            //find foundation
            foundations = GameObject.FindGameObjectsWithTag("Foundation");
            for (int i = 0; i < foundations.Length; i++)
            {
                foundations[i].transform.localScale = foundationOriginalScale * width * Config.config.relativeCardScale;
            }

            //find reactor 
            reactors = GameObject.FindGameObjectsWithTag("Reactor");
            for (int i = 0; i < reactors.Length; i++)
            {
                reactors[i].transform.localScale = reactorOriginalScale * width * Config.config.relativeCardScale;
            }

        }
    }

}
