using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBabyController : MonoBehaviour
{
    bool idling;
    public AudioSource audioSource;
    public Animator animator;
    public AudioClip happySound, angrySound, loseSound, counterSound, eatSound;

    public Sprite[] foodObjects;
    public GameObject foodPrefab;

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

    public void BabyWin(int matchNumber)
    {
        animator.Play("WinStart");
        StartCoroutine(BabyWinAnimation());
        StartCoroutine(EatAnimation());
    }

    IEnumerator BabyWinAnimation()
    {
        yield return new WaitForSeconds(1);
        animator.Play("WinEat");
    }

    IEnumerator EatAnimation()
    {
        List<GameObject> foods = new List<GameObject>();
        Vector3 outOfBounds = new Vector3(3.8f, 0, 0);

        for (int i = 0; i < foodObjects.Length; i++)
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
                if (foods.Count == 0)
                {
                    yield return new WaitForSeconds(0.4f);
                    BabyHappyAnim();
                    yield break;
                }

                position = foods[0].transform.position;
            }

            yield return null;
        }
    }
}
