using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBabyController : MonoBehaviour
{
    bool idling;
    public AudioSource audioSource;
    public Animator animator;
    public AudioClip happySound, angrySound, loseSound, counterSound;


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

}
