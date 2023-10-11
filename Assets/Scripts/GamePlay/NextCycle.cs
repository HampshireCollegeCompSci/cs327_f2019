using System.Collections;
using UnityEngine;

public class NextCycle : MonoBehaviour
{
    public static NextCycle Instance;
    private static readonly WaitForSeconds endCycleDelay = new(0.1f),
        emptyCycleDelay = new(2.2f);

    [SerializeField]
    private UnityEngine.UI.Button nextCycleButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    public void ManualStartCycleButton()
    {
        if (GameInput.Instance.InputStopped) return;
        if (Config.Instance.TutorialOn)
        {
            if (!Config.Instance.nextCycleEnabled) return;
            Config.Instance.nextCycleEnabled = false;
            nextCycleButton.interactable = false;
        }
        AchievementsManager.FailedAlwaysMoves();
        SoundEffectsController.Instance.VibrateMedium();
        StartCycle();
    }

    public void StartCycle()
    {
        GameInput.Instance.InputStopped = true;
        ActionCountScript.Instance.KnobDown();
        SpaceBabyController.Instance.BabyActionCounter();

        if (Actions.AreFoundationsEmpty())
        {
            StartCoroutine(EmptyCycle());
            return;
        }
        StartCoroutine(Cycle());
    }

    private IEnumerator Cycle()
    {
        foreach (FoundationScript foundationScript in GameInput.Instance.foundationScripts)
        {
            if (foundationScript.CardList.Count == 0) continue;

            GameObject topFoundationCard = foundationScript.CardList[^1];
            CardScript topCardScript = topFoundationCard.GetComponent<CardScript>();
            ReactorScript reactorScript = GameInput.Instance.reactorScripts[topCardScript.Card.Suit.Index];

            // turn off the moving cards hologram and make it appear in front of everything
            topCardScript.Hologram = false;
            topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = Constants.SortingLayerIDs.selectedCards;
            topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = Constants.SortingLayerIDs.selectedCards;

            // immediately unhide the next possible top foundation card and start its hologram
            if (foundationScript.CardList.Count > 1)
            {
                CardScript nextTopFoundationCard = foundationScript.CardList[^2].GetComponent<CardScript>();
                if (nextTopFoundationCard.Hidden)
                {
                    nextTopFoundationCard.NextCycleReveal();
                }
            }

            yield return Animate.SmoothstepTransform(topFoundationCard.transform,
                topFoundationCard.transform.position,
                reactorScript.GetNextCardPosition(),
                GameValues.AnimationDurataions.cardsToReactor);

            // set the sorting layer back to default
            topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = Constants.SortingLayerIDs.cards;
            topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = Constants.SortingLayerIDs.cards;

            SoundEffectsController.Instance.CardToReactorSound();
            topCardScript.MoveCard(Constants.CardContainerType.Reactor, reactorScript.gameObject, isCycle: true);

            // if the game is lost during the next cycle stop immediately
            if (Actions.GameOver)
            {
                Actions.MoveCounter++;
                GameInput.Instance.InputStopped = false;
                ActionCountScript.Instance.KnobUp();
                yield break;
            }
        }
        yield return endCycleDelay;
        EndCycle();
    }

    private IEnumerator EmptyCycle()
    {
        yield return emptyCycleDelay;
        EndCycle();
    }

    private void EndCycle()
    {
        GameInput.Instance.InputStopped = false;
        ActionCountScript.Instance.KnobUp();
        Actions.NextCycleUpdate();
    }
}
