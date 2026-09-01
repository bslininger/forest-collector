using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemRequirementTracker : MonoBehaviour
{
    [SerializeField] private List<ItemRequirement> _requirements = new();

    public IReadOnlyList<ItemRequirement> Requirements => _requirements;
    public bool IsComplete
    {
        get
        {
            if (_requirements.Count == 0)
                return false;
            foreach (ItemRequirement requirement in _requirements)
                if (requirement.RemainingAmount > 0)
                    return false;
            return true;
        }
    }
    public event Action<Item, int> ProgressChanged;

    public int GetRemainingAmount(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        int remainingAmount = 0;
        foreach (ItemRequirement requirement in _requirements)
            if (requirement.Item == item)
                remainingAmount += requirement.RemainingAmount;
        return remainingAmount;
    }

    // Returns the amount accepted.
    public int SubmitItem(Item item, int amount)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        int amountSubmitted = 0;
        foreach (ItemRequirement requirement in _requirements)
        {
            if (requirement.Item == item)
                amountSubmitted += requirement.AcceptItemTowardProgress(item, amount - amountSubmitted);
            if (amountSubmitted >= amount)
                break;
        }
        if (amountSubmitted > 0)
            ProgressChanged?.Invoke(item, amountSubmitted);
        return amountSubmitted;
    }
}
