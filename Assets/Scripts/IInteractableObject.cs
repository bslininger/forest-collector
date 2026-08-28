using UnityEngine;

public interface IInteractableObject : IWorldPositionedObject
{
    string InteractionPromptText { get; }
    void Interact();
}
