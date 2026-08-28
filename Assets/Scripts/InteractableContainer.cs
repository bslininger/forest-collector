using UnityEngine;

public class InteractableContainer : MonoBehaviour, IClickInteractableObject
{
    [SerializeField] private ForageRunController _gameController;
    [SerializeField] private InventoryContainer _inventoryContainer;

    public string InteractionPromptText => "Open";
    public Vector3 WorldPosition => transform.position;
    public InventoryContainer InventoryContainer => _inventoryContainer;

    public void Interact()
    {
        _gameController.HandleWorldContainerInteract(_inventoryContainer);
    }
}
