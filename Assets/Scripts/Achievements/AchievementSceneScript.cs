using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AchievementSceneScript : MonoBehaviour
{
    [SerializeField]
    private Transform menuContentTransform;

    [SerializeField]
    private GameObject achievementSectionPrefab;

    private void Start()
    {
        bool gameIsActive = SceneManager.GetActiveScene().name.Equals(Constants.ScenesNames.gameplay);

        foreach (Achievement achievement in Achievements.achievementList)
        {
            GameObject newAchievement = Instantiate(achievementSectionPrefab, menuContentTransform);
            Text[] achievementTexts = newAchievement.GetComponentsInChildren<Text>();
            if (achievementTexts == null || achievementTexts.Length != 3)
            {
                throw new Exception("the achievement prefab doesn't have 3 text components to use");
            }
            achievementTexts[0].text = achievement.Name;
            achievementTexts[2].text = achievement.Description;

            if (achievement.Value == 0)
            {
                achievementTexts[1].text = "";
                newAchievement.GetComponent<CanvasGroup>().alpha = 0.7f;
            }
            else
            {
                achievementTexts[1].text = achievement.Value.ToString();
            }

            if (!gameIsActive) continue;

            if (achievement.Status)
            {
                achievementTexts[0].color = Config.Instance.CurrentColorMode.Match.Color;
            }
            else if (achievement.IsFailureBased)
            {
                achievementTexts[0].color = Config.Instance.CurrentColorMode.Over.Color;
            }
        }
    }
}
