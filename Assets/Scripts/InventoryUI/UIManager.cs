using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Flags]
public enum UIInputLock
{
    None = 0,
    InventoryInteraction = 1 << 0,
    All = InventoryInteraction
}
public class UIManager : MonoBehaviour, IInputLockProvider
{
    public static UIManager Instance { get; private set; }

    // Actions
    [SerializeField] private InputActionReference _toggleInventoryAction;

    // Canvases
    [SerializeField] private Canvas _inventoryUICanvas;

    // Prefabs
    [SerializeField] private GameObject _stackSizeSelectorPrefab;

    // Controllers
    [SerializeField] private InventoryPanelController _inventoryPanelController;
    private StackSizeSelectorPanelController _activeStackSizeSelectorPanelController;

    // Accessors
    public Canvas InventoryUICanvas => _inventoryUICanvas;

    private UIInputLock activeLocks = UIInputLock.None;

    private void Awake()
    {
        // Keeps this UIManager as a singleton
        if (Instance != null)
        {
            Debug.LogWarning("Another UIManager tried to spawn and was destroyed!");
            Destroy(gameObject);
        }
        else
            Instance = this;
    }

    private void OnEnable()
    {
        _toggleInventoryAction.action.Enable();
        _toggleInventoryAction.action.performed += ToggleInventoryPanel;
    }

    private void OnDisable()
    {
        _toggleInventoryAction.action.performed -= ToggleInventoryPanel;
        _toggleInventoryAction.action.Disable();
    }

    private void ToggleInventoryPanel(InputAction.CallbackContext context)
    {
        if (!InputLocked(UIInputLock.InventoryInteraction))
            _inventoryPanelController.ToggleInventoryPanel();
    }

    public void ActivateInventoryPanel()
    {
        _inventoryPanelController.ActivateInventoryPanel();
    }

    public void ShowStackSizeSelectorPanel(InventorySlotDisplayInformation displayInformation, Vector2 location, Action<int> acceptButtonAction)
    {
        if (StackSizeSelectorPanelOpen)
        {
            Debug.Log("A stack size selector panel is already open somewhere. Not opening another one.");
            return;
        }

        if (InputLocked(UIInputLock.InventoryInteraction))
        {
            Debug.Log("Can't open stack size selector panel because the InventoryInteraction UIInputLock is already locked.");
            return;
        }

        GameObject panelInstance = Instantiate(_stackSizeSelectorPrefab, _inventoryUICanvas.transform);
        StackSizeSelectorPanelController stackSizeSelectorPanelController = panelInstance.GetComponent<StackSizeSelectorPanelController>();
        RectTransform stackSizeSelectorPanelRectTransform = panelInstance.GetComponent<RectTransform>();
        stackSizeSelectorPanelRectTransform.anchoredPosition = location + new Vector2(stackSizeSelectorPanelRectTransform.rect.width / 2 + 10.0f, 0.0f);

        stackSizeSelectorPanelController.InitializePreviewSlot(displayInformation);

        stackSizeSelectorPanelController.SetAcceptAction((int amount) =>
        {
            acceptButtonAction?.Invoke(amount);
            CloseStackSizeSelectorPanel();
        });
        stackSizeSelectorPanelController.SetCancelAction(() =>
        {
            CloseStackSizeSelectorPanel();
        });

        AddInputLock(UIInputLock.InventoryInteraction);
        _activeStackSizeSelectorPanelController = stackSizeSelectorPanelController;
    }

    public void CloseStackSizeSelectorPanel()
    {
        if (_activeStackSizeSelectorPanelController != null)
        {
            Destroy(_activeStackSizeSelectorPanelController.gameObject);
            _activeStackSizeSelectorPanelController = null;
            RemoveInputLock(UIInputLock.InventoryInteraction);
        }
    }

    public bool StackSizeSelectorPanelOpen => _activeStackSizeSelectorPanelController != null;

    #region Locks
    public void AddInputLock(UIInputLock lockType)
    {
        if (InputLocked(lockType))
        {
            Debug.LogError($"Attempted to lock {lockType}, which is already locked."); // This lock system allows only one entity to lock a specific lock at a time.
            return;
        }
        activeLocks |= lockType;
    }

    public void RemoveInputLock(UIInputLock lockType)
    {
        if (!InputLocked(lockType))
        {
            Debug.LogWarning($"Attempted to unlock {lockType}, but it wasn't locked. Was this intentional?");
            return;
        }
        activeLocks &= ~lockType;
    }

    #region IInputLockProvider
    public bool InputLocked(UIInputLock lockType)
    {
        return (activeLocks & lockType) != 0;
    }
    #endregion

    #endregion
}
