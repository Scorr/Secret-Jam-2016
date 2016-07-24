using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Place on a button to have it be default selected when the UI loads.
/// </summary>
public class SelectButtonOnLoad : MonoBehaviour {

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
