using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBabyController : MonoBehaviour
{
    bool idling;
    public AudioClip happySound, angrySound, loseSound, counterSound;


    // Start is called before the first frame update
    void Start()
    {
        idling = true;
        gameObject.GetComponent<Animator>().Play("IdlingAnim");
    }

    public void BabyHappyAnim()
    {
        if (idling)
        {
            gameObject.GetComponent<AudioSource>().clip = happySound;
            gameObject.GetComponent<AudioSource>().Play();

            gameObject.GetComponent<Animator>().Play("HappyAnim");
            StartCoroutine("BabyAnimTrans");
        }

    }

    public void BabyEatAnim()
    {
        idling = false;
        gameObject.GetComponent<Animator>().Play("EatingAnim");
        StartCoroutine("BabyAnimTrans");
    }

    public void BabyAngryAnim()
    {
        idling = false;
        gameObject.GetComponent<AudioSource>().clip = angrySound;
        gameObject.GetComponent<AudioSource>().Play();

        gameObject.GetComponent<Animator>().Play("AngryAnim");
        StartCoroutine("BabyAnimTrans");
    }

    public void BabyLoseSound()
    {
        gameObject.GetComponent<AudioSource>().clip = loseSound;
        gameObject.GetComponent<AudioSource>().Play();
    }

    public void BabyActionCounterSound()
    {
        gameObject.GetComponent<AudioSource>().clip = counterSound;
        gameObject.GetComponent<AudioSource>().Play();
    }

    IEnumerator BabyAnimTrans()
    {

        for (float ft = 1.5f; ft >= 0; ft -= 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }

        idling = true;
        gameObject.GetComponent<Animator>().Play("IdlingAnim");


    }

}
