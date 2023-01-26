using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinSequence : MonoBehaviour
{
    [SerializeField]
    private SpaceBabyController spaceBabyController;
    [SerializeField]
    private GameObject spaceShip;
    public Transform foodTarget;
    [SerializeField]
    private GameObject babyPlanet, panelOverlay;

    [SerializeField]
    private GameObject foodPrefab;
    [SerializeField]
    private Sprite[] foodObjects;

    // food objects
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector2 startScale;
    private Vector2 endScale;

    private Vector3 babyScale;
    private float babyScaleIncrease;
    private const float feedDuration = 0.85f;

    public void StartWinSequence()
    {
        if (Config.Instance.matchCounter == 0)
        {
            Debug.LogWarning("zero matches detected");
            return;
        }

        int percentage = Config.Instance.matchCounter * 100 / 26;
        int amount = foodObjects.Length * percentage / 100;

        StartCoroutine(SpaceBabyAnimationDelay());
        StartCoroutine(Feed(amount));
    }

    private IEnumerator SpaceBabyAnimationDelay()
    {
        yield return new WaitForSeconds(0.3f);
        spaceBabyController.PlayWinStartAnimation();
    }

    private IEnumerator Feed(int matches)
    {
        Debug.Log($"feeding the space baby: {matches}");

        startPosition = spaceShip.transform.position;
        endPosition = foodTarget.position;
        startScale = Vector2.zero;
        endScale = new(20, 20);

        babyScale = spaceBabyController.gameObject.transform.localScale;
        babyScaleIncrease = babyScale.x / 40;

        for (int i = 0; i < matches; i++)
        {
            StartCoroutine(FoodMove(foodObjects[i]));
            yield return new WaitForSeconds(0.75f);
        }

        yield return new WaitForSeconds(0.7f);
        if (Config.Instance.matchCounter == 26)
        {
            StartCoroutine(WinTransition());
        }
        else
        {
            spaceBabyController.BabyHappy();
        }
    }

    private IEnumerator FoodMove(Sprite foodSprite)
    {
        GameObject foody = Instantiate(foodPrefab, spaceShip.transform.position, Quaternion.identity, spaceShip.transform);
        foody.GetComponent<SpriteRenderer>().sprite = foodSprite;

        float timeElapsed = 0;
        while (timeElapsed < feedDuration)
        {
            float t = timeElapsed / feedDuration;
            foody.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            foody.transform.localScale = Vector2.Lerp(startScale, endScale, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        foody.transform.position = endPosition;
        foody.transform.localScale= endScale;
        yield return null;

        spaceBabyController.PlayEatSound();
        Destroy(foody);
        babyScale.x += babyScaleIncrease;
        spaceBabyController.gameObject.transform.localScale = babyScale;
    }

    private IEnumerator WinTransition()
    {
        spaceBabyController.BabyHappy();
        panelOverlay.SetActive(true);
        Image panelImage = panelOverlay.GetComponent<Image>();
        yield return Animate.FadeImage(panelImage, GameValues.FadeColors.grayFadeIn, GameValues.AnimationDurataions.gameSummaryBabyFade);
        spaceBabyController.gameObject.SetActive(false);
        babyPlanet.SetActive(true);
        yield return Animate.FadeImage(panelImage, GameValues.FadeColors.grayFadeOut, GameValues.AnimationDurataions.gameSummaryBabyFade);
        panelOverlay.SetActive(false);
    }
}
