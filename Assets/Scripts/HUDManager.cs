using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [System.Serializable]
    private class ItemTextPair
    {
        public CollectibleItem Item;
        public TMPro.TMP_Text Text;
    }

    [SerializeField] private InteractableObjectDetector _playerInteractableObjectDetector;
    [SerializeField] private List<ItemTextPair> _itemTextPairs;
    [SerializeField] private TMPro.TMP_Text _remainingCapacityText;
    [SerializeField] private TMPro.TMP_Text _deliveredItemsCountText;
    [SerializeField] private TMPro.TMP_Text _interactionPromptText;
    [SerializeField] private Image _dialogueBox;

    private Dictionary<string, TMPro.TMP_Text> _itemTextMap;
    private TMPro.TMP_Text _dialogueText;

    private void Awake()
    {
        _itemTextMap = new();
        foreach (ItemTextPair itemTextPair in _itemTextPairs)
            _itemTextMap[itemTextPair.Item.Id] = itemTextPair.Text;
        SetAllCarriedCountTextTo0();
        _dialogueText = _dialogueBox.GetComponentInChildren<TMPro.TMP_Text>();
    }

    private void OnEnable()
    {
        _playerInteractableObjectDetector.InteractionPromptChanged += SetInteractionPromptText;
    }

    private void OnDisable()
    {
        _playerInteractableObjectDetector.InteractionPromptChanged -= SetInteractionPromptText;
    }

    public void SetCarriedCountText(CollectibleItem collectibleItem, int count)
    {
        _itemTextMap[collectibleItem.Id].text = collectibleItem.DisplayName + " x " + count.ToString();
    }

    public void SetAllCarriedCountTextTo0()
    {
        foreach(ItemTextPair itemTextPair in _itemTextPairs)
            SetCarriedCountText(itemTextPair.Item, 0);
    }

    public void SetRemainingCapacityText(int remainingCapacity)
    {
        _remainingCapacityText.text = "Room left: " + remainingCapacity.ToString();
    }

    public void SetDeliveredItemsCountText(int deliveredItemsCount)
    {
        _deliveredItemsCountText.text = "Total items delivered: " + deliveredItemsCount.ToString();
    }

    public void SetInteractionPromptText(InteractionPromptInfo interactionPromptInfo)
    {
        if (interactionPromptInfo.InteractableObjectInRange)
        {
            _interactionPromptText.gameObject.SetActive(true);
            _interactionPromptText.text = interactionPromptInfo.KeyBindingText + ": " + interactionPromptInfo.ActionText;
        }
        else
        {
            _interactionPromptText.gameObject.SetActive(false);
            _interactionPromptText.text = "";
        }
    }

    public void ShowDialogue(string dialogueText)
    {
        _dialogueBox.gameObject.SetActive(true);
        _dialogueText.text = dialogueText;
        _dialogueText.gameObject.SetActive(true);
    }

    public void CloseDialogueBox()
    {
        _dialogueBox.gameObject.SetActive(false);
        _dialogueText.text = "";
        _dialogueText.gameObject.SetActive(false);
    }
}
