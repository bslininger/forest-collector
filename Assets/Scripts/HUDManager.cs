using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private InteractableObjectDetector _playerInteractableObjectDetector;
    [SerializeField] private TMPro.TMP_Text _interactionPromptText;
    [SerializeField] private Image _dialogueBox;
    private TMPro.TMP_Text _dialogueText;

    private void Awake()
    {
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
