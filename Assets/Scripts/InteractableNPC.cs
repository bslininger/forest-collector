using UnityEngine;

public class InteractableNPC : MonoBehaviour, IInteractableObject
{
    // Right now this is very much focused towards the one single NPC there is. This will get reworked to be more general as the game evolves.

    [SerializeField] private ForageRunController _gameController;
    [SerializeField] private string[] _dialogueLines;
    [SerializeField] private ItemRequirementTracker _itemRequirementTracker;

    public string InteractionPromptText => "Talk";
    public Vector3 WorldPosition => transform.position;

    public void Interact()
    {
        if (_gameController == null)
            return;

        int dialogueIndex = _itemRequirementTracker.IsComplete ? 1 : 0;
        _gameController.DisplayDialogue(_dialogueLines[dialogueIndex]);
    }
}
