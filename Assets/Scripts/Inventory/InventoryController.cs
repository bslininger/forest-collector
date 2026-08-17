using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private Inventory _controlledInventory;
    private Dictionary<InventorySlotUIController, int> slotUIControllerToIndexMap;

    private void Awake()
    {
        slotUIControllerToIndexMap = new Dictionary<InventorySlotUIController, int>();
    }

    public void RegisterUIInventorySlot(InventorySlotUIController slotUIController, int index)
    {
        slotUIControllerToIndexMap[slotUIController] = index;
    }

    public void UnregisterUIInventorySlot(InventorySlotUIController slotUIController)
    {
        slotUIControllerToIndexMap.Remove(slotUIController);
    }

    public void ClearAllRegistrations()
    {
        slotUIControllerToIndexMap.Clear();
    }

    private void PublishInventoryUpdate(InventoryOperationResult inventoryOperationResult)
    {
        if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoOperation || (!inventoryOperationResult.CursorSlotChanged && inventoryOperationResult.ChangedSlots.Count == 0))
            return;

        if (!inventoryOperationResult.CursorSlotChanged)
            PublishInventoryUpdate(inventoryOperationResult.ChangedSlots.ToArray());
        else
            PublishInventoryUpdate(inventoryOperationResult.ChangedSlots.Prepend(new InventoryOperationResult.ChangedSlot(_controlledInventory, Inventory.CursorSlotIndex)).ToArray());
    }

    private void PublishInventoryUpdate(params InventoryOperationResult.ChangedSlot[] changedSlots)
    {
        EventManager.TriggerInventoryUpdateEvent(changedSlots);
    }

    public InventoryOperationResult HandleAddItemToInventory(Item item, int amountToAdd, int? slotIndexChoice = null, bool allowOverflowOutsideChosenSlot = true)
    {
        if (item == null)
        {
            Debug.LogWarning("Tried to add a null item to the inventory.");
            return InventoryOperationResult.NoOperation();
        }
        InventoryOperationResult inventoryOperationResult = _controlledInventory.AddItem(item, amountToAdd, slotIndexChoice, allowOverflowOutsideChosenSlot);
        if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.ItemPartiallyAdded || inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoSpace)
            Debug.LogWarning($"Out of {amountToAdd} items to add, {inventoryOperationResult.LeftoverItemCount} were lost due to not having space in the inventory. (Overflow to other slots was {(allowOverflowOutsideChosenSlot ? "" : "not ")}enabled.)");
        PublishInventoryUpdate(inventoryOperationResult);
        return inventoryOperationResult;
    }

    public InventoryOperationResult HandlePutItemInCursorSlot(Item item, int amount)
    {
        if (item == null)
        {
            Debug.LogWarning("Tried to add a null item to the cursor.");
            return InventoryOperationResult.NoOperation();
        }
        InventoryOperationResult inventoryOperationResult = _controlledInventory.PutItemInCursorSlot(item, amount);
        if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoSpace)
            Debug.LogWarning("Tried to put something in the cursor inventory slot while it already had something in it.");
        else if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoOperation || !inventoryOperationResult.CursorSlotChanged)
            Debug.LogWarning("No change when attempting to add an item to the cursor slot.");

        PublishInventoryUpdate(inventoryOperationResult);
        return inventoryOperationResult;
    }

    public InventoryOperationResult HandleTakeAllFromCursorSlot(out Item item, out int amount)
    {
        InventoryOperationResult inventoryOperationResult = _controlledInventory.TakeAllFromCursorSlot(out item, out amount);
        PublishInventoryUpdate(inventoryOperationResult);
        return inventoryOperationResult;
    }

}
