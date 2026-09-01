using System;
using UnityEngine;

public class InventoryContainer : MonoBehaviour
{
    [SerializeField] private Inventory _contents; // Inventory representing just what is inside this container
    [SerializeField] private InventoryContainerAction _action; // What the action button does, if it exists

    public Inventory Contents => _contents;
    public bool HasAction => _action != null;
    public InventoryContainerActionDisplayState ActionDisplayState => HasAction ? _action.DisplayState : new InventoryContainerActionDisplayState(false, false, string.Empty);
    public event Action ActionDisplayStateChanged;

    private void OnEnable()
    {
        if (HasAction)
            _action.DisplayStateChanged += HandleActionDisplayStateChanged;
    }

    private void OnDisable()
    {
        if (HasAction)
            _action.DisplayStateChanged -= HandleActionDisplayStateChanged;
    }

    public bool TryExecuteAction()
    {
        if (!HasAction)
            return false;
        return _action.TryExecuteAction();
    }

    private void HandleActionDisplayStateChanged()
    {
        ActionDisplayStateChanged?.Invoke();
    }
}