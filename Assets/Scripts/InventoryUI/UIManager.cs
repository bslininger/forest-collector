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
    [SerializeField] private ContainerInventoryPanelController _containerInventoryPanelController;

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

    private static Vector3 GetBottomRightWorldCorner(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);  // Fills in the array of corner positions. Bottom-left: 0;  top-left: 1;  top-right: 2;  bottom-right: 3
        return corners[3];
    }

    // Returns true if the panel successfully opened and false if it did not.
    public bool ShowStackSizeSelectorPanel(InventorySlotDisplayInformation displayInformation, RectTransform inventorySlotRectTransform, Action<int> acceptButtonCallback, Action cancelButtonCallback)
    {
        if (StackSizeSelectorPanelOpen)
        {
            Debug.Log("A stack size selector panel is already open somewhere. Not opening another one.");
            return false;
        }

        if (InputLocked(UIInputLock.InventoryInteraction))
        {
            Debug.Log("Can't open stack size selector panel because the InventoryInteraction UIInputLock is already locked.");
            return false;
        }

        GameObject panelInstance = Instantiate(_stackSizeSelectorPrefab, _inventoryUICanvas.transform);
        StackSizeSelectorPanelController stackSizeSelectorPanelController = panelInstance.GetComponent<StackSizeSelectorPanelController>();
        RectTransform stackSizeSelectorPanelRectTransform = panelInstance.GetComponent<RectTransform>();
        Vector3 inventorySlotBottomRightCorner = GetBottomRightWorldCorner(inventorySlotRectTransform);
        Vector3 localAnchorLocation = ((RectTransform)stackSizeSelectorPanelRectTransform.parent).InverseTransformPoint(inventorySlotBottomRightCorner);
        stackSizeSelectorPanelRectTransform.localPosition = localAnchorLocation + new Vector3(stackSizeSelectorPanelRectTransform.rect.width / 2 + 10.0f, 0.0f, 0.0f);

        stackSizeSelectorPanelController.InitializePreviewSlot(displayInformation);

        stackSizeSelectorPanelController.SetAcceptCallback((int amount) =>
        {
            acceptButtonCallback?.Invoke(amount);
            CloseStackSizeSelectorPanel();
        });
        stackSizeSelectorPanelController.SetCancelCallback(() =>
        {
            cancelButtonCallback?.Invoke();
            CloseStackSizeSelectorPanel();
        });

        AddInputLock(UIInputLock.InventoryInteraction);
        _activeStackSizeSelectorPanelController = stackSizeSelectorPanelController;
        return true;
    }

    public void CancelStackSizeSelectorPanel()
    {
        // Simulates clicking the cancel button in cases where the selector needs to be closed without being directly initiated by the player (such as when its parent inventory is closed)
        if (_activeStackSizeSelectorPanelController != null)
            _activeStackSizeSelectorPanelController.OnCancelButtonClicked();
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

    public void OpenContainer(InventoryContainer container, string containerName, Sprite containerIcon)
    {
        _containerInventoryPanelController.OpenContainer(container, containerName, containerIcon);
    }

    public void CloseContainerIfOpened(InventoryContainer container)
    {
        _containerInventoryPanelController.CloseContainerIfActive(container);
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
