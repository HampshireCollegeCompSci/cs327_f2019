using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NextCycle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public static NextCycle Instance { get; private set; }
    private static readonly WaitForSeconds cycleDelay = new(0.2f),
        emptyCycleDelay = new(2.2f);

    [SerializeField]
    private Sprite buttonUp, buttonDown;
    [SerializeField]
    private Image buttonImage;

    private bool _buttonDisabled;
    private bool mouseOverButton, mousePressingButton;

    public bool ButtonDisabled
    {
        get => _buttonDisabled;
        set
        {
            if (_buttonDisabled == value) return;
            _buttonDisabled = value;
            if (value)
            {
                KnobDown();
                buttonImage.color = Color.gray;
            }
            else
            {
                KnobUp();
                buttonImage.color = Color.white;
            }
        }
    }

    private bool ButtonReady { get; set; }

    void Awake()
    {
        if (Instance != null)
            throw new System.Exception("there should not already be an instance of this");
        Instance = this;
        ButtonReady = true;
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
        if (ButtonDisabled || !ButtonReady || GameInput.Instance.InputStopped) return;
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
        VibrationController.Instance.VibrateMedium();
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
        yield return cycleDelay;
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
                    if (Config.Instance.TutorialOn)
                    {
                        nextTopFoundationCard.Obstructed = true;
                    }
                    nextTopFoundationCard.NextCycleReveal();
                }
            }

            yield return Animate.MoveTransformSmoothDamp(topFoundationCard.transform,
                reactorScript.GetNextCardPosition(),
                AutoPlacement.SpeedValue);

            // set the sorting layer back to default
            topFoundationCard.GetComponent<SpriteRenderer>().sortingLayerID = Constants.SortingLayerIDs.cards;
            topCardScript.Values.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID = Constants.SortingLayerIDs.cards;

            CardSounds.Instance.CardToReactorSound(reactorScript.gameObject.transform.position);
            topCardScript.MoveCard(Constants.CardContainerType.Reactor, reactorScript.gameObject, isCycle: true);
            
            if (Config.Instance.TutorialOn)
            {
                // the cards moved into the reactors are auto set to un-obstructed and need to be set again
                topCardScript.Obstructed = true;
            }

            // if the game is lost during the next cycle stop immediately
            if (Actions.GameOver)
            {
                Actions.MoveCounter++;
                GameInput.Instance.InputStopped = false;
                KnobUp();
                yield break;
            }
        }
        yield return cycleDelay;
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

        if (Config.Instance.TutorialOn && !ButtonDisabled)
        {
            ButtonDisabled = true;
        }
    }

    public void KnobDown()
    {
        buttonImage.sprite = buttonDown;
    }

    public void KnobUp()
    {
        buttonImage.sprite = buttonUp;
        ButtonReady = true;
    }
}
