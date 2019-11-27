using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBabyController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Animator>().Play("IdleAnim");
    }

    private void OnMouseDown()
    {
        gameObject.GetComponent<Animator>().Play("HappyAnim");
        StartCoroutine("BabyAnimTrans");
    }

    IEnumerator BabyAnimTrans()
    {

        for (float ft = 0.5f; ft >= 0; ft -= 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }

        gameObject.GetComponent<Animator>().Play("IdleAnim");

    }

}
