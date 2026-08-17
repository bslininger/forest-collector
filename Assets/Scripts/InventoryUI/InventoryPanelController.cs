using UnityEngine;

public class InventoryPanelController : MonoBehaviour
{
    [SerializeField] private Inventory _displayedInventory;
    [SerializeField] private InventoryController _inventoryController;
    [SerializeField] private InventoryInteractionController _inventoryInteractionController;
    [SerializeField] private GameObject _slotPrefab;
    private GameObject[] _inventorySlots;


    private void OnEnable()
    {
        EventManager.InventoryUpdateEvent += UpdateDirtySlots;
        PopulatePanelFromInventory();
    }

    private void OnDisable()
    {
        EventManager.InventoryUpdateEvent -= UpdateDirtySlots;
        DestroyAllSlots();
    }

    public void ActivateInventoryPanel()
    {
        gameObject.SetActive(true);
    }

    public void DeactivateInventoryPanel()
    {
        gameObject.SetActive(false);
    }

    public void ToggleInventoryPanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

     void PopulatePanelFromInventory()
     {
        // First, generate all slots
        _inventorySlots = new GameObject[_displayedInventory.InventorySize];
        for (int index = 0; index < _displayedInventory.InventorySize; ++index)
        {
            _inventorySlots[index] = Instantiate(_slotPrefab, transform);
            _inventorySlots[index].name = $"Inventory Slot {index}";
            InventorySlotUIController slotUIController = _inventorySlots[index].transform.GetComponent<InventorySlotUIController>();
            slotUIController.InputLockProvider = UIManager.Instance;
            _inventoryInteractionController.RegisterUIInventorySlot(slotUIController, _displayedInventory, index);
        }

        // Now, put inventory items into their proper slots in the inventory panel, based on their index locations in the inventory itself
        for (int index = 0; index < _displayedInventory.InventorySize; ++index)
        {
            InventorySlotUIController slotUIController = _inventorySlots[index].transform.GetComponent<InventorySlotUIController>();
            slotUIController.SetSlot(_displayedInventory.GetSlotDisplayInformation(index));
        }
    }

    void UpdateDirtySlots(InventoryOperationResult.ChangedSlot[] changedSlots)
    {
        foreach (InventoryOperationResult.ChangedSlot changedSlot in changedSlots)
        {
            if (changedSlot.Inventory == _displayedInventory && changedSlot.Index >= 0)  // index -1 (Inventory.CursorSlotIndex) is used for the cursor inventory slot
                UpdateSlot(changedSlot.Index);
        }
    }

    void UpdateSlot(int index)
    {
        InventorySlotUIController slotUIController = _inventorySlots[index].transform.GetComponent<InventorySlotUIController>();
        slotUIController.SetSlot(_displayedInventory.GetSlotDisplayInformation(index));
    }

    void DestroyAllSlots()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        _inventorySlots = null;
        _inventoryInteractionController.ClearRegistrationsByInventory(_displayedInventory);
    }
}
