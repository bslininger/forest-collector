using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContainerInventoryPanelController : MonoBehaviour
{
    [SerializeField] private InventoryController _inventoryController;
    [SerializeField] private InventoryInteractionController _inventoryInteractionController;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotGrid;
    [SerializeField] private TMP_Text _containerNameText;
    [SerializeField] private Image _containerIcon;
    [SerializeField] private Button _actionButton;
    [SerializeField] private GameObject _actionButtonRow;
    [SerializeField] private TMP_Text _actionButtonText;
    [SerializeField] private Button _closeButton;


    private InventoryContainer _activeContainer;
    private GameObject[] _containerInventorySlots;

    private void OnEnable()
    {
        EventManager.InventoryUpdateEvent += UpdateDirtySlots;
    }

    private void OnDisable()
    {
        EventManager.InventoryUpdateEvent -= UpdateDirtySlots;
    }

    public void OpenContainer(InventoryContainer container)
    {
        if (gameObject.activeSelf)
            return;

        _activeContainer = container;
        HandleActionButtonDisplayChanged();
        _activeContainer.ActionDisplayStateChanged += HandleActionButtonDisplayChanged;
        gameObject.SetActive(true);

        // First, generate all slots
        _containerInventorySlots = new GameObject[_activeContainer.Contents.InventorySize];
        for (int index = 0; index < _activeContainer.Contents.InventorySize; ++index)
        {
            _containerInventorySlots[index] = Instantiate(_slotPrefab, _slotGrid);
            _containerInventorySlots[index].name = $"Container Inventory Slot {index}";
            InventorySlotUIController slotUIController = _containerInventorySlots[index].GetComponent<InventorySlotUIController>();
            slotUIController.InputLockProvider = UIManager.Instance;
            _inventoryInteractionController.RegisterUIInventorySlot(slotUIController, _activeContainer.Contents, index);
        }
        // Now, put inventory items into their proper slots in the inventory panel, based on their index locations in the inventory itself
        for (int index = 0; index < _activeContainer.Contents.InventorySize; ++index)
        {
            InventorySlotUIController slotUIController = _containerInventorySlots[index].transform.GetComponent<InventorySlotUIController>();
            slotUIController.SetSlot(_activeContainer.Contents.GetSlotDisplayInformation(index));
        }
    }

    public void CloseContainer()
    {
        _inventoryInteractionController.CancelStackSizeSelectorPanelForInventory(_activeContainer.Contents);
        _inventoryInteractionController.ClearRegistrationsByInventory(_activeContainer.Contents);
        foreach (Transform child in _slotGrid)
            Destroy(child.gameObject);
        _activeContainer.ActionDisplayStateChanged -= HandleActionButtonDisplayChanged;
        _activeContainer = null;
        gameObject.SetActive(false);
    }

    public void CloseContainerIfActive(InventoryContainer inventoryContainer)
    {
        if (_activeContainer == null || _activeContainer != inventoryContainer)
            return;
        CloseContainer();
    }

    private void UpdateDirtySlots(InventoryOperationResult.ChangedSlot[] changedSlots)
    {
        foreach (InventoryOperationResult.ChangedSlot changedSlot in changedSlots)
        {
            if (changedSlot.Inventory == _activeContainer.Contents)
                UpdateSlot(changedSlot.Index);
        }
    }

    private void UpdateSlot(int index)
    {
        if (_activeContainer == null || _containerInventorySlots == null)
            return;
        InventorySlotUIController slotUIController = _containerInventorySlots[index].transform.GetComponent<InventorySlotUIController>();
        slotUIController.SetSlot(_activeContainer.Contents.GetSlotDisplayInformation(index));
    }

    private void HandleActionButtonDisplayChanged()
    {
        if (_activeContainer == null)
            return;
        InventoryContainerActionDisplayState actionButtonDisplayState = _activeContainer.ActionDisplayState;
        _actionButton.interactable = actionButtonDisplayState.Enabled;
        _actionButtonRow.gameObject.SetActive(actionButtonDisplayState.Visible);
        _actionButtonText.text = actionButtonDisplayState.Text;
    }

    public void PerformButtonAction()
    {
        if (_activeContainer != null)
            _activeContainer.TryExecuteAction();
    }
}
