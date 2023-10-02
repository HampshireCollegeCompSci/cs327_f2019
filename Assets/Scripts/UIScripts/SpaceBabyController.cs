using System.Collections;
using UnityEngine;

public class SpaceBabyController : MonoBehaviour, ISound
{
    // Singleton instance.
    public static SpaceBabyController Instance;
    private static readonly WaitForSeconds loseDelay = new(1.6f),
        idleDelay = new(2.1f);

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
            Debug.LogWarning("There shouldn't be two of these at a time.");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateMaxVolume(PersistentSettings.SoundEffectsVolume);
        BabyIdle();
    }

    public void UpdateMaxVolume(int newVolume)
    {
        Debug.Log($"updating space baby volume to: {newVolume}");
        audioSource.volume = ((float)newVolume) / GameValues.Settings.soundEffectsVolumeDenominator;
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
        audioSource.PlayOneShot(happySound, 0.4f);
        animator.Play("HappyAnim");
        DelayIdle();
    }

    public void BabyEat()
    {
        animator.Play("EatingAnim");
        DelayIdle();
    }

    public void BabyReactorHigh()
    {
        audioSource.PlayOneShot(reactorHighSound, 0.2f);
        AngryAnimation();
    }

    public void BabyLoseTransition()
    {
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
        audioSource.PlayOneShot(counterSound, 0.5f);
        AngryAnimation();
    }

    public void PlayLoseAnimation()
    {
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
        yield return loseDelay;
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
        yield return idleDelay;
        BabyIdle();
        idleCoroutine = null;
    }
}
