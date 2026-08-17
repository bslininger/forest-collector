using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryItemCursorFollower : MonoBehaviour
{
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private GameObject _slotPrefab;
    private RectTransform _rectTransform;
    private GameObject _slotUIElement;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // The cursor follower stays enabled all the time; it's the slotUIElement that is created and destroyed when items come and go from the cursor.
        EventManager.InventoryUpdateEvent += UpdateCursorItem;
    }

    private void OnDisable()
    {
        EventManager.InventoryUpdateEvent -= UpdateCursorItem;
    }

    void Update()
    {
        if (_slotUIElement == null)
            return;
        if (Mouse.current == null)
            return;

        // Convert screen point to canvas-local position
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform.parent as RectTransform,
            Mouse.current.position.ReadValue(),
            null,  // Using Screen Space - Overlay
            out localMousePosition
        );

        _rectTransform.anchoredPosition = localMousePosition + new Vector2(_rectTransform.rect.width / 2, -_rectTransform.rect.height / 2);
    }

    private void UpdateCursorItem(InventoryOperationResult.ChangedSlot[] changedSlots)
    {
        if (!Array.Exists(changedSlots, changedSlot => changedSlot.Inventory == _playerInventory && changedSlot.Index == Inventory.CursorSlotIndex))
            return;
        // Called when notified that the item on the cursor has changed
        InventorySlotDisplayInformation cursorSlotDisplayInformation = _playerInventory.GetSlotDisplayInformation(Inventory.CursorSlotIndex);
        if (!cursorSlotDisplayInformation.HasItem)
        {
            Destroy(_slotUIElement);
            _slotUIElement = null;
        }
        else
        {
            if (_slotUIElement == null)
                _slotUIElement = Instantiate(_slotPrefab, transform);
            // Set the RectTransform width and height to 72 x 72 to match other inventory slots, and the local position to x = 32, y = 32 to sit at the center of the parent container (which is 64 x 64)
            RectTransform rectTransform = _slotUIElement.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(72, 72);
            rectTransform.anchoredPosition = new Vector2(32, 32);
            // The background image for a slot shouldn't show up on the cursor, and regardless, it shouldn't be clickable.
            Image slotBackgroundImage = _slotUIElement.GetComponent<Image>();
            slotBackgroundImage.raycastTarget = false;
            slotBackgroundImage.enabled = false;
            InventorySlotUIController slotUIController = _slotUIElement.transform.GetComponent<InventorySlotUIController>();
            slotUIController.InputLockProvider = UIManager.Instance;
            slotUIController.SetSlot(cursorSlotDisplayInformation);
        }
    }
}
