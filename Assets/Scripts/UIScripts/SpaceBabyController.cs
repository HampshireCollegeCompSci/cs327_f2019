﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpaceBabyController : MonoBehaviour
{
    bool idling;
    public AudioSource audioSource;
    public Animator animator;
    public AudioClip happySound, angrySound, loseSound, counterSound, eatSound;

    // Start is called before the first frame update
    void Start()
    {
        idling = true;
        animator.Play("IdlingAnim");
    }

    public void BabyHappyAnim()
    {
        if (idling)
        {
            audioSource.PlayOneShot(happySound, 0.4f);

            animator.Play("HappyAnim");
            idling = false;
            StartCoroutine(BabyAnimTrans());
        }

    }

    public void BabyEatAnim()
    {
        idling = false;
        animator.Play("EatingAnim");
        StartCoroutine(BabyAnimTrans());
    }

    public void BabyAngryAnim()
    {
        idling = false;
        audioSource.PlayOneShot(angrySound, 0.2f);

        animator.Play("AngryAnim");
        StartCoroutine(BabyAnimTrans());
    }

    public void BabyLoseSound()
    {
        audioSource.PlayOneShot(loseSound, 0.5f);
    }

    public void BabyActionCounterSound()
    {
        audioSource.PlayOneShot(counterSound, 0.5f);
    }

    IEnumerator BabyAnimTrans()
    {
        yield return new WaitForSeconds(2);
        idling = true;
        animator.Play("IdlingAnim");
    }

    public void BabyLose()
    {
        animator.Play("Lose");
    }

    public void BabyWin(byte matchNumber)
    {
        animator.Play("WinStart");
        StartCoroutine(BabyWinAnimation());
        StartCoroutine(EatAnimation(matchNumber));
    }

    IEnumerator BabyWinAnimation()
    {
        yield return new WaitForSeconds(1);
        animator.Play("WinEat");
    }

    public Sprite[] foodObjects;
    public GameObject foodPrefab;
    IEnumerator EatAnimation(byte matchNumber)
    {
        List<GameObject> foods = new List<GameObject>();
        Vector3 outOfBounds = new Vector3(3.8f, 0, 0);
        Vector3 babyScale = gameObject.transform.localScale;

        byte limit = (byte)(matchNumber/2);
        if (limit > 10)
            limit = 10;
        else if (limit == 0)
            yield break;

        if (limit > foodObjects.Length)
            throw new System.ArgumentException("matchNumber cannot be greater than foodObjects.Length");

        for (int i = 0; i < limit; i++)
        {
            GameObject foody = Instantiate(foodPrefab, outOfBounds, Quaternion.identity);
            foody.GetComponent<SpriteRenderer>().sprite = foodObjects[i];
            foods.Add(foody);
            outOfBounds.x += 2.2f;
        }

        float minusX;
        Vector3 position;;
        while (true)
        {
            minusX = Time.deltaTime * 2.9f;
            for (int i = 0; i < foods.Count; i++)
            {
                position = foods[i].transform.position;
                position.x -= minusX;
                foods[i].transform.position = position;
            }

            if (foods[0].transform.position.x <= 0.1)
            {
                Destroy(foods[0]);
                foods.RemoveAt(0);
                audioSource.PlayOneShot(eatSound, 0.4f);
                babyScale.x += 0.02f;
                gameObject.transform.localScale = babyScale;
                if (foods.Count == 0)
                {
                    if (matchNumber == 26)
                        StartCoroutine(WinTransition());
                    else
                        BabyHappyAnim();

                    yield break;
                }
            }

            yield return null;
        }
    }

    public GameObject babyPlanet;
    public GameObject panel;
    IEnumerator WinTransition()
    {
        BabyHappyAnim();

        Image panelImage = panel.GetComponent<Image>();
        Color panelColor = new Color(1, 1, 1, 0);

        while (panelColor.a < 1)
        {
            panelColor.a += 0.05f;
            panelImage.color = panelColor;
            yield return new WaitForSeconds(0.05f);
        }

        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        babyPlanet.SetActive(true);

        while (panelColor.a > 0)
        {
            panelColor.a -= 0.05f;
            panelImage.color = panelColor;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
