using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    private bool loaded;

    private void Start()
    {
        loaded = true;
        CameraBoxer.Instance.AddCamera(_camera);
    }

    private void OnDestroy()
    {
        if (!loaded) return;
        CameraBoxer.Instance.RemoveCamera(_camera);
    }
}
