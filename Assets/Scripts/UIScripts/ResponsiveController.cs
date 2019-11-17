using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            //find foundation
            foundations = GameObject.FindGameObjectsWithTag("Foundation");
        }

        else if (Config.config.GetScreenToWorldWidth() != width)
        {
            float scale = Config.config.GetScreenToWorldWidth() / width;
            width = Config.config.GetScreenToWorldWidth();
            height = Config.config.GetScreenToWorldHeight();
            //scale cards by tag

            //for (int i = 0; i < cards.Length; i++)
            //{
            //    cards[i].transform.localScale *= scale;
            //}

            ////scale foundation
            //for (int i = 0; i < foundations.Length; i++)
            //{
            //    foundations[i].transform.localScale *= scale;
            //}

            //scale background
            GameObject.FindGameObjectWithTag("Background").transform.localScale = new Vector2(backgroundOriginalScale.x * width / backGroundSizeX, backgroundOriginalScale.y * height / backGroundSizeY);

        }
    }

}
