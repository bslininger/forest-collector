public struct InteractionPromptInfo
{
    public bool InteractableObjectInRange { get; }
    public string KeyBindingText { get; }
    public string ActionText { get; }

    public InteractionPromptInfo(bool interactableObjectInRange, string keyBindingText, string actionText)
    {
        InteractableObjectInRange = interactableObjectInRange;
        KeyBindingText = keyBindingText;
        ActionText = actionText;
    }
}
