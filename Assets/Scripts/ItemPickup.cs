using UnityEngine;
using UnityEngine.EventSystems;

public class ItemPickup : MonoBehaviour, IClickInteractableObject
{
    [SerializeField] private ForageRunController _gameController;
    [SerializeField] private CollectibleItem _scriptableCollectibleItem;
    [SerializeField] private Item _inventoryItem;
    private IInputLockProvider _inputLockProvider;
    public CollectibleItem ScriptableCollectibleItem => _scriptableCollectibleItem;
    public string InteractionPromptText => "Collect " + _scriptableCollectibleItem.DisplayName;
    public Vector3 WorldPosition => transform.position;
    public Item InventoryItem => _inventoryItem;

    private void Start()
    {
        _inputLockProvider = UIManager.Instance;
    }

    public void Remove()
    {
        Destroy(gameObject);
    }

    public void Interact()
    {
        Pickup();
    }

    public void Pickup()
    {
        _gameController.HandleCollectibleItemInteract(this);
    }
}
