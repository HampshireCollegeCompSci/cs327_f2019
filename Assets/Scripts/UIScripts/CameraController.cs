using UnityEngine;

public class CameraController : MonoBehaviour
{
    private bool loaded;

    private void Start()
    {
        loaded = true;
        Config.Instance.AddCamera(this.gameObject.GetComponent<Camera>());
    }

    private void OnDestroy()
    {
        if (!loaded) return;
        Config.Instance.RemoveCamera(this.gameObject.GetComponent<Camera>());
    }
}
