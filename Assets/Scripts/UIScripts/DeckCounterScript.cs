using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeckCounterScript : MonoBehaviour
{
    // Singleton instance.
    public static DeckCounterScript Instance { get; private set; }

    private enum DeckStatus
    {
        Populated,
        Empty,
        Flip
    }

    private DeckStatus _status;
    private int _counterNumber;
    private Text counterText;
    private Coroutine counterCoroutine;

    // Initialize the singleton instance.
    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.ArgumentException("there should not already be an instance of this");
        }

        Instance = this;
        counterText = GetComponent<Text>();
    }

    private int CounterNumber
    {
        get => _counterNumber;
        set
        {
            if (_counterNumber == value) return;
            _counterNumber = value;
            counterText.text = value.ToString();
        }
    }

    private DeckStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            switch (value)
            {
                case DeckStatus.Populated:
                    counterText.fontSize = 25;
                    break;
                case DeckStatus.Empty:
                    counterText.fontSize = 18;
                    counterText.text = "EMPTY";
                    break;
                case DeckStatus.Flip:
                    counterText.fontSize = 18;
                    counterText.text = "FLIP";
                    break;
            }
        }
    }

    /// <summary>
    /// Call this after the end of a game load or undo
    /// </summary>
    public void UpdateCounterInstantly()
    {
        TryStopCounterCoroutine();

        int deckCardCount = DeckScript.Instance.CardList.Count;
        if (deckCardCount != 0)
        {
            Status = DeckStatus.Populated;
            CounterNumber = deckCardCount;
        }
        else
        {
            _counterNumber = 0;
            ChangeStatus();
        }
    }

    public void TryChangeStatus()
    {
        if (Status == DeckStatus.Populated) return;
        ChangeStatus();
    }

    public void UpdateCounter(int update, float duration)
    {
        if (update == 0) return;

        if (Status != DeckStatus.Populated)
        {
            Status = DeckStatus.Populated;
            counterText.text = "0";
        }

        TryStopCounterCoroutine();

        int newCounterNumber = CounterNumber + update;
        // how fast each number should be added to the counter, +1 is used to sync things visually
        WaitForSeconds delay = new(duration / (Mathf.Abs(update) + 1));
        if (update > 0)
        {
            counterCoroutine = StartCoroutine(AddToText(newCounterNumber, delay));
        }
        else
        {
            counterCoroutine = StartCoroutine(SubtractFromText(newCounterNumber, delay));
        }
    }

    private IEnumerator AddToText(int newValue, WaitForSeconds dely)
    {
        while (CounterNumber < newValue)
        {
            yield return dely;
            CounterNumber++;
        }

        counterCoroutine = null;
    }

    private IEnumerator SubtractFromText(int newValue, WaitForSeconds dely)
    {
        while (CounterNumber > newValue)
        {
            yield return dely;
            CounterNumber--;
        }

        counterCoroutine = null;
        if (CounterNumber == 0)
        {
            ChangeStatus();
        }
    }

    /// <summary>
    /// Call this when the deck is empty and the counter status needs to be changed
    /// </summary>
    private void ChangeStatus()
    {
        bool canFlip = WastepileScript.Instance.CardList.Count > GameValues.GamePlay.cardsToDeal;
        Status = canFlip ? DeckStatus.Flip : DeckStatus.Empty;
    }

    private void TryStopCounterCoroutine()
    {
        if (counterCoroutine == null) return;
        StopCoroutine(counterCoroutine);
        counterCoroutine = null;
    }
}
