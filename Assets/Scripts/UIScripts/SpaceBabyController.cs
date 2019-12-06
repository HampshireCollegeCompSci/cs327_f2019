using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBabyController : MonoBehaviour
{
    bool idling;

    // Start is called before the first frame update
    void Start()
    {
        idling = true;
        gameObject.GetComponent<Animator>().Play("IdleAnim");
    }

    public void BabyHappyAnim()
    {
        if (idling)
        {
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
