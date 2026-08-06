using UnityEngine;

public interface IInteractableObject
{
    string InteractionPromptText { get; }
    Vector3 WorldPosition { get; }
    void Interact();
}
