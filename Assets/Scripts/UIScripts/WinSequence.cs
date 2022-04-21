﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WinSequence : MonoBehaviour
{
    public GameObject spaceShip;
    public Transform foodTarget;
    public GameObject babyPlanet;
    public GameObject panelOverlay;

    public GameObject foodPrefab;
    public Sprite[] foodObjects;

    private Vector3 babyScale;
    private Vector3 foodTargetPosition;
    private Vector3 targetScale = new Vector3(0.1f, 0.1f, 1);

    public void StartWinSequence()
    {
        byte matches = (byte)(Config.Instance.matchCounter / 2);
        if (matches > 10)
        {
            matches = 10;
        }
        else if (matches == 0)
        {
            Debug.LogWarning("zero matches detected");
            return;
        }

        if (matches > foodObjects.Length)
        {
            throw new System.ArgumentException("matchNumber cannot be greater than foodObjects.Length");
        }

        StartCoroutine(SpaceBabyAnimationDelay());
        StartCoroutine(Feed(matches));
    }

    IEnumerator SpaceBabyAnimationDelay()
    {
        yield return new WaitForSeconds(0.3f);
        SpaceBabyController.Instance.PlayWinStartAnimation();
    }

    IEnumerator Feed(int matches)
    {
        babyScale = SpaceBabyController.Instance.gameObject.transform.localScale;
        foodTargetPosition = foodTarget.position;

        for (int i = 0; i < matches; i++)
        {
            StartCoroutine(FoodMove(foodObjects[i]));
            yield return new WaitForSeconds(0.75f);
        }

        yield return new WaitForSeconds(0.7f);
        if (matches == 10)
        {
            StartCoroutine(WinTransition());
        }
        else
        {
            SpaceBabyController.Instance.BabyHappy();
        }
    }

    IEnumerator FoodMove(Sprite foodSprite)
    {
        GameObject foody = Instantiate(foodPrefab, spaceShip.transform.position, Quaternion.identity);
        foody.transform.localScale = Vector3.zero;
        foody.GetComponent<SpriteRenderer>().sprite = foodSprite;
        foody.GetComponent<SpriteRenderer>().sortingOrder = 2;

        while (foody.transform.position != foodTargetPosition)
        {
            foody.transform.position = Vector3.MoveTowards(foody.transform.position, foodTargetPosition,
                Time.deltaTime * 2);
            foody.transform.localScale = Vector3.MoveTowards(foody.transform.localScale, targetScale,
                Time.deltaTime);
            yield return null;
        }

        SpaceBabyController.Instance.PlayEatSound();
        Destroy(foody);
        babyScale.x += 0.02f;
        SpaceBabyController.Instance.gameObject.transform.localScale = babyScale;
    }

    IEnumerator WinTransition()
    {
        SpaceBabyController.Instance.BabyHappy();

        panelOverlay.SetActive(true);
        Image panelImage = panelOverlay.GetComponent<Image>();
        Color panelColor = new Color(1, 1, 1, 0);
        panelImage.color = panelColor;

        while (panelColor.a < 1)
        {
            panelColor.a += Time.deltaTime * 0.75f;
            panelImage.color = panelColor;
            yield return null;
        }

        SpaceBabyController.Instance.gameObject.SetActive(false);
        babyPlanet.SetActive(true);

        while (panelColor.a > 0)
        {
            panelColor.a -= Time.deltaTime * 0.75f;
            panelImage.color = panelColor;
            yield return null;
        }
    }
}