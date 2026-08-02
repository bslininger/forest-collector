using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractableObject
{
    [SerializeField] private ForageRunController _gameController;
    [SerializeField] private CollectibleItem _scriptableCollectibleItem;
    public CollectibleItem ScriptableCollectibleItem => _scriptableCollectibleItem;
    public string InteractionPromptText => "Collect " + _scriptableCollectibleItem.DisplayName;
    public bool RemoveAfterInteraction => true;
    public Vector3 WorldPosition => transform.position;

    public void Remove()
    {
        Destroy(gameObject);
    }

    public void Interact()
    {
        _gameController.HandleCollectibleItemInteract(this);
    }
}
