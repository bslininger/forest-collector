using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryInteractionController : MonoBehaviour
{
    private struct InventorySlotRegistration
    {
        // This type refers to an index within a specific inventory, and InventorySlotUIController objects are assigned to the one of these that they control.
        public Inventory Inventory { get; }
        public int Index { get; }
        public InventorySlotRegistration(Inventory inventory, int index)
        {
            this.Inventory = inventory;
            this.Index = index;
        }
    }

    [SerializeField] private Inventory _playerInventory;
    private Dictionary<InventorySlotUIController, InventorySlotRegistration> _slotUIControllerToInventoryIndexMap;
    private Inventory _stackSizeSelectorSourceInventory;

    private void Awake()
    {
        _slotUIControllerToInventoryIndexMap = new Dictionary<InventorySlotUIController, InventorySlotRegistration>();
    }

    private void OnEnable()
    {
        EventManager.InventorySlotClickedEvent += HandleUIInventorySlotClicked;
    }

    private void OnDisable()
    {
        EventManager.InventorySlotClickedEvent -= HandleUIInventorySlotClicked;
    }

    public void RegisterUIInventorySlot(InventorySlotUIController slotUIController, Inventory inventory, int index)
    {
        _slotUIControllerToInventoryIndexMap[slotUIController] = new InventorySlotRegistration(inventory, index);
    }

    public void UnregisterUIInventorySlot(InventorySlotUIController slotUIController)
    {
        _slotUIControllerToInventoryIndexMap.Remove(slotUIController);
    }

    public void ClearRegistrationsByInventory(Inventory inventory)
    {
        List<KeyValuePair<InventorySlotUIController, InventorySlotRegistration>> registrationsToRemove = new();
        foreach (KeyValuePair<InventorySlotUIController, InventorySlotRegistration> registration in _slotUIControllerToInventoryIndexMap)
            if (registration.Value.Inventory == inventory)
                registrationsToRemove.Add(registration);
        foreach (KeyValuePair<InventorySlotUIController, InventorySlotRegistration> registrationToRemove in registrationsToRemove)
            _slotUIControllerToInventoryIndexMap.Remove(registrationToRemove.Key);
    }

    private void ClearAllRegistrations()
    {
        _slotUIControllerToInventoryIndexMap.Clear();
    }

    private void HandleUIInventorySlotClicked(InventorySlotUIController controller)
    {
        if (!_slotUIControllerToInventoryIndexMap.TryGetValue(controller, out InventorySlotRegistration registration))
        {
            Debug.LogWarning("Clicked slot not registered with Inventory!");
            return;
        }

        InventoryOperationResult inventoryOperationResult;
        Inventory controlledInventory = registration.Inventory;
        int index = registration.Index;

        // Special keypresses: ctrl-click to pick 1 item from the stack, shift-click to pick up a specified number from the stack
        if (controller.SlotClickType == InventorySlotUIController.ClickType.Ctrl && !_playerInventory.ItemInCursorSlot)
            inventoryOperationResult = HandleUIInventorySlotClickedForSinglePull(controlledInventory, index);
        else if (controller.SlotClickType == InventorySlotUIController.ClickType.Shift && !_playerInventory.ItemInCursorSlot)
        {
            HandleUIInventorySlotClickedForStackSelection(controller, controlledInventory, index);
            return;
        }
        else
            inventoryOperationResult = _playerInventory.InteractCursorWithInventorySlot(controlledInventory, index);

        PublishInventoryUpdate(inventoryOperationResult);
    }

    private InventoryOperationResult HandleUIInventorySlotClickedForSinglePull(Inventory controlledInventory, int index)
    {
        return _playerInventory.TakeFromSlotIntoCursor(controlledInventory, index, 1);
    }

    private void HandleUIInventorySlotClickedForStackSelection(InventorySlotUIController controller, Inventory controlledInventory, int index)
    {
        Action<int> acceptButtonAction = (amountTaken) => OnStackSizeSelectorAccepted(controlledInventory, index, amountTaken);
        Action cancelButtonAction = () => OnStackSizeSelectorCanceled();

        // Get location for stack size selector panel to open, then open it
        RectTransform inventorySlotRectTransform = controller.GetComponent<RectTransform>();
        if (UIManager.Instance.ShowStackSizeSelectorPanel(controlledInventory.GetSlotDisplayInformation(index), inventorySlotRectTransform, acceptButtonAction, cancelButtonAction))
            _stackSizeSelectorSourceInventory = controlledInventory;
    }

    private void OnStackSizeSelectorAccepted(Inventory controlledInventory, int index, int amountTaken)
    {
        _stackSizeSelectorSourceInventory = null;
        InventoryOperationResult inventoryOperationResult = _playerInventory.TakeFromSlotIntoCursor(controlledInventory, index, amountTaken);
        PublishInventoryUpdate(inventoryOperationResult);
    }

    private void OnStackSizeSelectorCanceled()
    {
        _stackSizeSelectorSourceInventory = null;
    }

    public void CancelStackSizeSelectorPanelForInventory(Inventory closingInventory)
    {
        if (closingInventory == _stackSizeSelectorSourceInventory)
            UIManager.Instance.CancelStackSizeSelectorPanel();
    }

    private void PublishInventoryUpdate(InventoryOperationResult inventoryOperationResult)
    {
        if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoOperation || (!inventoryOperationResult.CursorSlotChanged && inventoryOperationResult.ChangedSlots.Count == 0))
            return;

        if (!inventoryOperationResult.CursorSlotChanged)
            PublishInventoryUpdate(inventoryOperationResult.ChangedSlots.ToArray());
        else
            PublishInventoryUpdate(inventoryOperationResult.ChangedSlots.Prepend(new InventoryOperationResult.ChangedSlot(_playerInventory, Inventory.CursorSlotIndex)).ToArray());
    }

    private void PublishInventoryUpdate(params InventoryOperationResult.ChangedSlot[] changedSlots)
    {
        EventManager.TriggerInventoryUpdateEvent(changedSlots);
    }
}
