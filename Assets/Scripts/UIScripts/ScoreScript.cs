using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public Text gameScore;
    public GameObject pointsPrefab;
    //private List<GameObject> pointEffects;

    public void UpdateScore(int addScore)
    {
        gameScore.text = Config.config.score.ToString();

        if (addScore != 0)
            StartCoroutine(PointFade(addScore));
    }

    IEnumerator PointFade(int addScore)
    {
        Vector3 newPosition = gameObject.transform.position;
        newPosition.y -= 0.9f;
        newPosition.x -= 0.1f;
        GameObject scorePointEffect = Instantiate(pointsPrefab, newPosition, Quaternion.identity, gameObject.transform);
        
        if (addScore > 0)
            scorePointEffect.GetComponent<Text>().text = "+" + addScore.ToString();
        else
            scorePointEffect.GetComponent<Text>().text = addScore.ToString();

        //pointEffects.Add(scorePointEffect);

        yield return new WaitForSeconds(1);

        Color fadeColor = scorePointEffect.GetComponent<Text>().color;

        while (fadeColor.a > 0)
        {
            yield return new WaitForSeconds(0.1f);
            fadeColor.a -= 0.05f;
            scorePointEffect.GetComponent<Text>().color = fadeColor;
        }

        //pointEffects.Remove(scorePointEffect);
        Destroy(scorePointEffect);
    }
}
