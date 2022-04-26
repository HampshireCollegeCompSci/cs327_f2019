using System.Collections;
using UnityEngine;

public class SpaceBabyController : MonoBehaviour
{
    public AudioSource audioSource;
    public Animator animator;
    public AudioClip happySound, reactorHighSound, counterSound, eatSound, loseSound;

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

    public void BabyReactorHigh()
    {
        Debug.Log("SpaceBaby Reactor High");

        audioSource.PlayOneShot(reactorHighSound, 0.2f);
        AngryAnimation();
    }

    public void BabyLoseTransition()
    {
        Debug.Log("SpaceBaby Lose Transition");
        audioSource.PlayOneShot(loseSound, 1);
        StopAllCoroutines();
        animator.Play("AngryAnim", -1, 0);
        StartCoroutine(LoseAnimTrans());
    }

    IEnumerator LoseAnimTrans()
    {
        yield return new WaitForSeconds(1.6f);
        animator.Play("Lose");
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
        animator.Play("IdlingAnim");
        idling = true;
        angry = false;
    }

    public void PlayLoseAnimation()
    {
        Debug.Log("SpaceBaby Lose Summary");

        animator.Play("Lose");
    }

    public void PlayWinStartAnimation()
    {
        animator.Play("WinStart");
    }

    public void PlayEatSound()
    {
        audioSource.PlayOneShot(eatSound);
    }
}
