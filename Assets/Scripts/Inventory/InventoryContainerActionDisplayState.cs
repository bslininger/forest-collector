public readonly struct InventoryContainerActionDisplayState
{
	public bool Enabled { get; }
	public bool Visible { get; }
	public string Text { get; }

	public InventoryContainerActionDisplayState(bool enabled, bool visible, string text)
	{
		Enabled = enabled;
		Visible = visible;
		Text = text;
	}
}
