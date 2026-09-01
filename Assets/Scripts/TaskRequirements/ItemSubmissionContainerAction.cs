using UnityEngine;

[RequireComponent(typeof(InventoryController))]
public class ItemSubmissionContainerAction : InventoryContainerAction
{
    private InventoryController _sourceInventoryController;
    [SerializeField] private ItemRequirementTracker _requirementTracker;
    [SerializeField] private string _buttonText = "Submit";

    public override InventoryContainerActionDisplayState DisplayState => new InventoryContainerActionDisplayState(!_requirementTracker.IsComplete, true, _buttonText);

    private void Awake()
    {
        _sourceInventoryController = GetComponent<InventoryController>();
    }

    private void OnEnable()
    {
        _requirementTracker.ProgressChanged += HandleRequirementProgressChanged;
    }

    private void OnDisable()
    {
        _requirementTracker.ProgressChanged -= HandleRequirementProgressChanged;
    }

    private void HandleRequirementProgressChanged(Item item, int amount)
    {
        NotifyDisplayStateChanged();
    }

    public override bool TryExecuteAction()
    {
        bool anySubmitted = false;
        foreach (ItemRequirement requirement in _requirementTracker.Requirements)
        {
            int remainingRequiredAmount = requirement.RemainingAmount;
            if (remainingRequiredAmount <= 0)
                continue;
            InventoryOperationResult inventoryOperationResult = _sourceInventoryController.HandleRemoveItemFromInventory(requirement.Item, remainingRequiredAmount, true);
            int amountRemoved;
            if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.ItemFullyRemoved)
                amountRemoved = remainingRequiredAmount;
            else if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.ItemPartiallyRemoved)
                amountRemoved = remainingRequiredAmount - inventoryOperationResult.LeftoverItemCount;
            else
                amountRemoved = 0;
            int amountSubmitted = _requirementTracker.SubmitItem(requirement.Item, amountRemoved);
            if (!anySubmitted && amountSubmitted > 0)
                anySubmitted = true;
        }
        return anySubmitted;
    }
}
