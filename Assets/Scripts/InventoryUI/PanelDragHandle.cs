using UnityEngine;
using UnityEngine.EventSystems;

public class PanelDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform _panel;
    [SerializeField] private RectTransform _dragAllowedBounds;
    private Vector2 _dragOffset; // Difference between panel's pivot position and player's click position

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_panel.parent, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            _dragOffset = (Vector2)_panel.localPosition - localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_panel.parent, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
        {
            _panel.localPosition = new Vector3(localPointerPosition.x + _dragOffset.x, localPointerPosition.y + _dragOffset.y, _panel.localPosition.z);
            ClampDrag();
        }
    }

    private void ClampDrag()
    {
        RectTransform dragHandleRectTransform = (RectTransform)transform;
        Bounds dragHandleBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_dragAllowedBounds, dragHandleRectTransform);
        Rect allowedRect = _dragAllowedBounds.rect;
        Vector2 dragPushback = Vector2.zero;

        if (dragHandleBounds.min.x < allowedRect.xMin)
            dragPushback.x = allowedRect.xMin - dragHandleBounds.min.x;
        else if (dragHandleBounds.max.x > allowedRect.xMax)
            dragPushback.x = allowedRect.xMax - dragHandleBounds.max.x;
        if (dragHandleBounds.min.y < allowedRect.yMin)
            dragPushback.y = allowedRect.yMin - dragHandleBounds.min.y;
        else if (dragHandleBounds.max.y > allowedRect.yMax)
            dragPushback.y = allowedRect.yMax - dragHandleBounds.max.y;

        _panel.localPosition += new Vector3(dragPushback.x, dragPushback.y, 0.0f);
    }
}
