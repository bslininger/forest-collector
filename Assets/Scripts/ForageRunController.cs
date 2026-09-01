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
    [SerializeField] private InputActionReference _uiNavigateAction;
    private bool _dialogueBoxOpen = false;

    private void OnEnable()
    {
        _clickAction.action.Enable();
        _clickAction.action.performed += HandleClick;
        _interactableObjectDetector.InteractableObjectExitedRange += HandleInteractableObjectExitedRange;
    }

    private void OnDisable()
    {
        _interactableObjectDetector.InteractableObjectExitedRange -= HandleInteractableObjectExitedRange;
        _clickAction.action.performed -= HandleClick;
        _clickAction.action.Disable();
    }

    private void Start()
    {
        SetPlayerMovementEnabled(true);
    }

    private void SetPlayerMovementEnabled(bool enabled)
    {
        _playerController.SetMovementEnabled(enabled);
        if (enabled)
            _uiNavigateAction.action.Disable();
        else
            _uiNavigateAction.action.Enable();
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
            IClickInteractableObject interactableObject = hit.collider.GetComponentInParent<IClickInteractableObject>();
            if (interactableObject == null)
                return;
            _interactableObjectDetector.TryInteract(interactableObject);
        }
    }

    public void HandleCollectibleItemInteract(ItemPickup collectedItem)
    {
        if (_playerInventoryController.ItemInCursorSlot)
            return;

        InventoryOperationResult inventoryOperationResult = _playerInventoryController.HandlePutItemInCursorSlot(collectedItem.InventoryItem, 1);
        if (inventoryOperationResult.OperationResultType != InventoryOperationResult.ResultType.PickupToCursor)
            return;
        collectedItem.Remove();
        _inventoryUIManager.ActivateInventoryPanel();
    }

    public void OnAdvanceDialogueAction(InputAction.CallbackContext context)
    {
        CloseDialogueBox();
    }

    public void DisplayDialogue(string dialogueLine)
    {
        if (_dialogueBoxOpen)
            return;

        SetPlayerMovementEnabled(false);
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
        SetPlayerMovementEnabled(true);
        _interactableObjectDetector.SetInteractionEnabled(true);
        _dialogueBoxOpen = false;
    }

    public void HandleWorldContainerInteract(InventoryContainer worldContainer, string containerName, Sprite containerIcon)
    {
        _inventoryUIManager.OpenContainer(worldContainer, containerName, containerIcon);
    }

    private void HandleInteractableObjectExitedRange(IInteractableObject interactableObject)
    {
        if (interactableObject is InteractableContainer interactableContainer)
            _inventoryUIManager.CloseContainerIfOpened(interactableContainer.InventoryContainer);
    }
}
