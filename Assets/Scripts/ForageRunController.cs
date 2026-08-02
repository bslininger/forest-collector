using UnityEngine;
using UnityEngine.InputSystem;

public class ForageRunController : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private InteractableObjectDetector _interactableObjectDetector;
    [SerializeField] private HUDManager _onScreenDisplay;
    [SerializeField] private InputActionReference _advanceDialogueAction;
    private ForageRunModel _gameModel;
    private bool _berryQuestCompleted = false;
    private bool _dialogueBoxOpen = false;

    public bool BerryQuestCompleted => _berryQuestCompleted;

    private void Awake()
    {
        _gameModel = new ForageRunModel();
        _onScreenDisplay.SetRemainingCapacityText(_gameModel.GetRemainingCapacity());
        _onScreenDisplay.SetDeliveredItemsCountText(_gameModel.GetDeliveredItemsCount());
    }

    public void HandleCollectibleItemInteract(ItemPickup collectedItem)
    {
        bool collected = _gameModel.CollectItem(collectedItem.ScriptableCollectibleItem.Id);
        if (!collected)
            return;
        _onScreenDisplay.SetCarriedCountText(collectedItem.ScriptableCollectibleItem, _gameModel.GetItemCount(collectedItem.ScriptableCollectibleItem.Id));
        collectedItem.Remove();
        _onScreenDisplay.SetRemainingCapacityText(_gameModel.GetRemainingCapacity());
    }

    public void HandleAllItemsDelivery()
    {
        bool delivered = _gameModel.DeliverAllCarriedItems();
        if (!delivered)
            return;

        _onScreenDisplay.SetRemainingCapacityText(_gameModel.GetRemainingCapacity());
        _onScreenDisplay.SetDeliveredItemsCountText(_gameModel.GetDeliveredItemsCount());
        _onScreenDisplay.SetAllCarriedCountTextTo0();

        if (!_berryQuestCompleted && _gameModel.GetSpecificDeliveredItemCount("berry") >= 3)
            _berryQuestCompleted = true;
    }

    public void OnAdvanceDialogueAction(InputAction.CallbackContext context)
    {
        CloseDialogueBox();
    }

    public void DisplayDialogue(string dialogueLine)
    {
        if (_dialogueBoxOpen)
            return;

        _playerController.SetMovementEnabled(false);
        _interactableObjectDetector.SetInteractionEnabled(false);
        _onScreenDisplay.ShowDialogue(dialogueLine);
        _advanceDialogueAction.action.Enable();
        _advanceDialogueAction.action.performed += OnAdvanceDialogueAction;
        _dialogueBoxOpen = true;
    }

    public void CloseDialogueBox()
    {
        if (!_dialogueBoxOpen)
            return;

        _advanceDialogueAction.action.performed -= OnAdvanceDialogueAction;
        _advanceDialogueAction.action.Disable();
        _onScreenDisplay.CloseDialogueBox();
        _playerController.SetMovementEnabled(true);
        _interactableObjectDetector.SetInteractionEnabled(true);
        _dialogueBoxOpen = false;
    }
}
