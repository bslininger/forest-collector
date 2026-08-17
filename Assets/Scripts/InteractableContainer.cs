using UnityEngine;

public class InteractableContainer : MonoBehaviour, IInteractableObject
{
    [SerializeField] private ForageRunController _gameController;
    [SerializeField] private InventoryContainer _inventoryContainer;

    public string InteractionPromptText => "Open";
    public Vector3 WorldPosition => transform.position;

    public void Interact()
    {
        _gameController.HandleWorldContainerInteract(_inventoryContainer);
    }
}
