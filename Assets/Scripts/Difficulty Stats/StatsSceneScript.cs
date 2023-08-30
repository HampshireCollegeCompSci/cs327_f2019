using System;
using UnityEngine;
using UnityEngine.UI;

public class StatsSceneScript : MonoBehaviour
{
    [SerializeField]
    private Transform menuContentTransform;

    [SerializeField]
    private GameObject statsSectionPrefab;

    private void Start()
    {
        foreach (Difficulty dif in Difficulties.difficultyArray)
        {
            if (dif.Equals(Difficulties.cheat) && dif.Stats.TimesWon == 0) continue;

            GameObject newStat = Instantiate(statsSectionPrefab, menuContentTransform);
            Text[] statTexts = newStat.GetComponentsInChildren<Text>();
            if (statTexts == null || statTexts.Length != 11)
            {
                throw new Exception("the achievement prefab doesn't have 11 text components to use");
            }
            statTexts[0].text = dif.Name;
            statTexts[2].text = dif.Stats.TimesWon.ToString();
            statTexts[4].text = dif.Stats.HighScore.ToString();
            statTexts[6].text = dif.Stats.LeastMoves.ToString();
            statTexts[8].text = dif.Stats.HighestCombo.ToString();
            statTexts[10].text = dif.Stats.FastestTime.ToString(Constants.Time.format);
        }
    }
}
