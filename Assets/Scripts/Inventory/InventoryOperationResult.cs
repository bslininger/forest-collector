using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct InventoryOperationResult
{
    public enum ResultType
    {
        NoOperation,
        ItemFullyAdded,
        ItemPartiallyAdded,
        ItemFullyRemoved,
        ItemPartiallyRemoved,
        InsufficientItems,
        NoSpace,
        PickupToCursor,
        PlaceFromCursor,
        SwapWithCursor,
        MergeFromCursor,
        TakeFromCursor,
    }

    public readonly struct ChangedSlot
    {
        public Inventory Inventory { get; }
        public int Index { get; }
        public ChangedSlot (Inventory inventory, int index)
        {
            this.Inventory = inventory;
            this.Index = index;
        }
    }

    public ResultType OperationResultType { get; }

    public bool CursorSlotChanged { get; }
    public int LeftoverItemCount { get; }  // Count of items that couldn't be processed from the total number requested: either ones that couldn't fit in an inventory because it ran out of room (the "overflow" item count), or requested items that could not be removed.
    public IReadOnlyList<ChangedSlot> ChangedSlots { get; }

    private InventoryOperationResult(ResultType operationResultType, bool cursorSlotChanged, int leftoverItemCount, params ChangedSlot[] changedSlots)
    {
        if (changedSlots == null)
            throw new ArgumentNullException(nameof(changedSlots));
        if (leftoverItemCount < 0)
            throw new ArgumentException("Leftover item count must be non-negative.");

        OperationResultType = operationResultType;
        CursorSlotChanged = cursorSlotChanged;
        LeftoverItemCount = leftoverItemCount;
        ChangedSlots = Array.AsReadOnly(changedSlots.ToArray());
    }

    // Factory methods
    public static InventoryOperationResult NoOperation()
    {
        return new InventoryOperationResult(ResultType.NoOperation, false, 0);
    }

    public static InventoryOperationResult ItemFullyAdded(params ChangedSlot[] changedSlots)
    {
        return new InventoryOperationResult(ResultType.ItemFullyAdded, false, 0, changedSlots);
    }

    public static InventoryOperationResult ItemPartiallyAdded(int leftoverItemCount, params ChangedSlot[] changedSlots)
    {
        return new InventoryOperationResult(ResultType.ItemPartiallyAdded, false, leftoverItemCount, changedSlots);
    }

    public static InventoryOperationResult ItemFullyRemoved(params ChangedSlot[] changedSlots)
    {
        return new InventoryOperationResult(ResultType.ItemFullyRemoved, false, 0, changedSlots);
    }

    public static InventoryOperationResult ItemPartiallyRemoved(int leftoverItemCount, params ChangedSlot[] changedSlots)
    {
        return new InventoryOperationResult(ResultType.ItemPartiallyRemoved, false, leftoverItemCount, changedSlots);
    }

    public static InventoryOperationResult InsufficientItems(int leftoverItemCount)
    {
        return new InventoryOperationResult(ResultType.InsufficientItems, false, leftoverItemCount);
    }

    public static InventoryOperationResult NoSpace(int leftoverItemCount)
    {
        return new InventoryOperationResult(ResultType.NoSpace, false, leftoverItemCount);
    }

    public static InventoryOperationResult PickupToCursor(ChangedSlot? changedSlot)
    {
        // A null changedSlot represents no inventory slots changing, just the cursor slot (for example, when receiving an item to the cursor, or picking one up off the ground)
        if (changedSlot.HasValue)
            return new InventoryOperationResult(ResultType.PickupToCursor, true, 0, changedSlot.Value);
        return new InventoryOperationResult(ResultType.PickupToCursor, true, 0);
    }

    public static InventoryOperationResult PlaceFromCursor(ChangedSlot changedSlot)
    {
        return new InventoryOperationResult(ResultType.PlaceFromCursor, true, 0, changedSlot);
    }

    public static InventoryOperationResult SwapWithCursor(ChangedSlot changedSlot)
    {
        return new InventoryOperationResult(ResultType.SwapWithCursor, true, 0, changedSlot);
    }

    public static InventoryOperationResult MergeFromCursor(ChangedSlot changedSlot)
    {
        return new InventoryOperationResult(ResultType.MergeFromCursor, true, 0, changedSlot);
    }

    public static InventoryOperationResult TakeFromCursor()
    {
        return new InventoryOperationResult(ResultType.TakeFromCursor, true, 0);
    }
}
