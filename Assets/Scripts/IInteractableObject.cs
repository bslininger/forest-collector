using UnityEngine;

public interface IInteractableObject : IWorldPositionedObject
{
    string InteractionPromptText { get; }
    bool CanInteract { get; }
    void Interact();
}
