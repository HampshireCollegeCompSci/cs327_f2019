﻿using System.Collections.Generic;
using UnityEngine;

public class FoundationScript : MonoBehaviour, ICardContainerHolo, IGlow
{
    [SerializeField]
    private List<GameObject> cardList;

    [SerializeField]
    private bool _glowing;
    [SerializeField]
    private byte _glowLevel;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        cardList = new(52);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        _glowing = false;
        _glowLevel = 0;
    }

    public List<GameObject> CardList
    {
        get => cardList;
    }

    public bool Glowing
    {
        get => _glowing;
        set
        {
            if (value && !_glowing)
            {
                _glowing = true;
            }
            else if (!value && _glowing)
            {
                _glowing = false;
                GlowLevel = Constants.HighlightColorLevel.normal;
            }
        }
    }

    public byte GlowLevel
    {
        get => _glowLevel;
        set
        {
            if (value != _glowLevel)
            {
                _glowLevel = value;
                spriteRenderer.color = Config.GameValues.highlightColors[value];
            }

            if (value != Constants.HighlightColorLevel.normal)
            {
                Glowing = true;
            }
        }
    }

    public void AddCard(GameObject card, bool showHolo)
    {
        AddCard(card);

        if (showHolo)
        {
            CardScript cardScript = card.GetComponent<CardScript>();
            cardScript.Hologram = true;
            cardScript.HitBox = true;
        }
    }

    public void AddCard(GameObject card)
    {
        if (cardList.Count != 0)
        {
            cardList[^1].GetComponent<CardScript>().Hologram = false;
        }

        cardList.Add(card);
        card.transform.SetParent(gameObject.transform);

        SetCardPositions();
    }

    public void RemoveCard(GameObject card, bool showHolo)
    {
        RemoveCard(card);

        if (cardList.Count != 0)
        {
            CardScript topCardScript = cardList[^1].GetComponent<CardScript>();

            if (topCardScript.Hidden)
            {
                topCardScript.Hidden = false;

                // when the tutorial is active, disable moving the next top card so that
                // we don't need to deal with some user interactions
                if (Config.Instance.tutorialOn)
                {
                    topCardScript.Obstructed = true;
                }
                else
                {
                    topCardScript.HitBox = true;
                }
            }

            if (showHolo)
            {
                topCardScript.Hologram = true;
            }
        }

        SetCardPositions();
    }

    public void RemoveCard(GameObject card)
    {
        cardList.RemoveAt(cardList.LastIndexOf(card));
    }

    public void SetCardPositions()
    {
        float zOffset = -0.1f;
        int hiddenCards = 0;
        float yOffset = 0;

        for (int i = 0; i < cardList.Count; i++)
        {
            // as we go through, place cards above and in-front the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(0, yOffset, zOffset);

            if (cardList[i].GetComponent<CardScript>().Hidden)  // don't show hidden cards as much
            {
                hiddenCards++;
                yOffset += cardList.Count switch
                {
                    >12 => 0.02f,
                    >10 => 0.07f,
                    _ => 0.15f
                };
            }
            else if (hiddenCards != 0 && cardList.Count > 16)
            {
                yOffset += 0.30f;
            }
            else if (cardList.Count - hiddenCards > 11)
            {
                yOffset += 0.30f;
            }
            else
            {
                yOffset += 0.33f;
            }

            zOffset -= 0.05f;
        }
    }

    public void GlowForGameEnd(bool turnOn)
    {
        if (turnOn)
        {
            // will turn glowing on but not set the flag for it
            // so that it will not be turned off later
            GlowLevel = Constants.HighlightColorLevel.win;
            _glowing = false;
        }
        else
        {
            _glowing = true;
            Glowing = false;
        }
    }
}
