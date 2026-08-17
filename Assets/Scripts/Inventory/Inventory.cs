using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int inventorySize = 30;

    public class InventoryEntry
    {
        public Item item { get; }
        public int stackSize { get; private set; }

        public InventoryEntry(Item item, int stackSize)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item can not be null");
            if (stackSize <= 0)
                throw new ArgumentException("Stack size must be at least 1");
            if (stackSize > item.maxStack)
                throw new ArgumentException($"Given stack size {stackSize} exceeds item's maximum allowable stack size of {item.maxStack}");

            this.item = item;
            this.stackSize = stackSize;
        }

        public InventoryEntry(InventoryEntry other)
        {
            if (other == null)
                throw new ArgumentNullException("Tried to copy frum a null InventoryEntry");
            item = other.item;
            stackSize = other.stackSize;
        }

        public void AddToStack(int amountToAdd)
        {
            if (amountToAdd < 0)
                throw new ArgumentException("Can only add nonnegative amounts to the stack size");

            int newSize = stackSize + amountToAdd;
            if (newSize > item.maxStack)
                throw new InvalidOperationException($"Adding {amountToAdd} to the item's stack results in a stack size of {newSize}, larger than the maximum allowable stack size of {item.maxStack}");

            stackSize = newSize;
        }

        public void RemoveFromStack(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Can only add nonnegative amounts from the stack size");

            int newSize = stackSize - amount;
            if (newSize <= 0)
                throw new InvalidOperationException($"Stack size can not be reduced below 1; attempted to set the stack size to {newSize}");

            stackSize = newSize;
        }

        public void SetStackSize(int newStackSize)
        {
            if (newStackSize < 1 || newStackSize > item.maxStack)
                throw new ArgumentException($"Tried to set stack size to {newStackSize} which is out of bounds (1 to {item.maxStack}");
            stackSize = newStackSize;
        }
    }

    private InventoryEntry[] inventoryEntries;
    private InventoryEntry cursorInventoryEntry;

    private void Awake()
    {
        inventoryEntries = new InventoryEntry[inventorySize];
    }

    private void Update()
    {
    }

    private static InventoryOperationResult.ChangedSlot SlotChange(Inventory inventory, int index)
    {
        return new InventoryOperationResult.ChangedSlot(inventory, index);
    }

    public InventoryOperationResult InteractCursorWithInventorySlot(Inventory otherInventory, int otherInventorySlotIndex)
    {
        InventoryEntry clickedEntry = otherInventory.inventoryEntries[otherInventorySlotIndex];
        if (!ItemInCursorSlot)
        {
            if (clickedEntry != null)
            {
                // Pull item from inventory onto cursor
                return TakeFromSlotIntoCursor(otherInventory, otherInventorySlotIndex, clickedEntry.stackSize);
            }

            // Otherwise, do nothing.
            return InventoryOperationResult.NoOperation();
        }
        else // Item in cursor slot
        {
            if (clickedEntry == null)
            {
                // Empty inventory slot: move item from cursor to inventory slot
                return PlaceFromCursorIntoSlot(otherInventory, otherInventorySlotIndex);
            }
            else
            {
                if (cursorInventoryEntry.item != clickedEntry.item)
                {
                    // Different items: swap
                    return SwapCursorWithSlot(otherInventory, otherInventorySlotIndex);
                }
                else
                {
                    if (clickedEntry.stackSize < clickedEntry.item.maxStack)
                    {
                        // Same item, and there is room for more in its stack in the inventory slot: add to stack, and if stack fills, keep remainder on cursor
                        return MergeFromCursorIntoSlot(otherInventory, otherInventorySlotIndex);
                    }
                    else
                    {
                        // Same item, but the inventory slot's stack is full: swap
                        return SwapCursorWithSlot(otherInventory, otherInventorySlotIndex);
                    }
                }
            }
        }
    }

    public InventoryOperationResult InteractWithSlot(int index)
    {
        return HandleInventorySlotInteraction(index);
    }

    private InventoryOperationResult HandleInventorySlotInteraction(int index)
    {
        InventoryEntry clickedEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (!ItemInCursorSlot)
        {
            if (clickedEntry != null)
            {
                // Pull item from inventory onto cursor
                return TakeFromSlotIntoCursor(index, clickedEntry.stackSize);
            }

            // Otherwise, do nothing.
            return InventoryOperationResult.NoOperation();
        }
        else // Item in cursor slot
        {
            if (clickedEntry == null)
            {
                // Empty inventory slot: move item from cursor to inventory slot
                return PlaceFromCursorIntoSlot(index);
            }
            else
            {
                if (cursorInventoryEntry.item != clickedEntry.item)
                {
                    // Different items: swap
                    return SwapCursorWithSlot(index);
                }
                else
                {
                    if (clickedEntry.stackSize < clickedEntry.item.maxStack)
                    {
                        // Same item, and there is room for more in its stack in the inventory slot: add to stack, and if stack fills, keep remainder on cursor
                        return MergeFromCursorIntoSlot(index);
                    }
                    else
                    {
                        // Same item, but the inventory slot's stack is full: swap
                        return SwapCursorWithSlot(index);
                    }
                }
            }
        }
    }

    public InventoryOperationResult TakeFromSlotIntoCursor(int index, int amount)
    {
        if (index < 0 || index >= inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Inventory index must be between 0 and {inventoryEntries.Length - 1}; index given was {index}");
        if (ItemInCursorSlot)
            throw new InvalidOperationException("Tried to add an item to the cursor slot when it was already occupied.");

        InventoryEntry sourceEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (sourceEntry == null)
            throw new InvalidOperationException($"Tried to pull from inventory index {index}, but it was empty.");
        if (amount <= 0 || amount > sourceEntry.stackSize)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Amount to pull must be positive and less than or equal to the amount currently in the inventory slot ({sourceEntry.stackSize}); was given {amount}");

        if (amount < sourceEntry.stackSize)
        {
            cursorInventoryEntry = new InventoryEntry(sourceEntry.item, amount);
            sourceEntry.RemoveFromStack(amount);
        }
        else
        {
            cursorInventoryEntry = sourceEntry;
            inventoryEntries[index] = null;
        }

        return InventoryOperationResult.PickupToCursor(SlotChange(this, index));
    }

    public InventoryOperationResult TakeFromSlotIntoCursor(Inventory sourceInventory, int sourceInventorySlotIndex, int amount)
    {
        if (sourceInventorySlotIndex < 0 || sourceInventorySlotIndex >= sourceInventory.inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(sourceInventorySlotIndex), $"Inventory index must be between 0 and {sourceInventory.inventoryEntries.Length - 1}; index given was {sourceInventorySlotIndex}");
        if (ItemInCursorSlot)
            throw new InvalidOperationException("Tried to add an item to the cursor slot when it was already occupied.");

        InventoryEntry sourceEntry = sourceInventory.inventoryEntries[sourceInventorySlotIndex]; // same reference as sourceInventory.inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (sourceEntry == null)
            throw new InvalidOperationException($"Tried to pull from inventory index {sourceInventorySlotIndex}, but it was empty.");
        if (amount <= 0 || amount > sourceEntry.stackSize)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Amount to pull must be positive and less than or equal to the amount currently in the inventory slot ({sourceEntry.stackSize}); was given {amount}");

        if (amount < sourceEntry.stackSize)
        {
            cursorInventoryEntry = new InventoryEntry(sourceEntry.item, amount);
            sourceEntry.RemoveFromStack(amount);
        }
        else
        {
            cursorInventoryEntry = sourceEntry;
            sourceInventory.inventoryEntries[sourceInventorySlotIndex] = null;
        }

        return InventoryOperationResult.PickupToCursor(SlotChange(sourceInventory, sourceInventorySlotIndex));
    }

    private InventoryOperationResult PlaceFromCursorIntoSlot(Inventory destinationInventory, int destinationInventorySlotIndex)
    {
        if (destinationInventorySlotIndex < 0 || destinationInventorySlotIndex >= destinationInventory.inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(destinationInventorySlotIndex), $"Inventory index must be between 0 and {destinationInventory.inventoryEntries.Length - 1}; index given was {destinationInventorySlotIndex}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry destinationEntry = destinationInventory.inventoryEntries[destinationInventorySlotIndex]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (destinationEntry != null)
            throw new InvalidOperationException($"Tried to place an item into inventory index {destinationInventorySlotIndex}, but it was already occupied.");

        destinationInventory.inventoryEntries[destinationInventorySlotIndex] = cursorInventoryEntry;
        cursorInventoryEntry = null;

        return InventoryOperationResult.PlaceFromCursor(SlotChange(destinationInventory, destinationInventorySlotIndex));
    }

    private InventoryOperationResult PlaceFromCursorIntoSlot(int index)
    {
        if (index < 0 || index >= inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Inventory index must be between 0 and {inventoryEntries.Length - 1}; index given was {index}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry destinationEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (destinationEntry != null)
            throw new InvalidOperationException($"Tried to place an item into inventory index {index}, but it was already occupied.");

        inventoryEntries[index] = cursorInventoryEntry;
        cursorInventoryEntry = null;

        return InventoryOperationResult.PlaceFromCursor(SlotChange(this, index));
    }

    private InventoryOperationResult MergeFromCursorIntoSlot(int index)
    {
        if (index < 0 || index >= inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Inventory index must be between 0 and {inventoryEntries.Length - 1}; index given was {index}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry destinationEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (destinationEntry == null)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {index}, but the inventory slot was empty.");
        if (destinationEntry.item != cursorInventoryEntry.item)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {index}, but the cursor and inventory slots did not contain the same item.");
        if (destinationEntry.stackSize >= destinationEntry.item.maxStack)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {index}, but the inventory slot's stack was already full.");

        int stackOnCursor = cursorInventoryEntry.stackSize;
        int remainingSpace = destinationEntry.item.maxStack - destinationEntry.stackSize;
        int amountAddedToInventoryStack = Mathf.Min(stackOnCursor, remainingSpace);
        destinationEntry.AddToStack(amountAddedToInventoryStack);
        if (stackOnCursor - amountAddedToInventoryStack > 0)
        {
            cursorInventoryEntry.RemoveFromStack(amountAddedToInventoryStack);
        }
        else
        {
            cursorInventoryEntry = null;
        }

        return InventoryOperationResult.MergeFromCursor(SlotChange(this, index));
    }

    private InventoryOperationResult MergeFromCursorIntoSlot(Inventory destinationInventory, int destinationInventorySlotIndex)
    {
        if (destinationInventorySlotIndex < 0 || destinationInventorySlotIndex >= destinationInventory.inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(destinationInventorySlotIndex), $"Inventory index must be between 0 and {destinationInventory.inventoryEntries.Length - 1}; index given was {destinationInventorySlotIndex}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry destinationEntry = destinationInventory.inventoryEntries[destinationInventorySlotIndex]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (destinationEntry == null)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {destinationInventorySlotIndex}, but the inventory slot was empty.");
        if (destinationEntry.item != cursorInventoryEntry.item)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {destinationInventorySlotIndex}, but the cursor and inventory slots did not contain the same item.");
        if (destinationEntry.stackSize >= destinationEntry.item.maxStack)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {destinationInventorySlotIndex}, but the inventory slot's stack was already full.");

        int stackOnCursor = cursorInventoryEntry.stackSize;
        int remainingSpace = destinationEntry.item.maxStack - destinationEntry.stackSize;
        int amountAddedToInventoryStack = Mathf.Min(stackOnCursor, remainingSpace);
        destinationEntry.AddToStack(amountAddedToInventoryStack);
        if (stackOnCursor - amountAddedToInventoryStack > 0)
        {
            cursorInventoryEntry.RemoveFromStack(amountAddedToInventoryStack);
        }
        else
        {
            cursorInventoryEntry = null;
        }

        return InventoryOperationResult.MergeFromCursor(SlotChange(destinationInventory, destinationInventorySlotIndex));
    }

    private InventoryOperationResult SwapCursorWithSlot(int index)
    {
        if (index < 0 || index >= inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Inventory index must be between 0 and {inventoryEntries.Length - 1}; index given was {index}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry clickedInventoryEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (clickedInventoryEntry == null)
            throw new InvalidOperationException($"Tried to swap the cursor slot with inventory index {index}, but the inventory slot was empty.");

        inventoryEntries[index] = cursorInventoryEntry;
        cursorInventoryEntry = clickedInventoryEntry;

        return InventoryOperationResult.SwapWithCursor(SlotChange(this, index));
    }

    private InventoryOperationResult SwapCursorWithSlot(Inventory otherInventory, int otherInventorySlotIndex)
    {
        if (otherInventorySlotIndex < 0 || otherInventorySlotIndex >= otherInventory.inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(otherInventorySlotIndex), $"Inventory index must be between 0 and {otherInventory.inventoryEntries.Length - 1}; index given was {otherInventorySlotIndex}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry clickedInventoryEntry = otherInventory.inventoryEntries[otherInventorySlotIndex]; // same reference as otherInventory.inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (clickedInventoryEntry == null)
            throw new InvalidOperationException($"Tried to swap the cursor slot with inventory index {otherInventorySlotIndex}, but the inventory slot was empty.");

        otherInventory.inventoryEntries[otherInventorySlotIndex] = cursorInventoryEntry;
        cursorInventoryEntry = clickedInventoryEntry;

        return InventoryOperationResult.SwapWithCursor(SlotChange(otherInventory, otherInventorySlotIndex));
    }

    public InventoryOperationResult AddItem(Item item, int amountToAdd, int? slotIndexChoice = null, bool allowOverflowOutsideChosenSlot = true)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (slotIndexChoice.HasValue && (slotIndexChoice.Value < 0 || slotIndexChoice.Value >= inventoryEntries.Length))
            throw new ArgumentOutOfRangeException(nameof(slotIndexChoice), $"Inventory entry slot choice must be from 0 to {inventoryEntries.Length - 1}; given value was {slotIndexChoice.Value}. Use PutItemInCursorSlot() to add an item to the cursor.");
        if (amountToAdd < 0)
            throw new ArgumentOutOfRangeException(nameof(amountToAdd));

        if (amountToAdd == 0)
            return InventoryOperationResult.NoOperation();

        int amountLeftToAdd = amountToAdd;
        int maxStackSize = item.IsStackable ? item.maxStack : 1;
        List<InventoryOperationResult.ChangedSlot> changedSlots = new List<InventoryOperationResult.ChangedSlot>();

        // First: try to put all items in the slot at the chosen index. If that is successful, then return early.
        if (slotIndexChoice.HasValue && slotIndexChoice.Value < inventorySize)
        {
            int inventoryEntryIndex = slotIndexChoice.Value;
            if (inventoryEntries[inventoryEntryIndex] == null)
            {
                // Option 1: The chosen inventory slot is empty
                int amountAddedToStack = Mathf.Min(amountLeftToAdd, maxStackSize);
                inventoryEntries[inventoryEntryIndex] = new InventoryEntry(item, amountAddedToStack);
                changedSlots.Add(SlotChange(this, inventoryEntryIndex));
                amountLeftToAdd -= amountAddedToStack;
            }
            else if (inventoryEntries[inventoryEntryIndex].item == item && inventoryEntries[inventoryEntryIndex].stackSize < maxStackSize)
            {
                // Option 2: The chosen inventory slot has the same item as the one being added, and the stack at that location isn't full.
                int remainingSpace = maxStackSize - inventoryEntries[inventoryEntryIndex].stackSize;
                int amountAddedToStack = Mathf.Min(amountLeftToAdd, remainingSpace);
                inventoryEntries[inventoryEntryIndex].AddToStack(amountAddedToStack);
                changedSlots.Add(SlotChange(this, inventoryEntryIndex));
                amountLeftToAdd -= amountAddedToStack;
            }
            if (amountLeftToAdd < 1)
                return InventoryOperationResult.ItemFullyAdded(changedSlots.ToArray());
        }

        if (!slotIndexChoice.HasValue || (slotIndexChoice.HasValue && allowOverflowOutsideChosenSlot))
        {
            // Fill in any existing stacks.
            for (int index = 0; index < inventorySize; ++index)
            {
                InventoryEntry entry = inventoryEntries[index];
                if (entry != null && entry.item == item && entry.stackSize < maxStackSize)
                {
                    int remainingSpace = maxStackSize - entry.stackSize;
                    int amountAddedToStack = Mathf.Min(amountLeftToAdd, remainingSpace);
                    entry.AddToStack(amountAddedToStack);
                    changedSlots.Add(SlotChange(this, index));
                    amountLeftToAdd -= amountAddedToStack;
                }
                if (amountLeftToAdd < 1)
                    return InventoryOperationResult.ItemFullyAdded(changedSlots.ToArray());
            }

            // If there are still items to add, add them in empty entries in order from beginning to end.
            Queue<int> emptyEntryIndices = new Queue<int>();
            for (int index = 0; index < inventorySize; ++index)
            {
                if (inventoryEntries[index] == null)
                {
                    emptyEntryIndices.Enqueue(index);
                }
            }

            while (amountLeftToAdd > 0 && emptyEntryIndices.Count > 0)
            {
                int index = emptyEntryIndices.Dequeue();
                int thisStackSize = Mathf.Min(maxStackSize, amountLeftToAdd);
                inventoryEntries[index] = new InventoryEntry(item, thisStackSize);
                changedSlots.Add(SlotChange(this, index));
                amountLeftToAdd -= thisStackSize;
            }
        }

        if (amountLeftToAdd > 0)
        {
            // There are still items to add but no room to add them (either because overflow is off or the inventory is completely full)
            if (amountLeftToAdd == amountToAdd)
                return InventoryOperationResult.NoSpace(amountLeftToAdd);
            return InventoryOperationResult.ItemPartiallyAdded(amountLeftToAdd, changedSlots.ToArray());
        }
        return InventoryOperationResult.ItemFullyAdded(changedSlots.ToArray());
    }

    public InventoryOperationResult PutItemInCursorSlot(Item item, int amount)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (amount < 0 || amount > item.maxStack)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Stack size needs to be a number from 0 to the max stack size ({item.maxStack}), given amount was {amount}.");

        if (amount == 0)
            return InventoryOperationResult.NoOperation();
        if (cursorInventoryEntry != null)
            return InventoryOperationResult.NoSpace(amount);

        cursorInventoryEntry = new InventoryEntry(item, amount);
        InventoryOperationResult result = InventoryOperationResult.PickupToCursor(null);
        return result;
    }

    public InventoryOperationResult TakeAllFromCursorSlot(out Item item, out int amount)
    {
        if (cursorInventoryEntry == null)
        {
            item = null;
            amount = 0;
            return InventoryOperationResult.NoOperation();
        }
        item = cursorInventoryEntry.item;
        amount = cursorInventoryEntry.stackSize;
        cursorInventoryEntry = null;
        return InventoryOperationResult.TakeFromCursor();
    }

    public InventorySlotDisplayInformation GetSlotDisplayInformation(int index)
    {
        if ((index < 0 || index >= inventoryEntries.Length) && index != CursorSlotIndex)
            throw new ArgumentOutOfRangeException(nameof(index), $"Slot index must be either the special value of Inventory.CursorSlotIndex ({CursorSlotIndex}), or a value in the range of the inventory size (0 to {inventoryEntries.Length - 1}); given value was {index}.");

        InventoryEntry entry = (index == CursorSlotIndex) ? cursorInventoryEntry : inventoryEntries[index];

        if (entry == null)
            return InventorySlotDisplayInformation.Empty(index);
        return InventorySlotDisplayInformation.Occupied(index, entry.item.itemName, entry.item.icon, entry.stackSize, entry.item.maxStack);
    }

    public int InventorySize => inventorySize;
    public const int CursorSlotIndex = -1;
    public bool ItemInCursorSlot => cursorInventoryEntry != null;
}
