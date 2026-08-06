using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ForageRunController : MonoBehaviour
{
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private InventoryController _playerInventoryController;
    [SerializeField] private InteractableObjectDetector _interactableObjectDetector;
    [SerializeField] private HUDManager _onScreenDisplay;
    [SerializeField] private UIManager _inventoryUIManager;
    [SerializeField] private InputActionReference _clickAction;
    [SerializeField] private InputActionReference _advanceDialogueAction;
    private ForageRunModel _gameModel;
    private bool _berryQuestCompleted = false;
    private bool _dialogueBoxOpen = false;

    public bool BerryQuestCompleted => _berryQuestCompleted;

    private void Awake()
    {
        _gameModel = new ForageRunModel();
        _onScreenDisplay.SetDeliveredItemsCountText(_gameModel.GetDeliveredItemsCount());
    }

    private void OnEnable()
    {
        _clickAction.action.Enable();
        _clickAction.action.performed += HandleClick;
    }

    private void OnDisable()
    {
        _clickAction.action.performed -= HandleClick;
        _clickAction.action.Disable();
    }

    private bool MousePointerIsOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        List<RaycastResult> raycastResults = new List<RaycastResult>(); // Will hold the list of UI elements that were hit by the raycast from the mouse pointer
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        return raycastResults.Count > 0;  // No results means no UI elements were clicked, and something in the world was clicked instead (or just empty space)
    }

    private void HandleClick(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (MousePointerIsOverUI(mousePosition))
            return;
        Ray ray = _worldCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ItemPickup itemPickup = hit.collider.GetComponentInParent<ItemPickup>();
            if (itemPickup == null)
                return;
            itemPickup.Pickup();
        }
    }

    public void HandleCollectibleItemInteract(ItemPickup collectedItem)
    {
        InventoryOperationResult inventoryOperationResult = _playerInventoryController.HandlePutItemInCursorSlot(collectedItem.InventoryItem, 1);
        if (inventoryOperationResult.OperationResultType != InventoryOperationResult.ResultType.PickupToCursor)
            return;
        collectedItem.Remove();
        _inventoryUIManager.ActivateInventoryPanel();
    }

    public void HandleAllItemsDelivery()
    {
        InventoryOperationResult inventoryOperationResult = _playerInventoryController.HandleTakeAllFromCursorSlot(out Item itemTaken, out int amountTaken);
        if (inventoryOperationResult.OperationResultType != InventoryOperationResult.ResultType.TakeFromCursor)
            return;

        _gameModel.RecordDeliveredItems(itemTaken.itemName, amountTaken);
        int berriesDelivered = _gameModel.GetSpecificDeliveredItemCount("Berry");
        _onScreenDisplay.SetDeliveredItemsCountText(_gameModel.GetDeliveredItemsCount());
        if (!_berryQuestCompleted && berriesDelivered >= 3)
            _berryQuestCompleted = true;
    }

    public void OnAdvanceDialogueAction(InputAction.CallbackContext context)
    {
        CloseDialogueBox();
    }

    public void DisplayDialogue(string dialogueLine)
    {
        if (_dialogueBoxOpen)
            return;

        _playerController.SetMovementEnabled(false);
        _interactableObjectDetector.SetInteractionEnabled(false);
        _onScreenDisplay.ShowDialogue(dialogueLine);
        _advanceDialogueAction.action.Enable();
        _advanceDialogueAction.action.performed += OnAdvanceDialogueAction;
        _dialogueBoxOpen = true;
    }

    public void CloseDialogueBox()
    {
        if (!_dialogueBoxOpen)
            return;

        _advanceDialogueAction.action.performed -= OnAdvanceDialogueAction;
        _advanceDialogueAction.action.Disable();
        _onScreenDisplay.CloseDialogueBox();
        _playerController.SetMovementEnabled(true);
        _interactableObjectDetector.SetInteractionEnabled(true);
        _dialogueBoxOpen = false;
    }
}
