using System;
using UnityEngine;

[Serializable]
public class ItemRequirement
{
    [SerializeField] private Item _item;
    [SerializeField, Min(1)] private int _requiredAmount;
    private int _fulfilledAmount;

    public Item Item => _item;
    public int RequiredAmount => _requiredAmount;
    public int FulfilledAmount => _fulfilledAmount;
    public int RemainingAmount => _requiredAmount - _fulfilledAmount;

    internal int AcceptItemTowardProgress(Item item, int amount)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (item != _item)
            return 0;
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount == 0)
            return 0;
        if (RemainingAmount <= 0)
            return 0;
        int amountAccepted = Math.Min(amount, RemainingAmount);
        _fulfilledAmount += amountAccepted;
        return amountAccepted;
    }
}
