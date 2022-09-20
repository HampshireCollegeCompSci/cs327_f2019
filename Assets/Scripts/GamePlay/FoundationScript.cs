using System.Collections.Generic;
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
        cardList = new();
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
                GlowLevel = Constants.defaultHighlightColorLevel;
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

            if (value != Constants.defaultHighlightColorLevel)
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
            cardList[0].GetComponent<CardScript>().Hologram = false;
        }

        cardList.Insert(0, card);
        card.transform.SetParent(gameObject.transform);

        SetCardPositions();
    }

    public void RemoveCard(GameObject card, bool showHolo)
    {
        RemoveCard(card);
        if (cardList.Count != 0 && showHolo)
        {
            cardList[0].GetComponent<CardScript>().Hologram = true;
        }
    }

    public void RemoveCard(GameObject card)
    {
        cardList.Remove(card);

        if (cardList.Count != 0)
        {
            CardScript cardScript = cardList[0].GetComponent<CardScript>();

            if (cardScript.Hidden)
            {
                cardScript.Hidden = false;

                // when the tutorial is active, disable moving the next top card so that
                // we don't need to deal with some user interactions
                if (Config.Instance.tutorialOn)
                {
                    cardScript.Obstructed = true;
                }
                else
                {
                    cardScript.HitBox = true;
                }
            }
        }

        SetCardPositions();
    }

    public void SetCardPositions()
    {
        float zOffset = -0.1f;
        int hiddenCards = 0;
        float yOffset = 0;

        int count = cardList.Count;
        for (int i = count - 1; i >= 0; i--) // go backwards through the list
        {
            // as we go through, place cards above and in-front the previous one
            cardList[i].transform.position = gameObject.transform.position + new Vector3(0, yOffset, zOffset);

            if (cardList[i].GetComponent<CardScript>().Hidden)  // don't show hidden cards as much
            {
                hiddenCards++;
                if (count > 12)
                {
                    yOffset += 0.02f;
                }
                else if (count > 10)
                {
                    yOffset += 0.07f;
                }
                else
                {
                    yOffset += 0.15f;
                }
            }
            else if (hiddenCards != 0 && count > 16)
            {
                yOffset += 0.30f;
            }
            else if (count - hiddenCards > 11)
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
            GlowLevel = Constants.winHighlightColorLevel;
            _glowing = false;
        }
        else
        {
            _glowing = true;
            Glowing = false;
        }
    }
}
