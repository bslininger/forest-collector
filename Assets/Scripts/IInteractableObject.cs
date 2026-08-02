using UnityEngine;

public interface IInteractableObject
{
    string InteractionPromptText { get; }
    bool RemoveAfterInteraction { get; }
    Vector3 WorldPosition { get; }
    void Interact();
}
