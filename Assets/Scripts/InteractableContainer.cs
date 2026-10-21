using UnityEngine;

public class InteractableContainer : MonoBehaviour, IClickInteractableObject
{
    [SerializeField] private InventoryContainer _inventoryContainer;
    [SerializeField] private string _containerName;
    [SerializeField] private Sprite _containerIcon;

    public string InteractionPromptText => "Open";
    public bool CanInteract => true;
    public Vector3 WorldPosition => transform.position;
    public InventoryContainer InventoryContainer => _inventoryContainer;

    public void Interact()
    {
        UIManager.Instance.OpenContainer(_inventoryContainer, _containerName, _containerIcon);
    }
}
