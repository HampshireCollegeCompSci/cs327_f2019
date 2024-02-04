using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NextCycle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public static NextCycle Instance { get; private set; }
    private static readonly WaitForSeconds endCycleDelay = new(0.1f),
        emptyCycleDelay = new(2.2f);

    [SerializeField]
    private Sprite buttonUp, buttonDown;
    private Image buttonImage;

    private bool mouseOverButton, mousePressingButton;

    public bool ButtonReady { get; set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            buttonImage = this.GetComponent<Image>();
            ButtonReady = true;
        }
        else if (Instance != this)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOverButton = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOverButton = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!ButtonReady || GameInput.Instance.InputStopped) return;
        ButtonReady = false;
        mousePressingButton = true;
        GameInput.Instance.InputStopped = true;
        KnobDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!mousePressingButton) return;
        mousePressingButton = false;
        GameInput.Instance.InputStopped = false;

        bool somethingWillHappen = Actions.ActionsDone != 0 || !Actions.AreFoundationsEmpty();
        if (mouseOverButton && somethingWillHappen)
        {
            ManualStartCycleButton();
        }
        else
        {
            KnobUp();
        }
    }

    private void ManualStartCycleButton()
    {
        AchievementsManager.FailedAlwaysMoves();
        SoundEffectsController.Instance.VibrateMedium();
        StartCycle();
    }

    public void StartCycle()
    {
        GameInput.Instance.InputStopped = true;
        ButtonReady = false;
        KnobDown();
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
                KnobUp();
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
        KnobUp();
        Actions.NextCycleUpdate();
        //ButtonReady = !Config.Instance.TutorialOn;
    }

    public void KnobDown()
    {
        buttonImage.sprite = buttonDown;
    }

    public void KnobUp()
    {
        buttonImage.sprite = buttonUp;
        ButtonReady = !Config.Instance.TutorialOn;
    }

}
