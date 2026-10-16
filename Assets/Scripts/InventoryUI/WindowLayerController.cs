using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WindowLayerController : MonoBehaviour
{
    [SerializeField] private Vector2 _windowCascadeStep;

    private void Update()
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

    public void PositionNewWindow(RectTransform window)
    {
        Vector2 initialAnchoredPosition = window.anchoredPosition;
        Vector2 initialTopLeftPoint = GetTopLeftPointOfWindow(window);
        List<Vector2> occupiedPositions = new();
        foreach (Transform otherWindow in transform) // All windows are children of this WindowLayerController
        {
            if (window == otherWindow)
                continue; // Skip this one, it's not an "other" window
            if (!otherWindow.gameObject.activeInHierarchy)
                continue;
            occupiedPositions.Add(GetTopLeftPointOfWindow((RectTransform)otherWindow));
        }
        float squaredCascadeStepDistanceTolerance = _windowCascadeStep.sqrMagnitude / 4.0f; // Half of _windowCascadeStep, squared
        for (int positionIndex = 0; positionIndex <= occupiedPositions.Count; ++positionIndex)
        {
            // Start at the initial top left point, and check that point, plus all points a number of cascade steps away, for an empty space (top-left corner position) to position this window.
            // A space is empty if the top-left corner of another window isn't inside a circle centered at the candidate location with a radius of half of _windowCascadeStep.
            // Go one more step than the number of open windows (the <= in the for-loop) in case all spaces are occupied.
            Vector2 candidateTopLeftPoint = initialTopLeftPoint + positionIndex * _windowCascadeStep;
            if (!occupiedPositions.Exists(occupiedPosition => (occupiedPosition - candidateTopLeftPoint).sqrMagnitude < squaredCascadeStepDistanceTolerance))
            {
                window.anchoredPosition = initialAnchoredPosition + positionIndex * _windowCascadeStep;
                break;
            }
        }
    }

    private Vector2 GetTopLeftPointOfWindow(RectTransform window)
    {
        Vector3[] fourCornersArray = new Vector3[4];
        window.GetWorldCorners(fourCornersArray);
        Vector3 topLeftCornerWorld = fourCornersArray[1]; // The top left corner is assigned to index 1 after calling GetWorldCorners()
        Vector3 topLeftCornerLocal = transform.InverseTransformPoint(topLeftCornerWorld);
        return topLeftCornerLocal;
    }
}
