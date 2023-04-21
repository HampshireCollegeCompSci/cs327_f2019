using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchCards : MonoBehaviour
{
    [SerializeField]
    private GameObject gameUI, matchExplosionPrefab, matchPointsPrefab;

    public void Match(CardScript card1Script, CardScript card2Script, GameObject selectedCardCopy)
    {
        // stop the hologram fade in coroutine so that its alpha value doesn't change anymore
        card1Script.StopAllCoroutines();
        card2Script.StopAllCoroutines();

        int points = GameValues.Points.matchPoints + (Config.Instance.consecutiveMatches * GameValues.Points.scoreMultiplier);
        StartCoroutine(MatchEffect(points, selectedCardCopy));

        card2Script.MoveCard(Constants.CardContainerType.MatchedPile, MatchedPileScript.Instance.gameObject);
        card1Script.MoveCard(Constants.CardContainerType.MatchedPile, MatchedPileScript.Instance.gameObject);

        ScoreScript.Instance.UpdateScore(points);
        SoundEffectsController.Instance.FoodMatch(card1Script.Card.Suit);
        SpaceBabyController.Instance.BabyEat();
    }

    private IEnumerator MatchEffect(int points, GameObject selectedCardCopy)
    {
        // get the hologram object and make sure it stays where it is
        GameObject comboHologram = selectedCardCopy.GetComponent<CardScript>().HologramFood;
        comboHologram.transform.parent = null;
        SpriteRenderer comboSR = comboHologram.GetComponent<SpriteRenderer>();
        comboSR.color = Color.white;

        Vector3 position = selectedCardCopy.transform.position;
        // random rotation
        GameObject matchExplosion = Instantiate(matchExplosionPrefab, position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        matchExplosion.transform.localScale = new Vector3(GameValues.Transforms.matchExplosionScale, GameValues.Transforms.matchExplosionScale);

        // instantiate the points slightly below
        position.y += 0.25f;
        GameObject matchPointsEffect = Instantiate(matchPointsPrefab, position, Quaternion.identity, gameUI.transform);

        // set the points readout
        Text pointText = matchPointsEffect.GetComponent<Text>();
        pointText.text = $"+{points} ";
        // set the combo readout
        Text comboText = matchPointsEffect.transform.GetChild(0).GetComponent<Text>();

        comboText.text = (Config.Instance.consecutiveMatches >= 1) switch
        {
            true => $"X{Config.Instance.consecutiveMatches + 1} COMBO",
            false => "",
        };

        // set the color of the points and combo
        Color pointComboColor = Config.Instance.consecutiveMatches switch
        {
            0 => Config.Instance.CurrentColorMode.Match.Color,
            1 => Config.Instance.CurrentColorMode.Move.Color,
            >= 2 => Config.Instance.CurrentColorMode.Over.Color,
            _ => throw new System.NotImplementedException($"how are there {Config.Instance.consecutiveMatches} number of matches?")
        };

        pointText.color = pointComboColor;
        comboText.color = pointComboColor;

        // get the start and end values for the points
        Color pointFadeColor = pointText.color;
        Vector3 pointScaleStart = Vector3.zero;
        Vector3 pointScaleEnd = Vector3.one;

        // lerp scale up and fade in only the points
        float duration = GameValues.AnimationDurataions.comboPointsFadeIn;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            matchPointsEffect.transform.localScale = Vector3.Lerp(pointScaleStart, pointScaleEnd, t);

            pointFadeColor.a = Mathf.Lerp(0, 1, t);
            pointText.color = pointFadeColor;
            comboText.color = pointFadeColor;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(GameValues.AnimationDurataions.comboWait);

        // how much the scale up will be
        Vector2 scaleMulti = new(1.2f, 1.2f);

        // get the start and end values for the combo object
        Color comboFadeColor = comboSR.color;
        Vector3 comboScaleStart = comboSR.transform.localScale;
        Vector3 comboScaleEnd = Vector3.Scale(comboScaleStart, scaleMulti);

        // get the start and end values for the points
        pointScaleStart = pointScaleEnd;
        pointScaleEnd = Vector3.Scale(pointScaleStart, scaleMulti);

        // lerp scale up and fade out both points and combo object
        duration = GameValues.AnimationDurataions.comboFadeOut;
        timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            float alpha = Mathf.Lerp(1, 0, t);
            // scale up
            matchPointsEffect.transform.localScale = Vector3.Lerp(pointScaleStart, pointScaleEnd, t);
            comboSR.transform.localScale = Vector3.Lerp(comboScaleStart, comboScaleEnd, t); ;
            // point color
            pointFadeColor.a = alpha;
            pointText.color = pointFadeColor;
            comboText.color = pointFadeColor;
            // combo object color
            comboFadeColor.a = alpha;
            comboSR.color = comboFadeColor;

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // destroy the temporary objects
        Destroy(matchPointsEffect);
        Destroy(matchExplosion);
        Destroy(comboHologram);
    }
}
