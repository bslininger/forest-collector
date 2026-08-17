using UnityEngine;
using System;
using System.Collections.Generic;

public static class EventManager
{
    public static event Action<InventoryOperationResult.ChangedSlot[]> InventoryUpdateEvent;
    public static event System.Action<InventorySlotUIController> InventorySlotClickedEvent;

    public static void TriggerInventoryUpdateEvent(InventoryOperationResult.ChangedSlot[] changedSlots)
    {
        InventoryUpdateEvent?.Invoke(changedSlots);
    }

    public static void TriggerInventorySlotClickedEvent(InventorySlotUIController slotUIController)
    {
        InventorySlotClickedEvent.Invoke(slotUIController);
    }
}
