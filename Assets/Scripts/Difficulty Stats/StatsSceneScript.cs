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
            PopoulateText(statTexts[2], dif.Stats.TimesWon);
            PopoulateText(statTexts[4], dif.Stats.HighScore);
            PopoulateText(statTexts[6], dif.Stats.LeastMoves);
            PopoulateText(statTexts[8], dif.Stats.HighestCombo);

            TimeSpan ts = dif.Stats.FastestTime;
            statTexts[10].text = ts == TimeSpan.Zero ? GameValues.Text.noValue : ts.ToString(Constants.Time.format);
        }
    }

    private void PopoulateText(Text txt, int num)
    {
       txt.text = num != 0 ? num.ToString() : GameValues.Text.noValue;
    }
}
