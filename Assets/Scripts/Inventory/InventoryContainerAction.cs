using System;
using UnityEngine;

public abstract class InventoryContainerAction : MonoBehaviour
{
    public abstract InventoryContainerActionDisplayState DisplayState { get; }

    public event Action DisplayStateChanged;

    protected void NotifyDisplayStateChanged()
    {
        DisplayStateChanged?.Invoke();
    }

    // Returns true if the action was accepted and performed, and false if the action did not get performed.
    public abstract bool TryExecuteAction();
}
