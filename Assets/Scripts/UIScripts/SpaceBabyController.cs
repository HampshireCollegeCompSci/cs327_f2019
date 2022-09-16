using System.Collections;
using UnityEngine;

public class SpaceBabyController : MonoBehaviour, ISound
{
    // Singleton instance.
    public static SpaceBabyController Instance;

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private AudioClip happySound, reactorHighSound, counterSound, eatSound, loseSound;

    private Coroutine idleCoroutine;

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
        UpdateMaxVolume(PlayerPrefKeys.GetSoundEffectsVolume());
        BabyIdle();
    }

    public void UpdateMaxVolume(float newVolume)
    {
        Debug.Log($"updating space baby volume to: {newVolume}");
        audioSource.volume = newVolume;
    }

    public void ResetBaby()
    {
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
        audioSource.Stop();
        BabyIdle();
    }

    public void BabyIdle()
    {
        animator.Play("IdlingAnim");
    }

    public void BabyHappy()
    {
        Debug.Log("SpaceBaby Happy");

        audioSource.PlayOneShot(happySound, 0.4f);
        animator.Play("HappyAnim");
        DelayIdle();
    }

    public void BabyEat()
    {
        Debug.Log("SpaceBaby Eat");

        animator.Play("EatingAnim");
        DelayIdle();
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

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
        }

        animator.Play("AngryAnim", -1, 0);
        idleCoroutine = StartCoroutine(LoseAnimTrans());
    }

    public void BabyActionCounter()
    {
        Debug.Log("SpaceBaby ActionCounter");

        audioSource.PlayOneShot(counterSound, 0.5f);
        AngryAnimation();
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

    private IEnumerator LoseAnimTrans()
    {
        yield return new WaitForSeconds(1.6f);
        animator.Play("Lose");
    }

    private void AngryAnimation()
    {
        animator.Play("AngryAnim");
        DelayIdle();
    }

    private void DelayIdle()
    {
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
        }
        idleCoroutine = StartCoroutine(BabyAnimTrans());
    }

    private IEnumerator BabyAnimTrans()
    {
        yield return new WaitForSeconds(2);
        BabyIdle();
        idleCoroutine = null;
    }
}
