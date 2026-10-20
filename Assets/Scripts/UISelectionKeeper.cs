using UnityEngine;
using UnityEngine.EventSystems;

public class UISelectionKeeper : MonoBehaviour
{
    EventSystem eventSystem;
    GameObject lastSelected;

    void Awake() => eventSystem = GetComponent<EventSystem>();

    void Update()
    {
        var current = eventSystem.currentSelectedGameObject;
        if (current != null && current.activeInHierarchy)
        {
            lastSelected = current;
            return;
        }

        if (lastSelected != null && lastSelected.activeInHierarchy)
            eventSystem.SetSelectedGameObject(lastSelected);
    }
}
