using UnityEngine;

public class ButtonAnimScript : MonoBehaviour
{
    [SerializeField]
    private GameObject pressAnimation;

    [SerializeField]
    private void ButtonPressed()
    {
        pressAnimation.SetActive(true);
    }

    [SerializeField]
    private void ButtonReleased()
    {
        pressAnimation.SetActive(false);
    }
}
