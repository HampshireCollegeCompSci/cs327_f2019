using UnityEngine;

public class ContinueHider : MonoBehaviour
{
    public GameObject continueButton;
    private void Awake()
    {
        if (!SaveState.Exists())
        {
            continueButton.SetActive(false);
        }
    }
}
