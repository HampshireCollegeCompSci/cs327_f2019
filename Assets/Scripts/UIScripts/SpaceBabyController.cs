using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpaceBabyController : MonoBehaviour
{
    public AudioSource audioSource;
    public Animator animator;
    public AudioClip happySound, angrySound, counterSound, eatSound;

    // game over win condition
    public Sprite[] foodObjects;
    public GameObject foodPrefab;
    public GameObject babyPlanet;
    public GameObject panel;

    private bool idling, angry;

    // Singleton instance.
    public static SpaceBabyController Instance = null;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // since the pause scene creates a duplicate spacebby on top of the gameplay scene's this will happen
            Debug.LogWarning("There really shouldn't be two of these but oh well.");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateMaxVolume(PlayerPrefs.GetFloat(Constants.soundEffectsVolumeKey));
        idling = true;
        angry = false;
        animator.Play("IdlingAnim");
    }

    public void UpdateMaxVolume(float newVolume)
    {
        Debug.Log($"updating space baby volume to: {newVolume}");
        audioSource.volume = newVolume;
    }

    // the 2D collider on the spacebby calls this
    void OnMouseDown()
    {
        Debug.Log("SpaceBaby Clicked");
        if (Config.Instance.gamePaused) return;
        BabyHappy();
    }

    public void BabyHappy()
    {
        Debug.Log("SpaceBaby Happy");

        if (idling)
        {
            audioSource.PlayOneShot(happySound, 0.4f);
            idling = false;
            animator.Play("HappyAnim");
            StartCoroutine(BabyAnimTrans());
        }
    }

    public void BabyEat()
    {
        Debug.Log("SpaceBaby Eat");

        idling = false;
        animator.Play("EatingAnim");
        StartCoroutine(BabyAnimTrans());
    }

    public void BabyAngry()
    {
        Debug.Log("SpaceBaby Angry");

        audioSource.PlayOneShot(angrySound, 0.2f);
        AngryAnimation();
    }

    public void BabyLoseTransition()
    {
        Debug.Log("SpaceBaby Lose Transition");

        AngryAnimation();
    }

    public void BabyActionCounter()
    {
        Debug.Log("SpaceBaby ActionCounter");

        audioSource.PlayOneShot(counterSound, 0.5f);
        AngryAnimation();
    }

    private void AngryAnimation()
    {
        if (!angry)
        {
            idling = false;
            angry = true;
            animator.Play("AngryAnim");
            StartCoroutine(BabyAnimTrans());
        }
    }

    IEnumerator BabyAnimTrans()
    {
        yield return new WaitForSeconds(2);
        idling = true;
        angry = false;
        animator.Play("IdlingAnim");
    }

    public void BabyLoseSummary()
    {
        Debug.Log("SpaceBaby Lose Summary");

        animator.Play("Lose");
    }

    public void BabyWinSummary(byte matchNumber)
    {
        Debug.Log("SpaceBaby Win");

        animator.Play("WinStart");
        StartCoroutine(BabyWinAnimation());
        StartCoroutine(EatAnimation(matchNumber));
    }

    IEnumerator BabyWinAnimation()
    {
        yield return new WaitForSeconds(1);
        animator.Play("WinEat");
    }

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
                        BabyHappy();

                    yield break;
                }
            }

            yield return null;
        }
    }

    IEnumerator WinTransition()
    {
        BabyHappy();

        Image panelImage = panel.GetComponent<Image>();
        Color panelColor = new Color(1, 1, 1, 0);

        while (panelColor.a < 1)
        {
            panelColor.a += Time.deltaTime * 0.75f;
            panelImage.color = panelColor;
            yield return null;
        }

        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        babyPlanet.SetActive(true);

        while (panelColor.a > 0)
        {
            panelColor.a -= Time.deltaTime * 0.75f;
            panelImage.color = panelColor;
            yield return null;
        }
    }
}
