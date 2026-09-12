using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WindowLayerController : MonoBehaviour
{
    void Update()
    {
        if (Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 cursorPosition = Pointer.current.position.ReadValue();
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = cursorPosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            if (raycastResults.Count == 0)
                return;
            Transform hitTarget = raycastResults[0].gameObject.transform;  // The objects in raycastResults are ordered by priority, with the "topmost" element at position 0.
            if (hitTarget == transform || !hitTarget.IsChildOf(transform))  // Make sure the hit object is somewhere under the window layer in the hierarchy.
                return;
            while (hitTarget.parent != null && hitTarget.parent != transform)
                hitTarget = hitTarget.parent;
            hitTarget.SetAsLastSibling();
        }
    }
}
