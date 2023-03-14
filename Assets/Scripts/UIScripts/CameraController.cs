using UnityEngine;

public class CameraController : MonoBehaviour
{
    private void Start()
    {
        Config.Instance.AddCamera(this.gameObject.GetComponent<Camera>());
    }

    private void OnDestroy()
    {
        Config.Instance.RemoveCamera(this.gameObject.GetComponent<Camera>());
    }
}
