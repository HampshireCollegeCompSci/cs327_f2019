using UnityEngine;

public class SpaceBabyController : MonoBehaviour
{
    // Singleton instance.
    public static SpaceBabyController Instance { get; private set; }

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private AudioClip happySound, reactorHighSound, counterSound, eatSound, loseSound;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance != null)
            throw new System.ArgumentException("there should not already be an instance of this");
        Instance = this;
    }

    public void SetInstanceNull()
    {
        Instance = null;
    }

    public void ResetBaby()
    {
        audioSource.Stop();
        animator.SetBool(Constants.AnimatorIDs.loseTransitionID, false);
        BabyIdle();
    }

    public void BabyIdle()
    {
        animator.Play(Constants.AnimatorIDs.SpaceBaby.idleID);
    }

    public void BabyHappy()
    {
        PlayBabyHappySound();
        animator.Play(Constants.AnimatorIDs.SpaceBaby.happyID);
    }

    public void PlayBabyHappySound()
    {
        audioSource.PlayOneShot(happySound, 0.4f);
    }

    public void BabyEat()
    {
        animator.Play(Constants.AnimatorIDs.SpaceBaby.eatingID);
    }

    public void BabyReactorHigh()
    {
        if (!Config.Instance.HintsEnabled) return;
        audioSource.PlayOneShot(reactorHighSound, 0.2f);
        AngryAnimation();
    }

    public void BabyLoseTransition()
    {
        audioSource.PlayOneShot(loseSound, 1);
        animator.SetBool(Constants.AnimatorIDs.loseTransitionID, true);
        animator.Play(Constants.AnimatorIDs.SpaceBaby.angryID);
    }

    public void BabyActionCounter()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash ==
            Constants.AnimatorIDs.SpaceBaby.angryID) return;
        audioSource.PlayOneShot(counterSound, 0.5f);
        AngryAnimation();
    }

    public void PlayLoseAnimation()
    {
        animator.Play(Constants.AnimatorIDs.SpaceBaby.loseID);
    }

    public void PlayWinStartAnimation()
    {
        animator.Play(Constants.AnimatorIDs.SpaceBaby.winID);
    }

    public void PlayDoneEatingAnimation()
    {
        animator.SetBool(Constants.AnimatorIDs.doneEatingTransitionID, true);
    }

    public void PlayEatSound()
    {
        audioSource.PlayOneShot(eatSound);
    }

    private void AngryAnimation()
    {
        animator.Play(Constants.AnimatorIDs.SpaceBaby.angryID);
    }
}
