using UnityEngine;

public class DropoffContainerController : MonoBehaviour, IInteractableObject
{
    [SerializeField] private ForageRunController _gameController;
    public string InteractionPromptText => "Drop off collected items";
    public Vector3 WorldPosition => transform.position;

    public void Interact()
    {
        _gameController.HandleAllItemsDelivery();
    }
}
