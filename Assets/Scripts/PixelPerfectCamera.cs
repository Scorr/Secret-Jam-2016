using UnityEngine;

/// <summary>
/// Turns an orthographic camera into a pixel perfect one.
/// </summary>
public class PixelPerfectCamera : MonoBehaviour
{
    private int _zoomLevel = 4; // 4 = 1:1
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        _camera.orthographicSize = _zoomLevel*(Screen.height*0.5f*0.01f*0.25f); // the fuck was i thinking??
    }
}