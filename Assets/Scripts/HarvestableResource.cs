using UnityEngine;

public class HarvestableResource : MonoBehaviour, IClickInteractableObject
{
    [SerializeField] private ForageRunController _gameController;
    [SerializeField] private Item _item;
    [SerializeField] [Min(0)] private int _startingQuantity;
    [SerializeField] private GameObject _availableModel;
    [SerializeField] private GameObject _depletedModel;
    [SerializeField] private string _interactionPromptText;

    private int _remainingQuantity;

    private bool IsDepleted => _remainingQuantity <= 0;

    public string InteractionPromptText => _interactionPromptText;
    public bool CanInteract => !IsDepleted;
    public Vector3 WorldPosition => transform.position;

    private void Awake()
    {
        _remainingQuantity = _startingQuantity;
        DisplayCorrectModel();
    }

    public void Interact()
    {
        if (!CanInteract)
            return;
        if (_gameController.TryPutItemInPlayerCursor(_item, 1))
        {
            _remainingQuantity -= 1;
            DisplayCorrectModel();
        }
    }

    private void DisplayCorrectModel()
    {
        _availableModel.SetActive(!IsDepleted);
        _depletedModel.SetActive(IsDepleted);
    }
}
