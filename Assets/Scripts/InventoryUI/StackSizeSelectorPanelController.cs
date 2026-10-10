using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StackSizeSelectorPanelController : MonoBehaviour
{
    [SerializeField] private PanelDragHandle _dragHandle;
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private InventorySlotUIController _slotUIController;
    private InventorySlotDisplayInformation _inventorySlotDisplayInformation;

    private Action<int> _onAccept; // This is the callback for the Accept button, defined outside this controller but stored inside it. Destroying afterward handled outside as well.
    private Action _onCancel;
    private int _currentSelectedAmount => Mathf.RoundToInt(_slider.value);

    public RectTransform DragAllowedBounds { set => _dragHandle.DragAllowedBounds = value; }

    void Start()
    {
        _inputField.text = "1";

        _slider.onValueChanged.AddListener((float value) =>
        {
            _inputField.text = Mathf.RoundToInt(value).ToString();
        });
        _slider.minValue = 1;
        _slider.wholeNumbers = true;

        _inputField.onValueChanged.AddListener((string text) =>
        {
            if (int.TryParse(text, out int value))
            {
                value = Mathf.Clamp(value, 1, (int)_slider.maxValue);
                _slider.value = value;
                _slotUIController.SetSlot(_inventorySlotDisplayInformation);
                _slotUIController.OverrideStackText(value);
            }
        });
    }

    public void InitializePreviewSlot(InventorySlotDisplayInformation displayInformation)
    {
        _inventorySlotDisplayInformation = displayInformation;
        _itemNameText.text = displayInformation.ItemName;
        _slider.maxValue = _inventorySlotDisplayInformation.StackSize;
        _slider.value = 1;
        _slotUIController.SetSlot(_inventorySlotDisplayInformation);
        _slotUIController.OverrideStackText(1);
    }

    public void SetAcceptCallback(Action<int> callback)
    {
        _onAccept = callback;
    }

    public void SetCancelCallback(Action callback)
    {
        _onCancel = callback;
    }

    public void OnAcceptButtonClicked()
    {
        _onAccept?.Invoke(_currentSelectedAmount);
    }

    public void OnCancelButtonClicked()
    {
        _onCancel?.Invoke();
    }

    public void IncrementValue()
    {
        _slider.value += 1;
    }

    public void DecrementValue()
    {
        _slider.value -= 1;
    }
}
